using System;
using System.Collections.Generic;
using FunctionalUtility.ResultDetails.Errors;
using Microsoft.AspNetCore.Http;

namespace ModelsValidation.ResultDetails {
    public class ArgumentValidationError : ErrorDetail {
        public ArgumentValidationError (List<string> errors, string? title = null,
                string? message = null, Exception? exception = null,
                bool showDefaultMessageToUser = true, object? moreDetail = null):
            base (StatusCodes.Status400BadRequest,
                title ?? nameof (ArgumentValidationError),
                message ?? "One or more validation failed.",
                exception : exception,
                showDefaultMessageToUser : showDefaultMessageToUser,
                moreDetails : moreDetail) {
                Errors = errors;
            }

        public List<string> Errors { get; }

        public override object GetViewModel () =>
            ShowDefaultMessageToUser?(object) new {
                StatusCode,
                Title = GetViewTitle (),
                Message = GetViewMessage ()
            }:
            new { StatusCode, Title, Message, Errors };

        public override string GetViewTitle () => nameof (ArgumentValidationError);
        public override string GetViewMessage () => "One or more validation failed.";
    }
}