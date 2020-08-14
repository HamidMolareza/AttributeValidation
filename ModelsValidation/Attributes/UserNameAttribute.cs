using System;
using System.ComponentModel.DataAnnotations;
using ModelsValidation.Utility;

namespace ModelsValidation.Attributes {
    [AttributeUsage (AttributeTargets.Property | AttributeTargets.Field |
        AttributeTargets.Parameter)]
    public class UserNameAttribute : ValidationAttribute {
        public string? Regex { get; set; }
        public int? MinimumLength { get; set; }
        public int? MaximumLength { get; set; }

        protected override ValidationResult IsValid (object? obj,
                ValidationContext validationContext) =>
            AttributeUtility.AttributeValidation<string> (obj,
                username => username.UserNameMustValid (
                    regex: Regex, minimumLength: MinimumLength, maximumLength: MaximumLength), ErrorMessage);
    }
}