using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ModelsValidation.Utility;

namespace ModelsValidation.Attributes {
    [AttributeUsage (AttributeTargets.Property | AttributeTargets.Field |
        AttributeTargets.Parameter)]
    public class EmailAttribute : ValidationAttribute, IClientModelValidator {
        public int MinimumLength { get; set; } = 10;
        public int MaximumLength { get; set; } = 90;

        protected override ValidationResult IsValid (object? obj,
                ValidationContext validationContext) =>
            AttributeUtility.AttributeValidation<string> (obj,
                email => email.EmailMustValid (
                    minimumLength: MinimumLength, maximumLength: MaximumLength),
                message => Format (message, validationContext.DisplayName, MinimumLength, MaximumLength),
                ErrorMessage);

        private static string Format (string message, string displayName, int minimumLength, int maximumLength) =>
            string.Format (message, displayName, minimumLength, maximumLength);

        public void AddValidation (ClientModelValidationContext context) {
            AttributeUtility.MergeAttribute (context.Attributes, "data-val", "true");

            var errorMessage = string.IsNullOrEmpty (ErrorMessage) ?
                "The email is not valid." : ErrorMessage;
            AttributeUtility.MergeAttribute (context.Attributes, "data-val-email", errorMessage);

            var minLength = MinimumLength.ToString (CultureInfo.InvariantCulture);
            AttributeUtility.MergeAttribute (context.Attributes, "data-val-email-minLength", minLength);

            var maxLength = MaximumLength.ToString (CultureInfo.InvariantCulture);
            AttributeUtility.MergeAttribute (context.Attributes, "data-val-email-maxLength", maxLength);
        }
    }
}