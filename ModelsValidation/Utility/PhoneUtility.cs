using System.Text.RegularExpressions;
using FunctionalUtility.Extensions;
using FunctionalUtility.ResultDetails.Errors;
using FunctionalUtility.ResultUtility;
using Microsoft.AspNetCore.Http;
using PhoneNumbers;

namespace ModelsValidation.Utility {
    public static class PhoneUtility {
        public static string? TryGetPhone (
                string phone) =>
            TryExtensions.Try (() => GetPhone (phone))
            .Map (methodResult => methodResult.IsSuccess? methodResult.Value : null);

        public static MethodResult<string> GetPhone (
                string phone) =>
            phone.PhoneMustValid ()
            .OnSuccess (() => GetParsedPhoneNumber (phone))
            .TryOnSuccess (parsedPhone =>
                string.Concat ("+", parsedPhone.CountryCode, parsedPhone.NationalNumber));

        public static MethodResult<string> PhoneMustValid (this string @this,
                string pattern = @"^[+0-9][0-9]+$", int minLength = 5, int maxLength = 30) =>
            @this.IsNotNull<string> ()
            .TryOnSuccess (phoneNumber =>
                TryExtensions.Try (() =>
                    phoneNumber.MustMatchRegex (new Regex (pattern),
                        new ErrorDetail (StatusCodes.Status400BadRequest,
                            message: "{0} pattern is not valid.")))
                .OnSuccess (() => GetParsedPhoneNumber (phoneNumber))
                .OnFail (new ErrorDetail (StatusCodes.Status400BadRequest, message: "{0} is not valid."))
                .OnSuccess (() => phoneNumber.Must (
                    phoneNumber.Length >= minLength && phoneNumber.Length <= maxLength,
                    new ErrorDetail (StatusCodes.Status400BadRequest,
                        "{0} is not valid.", "The {0} length is not valid.")))
            ).OnFail (new { Phone = @this });

        private static MethodResult<PhoneNumber> GetParsedPhoneNumber (
                string phone) => PhoneNumberUtil.GetInstance ()
            .Map (phoneUtil => AddPlusIfIsNotExist (phone)
                .TryMap (standardPhone => phoneUtil.Parse (standardPhone, null))
            );

        private static string AddPlusIfIsNotExist (string phone) =>
            phone.StartsWith ("+") ? phone : phone.Insert (0, "+");
    }
}