using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FunctionalUtility.Extensions;
using FunctionalUtility.ResultUtility;
using ModelsValidation.ResultDetails;

namespace ModelsValidation.Utility {
    public static class AttributeUtility {
        private static string GetErrorMessage (
            ResultDetail resultDetail, Func<string, string> format) => format (resultDetail.Message);

        public static ValidationResult MapToValidationResult (
            this MethodResult @this, Func<string, string> format
        ) => @this.MapMethodResult (
            () => ValidationResult.Success,
            errorDetail =>
            new ValidationResult (GetErrorMessage (errorDetail, format))
        );

        public static ValidationResult MapToValidationResult<T> (
            this MethodResult<T> @this, Func<string, string> format
        ) => @this.MapMethodResult ().MapToValidationResult (format);

        public static ValidationResult AttributeValidation<T> (
                object? value, Func<T, bool> predicate, Func<string, string> format, string? errorMessage) =>
            value is null ?
            ValidationResult.Success :
            value!.As<T> ()
            .OnSuccessFailWhen (predicate,
                new AttributeValidationError (message: errorMessage ?? "{0} is not valid."))
            .MapToValidationResult (format);

        public static ValidationResult AttributeValidation<T> (
                object? value, Func<T, MethodResult> predicate, Func<string, string> format, string? errorMessage) =>
            value is null ?
            ValidationResult.Success :
            value!.As<T> ()
            .OnSuccess (predicate)
            .OnFail (result => result.Fail (
                new AttributeValidationError (message: errorMessage ?? result.Detail.Message)))
            .MapToValidationResult (format);

        public static ValidationResult AttributeValidation<T> (
                object? value, Func<T, MethodResult<T>> predicate, Func<string, string> format, string? errorMessage) =>
            value is null ?
            ValidationResult.Success :
            value!.As<T> ()
            .OnSuccess (predicate)
            .OnFail (result => result.Fail (
                new AttributeValidationError (message: errorMessage ?? result.Detail.Message)))
            .MapToValidationResult (format);

        public static bool MergeAttribute (IDictionary<string, string> attributes, string key, string value) {
            if (attributes.ContainsKey (key)) {
                return false;
            }

            attributes.Add (key, value);
            return true;
        }
    }
}