using System;
using System.Collections.Generic;
using FunctionalUtility.ResultDetails;
using Microsoft.AspNetCore.Http;

namespace ModelsValidation.ResultDetails {
    public class ArgumentValidationError : ErrorDetail {
        public ArgumentValidationError (List<string> errors, string? title = null,
                string? message = null, Exception? exception = null,
                bool showDefaultMessageToUser = true):
            base (StatusCodes.Status400BadRequest,
                title ?? nameof (ArgumentValidationError),
                message ?? "One or more validation failed.",
                exception : exception,
                showDefaultMessageToUser : showDefaultMessageToUser) {
                Errors = errors;
            }

        public List<string> Errors { get; }

        public override object GetViewModel () =>
            ShowDefaultMessageToUser?(object) new {
                StatusCode,
                Title = nameof (ArgumentValidationError),
                Message = "One or more validation failed."
            }:
            new { StatusCode, Title, Message, Errors };
    }
}