using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ModelsValidation.Utility;

namespace ModelsValidation.Attributes {
    [AttributeUsage (AttributeTargets.Property | AttributeTargets.Field |
        AttributeTargets.Parameter)]
    public class UserNameAttribute : ValidationAttribute, IClientModelValidator {
        public string Regex { get; set; } = @"^[a-zA-Z_][a-zA-Z0-9_]*$";
        public int MinimumLength { get; set; } = 5;
        public int MaximumLength { get; set; } = 35;

        protected override ValidationResult IsValid (object? obj,
                ValidationContext validationContext) =>
            AttributeUtility.AttributeValidation<string> (obj,
                username => username.UserNameMustValid (
                    regex: Regex, minimumLength: MinimumLength, maximumLength: MaximumLength),
                message => Format (message, validationContext.DisplayName, MinimumLength, MaximumLength),
                ErrorMessage);

        private static string Format (string message, string displayName, int minimumLength, int maximumLength) =>
            string.Format (message, displayName, minimumLength, maximumLength);

        public void AddValidation (ClientModelValidationContext context) {
            AttributeUtility.MergeAttribute (context.Attributes, "data-val", "true");

            var errorMessage = string.IsNullOrEmpty (ErrorMessage) ?
                "The user name is not valid." : ErrorMessage;
            AttributeUtility.MergeAttribute (context.Attributes, "data-val-userName", errorMessage);

            AttributeUtility.MergeAttribute (context.Attributes, "data-val-userName-pattern", Regex);

            var minLength = MinimumLength.ToString (CultureInfo.InvariantCulture);
            AttributeUtility.MergeAttribute (context.Attributes, "data-val-userName-minLength", minLength);

            var maxLength = MaximumLength.ToString (CultureInfo.InvariantCulture);
            AttributeUtility.MergeAttribute (context.Attributes, "data-val-userName-maxLength", maxLength);
        }
    }
}