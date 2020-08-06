using System;
using FunctionalUtility.ResultDetails;
using Microsoft.AspNetCore.Http;

namespace ModelsValidation.ResultDetails {
    public class BadRequestError : ErrorDetail {
        public BadRequestError (string? title = null, string? message = null,
                Exception? exception = null,
                bool showDefaultMessageToUser = true, object? detail = null):
            base (StatusCodes.Status400BadRequest, title ?? "ArgumentError",
                message ?? "One or more validation failed.", exception,
                showDefaultMessageToUser, detail) { }
    }
}