using FunctionalUtility.ResultDetails.Errors;
using Microsoft.AspNetCore.Http;

namespace ModelsValidation.ResultDetails {
    internal class AttributeValidationError : ErrorDetail {
        public AttributeValidationError (string? message = null) : base (StatusCodes.Status400BadRequest,
            nameof (AttributeValidationError), message ?? "{0} is not valid.", null, false) { }
    }
}