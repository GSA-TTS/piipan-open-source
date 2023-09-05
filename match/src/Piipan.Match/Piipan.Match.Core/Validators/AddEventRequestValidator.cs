using FluentValidation;
using FluentValidation.Internal;
using Org.BouncyCastle.Math.EC.Rfc7748;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.Shared.API.Validation;
using Piipan.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Piipan.Match.Core.Validators
{
    /// <summary>
    /// Validates the API match rsolution Add Event request from a client
    /// </summary>
    public class AddEventRequestValidator : AbstractValidator<AddEventRequest>
    {
        public AddEventRequestValidator()
        {
            RuleFor(r => r.Data)
                .NotNull()
                .NotEmpty();

            When(x => x.Data != null, () =>
            {
                RuleFor(r => r.Data)
                .NotNull()
                .NotEmpty()
                .Custom((val, context) =>
                {
                    var results = new List<ValidationResult>();
                    ValidationContext dataAnnotationContext = new ValidationContext(val);
                    
                    if (!Validator.TryValidateObject(val, dataAnnotationContext, results, true))
                    {
                        foreach (var result in results)
                        {
                            var validateColumn = result.MemberNames.FirstOrDefault();
                            string displayNameAttributeValue = ObjectProperty.GetDisplayNameAttribute(val.GetType(), validateColumn);

                            if (validateColumn == nameof(Disposition.FinalDispositionDate))
                            {
                                string finalDispositionDateDisplay = val.FinalDisposition switch
                                {
                                    "Benefits Approved" => "Benefits Start Date",
                                    "Benefits Terminated" => "Benefits End Date",
                                    _ => "Final Disposition Date"
                                };
                                displayNameAttributeValue = finalDispositionDateDisplay;
                            }

                            context.AddFailure(result.ErrorMessage.Replace(ValidationConstants.ValidationFieldPlaceholder, displayNameAttributeValue));
                        }

                    }
                });

                RuleFor(r => r.Data.InitialActionAt).Custom((val, context) =>
                {
                    CheckInitialActionAt(val, context);

                });

                RuleFor(r => r.Data.InitialActionTaken).Custom((val, context) =>
                {
                    CheckInitialActionTaken(val, context);
                });

                RuleFor(r => r.Data.FinalDispositionDate).Custom((val, context) =>
                {
                    if (val == null && !string.IsNullOrEmpty(context.InstanceToValidate.Data.FinalDisposition))
                    {
                        context.AddFailure("Final Disposition Date is required");
                    }
                });

                RuleFor(r => r.Data.FinalDisposition).Custom((val, context) =>
                {
                    if (string.IsNullOrEmpty(val) && context.InstanceToValidate.Data.FinalDispositionDate != null)
                    {
                        context.AddFailure("Final Disposition Taken is required because a date has been selected");
                    }
                });

                RuleFor(r => r.Data.InvalidMatchReason).Custom((val, context) =>
                {
                    CheckInvalidMatchReason(val, context);
                });


                RuleFor(r => r.Data.OtherReasoningForInvalidMatch).Custom((val, context) =>
                {
                    CheckOtherReasoningForInvalidMatch(val, context);
                });

            });

        }
        private void CheckInitialActionAt(System.DateTime? val, ValidationContext<AddEventRequest> context)
        {
            if (val == null)
            {
                if (!string.IsNullOrEmpty(context.InstanceToValidate.Data.InitialActionTaken))
                {
                    context.AddFailure("Initial Action Date is required");
                }
                else if (!string.IsNullOrEmpty(context.InstanceToValidate.Data.FinalDisposition))
                {
                    context.AddFailure("Initial Action Date is required because a Final Disposition has been selected");
                }
            }
        }
        private void CheckInitialActionTaken(string val, ValidationContext<AddEventRequest> context)
        {
            if(string.IsNullOrEmpty(val))
            {
                if (context.InstanceToValidate.Data.InitialActionAt != null)
                {
                    context.AddFailure("Initial Action Taken is required because a date has been selected");
                }
                if (!string.IsNullOrEmpty(context.InstanceToValidate.Data.FinalDisposition))
                {
                    context.AddFailure("Initial Action Taken is required because a Final Disposition has been selected");
                }
            }
        }
        private void CheckInvalidMatchReason(string val, ValidationContext<AddEventRequest> context)
        {
            if (string.IsNullOrEmpty(val))
            {
                if (context.InstanceToValidate.Data.InvalidMatch == true)
                {
                    context.AddFailure("Invalid Match Reason is required because Invalid Match was set to true");
                }
            }
            else
            {
                if (context.InstanceToValidate.Data.InvalidMatch != true)
                {
                    context.AddFailure("Invalid Match Reason is not allowed when Invalid Match is set to false");
                }
            }
        }
        private void CheckOtherReasoningForInvalidMatch(string val, ValidationContext<AddEventRequest> context)
        {
            if (string.IsNullOrEmpty(val))
            {
                if (context.InstanceToValidate.Data.InvalidMatchReason == "Other" && context.InstanceToValidate.Data.InvalidMatch == true)
                {
                    context.AddFailure("Reason for Other is required because Invalid Match Reason was set to Other");
                }
            }
            else
            {
                if (context.InstanceToValidate.Data.InvalidMatchReason != "Other")
                {
                    context.AddFailure("Reason for Other is not allowed when Invalid Match Reason is set to a value besides Other");
                }
                if (context.InstanceToValidate.Data.InvalidMatch == false)
                {
                    context.AddFailure("Reason for Other is not allowed when Invalid Match is set to false");
                }
            }
        }
    }
}
