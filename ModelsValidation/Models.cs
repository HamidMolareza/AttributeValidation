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
        public static MethodResult<T> ModelsMustValid<T> (
            this T models, bool showDefaultMessageToUser = true)
        where T : IEnumerable<object> =>
            models.ForEachUntilIsSuccess (model =>
                ModelMustValid (model, showDefaultMessageToUser).MapMethodResult ())
            .MapMethodResult (models);

        public static MethodResult<T> ModelMustValid<T> (
                this T model, bool showDefaultMessageToUser = true) =>
            MethodResult<T>.Ok (model)
            .OnSuccessOperateWhen (model.IsNotNull<T> ().IsSuccess,
                () => model!.GetType ().GetProperties ()
                .OperateWhen (propertyInfos => propertyInfos.Any (),
                    propertyInfos => propertyInfos.PropertiesMustValid (model, showDefaultMessageToUser)
                    .MapMethodResult (propertyInfos))
                .MapMethodResult (model)
            );

        internal static MethodResult ModelMustValid (
                IReadOnlyCollection<CustomAttributeData> attributes,
                string? propertyName,
                object? value,
                ValidationContext validationContext,
                bool showDefaultMessageToUser) =>
            OperateExtensions.OperateWhen (attributes.Any (),
                () => PropertyMustValid (attributes, propertyName, value, showDefaultMessageToUser, validationContext))
            .OnSuccessOperateWhen (value != null && NeedToCheck (value.GetType ()),
                () => ModelMustValid (value, showDefaultMessageToUser)
                .MapMethodResult ());

        private static bool NeedToCheck (Type type) {
            if (type.Namespace != null && type.Namespace.StartsWith ("System"))
                return false;
            return !type.IsArray && type.IsClass && !type.IsEnum && !type.IsInterface && !type.IsPointer;
        }

        private static MethodResult PropertiesMustValid<T> (
                this IEnumerable<PropertyInfo> propertyInfos, T model,
                bool showDefaultMessageToUser) =>
            model.IsNotNull<T> ()
            .OnSuccess (() => propertyInfos.ForEachUntilIsSuccess (
                propertyInfo => ModelMustValid (
                    propertyInfo.GetCustomAttributesData ().ToList (),
                    propertyInfo.Name, propertyInfo.TryGetValue (model!),
                    new ValidationContext (model), showDefaultMessageToUser)
            ));

        private static object? TryGetValue (
                this PropertyInfo propertyInfo,
                object values) => values
            .Try (propertyInfo.GetValue)
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