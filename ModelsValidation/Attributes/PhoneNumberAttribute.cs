using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ModelsValidation.Utility;

namespace ModelsValidation.Attributes {
    [AttributeUsage (AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class PhoneNumberAttribute : ValidationAttribute, IClientModelValidator {
        public int MinimumLength { get; set; } = 5;
        public int MaximumLength { get; set; } = 30;
        public string Pattern { get; set; } = @"^[+0-9][0-9]+$";

        protected override ValidationResult IsValid (object? obj,
                ValidationContext validationContext) =>
            AttributeUtility.AttributeValidation<string> (obj,
                phoneNumber => phoneNumber.PhoneMustValid (
                    pattern: Pattern, minLength: MinimumLength, maxLength: MaximumLength),
                message => Format (message, validationContext.DisplayName),
                ErrorMessage);

        private static string Format (string message, string displayName) =>
            string.Format (message, displayName);

        public void AddValidation (ClientModelValidationContext context) {
            AttributeUtility.MergeAttribute (context.Attributes, "data-val", "true");

            var errorMessage = string.IsNullOrEmpty (ErrorMessage) ?
                "The phone is not valid." : ErrorMessage;
            AttributeUtility.MergeAttribute (context.Attributes, "data-val-phoneNumber", errorMessage);

            AttributeUtility.MergeAttribute (context.Attributes, "data-val-phoneNumber-pattern", Pattern);

            var minLength = MinimumLength.ToString (CultureInfo.InvariantCulture);
            AttributeUtility.MergeAttribute (context.Attributes, "data-val-phoneNumber-minLength", minLength);

            var maxLength = MaximumLength.ToString (CultureInfo.InvariantCulture);
            AttributeUtility.MergeAttribute (context.Attributes, "data-val-phoneNumber-maxLength", maxLength);
        }
    }
}