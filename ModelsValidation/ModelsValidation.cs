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
    public static class Validation {
        //TODO: 1
        public static MethodResult<MethodBase> MethodParametersMustValid (
                this MethodBase methodBase,
                IReadOnlyCollection<object?> ? values) =>
            InputsMustValid (methodBase, values)
            .OnSuccessOperateWhen (parameters => parameters.IsNotNullOrEmpty (),
                parameters =>
                MethodParametersMustValid (parameters, values)
                .MapMethodResult (parameters)
            )
            .MapMethodResult (methodBase);

        //TODO: 2
        private static MethodResult<ParameterInfo[]> InputsMustValid (
                MethodBase methodBase,
                IReadOnlyCollection<object?> ? values) =>
            methodBase.IsNotNull<MethodBase> (new ErrorDetail (StatusCodes.Status400BadRequest,
                $"{nameof(methodBase)} is required."))
            .TryOnSuccess (methodBase.GetParameters)
            .OnSuccessFailWhen (parameters =>
                parameters.IsNullOrEmpty () && !values.IsNullOrEmpty (), new ErrorDetail (
                    StatusCodes.Status400BadRequest,
                    message: "The method parameters are inconsistent with the given input."))
            .OnSuccessFailWhen (parameters =>
                !parameters.IsNullOrEmpty () &&
                (values.IsNullOrEmpty () || parameters.Length != values!.Count),
                new ErrorDetail (StatusCodes.Status400BadRequest,
                    message: "The method parameters are inconsistent with the given input."));

        //TODO: 3
        private static MethodResult MethodParametersMustValid (
                IReadOnlyCollection<ParameterInfo> parameters,
                IReadOnlyCollection<object?> ? values
            ) => MapParametersAndValues (parameters, values)
            .OnSuccess (parametersWithValues =>
                (validationContext: new ValidationContext (values), parametersWithValues))
            .OnSuccess (result =>
                result.parametersWithValues.ForEachUntilIsSuccess (parameterWithValue =>
                    ModelPropertiesMustValid (
                        parameterWithValue.Key.GetCustomAttributesData ().ToList (),
                        parameterWithValue.Key.Name,
                        parameterWithValue.Value, result.validationContext)
                )
            );

        //TODO: 4
        //TODO: ===
        private static MethodResult<List<KeyValuePair<ParameterInfo, object>>> MapParametersAndValues (
            IReadOnlyCollection<ParameterInfo> parameters, IReadOnlyCollection<object?> ? values) {
            var result = new List<KeyValuePair<ParameterInfo, object>> (parameters.Count);

            for (var i = 0; i < parameters.Count; i++) {
                var value = values.ElementAt (i);
                var parameter = parameters.ElementAt (i);
                if (value != null && value.GetType () != parameter.ParameterType) {
                    return MethodResult<List<KeyValuePair<ParameterInfo, object>>>.Fail (
                        new ArgumentError (message: "input types are inconsistent with parameters type."));
                }

                result.Add (new KeyValuePair<ParameterInfo, object> (parameter, value));
            }

            return MethodResult<List<KeyValuePair<ParameterInfo, object>>>.Ok (result);
        }

        //TODO: 5
        private static MethodResult ModelPropertiesMustValid (
                IReadOnlyCollection<CustomAttributeData> attributes,
                string? propertyName,
                object? value,
                ValidationContext validationContext) =>
            OperateExtensions.OperateWhen (attributes.Any (),
                () => PropertyMustValid (attributes, propertyName, value, validationContext))
            .OnSuccessOperateWhen (value != null && value.GetType ().Namespace != "System",
                () => ModelPropertiesMustValid (value)
                .MapMethodResult ());

        //TODO: 6
        private static MethodResult PropertyMustValid (
                IReadOnlyCollection<CustomAttributeData> attributes,
                string? propertyName,
                object? value,
                ValidationContext? instance = null) =>
            OperateExtensions.OperateWhen (attributes.Any (),
                () => GetValidParameterName (propertyName, instance)
                .IsNotNull<string> (new ErrorDetail (StatusCodes.Status400BadRequest,
                    "Can not detect parameter name."))
                .OnSuccess (validParameterName =>
                    attributes.ForEachUntilIsSuccess (attribute =>
                        ValidateByAttribute (attribute, validParameterName, value, instance)
                    )
                )
            );

        //TODO: 7
        public static MethodResult<T> ModelPropertiesMustValid<T> (
                this T model) => MethodResult<T>.Ok (model)
            .OnSuccessOperateWhen (model.IsNotNull<T> ().IsSuccess,
                () => model!.GetType ().GetProperties ()
                .OperateWhen (propertyInfos => propertyInfos.Any (),
                    propertyInfos => propertyInfos.ModelPropertiesMustValid (model)
                    .MapMethodResult (propertyInfos))
                .MapMethodResult (model)
            );

        //TODO: 8
        private static MethodResult ModelPropertiesMustValid<T> (
                this IEnumerable<PropertyInfo> propertyInfos, T model) =>
            propertyInfos.ForEachUntilIsSuccess (
                propertyInfo => ModelPropertiesMustValid (
                    propertyInfo.GetCustomAttributesData ().ToList (),
                    propertyInfo.Name, propertyInfo.TryGetValue (model), new ValidationContext (model))
            );

        public static MethodResult<T> ModelsPropertiesMustValid<T> (
                this T models) where T : IEnumerable<object> =>
            models.ForEachUntilIsSuccess (model => ModelPropertiesMustValid (model).MapMethodResult ())
            .MapMethodResult (models);

        private static string? GetValidParameterName (
                string? propertyName,
                ValidationContext? instance) =>
            !string.IsNullOrEmpty (propertyName) ? propertyName :
            instance is null || string.IsNullOrEmpty (instance.DisplayName) ? null :
            instance.DisplayName;

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

        //TODO: Here - 1
        private static MethodResult ValidateByAttribute (
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

        private static object? TryGetValue (
                this PropertyInfo propertyInfo,
                object values) => values
            .Try (propertyInfo.GetValue)
            .OnFail (() => MethodResult<object?>.Ok (null))
            .GetValue ();
    }
}