using System;
using System.Collections.Generic;
using FunctionalUtility.ResultDetails.Errors;
using Microsoft.AspNetCore.Http;

namespace ModelsValidation.ResultDetails {
    public class ArgumentValidationError : ErrorDetail {
        public ArgumentValidationError (KeyValuePair<string, string> modelError, string? title = null,
                string? message = null, Exception? exception = null,
                bool showDefaultMessageToUser = true, object? moreDetail = null):
            base (StatusCodes.Status400BadRequest,
                title ?? nameof (ArgumentValidationError),
                message ?? "One or more validation failed.",
                exception : exception,
                showDefaultMessageToUser : showDefaultMessageToUser,
                moreDetails : moreDetail) {
                ModelErrors.Add (modelError);
            }

        public ArgumentValidationError (IEnumerable<KeyValuePair<string, string>> modelErrors, string? title = null,
                string? message = null, Exception? exception = null,
                bool showDefaultMessageToUser = true, object? moreDetail = null):
            base (StatusCodes.Status400BadRequest,
                title ?? nameof (ArgumentValidationError),
                message ?? "One or more validation failed.",
                exception : exception,
                showDefaultMessageToUser : showDefaultMessageToUser,
                moreDetails : moreDetail) {
                ModelErrors.AddRange (modelErrors);
            }

        public List<KeyValuePair<string, string>> ModelErrors { get; } = new List<KeyValuePair<string, string>> ();

        public override object GetViewModel () =>
            ShowDefaultMessageToUser?(object) new {
                StatusCode,
                Title = GetViewTitle (),
                Message = GetViewMessage ()
            }:
            new { StatusCode, Title, Message, ModelErrors };

        public override string GetViewTitle () => nameof (ArgumentValidationError);
        public override string GetViewMessage () => "One or more validation failed.";
    }
}