namespace Piipan.Shared.API.Validation
{
    /// <summary>
    /// Common validation messages that can be used in multiple client-side projects. @@@ is a placeholder, that will be replaced later by
    /// the consuming application. It will put in the name of the field, and optionally create a link that when clicked, focuses the cursor
    /// into that field.
    /// </summary>
    public class ValidationConstants
    {
        /// <summary>
        /// Used for a placeholder for the name of whatever field is being validated.
        /// </summary>
        public const string ValidationFieldPlaceholder = "@@@";

        /// <summary>
        /// Used when a field is invalid, such as an invalid date
        /// </summary>
        public const string InvalidMessage = $"{ValidationFieldPlaceholder} is invalid";

        /// <summary>
        /// Used for when a field is required, and is not filled in
        /// </summary>
        public const string RequiredMessage = $"{ValidationFieldPlaceholder} is required";

        /// <summary>
        /// Used for social security number validation, when the SSN is not in the correct format
        /// </summary>
        public const string SSNInvalidFormatMessage = $"{ValidationFieldPlaceholder} must have the form ###-##-####";

        /// <summary>
        /// Used for social security number validation, when the SSN doesn't have valid first 3 digits
        /// </summary>
        public const string SSNInvalidFirstThreeDigitsMessage = $"The first three numbers of {ValidationFieldPlaceholder} cannot be {{0}}";

        /// <summary>
        /// Used for social security number validation, when the SSN doesn't have valid middle 2 digits
        /// </summary>
        public const string SSNInvalidMiddleTwoDigitsMessage = $"The middle two numbers of {ValidationFieldPlaceholder} cannot be 00";

        /// <summary>
        /// Used for social security number validation, when the SSN doesn't have valid last 4 digits
        /// </summary>
        public const string SSNInvalidLastFourDigitsMessage = $"The last four numbers of {ValidationFieldPlaceholder} cannot be 0000";

        /// <summary>
        /// Used for names when validating they would have at least one character after normalization
        /// </summary>
        public const string MustStartWithALetter = $"{ValidationFieldPlaceholder} must start with a letter";

        /// <summary>
        /// Used for dates that we want to make sure are between two dates
        /// </summary>
        public const string DateBetweenMessage = $"{ValidationFieldPlaceholder} must be between {{0}} and {{1}}";

        /// <summary>
        /// Used for dates that we want to make sure are before another date
        /// </summary>
        public const string DateBeforeMessage = $"{ValidationFieldPlaceholder} must be before {{0}}";

        /// <summary>
        /// Used for dates that we want to make sure are after another date
        /// </summary>
        public const string DateAfterMessage = $"{ValidationFieldPlaceholder} must be after {{0}}";

        /// <summary>
        /// Used for dates that we want to make sure are between 1/1/1900 and 12/31/2050
        /// </summary>
        public const string DateBetween1900_2050Message = $"{ValidationFieldPlaceholder} must have a year between 1900 and 2050";

        /// <summary>
        /// Used for names when checking to see if it contains any non-ascii characters
        /// </summary>
        public const string InvalidCharacterInNameMessage = $"Change {{0}} in {{1}}. The {ValidationFieldPlaceholder} should only contain standard ASCII characters, including the letters A-Z, numbers 0-9, and some select characters including hyphens.";

        /// <summary>
        /// Used for things like Participant ID and Case ID where only numbers, letters, underscores, and hyphens are allowed
        /// </summary>
        public const string CanOnlyContainAlphanumericUnderscoreHyphenMessage = $"{ValidationFieldPlaceholder} must contain uppercase letters (A-Z), lowercase letters (a-z), numbers (0-9), underscore (_), dash (-).";

        /// <summary>
        /// Used for when a field has a max length
        /// </summary>
        public const string MaxLengthMessage = $"{ValidationFieldPlaceholder} can be maximum {{0}} characters.";
    }
}
