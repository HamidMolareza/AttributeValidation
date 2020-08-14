using System.Text.RegularExpressions;
using FunctionalUtility.Extensions;
using FunctionalUtility.ResultDetails;
using FunctionalUtility.ResultUtility;
using Microsoft.AspNetCore.Http;
using PhoneNumbers;

namespace ModelsValidation.Utility {
    public static class PhoneUtility {
        public static string TryGetPhone (
                string phone) =>
            GetPhone (phone)
            .OnFail (() => MethodResult<string>.Ok (string.Empty))
            .GetValue ();

        public static MethodResult<string> GetPhone (
                string phone) =>
            phone.PhoneMustValid ()
            .OnSuccess (() => GetParsedPhoneNumber (phone))
            .TryOnSuccess (parsedPhone =>
                string.Concat ("+", parsedPhone.CountryCode, parsedPhone.NationalNumber));

        public static MethodResult<string> PhoneMustValid (this string @this) =>
            @this.IsNotNull<string> ()
            .TryTeeOnSuccess (phoneNumber =>
                phoneNumber.TryTee (() =>
                    phoneNumber.MustMatchRegex (new Regex (@"^[+0-9][0-9]+$"),
                        new ErrorDetail (StatusCodes.Status400BadRequest,
                            "{0} is not valid.")))
                .TeeOnSuccess (() => GetParsedPhoneNumber (phoneNumber))
                .TeeOnSuccess (() => phoneNumber.Must (
                    phoneNumber.Length >= 5 && phoneNumber.Length <= 30,
                    new ErrorDetail (StatusCodes.Status400BadRequest,
                        "Phone is not valid.", "Length error.")))
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