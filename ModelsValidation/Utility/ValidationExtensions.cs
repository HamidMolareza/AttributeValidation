using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using FunctionalUtility.Extensions;
using FunctionalUtility.ResultDetails;
using FunctionalUtility.ResultUtility;
using Microsoft.AspNetCore.Http;

namespace ModelsValidation.Utility {
    public static class ValidationExtensions {
        public static MethodResult<string> UserNameMustValid (
                this string @this, string? regex = null, int? minimumLength = null, int? maximumLength = null) =>
            @this.IsNotNull<string> ()
            .OnSuccess (username => username.MustMatchRegex (new Regex (regex?? @"^[a-zA-Z_][a-zA-Z0-9_]*$"),
                new ErrorDetail (StatusCodes.Status400BadRequest,
                    message: "{0} Format is not valid.")))
            .OnSuccess (username =>
                username.Must (username.Length >= (minimumLength ?? 5) && username.Length <= (maximumLength ?? 35),
                    new ErrorDetail (StatusCodes.Status400BadRequest,
                        "{0} is not valid.", "Length error.")))
            .OnFail (new { UserName = @this });

        public static MethodResult<int> MustPositiveInteger (this int @this) =>
            @this.IsNotNull<int> ()
            .OnSuccess (input =>
                input.Must (input >= 1, new ErrorDetail (StatusCodes.Status400BadRequest,
                    "{0} is not valid.", "Length error.")))
            .OnFail (new { PositiveInteger = @this });

        public static MethodResult<int> MustNonNegativeInteger (this int @this) =>
            @this.IsNotNull<int> ()
            .OnSuccess (input =>
                input.Must (input >= 0, new ErrorDetail (StatusCodes.Status400BadRequest,
                    "{0} is not valid.", "Length error.")))
            .OnFail (new { NonNegativeInteger = @this });

        public static MethodResult<string> EmailMustValid (
                this string @this, int? minimumLength = null, int? maximumLength = null) =>
            @this.IsNotNull<string> ()
            .TryTeeOnSuccess (email => new EmailAddressAttribute ().Validate (email, "Email"))
            .OnFail (result => MethodResult<string>.Fail (new ErrorDetail (StatusCodes.Status400BadRequest,
                message: "{0} is not valid.")))
            .OnSuccess (email =>
                email.Must (
                    email.Length >= (minimumLength ?? 10) && email.Length <= (maximumLength ?? 90), //90 is big enough.
                    new ErrorDetail (StatusCodes.Status400BadRequest,
                        "{0} is not valid.", "Length error.")))
            .OnFail (new { Email = @this });
    }
}