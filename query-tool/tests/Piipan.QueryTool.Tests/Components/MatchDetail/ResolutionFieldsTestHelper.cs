using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bunit;
using Piipan.Components.Alerts;
using Piipan.Components.Forms;
using Xunit;
using static Piipan.Components.Forms.FormConstants;

namespace Piipan.QueryTool.Tests.Components.MatchDetail
{
    public static class ResolutionFieldsTestHelper
    {
        /// <summary>
        /// Helper method to validate the form and then check for errors
        /// </summary>
        public static async Task VerifyErrors(IRenderedComponent<UsaForm> usaForm, int errorCount, List<string> errors, bool validateForm = true, int? inputErrorMessagesCount = null)
        {
            bool isFormValid = false;
            if (validateForm)
            {
                await usaForm.InvokeAsync(async () =>
                {
                    isFormValid = await usaForm.Instance.ValidateForm();
                });
            }

            inputErrorMessagesCount ??= errorCount;
            var alertBox = usaForm.FindComponent<UsaAlertBox>();
            var alertBoxErrors = alertBox.FindAll("li");
            var inputErrorMessages = usaForm.FindAll($".{InputErrorMessageClass}");

            // Assert
            if (validateForm)
            {
                Assert.False(isFormValid);
            }

            Assert.Equal(errorCount, alertBoxErrors.Count);
            Assert.Equal(inputErrorMessagesCount, inputErrorMessages.Count);

            for (int i = 0; i < alertBoxErrors.Count; i++)
            {
                string error = alertBoxErrors[i].TextContent.Replace("\n", "");
                error = Regex.Replace(error, @"\s+", " ");
                Assert.Contains(errors[i], error);
            }

            for (int i = 0; i < inputErrorMessages.Count; i++)
            {
                string error = inputErrorMessages[i].TextContent.Replace("\n", "");
                error = Regex.Replace(error, @"\s+", " ");
                Assert.Contains(errors[i], error);
            }
        }
    }
}
