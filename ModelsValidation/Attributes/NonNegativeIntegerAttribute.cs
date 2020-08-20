using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ModelsValidation.Utility;

namespace ModelsValidation.Attributes {
    [AttributeUsage (AttributeTargets.Property | AttributeTargets.Field |
        AttributeTargets.Parameter)]
    public class NonNegativeIntegerAttribute : ValidationAttribute, IClientModelValidator {
        protected override ValidationResult IsValid (object? obj,
                ValidationContext validationContext) =>
            AttributeUtility.AttributeValidation<int> (obj,
                intValue => intValue.MustNonNegativeInteger (),
                message => Format (message, validationContext.DisplayName),
                ErrorMessage);

        private static string Format (string message, string displayName) =>
            string.Format (message, displayName);

        public void AddValidation (ClientModelValidationContext context) {
            AttributeUtility.MergeAttribute (context.Attributes, "data-val", "true");

            var errorMessage = string.IsNullOrEmpty (ErrorMessage) ?
                "The input is not valid." : ErrorMessage;
            AttributeUtility.MergeAttribute (context.Attributes, "data-val-nonNegativeInteger", errorMessage);
        }
    }
}