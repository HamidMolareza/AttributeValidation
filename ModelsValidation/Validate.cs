using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using FunctionalUtility.Extensions;
using FunctionalUtility.ResultDetails;
using FunctionalUtility.ResultUtility;
using Microsoft.AspNetCore.Http;
using ModelsValidation.ResultDetails;

namespace ModelsValidation {
    public static class Validate {
        internal static MethodResult ValidateByAttribute (
                CustomAttributeData attribute, string parameterName,
                object? value, ValidationContext? validationContext) =>
            OperateExtensions.OperateWhen (attribute.AttributeType.IsSubclassOf (typeof (ValidationAttribute)),
                () => CreateAttributeObject (attribute)
                .OnSuccess (attributeObj => ValidateByAttribute (attribute, attributeObj, parameterName, value,
                    validationContext).OnSuccess (validationResult => (attribute, validationResult)))
                .OnSuccess (result =>
                    ValidationResultIsSuccess (result.validationResult, attribute.AttributeType,
                        result.attribute, parameterName))
            );

        //TODO: Refactor
        private static MethodResult ValidationResultIsSuccess (
            object? validationResult, Type attributeType,
            object classObject, string parameterName) {
            if (validationResult is null)
                return MethodResult.Ok ();

            switch (validationResult) {
                case bool isValid:
                    {
                        if (!isValid) {
                            var errorMessageMethod = attributeType.GetMethod ("FormatErrorMessage");
                            if (errorMessageMethod is null)
                                return MethodResult.Fail (
                                    new ArgumentError (message: "Can not find (FormatErrorMessage) to get error."));

                            var errorMessage = errorMessageMethod.Invoke (classObject, new object[] { parameterName });
                            if (errorMessage != null) {
                                if (!(errorMessage is string errorResult))
                                    return MethodResult.Fail (
                                        new ArgumentError (message: "Type of error message is not expected."));
                                return MethodResult.Fail (new ArgumentValidationError (new List<string> { errorResult }));
                            }
                        }

                        break;
                    }
                case ValidationResult result:
                    {
                        if (!string.IsNullOrEmpty (result.ErrorMessage))
                            return MethodResult.Fail (new ArgumentValidationError (new List<string> { result.ErrorMessage }));
                        break;
                    }
                default:
                    {
                        return MethodResult.Fail (new ErrorDetail (StatusCodes.Status500InternalServerError,
                            title: "OutOfRangeError", message: $"Type of {nameof(validationResult)} is not expected." +
                            $" ({typeof(ValidationResult)})"));
                    }
            }

            return MethodResult.Ok ();
        }

        //TODO: Refactor
        private static MethodResult<object> ValidateByAttribute (
            CustomAttributeData attribute, object classObj, string parameterName,
            object? value, ValidationContext? validationContext) {
            try {
                MethodInfo? validationMethod;
                object? validationResult;
                if (validationContext is null) {
                    validationMethod = attribute.AttributeType.GetMethod ("IsValid", new [] { typeof (object) });
                    if (validationMethod is null)
                        return MethodResult<object>.Fail (
                            new ArgumentError (message: $"Can not find validation method. ({attribute.AttributeType})"));

                    var parameters = value is null ? new object[] { } : new [] { value };
                    validationResult = validationMethod.Invoke (classObj, parameters);
                } else {
                    validationMethod = attribute.AttributeType.GetMethod ("GetValidationResult",
                        new [] { typeof (object), typeof (ValidationContext) });
                    if (validationMethod is null) {
                        validationMethod = attribute.AttributeType.GetMethod ("IsValid", new [] { typeof (object) });
                        if (validationMethod is null)
                            return MethodResult<object>.Fail (
                                new ArgumentError (
                                    message: $"Can not find validation method. ({attribute.AttributeType})"));

                        //TODO: Check parameters
                        //var parameters = value is null ? new object[] { } : new [] { value };
                        validationResult = validationMethod.Invoke (classObj, new [] { value });
                    } else {
                        validationContext.DisplayName = parameterName;
                        validationResult = validationMethod.Invoke (classObj, new [] { value, validationContext });
                    }
                }

                return MethodResult<object>.Ok (validationResult);
            } catch (TargetInvocationException e) when (e.InnerException is NullReferenceException) {
                return MethodResult<object>.Fail (
                    new ArgumentError (message: $"Missing parameters. ({parameterName})"));
            }
        }

        private static MethodResult<object> CreateAttributeObject (
                CustomAttributeData attribute) =>
            new List<CustomAttributeTypedArgument> (attribute.ConstructorArguments)
            .Map (constructorArguments =>
                attribute.AttributeType.GetConstructor (constructorArguments
                    .Select (x => x.ArgumentType).ToArray ())
                .IsNotNull<ConstructorInfo> (
                    new ErrorDetail (StatusCodes.Status400BadRequest,
                        $"Can not find constructor. ({attribute.AttributeType})"))
                .TryOnSuccess (constructorInfo => constructorInfo
                    .Invoke (constructorArguments
                        .Select (x => x.Value).ToArray ())
                )
                .OnSuccess (classObj =>
                    SetClassNamedArguments (
                        (IReadOnlyCollection<CustomAttributeNamedArgument>) attribute.NamedArguments, classObj)
                )
            );

        private static MethodResult<object> SetClassNamedArguments (
                IReadOnlyCollection<CustomAttributeNamedArgument> namedArguments,
                object classObj) => classObj
            .OperateWhen (namedArguments.Any (),
                () => namedArguments.ForEachUntilIsSuccess (item => classObj
                    .GetType ()
                    .GetProperty (item.MemberName, BindingFlags.Public | BindingFlags.Instance)
                    .OperateWhen (property => property != null && property.CanWrite,
                        property => TryExtensions.Try (() => property!.SetValue (classObj,
                            item.TypedValue.Value, null)))
                    .MapMethodResult ()
                ).MapMethodResult (classObj)
            );
    }
}