using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ModelsValidation.Utility;

namespace ModelsValidation.Attributes {

    [AttributeUsage (AttributeTargets.Property | AttributeTargets.Field |
        AttributeTargets.Parameter)]
    public class AgreementAttribute : ValidationAttribute, IClientModelValidator {
        protected override ValidationResult IsValid (object? obj,
                ValidationContext validationContext) =>
            AttributeUtility.AttributeValidation<bool> (obj,
                value => value == false,
                message => Format (message, validationContext.DisplayName),
                ErrorMessage ?? "{0} is required.");

        private static string Format (string message, string displayName) =>
            string.Format (message, displayName);

        public void AddValidation (ClientModelValidationContext context) {
            AttributeUtility.MergeAttribute (context.Attributes, "data-val", "true");

            var errorMessage = string.IsNullOrEmpty (ErrorMessage) ?
                "item is required." : ErrorMessage;
            AttributeUtility.MergeAttribute (context.Attributes, "data-val-agreement", errorMessage);
        }
    }
}