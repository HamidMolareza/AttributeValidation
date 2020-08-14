using System;
using System.ComponentModel.DataAnnotations;
using FunctionalUtility.Extensions;
using FunctionalUtility.ResultUtility;
using ModelsValidation.ResultDetails;

namespace ModelsValidation.Utility {
    public static class AttributeUtility {
        private static string GetErrorMessage (
            ResultDetail resultDetail) => resultDetail.Message;

        public static ValidationResult MapToValidationResult (
            this MethodResult @this
        ) => @this.MapMethodResult (
            () => ValidationResult.Success,
            errorDetail =>
            new ValidationResult (GetErrorMessage (errorDetail))
        );

        public static ValidationResult MapToValidationResult<T> (
            this MethodResult<T> @this
        ) => @this.MapMethodResult ().MapToValidationResult ();

        public static ValidationResult AttributeValidation<T> (
                object? value, Func<T, bool> predicate, string? errorMessage) =>
            value is null ?
            ValidationResult.Success :
            value!.As<T> ()
            .OnSuccessFailWhen (predicate,
                new AttributeValidationError (message: errorMessage ?? "{0] is not valid."))
            .MapToValidationResult ();

        public static ValidationResult AttributeValidation<T> (
                object? value, Func<T, MethodResult> predicate, string? errorMessage) =>
            value is null ?
            ValidationResult.Success :
            value!.As<T> ()
            .OnSuccess (predicate)
            .OnFail (result => result.Fail (
                new AttributeValidationError (message: errorMessage ?? GetErrorMessage (result.Detail))))
            .MapToValidationResult ();

        public static ValidationResult AttributeValidation<T> (
                object? value, Func<T, MethodResult<T>> predicate, string? errorMessage) =>
            value is null ?
            ValidationResult.Success :
            value!.As<T> ()
            .OnSuccess (predicate)
            .OnFail (result => result.Fail (
                new AttributeValidationError (message: errorMessage ?? GetErrorMessage (result.Detail))))
            .MapToValidationResult ();
    }
}