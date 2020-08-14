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
                value => value == false, ErrorMessage ?? "{0} is required.");
    }
}