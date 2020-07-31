using System;
using FunctionalUtility.ResultDetails;
using Microsoft.AspNetCore.Http;

namespace ModelsValidation.ResultDetails {
    public class ArgumentError : ErrorDetail {
        public ArgumentError (string? title = null, string? message = null, Exception? exception = null):
            base (StatusCodes.Status400BadRequest, title ?? nameof (ArgumentError), message ?? "One or more validation failed.", exception : exception) { }
    }
}