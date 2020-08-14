using System;
using System.ComponentModel.DataAnnotations;
using ModelsValidation.Utility;

namespace ModelsValidation.Attributes {
    [AttributeUsage (AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class PhoneNumberAttribute : ValidationAttribute {
        protected override ValidationResult IsValid (object? obj,
                ValidationContext validationContext) =>
            AttributeUtility.AttributeValidation<string> (obj,
                phoneNumber => phoneNumber.PhoneMustValid (),
                message => Format (message, validationContext.DisplayName),
                ErrorMessage);

        private static string Format (string message, string displayName) =>
            string.Format (message, displayName);
    }
}