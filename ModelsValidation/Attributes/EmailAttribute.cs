using System;
using System.ComponentModel.DataAnnotations;
using ModelsValidation.Utility;

namespace ModelsValidation.Attributes {
    [AttributeUsage (AttributeTargets.Property | AttributeTargets.Field |
        AttributeTargets.Parameter)]
    public class EmailAttribute : ValidationAttribute {
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
    }
}