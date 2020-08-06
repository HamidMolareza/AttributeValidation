using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using FunctionalUtility.Extensions;
using FunctionalUtility.ResultUtility;
using ModelsValidation.ResultDetails;

namespace ModelsValidation {
    public static class Method {
        public static MethodResult<MethodBase> MethodParametersMustValid (
                this MethodBase methodBase,
                IReadOnlyCollection<object?> ? values,
                bool showDefaultMessageToUser = true,
                int maximumDepth = SettingClass.MaximumDepth) =>
            InputsMustValid (methodBase, values)
            .OnSuccessOperateWhen (parameters => parameters.IsNotNullOrEmpty (),
                parameters =>
                MethodParametersMustValid (parameters, values,
                    showDefaultMessageToUser : showDefaultMessageToUser,
                    maximumDepth : maximumDepth)
                .MapMethodResult (parameters)
            )
            .MapMethodResult (methodBase);

        private static MethodResult<ParameterInfo[]> InputsMustValid (
                MethodBase methodBase,
                IReadOnlyCollection<object?> ? values) =>
            methodBase.IsNotNull<MethodBase> (new BadRequestError ($"{nameof(methodBase)} is required."))
            .TryOnSuccess (methodBase.GetParameters)
            .OnSuccessFailWhen (parameters =>
                parameters.IsNullOrEmpty () && !values.IsNullOrEmpty (), new BadRequestError (
                    message: "The method parameters are inconsistent with the given input."))
            .OnSuccessFailWhen (parameters =>
                !parameters.IsNullOrEmpty () &&
                (values.IsNullOrEmpty () || parameters.Length != values!.Count),
                new BadRequestError (message:
                    "The method parameters are inconsistent with the given input."));

        private static MethodResult MethodParametersMustValid (
                IReadOnlyCollection<ParameterInfo> parameters,
                IReadOnlyCollection<object?> ? values,
                bool showDefaultMessageToUser,
                int maximumDepth
            ) => MapParametersAndValues (parameters, values)
            .OnSuccess (parametersWithValues =>
                (validationContext: new ValidationContext (values), parametersWithValues))
            .OnSuccess (result =>
                result.parametersWithValues.ForEachUntilIsSuccess (parameterWithValue =>
                    Models.ModelMustValid (
                        parameterWithValue.Key.GetCustomAttributesData ().ToList (),
                        parameterWithValue.Key.Name,
                        parameterWithValue.Value, result.validationContext,
                        showDefaultMessageToUser,
                        SettingClass.BeginDepth, maximumDepth)
                )
            );

        private static MethodResult<List<KeyValuePair<ParameterInfo, object?>>> MapParametersAndValues (
            IReadOnlyCollection<ParameterInfo> parameters, IReadOnlyCollection<object?> ? values) {
            if (parameters.Count > 0 && (values is null || values.Count != parameters.Count))
                return MethodResult<List<KeyValuePair<ParameterInfo, object?>>>.Fail (
                    new BadRequestError (message: "The method parameters are inconsistent with the given input."));

            var result = new List<KeyValuePair<ParameterInfo, object?>> (parameters.Count);

            for (var i = 0; i < parameters.Count; i++) {
                var parameter = parameters.ElementAt (i);
                var value = values.ElementAt (i);

                result.Add (new KeyValuePair<ParameterInfo, object?> (parameter, value));
            }

            return MethodResult<List<KeyValuePair<ParameterInfo, object?>>>.Ok (result);
        }
    }
}