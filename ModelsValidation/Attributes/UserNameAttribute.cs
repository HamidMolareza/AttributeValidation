using System;
using System.ComponentModel.DataAnnotations;
using ModelsValidation.Utility;

namespace ModelsValidation.Attributes {
    [AttributeUsage (AttributeTargets.Property | AttributeTargets.Field |
        AttributeTargets.Parameter)]
    public class UserNameAttribute : ValidationAttribute {
        public string? Regex { get; set; }
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
    }
}