using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using FunctionalUtility.Extensions;
using FunctionalUtility.ResultDetails.Errors;
using FunctionalUtility.ResultUtility;
using ModelsValidation.ResultDetails;

namespace ModelsValidation {
    public static class Method {
        public static MethodResult MethodParametersMustValid (
                IReadOnlyCollection<object?> ? values, [CallerMemberName] string? callerName = null,
                bool showDefaultMessageToUser = true,
                int maximumDepth = SettingClass.MaximumDepth) =>
            GetParameters (callerName!)
            .OnSuccess (parameters =>
                InputsMustValid (parameters, values)
                .OnSuccessOperateWhen (parameters.IsNotNullOrEmpty (),
                    () => MethodParametersMustValid (parameters, values,
                        showDefaultMessageToUser : showDefaultMessageToUser,
                        maximumDepth : maximumDepth))
            );

        private static MethodResult<ParameterInfo[]> GetParameters (string callerName) {
            if (string.IsNullOrEmpty (callerName)) {
                return MethodResult<ParameterInfo[]>.Fail (
                    new InternalError (title: "DetectCallerMethod", message: "Can not detect caller name."));
            }

            var frames = new StackTrace ()
                .GetFrames ();
            if (frames is null || frames.Length < 1) {
                return MethodResult<ParameterInfo[]>.Fail (
                    new InternalError (title: "GetMethodParametersError",
                        message: "The StackTrace frames is null or empty.",
                        moreDetails : new { callerName, StackTrace = new StackTrace () }));
            }

            var frame = frames
                .FirstOrDefault (stackFrame => stackFrame?.GetMethod ()?.Name == callerName);
            if (frame is null) {
                return MethodResult<ParameterInfo[]>.Fail (
                    new InternalError (title: "DetectMethod", message: "Can not detect caller method.",
                        moreDetails : new { callerName, StackTrace = new StackTrace () }));
            }

            var method = frame.GetMethod ();
            if (method is null) {
                return MethodResult<ParameterInfo[]>.Fail (
                    new InternalError (title: "DetectMethod", message: "Can not detect caller method.",
                        moreDetails : new { callerName, StackTrace = new StackTrace () }));
            }

            return MethodResult<ParameterInfo[]>.Ok (method.GetParameters ());
        }

        private static MethodResult InputsMustValid (
                ParameterInfo[] parameters,
                IReadOnlyCollection<object?> ? values) =>
            FailExtensions.FailWhen (() =>
                parameters.IsNullOrEmpty () && !values.IsNullOrEmpty (),
                new BadRequestError (
                    message: "The method parameters are inconsistent with the given input.",
                    moreDetails : new { parameters, values }))
            .OnSuccessFailWhen (() =>
                !parameters.IsNullOrEmpty () &&
                (values.IsNullOrEmpty () || parameters.Length != values!.Count),
                new BadRequestError (message:
                    "The method parameters are inconsistent with the given input.",
                    moreDetails : new { parameters, values }));

        private static MethodResult MethodParametersMustValid (
            IReadOnlyCollection<ParameterInfo> parameters, IReadOnlyCollection<object?> ? values,
            bool showDefaultMessageToUser, int maximumDepth) {
            var methodResult = MapParametersAndValues (parameters, values);
            if (!methodResult.IsSuccess)
                return MethodResult.Fail (methodResult.Detail);

            var validationContext = new ValidationContext (values);
            var modelErrors = new List<KeyValuePair<string, string>> ();
            foreach (var (parameter, value) in methodResult.Value) {
                var validationResult = Models.ModelMustValid (
                    parameter.GetCustomAttributesData ().ToList (),
                    parameter.Name,
                    value, validationContext,
                    showDefaultMessageToUser,
                    SettingClass.BeginDepth, maximumDepth);
                if (validationResult.IsSuccess) continue;

                if (validationResult.Detail.GetType () != typeof (ArgumentValidationError))
                    return validationResult;
                var errors = (validationResult.Detail as ArgumentValidationError) !.ModelErrors;
                modelErrors.AddRange (errors);
            }

            return modelErrors.Any () ? MethodResult.Fail (new ArgumentValidationError (modelErrors)) : MethodResult.Ok ();
        }

        private static MethodResult<List<KeyValuePair<ParameterInfo, object?>>> MapParametersAndValues (
            IReadOnlyCollection<ParameterInfo> parameters, IReadOnlyCollection<object?> ? values) {
            if (parameters.Count > 0 && (values is null || values.Count != parameters.Count))
                return MethodResult<List<KeyValuePair<ParameterInfo, object?>>>.Fail (
                    new BadRequestError (
                        message: "The method parameters are inconsistent with the given input.",
                        moreDetails : new { parameters, values }));

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