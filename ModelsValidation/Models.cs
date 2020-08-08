using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using FunctionalUtility.Extensions;
using FunctionalUtility.ResultUtility;
using ModelsValidation.ResultDetails;

namespace ModelsValidation {
    public static class Models {
        public static MethodResult<object?[]> ModelsMustValid (
                this IEnumerable<object?> models, bool showDefaultMessageToUser = true,
                int maximumDepth = SettingClass.MaximumDepth) =>
            models is null ?
            MethodResult<object?[]>.Fail (new ArgumentValidationError (
                new List<string> { $"{nameof(models)} is null." })) :
            models.ToArray ().ModelsMustValid (showDefaultMessageToUser: showDefaultMessageToUser,
                maximumDepth: maximumDepth);

        public static MethodResult<object?[]> ModelsMustValid (
                this object?[] models, bool showDefaultMessageToUser = true,
                int maximumDepth = SettingClass.MaximumDepth) =>
            models is null ?
            MethodResult<object?[]>.Fail (new ArgumentValidationError (
                new List<string> { $"{nameof(models)} is null." })) :
            models.ForEachUntilIsSuccess (model =>
                model is null ?
                MethodResult.Ok () :
                ModelMustValid (model, maximumDepth : maximumDepth,
                    showDefaultMessageToUser : showDefaultMessageToUser)
                .MapMethodResult ()
            )
            .MapMethodResult (models);

        public static MethodResult<object> ModelMustValid (
                this object model, int maximumDepth = SettingClass.MaximumDepth,
                bool showDefaultMessageToUser = true) =>
            model is null?
        MethodResult<object>.Fail (new ArgumentValidationError (new List<string> { $"{nameof(model)} is null." })):
            model.ModelMustValid (SettingClass.BeginDepth + 1, maximumDepth, showDefaultMessageToUser);

        private static MethodResult<object> ModelMustValid (
                this object model, int depth, int maximumDepth,
                bool showDefaultMessageToUser = true) =>
            model is null?
        MethodResult<object>.Fail (new ArgumentValidationError (new List<string> { $"{nameof(model)} is null." })):
            model.GetType ().GetProperties ()
            .OperateWhen (propertyInfos => propertyInfos.Any (),
                propertyInfos => propertyInfos.PropertiesMustValid (
                    model, showDefaultMessageToUser, depth, maximumDepth)
                .MapMethodResult (propertyInfos))
            .MapMethodResult (model);

        internal static MethodResult ModelMustValid (
                IReadOnlyCollection<CustomAttributeData> attributes,
                string? propertyName,
                object? value,
                ValidationContext validationContext,
                bool showDefaultMessageToUser,
                int depth, int maximumDepth) =>
            OperateExtensions.OperateWhen (attributes.Any (),
                () => PropertyMustValid (attributes, propertyName,
                    value, showDefaultMessageToUser, validationContext))
            .OnSuccessOperateWhen (value != null && NeedToCheck (value.GetType (), depth, maximumDepth),
                () => ModelMustValid (value!, depth + 1, maximumDepth,
                    showDefaultMessageToUser : showDefaultMessageToUser)
                .MapMethodResult ());

        private static bool NeedToCheck (Type type, int depth, int maximumDepth) {
            if (depth > maximumDepth)
                return false;
            if (type.Namespace != null && type.Namespace.StartsWith ("System"))
                return false;
            return !type.IsArray && type.IsClass && !type.IsEnum && !type.IsInterface && !type.IsPointer;
        }

        private static MethodResult PropertiesMustValid<T> (
                this IEnumerable<PropertyInfo> propertyInfos, T model,
                bool showDefaultMessageToUser,
                int depth,
                int maximumDepth) =>
            model.IsNotNull<T> ()
            .OnSuccess (() => propertyInfos.ForEachUntilIsSuccess (
                propertyInfo => ModelMustValid (
                    propertyInfo.GetCustomAttributesData ().ToList (),
                    propertyInfo.Name, propertyInfo.TryGetValue (model!),
                    new ValidationContext (model), showDefaultMessageToUser, depth, maximumDepth)
            ));

        private static object? TryGetValue (
                this PropertyInfo propertyInfo,
                object values) => values
            .TryMap (propertyInfo.GetValue)
            .OnFail (() => MethodResult<object?>.Ok (null))
            .GetValue ();

        private static MethodResult PropertyMustValid (
                IReadOnlyCollection<CustomAttributeData> attributes,
                string? propertyName,
                object? value, bool showDefaultMessageToUser,
                ValidationContext? instance = null) =>
            OperateExtensions.OperateWhen (attributes.Any (),
                () => GetValidParameterName (propertyName, instance)
                .IsNotNull<string> (new BadRequestError (message: "Can not detect parameter name."))
                .OnSuccess (validParameterName =>
                    attributes.ForEachUntilIsSuccess (attribute =>
                        Validate.ValidateByAttribute (attribute, validParameterName, value,
                            instance, showDefaultMessageToUser))
                )
            );

        private static string? GetValidParameterName (
                string? propertyName,
                ValidationContext? instance) =>
            !string.IsNullOrEmpty (propertyName) ? propertyName :
            instance is null || string.IsNullOrEmpty (instance.DisplayName) ? null :
            instance.DisplayName;
    }
}