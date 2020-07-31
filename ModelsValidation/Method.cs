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
    public static class Method {
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
                    Models.ModelMustValid (
                        parameterWithValue.Key.GetCustomAttributesData ().ToList (),
                        parameterWithValue.Key.Name,
                        parameterWithValue.Value, result.validationContext)
                )
            );

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
    }
}