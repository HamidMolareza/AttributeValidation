using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using AttributeValidation.ResultDetails;
using FunctionalUtility.Extensions;
using FunctionalUtility.ResultDetails;
using FunctionalUtility.ResultUtility;
using Microsoft.AspNetCore.Http;

namespace AttributeValidation {
    public static class AttributeValidation {
        private static MethodResult<ParameterInfo[]> ValidateInputs (
                MethodBase methodBase,
                IReadOnlyCollection<object?> ? values) => methodBase
            .IsNotNull<MethodBase> (new ErrorDetail (StatusCodes.Status400BadRequest,
                $"{nameof(methodBase)} is required."))
            .TryOnSuccess (methodBase.GetParameters)
            .OnSuccessFailWhen (parameters =>
                parameters.IsNullOrEmpty () && !values.IsNullOrEmpty (), new ArgumentError (
                    message: "The method parameters are inconsistent with the given input."))
            .OnSuccessFailWhen (parameters =>
                !parameters.IsNullOrEmpty () && (values.IsNullOrEmpty () || parameters.Length != values!.Count),
                new ArgumentError (message: "The method parameters are inconsistent with the given input."));

        private static MethodResult<List<KeyValuePair<ParameterInfo, object>>> MapParametersAndValues (
            IReadOnlyCollection<ParameterInfo> parameters, IReadOnlyCollection<object?> values) {
            var result = new List<KeyValuePair<ParameterInfo, object>> (parameters.Count);
            for (var i = 0; i < parameters.Count; i++) {
                var value = values.ElementAt (i);
                var parameter = parameters.ElementAt (i);
                if (value != null && value.GetType () != parameter.ParameterType) {
                    return MethodResult<List<KeyValuePair<ParameterInfo, object>>>.Fail (
                        new ArgumentError (message: "input types are inconsistent with parameters type."));
                }

                result.Add (new KeyValuePair<ParameterInfo, object> (parameter, values));
            }

            return MethodResult<List<KeyValuePair<ParameterInfo, object>>>.Ok (result);
        }

        private static MethodResult ValidateProperties (
                IReadOnlyCollection<CustomAttributeData> attributes,
                string? propertyName,
                object? value,
                ValidationContext validationContext) =>
            OperateExtensions.OperateWhen (attributes.Any (),
                () => PropertyMustValid (attributes, propertyName, value, validationContext))
            .OnSuccessOperateWhen (value != null,
                () => ModelPropertiesMustValid (value).MapMethodResult ());

        private static MethodResult MethodParametersMustValid (
                IReadOnlyCollection<ParameterInfo> parameters,
                IReadOnlyCollection<object?> values
            ) => MapParametersAndValues (parameters, values)
            .OnSuccess (parametersWithValues =>
                (validationContext: new ValidationContext (values), parametersWithValues))
            .OnSuccess (result =>
                result.parametersWithValues.ForEachUntilIsSuccess (parameterWithValue =>
                    ValidateProperties (
                        parameterWithValue.Key.CustomAttributes as IReadOnlyCollection<CustomAttributeData>, parameterWithValue.Key.Name,
                        parameterWithValue.Value, result.validationContext)
                )
            );

        public static MethodResult<MethodBase> MethodParametersMustValid (
                this MethodBase methodBase,
                IReadOnlyCollection<object?> ? values) =>
            ValidateInputs (methodBase, values)
            .OnSuccessOperateWhen (parameters => !parameters.IsNullOrEmpty (),
                parameters =>
                MethodParametersMustValid (parameters, values).MapMethodResult (parameters)
            ).MapMethodResult (methodBase);

        public static MethodResult<T> ModelsPropertiesMustValid<T> (
                this T models) where T : IEnumerable<object> => models
            .ForEachUntilIsSuccess (model => ModelPropertiesMustValid (model).MapMethodResult ())
            .MapMethodResult (models);

        public static MethodResult<T> ModelPropertiesMustValid<T> (
                this T model) =>
            model.IsNotNull<T> ()
            .TryOnSuccess (() => model.GetType ().GetProperties ())
            .OnSuccessOperateWhen (propertyInfos => propertyInfos.Any (),
                propertyInfos =>
                propertyInfos.ForEachUntilIsSuccess (propertyInfo =>
                    ValidateProperties (
                        propertyInfo.CustomAttributes as IReadOnlyCollection<CustomAttributeData>,
                        propertyInfo.Name, propertyInfo.TryGetValue (model), new ValidationContext (model))
                ).MapMethodResult (propertyInfos)
            ).MapMethodResult (model);

        private static string? GetValidParameterName (
                string propertyName,
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
                        property => TryExtensions.Try (() => property!.SetValue (classObj, item.TypedValue.Value, null))
                    ).MapMethodResult ()
                ).MapMethodResult (classObj)
            );

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

        //TODO: Refactor
        private static MethodResult ValidationResultIsSuccess (
            object? validationResult, Type attributeType,
            object classObject, string parameterName) {
            if (validationResult is null)
                return MethodResult.Ok ();

            switch (validationResult) {
                case bool isValid:
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
                case ValidationResult result:
                    if (!string.IsNullOrEmpty (result.ErrorMessage))
                        return MethodResult.Fail (new ArgumentValidationError (new List<string> { result.ErrorMessage }));
                    break;
                default:
                    return MethodResult.Fail (new ArgumentError (
                        title: "OutOfRangeError", message: $"Type of {nameof(validationResult)} is not expected."));
            }

            return MethodResult.Ok ();
        }

        private static MethodResult ValidateByAttribute (
                CustomAttributeData attribute, string parameterName,
                object value, ValidationContext validationContext) =>
            OperateExtensions.OperateWhen (attribute.AttributeType == typeof (ValidationAttribute),
                () => CreateAttributeObject (attribute)
                .OnSuccess (attributeObj =>
                    (validationResult: ValidateByAttribute (attribute, attributeObj, parameterName, value,
                        validationContext), attributeObj))
                .OnSuccess (result =>
                    ValidationResultIsSuccess (result.validationResult, attribute.AttributeType,
                        result.attributeObj, parameterName))
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

                        var parameters = value is null ? new object[] { } : new [] { value };
                        validationResult = validationMethod.Invoke (classObj, parameters);
                    } else {
                        var parameters = value is null ?
                            new object[] { validationContext } :
                            new [] { value, validationContext };
                        validationResult = validationMethod.Invoke (classObj, parameters);
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