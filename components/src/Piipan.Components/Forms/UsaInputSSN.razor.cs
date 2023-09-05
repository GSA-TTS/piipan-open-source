using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Piipan.Components.Forms
{
    /// <summary>
    /// The Social Security Number component
    /// </summary>
    public partial class UsaInputSSN
    {
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;
        IJSObjectReference ssnJavascriptReference;

        /// <summary>
        /// This timer is used to hide the last SSN character typed after 1 second. This protects it as expected, but allows
        /// the user to see what they're typing to validate it's correct.
        /// </summary>
        Timer ssnProtectionTimer = new Timer();

        protected override void OnInitialized()
        {
            if (!string.IsNullOrEmpty(CurrentValue))
            {
                MaskedValue = string.Join("", CurrentValue.Select(x => x != '-' ? '*' : x));
            }
            base.OnInitialized();
            ssnProtectionTimer.Interval = 1000;
            ssnProtectionTimer.Elapsed += async (object sender, ElapsedEventArgs e) =>
            {
                ssnProtectionTimer.Stop();
                MaskedValue ??= "";
                MaskedValue = string.Join("", MaskedValue.Select(n => n != '-' ? '*' : n));
                if (!unmasked)
                {
                    int cursorPosition = await ssnJavascriptReference.InvokeAsync<int>("GetCursorPosition", ElementReference);
                    await InvokeAsync(StateHasChanged);
                    await ssnJavascriptReference.InvokeVoidAsync("SetCursorPosition", ElementReference, cursorPosition);
                }
            };
        }

        /// <summary>
        /// Grab the ssn javascript reference to be used later
        /// </summary>
        /// <param name="firstRender"></param>
        /// <returns></returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (ssnJavascriptReference == null)
            {
                ssnJavascriptReference = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Piipan.Components/Forms/UsaInputSSN.razor.js");
            }
        }

        private string GetBackingSsnAfterInput(string inputValue, int cursorPosition)
        {
            char[] maskingCharacters = new char[] { '*', '-' };
            StringBuilder beginningSsnSegment = new StringBuilder();
            var middleSsnSegment = "";
            if (cursorPosition < 0)
            {
                return inputValue;
            }

            int numberCharsInEndSsnSegment = 0;
            for (int i = inputValue.Length - 1; i >= cursorPosition && maskingCharacters.Contains(inputValue[i]); i--)
            {
                numberCharsInEndSsnSegment++;
            }
            string endSsnSegment = numberCharsInEndSsnSegment <= CurrentValue.Length ? CurrentValue.Substring(CurrentValue.Length - numberCharsInEndSsnSegment) : "";

            int numberCharsInBeginningSsnSegment = 0;
            for (int i = 0; i < inputValue.Length - numberCharsInEndSsnSegment && i < cursorPosition && maskingCharacters.Contains(inputValue[i]); i++)
            {
                if (CurrentValue.Length > i)
                {
                    beginningSsnSegment.Append(CurrentValue[i]);
                }
                else
                {
                    beginningSsnSegment.Append(inputValue[i]);
                }
                numberCharsInBeginningSsnSegment++;
            }

            // If there are more chars than exist in the beginning and end ssn segment, we are missing the middle segment
            if (numberCharsInBeginningSsnSegment + numberCharsInEndSsnSegment < inputValue.Length)
            {
                middleSsnSegment = inputValue.Substring(numberCharsInBeginningSsnSegment, inputValue.Length - numberCharsInBeginningSsnSegment - numberCharsInEndSsnSegment);
            }
            if (!string.IsNullOrEmpty(beginningSsnSegment.ToString()) || !string.IsNullOrEmpty(middleSsnSegment) || !string.IsNullOrEmpty(endSsnSegment))
            {
                return beginningSsnSegment.ToString() + middleSsnSegment + endSsnSegment;
            }
            return inputValue;
        }

        /// <summary>
        /// Returns the last character that was typed into the SSN field
        /// </summary>
        private char? GetLastInputChar(string originalInputString, int cursorPosition)
        {
            if (originalInputString.Length > cursorPosition - 1 && cursorPosition > 0 && originalInputString.Length == MaskedValue?.Length + 1)
            {
                return originalInputString[cursorPosition - 1];
            }
            return null;
        }

        private void RemoveHyphens(ref string newSsnValue, ref int cursorPosition)
        {
            int hyphensRemovedBeforeCursor = newSsnValue.Substring(0, cursorPosition).Count((c) => c == '-');
            cursorPosition -= hyphensRemovedBeforeCursor;
            newSsnValue = newSsnValue.Replace("-", "");
        }

        private void AddHyphen(ref string originalInputString, ref string maskedSsn, ref int cursorPosition, int hyphenIndex, bool didAddCharacters)
        {
            if (originalInputString.Length > hyphenIndex || (originalInputString.Length == hyphenIndex && didAddCharacters))
            {
                originalInputString = originalInputString.Insert(hyphenIndex, "-");
                maskedSsn = maskedSsn.Insert(hyphenIndex, "-");
                if (cursorPosition >= hyphenIndex)
                {
                    cursorPosition++;
                }
            }
        }

        private (string unmaskedValue, string maskedValue) GetUnmaskedAndMaskedValues(string originalInputString,
            string newSsnValue, ref int cursorPosition, bool isCurrentlyUnmasked)
        {
            string unmaskedSsnValue = newSsnValue;
            char? lastChar = GetLastInputChar(originalInputString, cursorPosition);
            bool didAddCharacters = newSsnValue.Length > CurrentValue.Length;

            RemoveHyphens(ref unmaskedSsnValue, ref cursorPosition);

            var maskedSsn = new string('*', unmaskedSsnValue.Length);

            // if the SSN is currently masked, show the last character typed for a small period of time.
            if (lastChar != null && !isCurrentlyUnmasked)
            {
                char[] array = maskedSsn.ToCharArray();
                array[cursorPosition - 1] = lastChar.Value;
                maskedSsn = new string(array);
            }
            AddHyphen(ref unmaskedSsnValue, ref maskedSsn, ref cursorPosition, 3, didAddCharacters);
            AddHyphen(ref unmaskedSsnValue, ref maskedSsn, ref cursorPosition, 6, didAddCharacters);
            if (unmaskedSsnValue.Length > 11)
            {
                unmaskedSsnValue = unmaskedSsnValue.Substring(0, 11);
                maskedSsn = maskedSsn.Substring(0, 11);
            }
            return (unmaskedSsnValue, maskedSsn);
        }

        /// <summary>
        /// On input, we need to do add/remove hyphens, as well as potentially protect the value from prying eyes.
        /// </summary>
        private async Task Input(ChangeEventArgs e)
        {
            CurrentValue ??= "";
            string value = e.Value as string;
            var cursorPosition = await ssnJavascriptReference.InvokeAsync<int>("GetCursorPosition", ElementReference);
            cursorPosition = Math.Min(cursorPosition, value.Length);

            if (!unmasked)
            {
                ssnProtectionTimer.Stop();
                value = GetBackingSsnAfterInput(value, cursorPosition);
                ssnProtectionTimer.Start();
            }

            (string unmaskedSsn, string maskedSsn) = GetUnmaskedAndMaskedValues(e.Value as string, value, ref cursorPosition, unmasked);

            if ((unmasked && CurrentValue == unmaskedSsn) || (!unmasked && MaskedValue == maskedSsn))
            {
                // Reset the value. Blazor won't rebind, but we need to refresh it anyway
                // This happens when you try deleting a hyphen that's in the middle of the SSN and the above logic puts it back in.
                await ssnJavascriptReference.InvokeVoidAsync("SetValue", ElementReference, unmasked ? unmaskedSsn : maskedSsn);
            }
            CurrentValue = unmaskedSsn;
            MaskedValue = maskedSsn;
            StateHasChanged();
            await ValueChanged.InvokeAsync(unmaskedSsn);
            await ssnJavascriptReference.InvokeVoidAsync("SetCursorPosition", ElementReference, cursorPosition);
        }
    }
}
