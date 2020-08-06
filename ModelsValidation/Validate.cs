using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using FunctionalUtility.Extensions;
using FunctionalUtility.ResultUtility;
using ModelsValidation.ResultDetails;

namespace ModelsValidation {
    public static class Validate {
        internal static MethodResult ValidateByAttribute (
                CustomAttributeData attribute, string parameterName,
                object? value, ValidationContext? validationContext, bool showDefaultMessageToUser) =>
            OperateExtensions.OperateWhen (
                attribute.AttributeType.IsSubclassOf (typeof (ValidationAttribute)),
                () => CreateAttributeObject (attribute)
                .OnSuccess (attributeObj => ValidateByAttribute (attribute, attributeObj,
                        parameterName, value, validationContext)
                    .OnSuccess (validationResult => (attribute, validationResult))
                )
                .OnSuccess (result =>
                    ProcessValidationResult (result.validationResult, attribute.AttributeType,
                        result.attribute, parameterName, showDefaultMessageToUser))
            );

        private static MethodResult ProcessValidationResult (
            object? validationResult, Type attributeType,
            object classObject, string parameterName, bool showDefaultMessageToUser) {
            if (validationResult is null)
                return MethodResult.Ok ();

            switch (validationResult) {
                case bool isValid:
                    {
                        if (!isValid) {
                            var errorMessageMethod = attributeType.GetMethod ("FormatErrorMessage");
                            if (errorMessageMethod is null)
                                return MethodResult.Fail (
                                    new BadRequestError (
                                        message: $"Can not find (FormatErrorMessage) to get error. Type of class object: {classObject.GetType()}.",
                                        moreDetail : new { validationResult, attributeType, parameterName }));

                            var errorMessage = errorMessageMethod.Invoke (
                                classObject, new object[] { parameterName });
                            if (errorMessage != null) {
                                if (!(errorMessage is string errorResult))
                                    return MethodResult.Fail (
                                        new BadRequestError (message: $"Type of error message is not expected.. Type of class object: {classObject.GetType()}.",
                                            moreDetail : new { errorMessage, validationResult, attributeType, parameterName }));
                                return MethodResult.Fail (new ArgumentValidationError (
                                    new List<string> { errorResult }, showDefaultMessageToUser : showDefaultMessageToUser));
                            }
                        }

                        break;
                    }
                case ValidationResult result:
                    {
                        if (!string.IsNullOrEmpty (result.ErrorMessage))
                            return MethodResult.Fail (new ArgumentValidationError (
                                new List<string> { result.ErrorMessage }, showDefaultMessageToUser : showDefaultMessageToUser));
                        break;
                    }
                default:
                    {
                        return MethodResult.Fail (new BadRequestError ("OutOfRangeError",
                            $"Type of {nameof(validationResult)} is not expected." +
                            $" ({typeof(ValidationResult)}). Type of class object: {classObject.GetType()}.",
                            moreDetail : new { validationResult, attributeType, parameterName }));
                    }
            }

            return MethodResult.Ok ();
        }

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
                            new BadRequestError (message: $"Can not find validation method. (IsValid)-({attribute.AttributeType})",
                                moreDetail : new { parameterName, value, validationContext }));

                    validationResult = validationMethod.Invoke (classObj, new [] { value });
                } else {
                    validationMethod = attribute.AttributeType.GetMethod ("GetValidationResult",
                        new [] { typeof (object), typeof (ValidationContext) });
                    if (validationMethod is null) {
                        validationMethod = attribute.AttributeType.GetMethod (
                            "IsValid", new [] { typeof (object) });
                        if (validationMethod is null)
                            return MethodResult<object>.Fail (new BadRequestError (
                                message: $"Can not find validation method. (IsValid)-({attribute.AttributeType})",
                                moreDetail : new { parameterName, value, validationContext }));

                        validationResult = validationMethod.Invoke (classObj, new [] { value });
                    } else {
                        validationContext.DisplayName = parameterName;
                        validationResult = validationMethod.Invoke (classObj, new [] { value, validationContext });
                    }
                }

                return MethodResult<object>.Ok (validationResult!);
            } catch (TargetInvocationException e) when (e.InnerException is NullReferenceException) {
                return MethodResult<object>.Fail (
                    new BadRequestError (message: $"Missing parameter. ({parameterName})",
                        moreDetail : new { parameterName, value, validationContext }));
            }
        }

        private static MethodResult<object> CreateAttributeObject (
                CustomAttributeData attribute) =>
            new List<CustomAttributeTypedArgument> (attribute.ConstructorArguments)
            .Map (constructorArguments =>
                attribute.AttributeType.GetConstructor (constructorArguments
                    .Select (x => x.ArgumentType).ToArray ())
                .IsNotNull<ConstructorInfo> (
                    new BadRequestError (message: $"Can not find constructor. ({attribute.AttributeType})"))
                .TryOnSuccess (constructorInfo => constructorInfo
                    .Invoke (constructorArguments
                        .Select (x => x.Value).ToArray ())
                )
                .OnSuccess (classObj => SetClassNamedArguments (
                    (IReadOnlyCollection<CustomAttributeNamedArgument>) attribute.NamedArguments, classObj))
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