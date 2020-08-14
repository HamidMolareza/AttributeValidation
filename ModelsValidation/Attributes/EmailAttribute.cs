using System;
using System.ComponentModel.DataAnnotations;
using ModelsValidation.Utility;

namespace ModelsValidation.Attributes {
    [AttributeUsage (AttributeTargets.Property | AttributeTargets.Field |
        AttributeTargets.Parameter)]
    public class EmailAttribute : ValidationAttribute {
        public int? MinimumLength { get; set; }
        public int? MaximumLength { get; set; }

        protected override ValidationResult IsValid (object? obj,
                ValidationContext validationContext) =>
            AttributeUtility.AttributeValidation<string> (obj,
                email => email.EmailMustValid (
                    minimumLength: MinimumLength, maximumLength: MaximumLength), ErrorMessage);
    }
}