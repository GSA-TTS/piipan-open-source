using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Piipan.Components.Routing;

namespace Piipan.Components.Forms
{
    public partial class UsaForm
    {
        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] protected PiipanNavigationManager NavigationManager { get; set; }
        private bool HasErrors => currentErrors?.Count() > 0;
        private EditContext editContext;
        private bool showAlertBox = false;
        private bool refreshAlertBox = false;
        [Parameter] public string Id { get; set; } = "f" + Guid.NewGuid();
        public List<UsaFormGroup> FormGroups { get; set; } = new List<UsaFormGroup>();
        private List<(UsaFormGroup FormGroup, IEnumerable<string> Errors)> currentErrors =
            new List<(UsaFormGroup FormGroup, IEnumerable<string> Errors)>();

        private bool _isDirty = false;
        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                _isDirty = value;
                IsDirtyChanged?.Invoke(_isDirty);
            }
        }
        public Func<bool, Task> IsDirtyChanged { get; set; }
        private bool readyToRender = false;

        /// <summary>
        /// Set the edit context of this form when it's initialized
        /// </summary>
        protected override void OnInitialized()
        {
            editContext = new EditContext(Model);
        }

        public async Task SubmitForm()
        {
            currentErrors.Clear();
            await OnSubmit();
            if (SubmitErrors?.Count > 0)
            {
                foreach (var error in SubmitErrors)
                {
                    var foundFormGroup = string.IsNullOrEmpty(error.Property) ? null :
                        FormGroups.FirstOrDefault(n => n.InputElementId == error.Property.Replace('.', '_'));
                    if (foundFormGroup != null)
                    {
                        currentErrors.Add((foundFormGroup, new List<string>() { error.Error }));
                    }
                    else
                    {
                        currentErrors.Add((null, new List<string>() { error.Error }));
                    }
                }
                showAlertBox = true;
                IsDirty = true;
                StateHasChanged();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                // Register the form for validation. That hooks up a "submit" event, which Javascript will then fire off when the form is attempted to be submitted.
                // When that occurs, we'll perform error checks (it eventually calls ValidateForm and, if there are no errors, PresubmitForm below).
                readyToRender = true;
                StateHasChanged();
            }
        }

        /// <summary>
        /// Update the state of the form, such as after a field changes its value
        /// </summary>
        public void UpdateState()
        {
            if (refreshAlertBox)
            {
                currentErrors.Clear();
                foreach (var formGroup in FormGroups)
                {
                    if (formGroup.ValidationMessages.Any())
                    {
                        currentErrors.Add((formGroup, formGroup.ValidationMessages));
                    }
                }
                StateHasChanged();
            }
            IsDirty = true;
        }

        public async Task<bool> ValidateForm()
        {
            currentErrors.Clear();
            for (int i = 0; i < FormGroups.Count; i++)
            {
                await FormGroups[i].GetValidationErrorsAsync(editContext);
            }
            refreshAlertBox = true;
            UpdateState();
            showAlertBox = currentErrors.Count != 0;
            StateHasChanged();
            if (showAlertBox)
            {
                await ScrollToElement($"{Id}-alert");
            }
            return !showAlertBox;
        }

        private async Task FocusElement(string elementId)
        {
            await JSRuntime.InvokeVoidAsync("piipan.utilities.focusElement", elementId);
        }

        private async Task ScrollToElement(string elementId)
        {
            await JSRuntime.InvokeVoidAsync("piipan.utilities.scrollToElement", elementId);
        }
    }
}
