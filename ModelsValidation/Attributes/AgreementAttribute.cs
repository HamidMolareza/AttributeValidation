using System;
using System.ComponentModel.DataAnnotations;
using ModelsValidation.Utility;

namespace ModelsValidation.Attributes {

    [AttributeUsage (AttributeTargets.Property | AttributeTargets.Field |
        AttributeTargets.Parameter)]
    public class AgreementAttribute : ValidationAttribute {
        protected override ValidationResult IsValid (object? obj,
                ValidationContext validationContext) =>
            AttributeUtility.AttributeValidation<bool> (obj,
                value => value == false,
                message => Format (message, validationContext.DisplayName),
                ErrorMessage ?? "{0} is required.");

        private static string Format (string message, string displayName) =>
            string.Format (message, displayName);
    }
}