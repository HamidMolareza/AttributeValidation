using FunctionalUtility.ResultUtility;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModelsValidation.ResultDetails;

namespace ModelsValidation.Extensions {
    public static class ModelStateExtensions {
        public static void AddMethodResultError (this ModelStateDictionary modelState, ResultDetail errorDetail) {
            if (errorDetail is ArgumentValidationError argumentError) {
                foreach (var (key, value) in argumentError.ModelErrors)
                    modelState.AddModelError (key, value);
            } else {
                modelState.AddModelError (string.Empty, errorDetail.GetViewMessage ());
            }
        }
    }
}