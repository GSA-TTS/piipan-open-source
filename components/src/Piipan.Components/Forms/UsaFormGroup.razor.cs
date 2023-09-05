using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using Piipan.Components.Enums;

namespace Piipan.Components.Forms
{
    public partial class UsaFormGroup : IDisposable
    {
        public string Label { get; set; }
        public FieldIdentifier FieldIdentifier { get; set; }
        public InputStatus Status { get; private set; } = InputStatus.None;
        public string InputElementId { get; set; }

        public List<string> ValidationMessages { get; set; } = new List<string>();
        public Func<Task<List<string>>> PreverificationChecks { get; set; } = null;
        public List<string> FieldDependencies { get; set; } = new List<string>();
        protected bool HasErrors => ValidationMessages.Count > 0;

        /// <summary>
        /// Get all of the errors associated with this form group. First do prevalidation checks, and then if they pass validate the whole edit context.
        /// </summary>
        /// <param name="editContext"></param>
        /// <returns></returns>
        public async Task GetValidationErrorsAsync(EditContext editContext, bool refreshDependencies = false)
        {
            List<string> preverficiationErrors = PreverificationChecks == null ? null : (await PreverificationChecks());
            if (preverficiationErrors?.Count > 0)
            {
                ValidationMessages = preverficiationErrors;
            }
            else
            {
                editContext.Validate();
                ValidationMessages = editContext.GetValidationMessages(FieldIdentifier).ToList();
            }
            if (refreshDependencies)
            {
                var formGroupsToRefresh = Form.FormGroups.Where(n => n.FieldDependencies.Contains(FieldIdentifier.FieldName)).ToList();
                foreach (var group in formGroupsToRefresh)
                {
                    await group.GetValidationErrorsAsync(editContext, false);
                }
            }
            Status = HasErrors ? InputStatus.Error : InputStatus.None;
            this.StateHasChanged();
            Form.UpdateState();
        }

        /// <summary>
        /// When this form group is initialized, add this form group to the form
        /// </summary>
        protected override void OnInitialized()
        {
            Form.FormGroups.Add(this);
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
            if (firstRender)
            {
                // We need to re-check the state since the GroupId was likely set by a form input
                StateHasChanged();
            }
        }

        /// <summary>
        /// When this form group is disposed, remove it from the form
        /// </summary>
        public void Dispose()
        {
            Form.FormGroups.Remove(this);
        }
    }
}
