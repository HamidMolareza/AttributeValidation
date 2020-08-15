using System.Collections.Generic;
using ModelsValidation.ResultDetails;
using TestModelsValidation.Utility;
using Xunit;

namespace TestModelsValidation {
    public class AttributeUtility {
        [Fact]
        public void AttributeValidation_AttributeWithWrongValue_ErrorWithParameterName () {
            var result = MethodUtility.SimpleAttribute (false);

            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentValidationError);

            var errors = ((ArgumentValidationError) result.Detail).Errors;
            Assert.Single (errors);
            Assert.Equal ("agreement is required.", errors[0]);
        }

        [Fact]
        public void AttributeValidation_AgreementAttributeOnInvalidType_Error () {
            var result = MethodUtility.AgreementAttributeOnInvalidType ("fake");

            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentValidationError);

            var errors = ((ArgumentValidationError) result.Detail).Errors;
            Assert.Single (errors);
            Assert.Equal ("(fake - Type of (System.String)) is not System.Boolean", errors[0]);
        }

        [Fact]
        public void AttributeValidation_AgreementIsTrue_Success () {
            var result = MethodUtility.AgreementAttribute (true);

            Assert.True (result.IsSuccess);
        }

        [Fact]
        public void AttributeValidation_AgreementIsFalse_Error () {
            var result = MethodUtility.AgreementAttribute (false);

            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentValidationError);

            var errors = ((ArgumentValidationError) result.Detail).Errors;
            Assert.Single (errors);
            Assert.Equal ("agreement is required.", errors[0]);
        }

        [Fact]
        public void AttributeValidation_EmailAttributeOnInvalidType_Error () {
            var result = MethodUtility.EmailAttributeOnInvalidType (true);

            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentValidationError);

            var errors = ((ArgumentValidationError) result.Detail).Errors;
            Assert.Single (errors);
            Assert.Equal ("email is not valid.", errors[0]);
        }

        [Fact]
        public void AttributeValidation_EmailIsCorrect_Success () {
            var result = MethodUtility.EmailAttribute ("email@domain.com");

            Assert.True (result.IsSuccess);
        }

        [Fact]
        public void AttributeValidation_EmailIsInvalid_Error () {
            var result = MethodUtility.EmailAttribute ("email.com");

            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentValidationError);

            var errors = ((ArgumentValidationError) result.Detail).Errors;
            Assert.Single (errors);
            Assert.Equal ("email is not valid.", errors[0]);
        }

        [Fact]
        public void AttributeValidation_EmailLengthIsInvalid_Error () {
            var result = MethodUtility.EmailAttribute ("a@a.a");

            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentValidationError);

            var errors = ((ArgumentValidationError) result.Detail).Errors;
            Assert.Single (errors);
            Assert.Equal ("The length must be between 10 and 90.", errors[0]);
        }

        [Fact]
        public void AttributeValidation_NonNegativeIntegerAttributeOnInvalidType_Error () {
            var result = MethodUtility.NonNegativeIntegerAttributeOnInvalidType (new List<string> ());

            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentValidationError);

            var errors = ((ArgumentValidationError) result.Detail).Errors;
            Assert.Single (errors);
            Assert.Equal ("(System.Collections.Generic.List`1[System.String] - Type of (System.Collections.Generic.List`1[System.String])) is not System.Int32", errors[0]);
        }

        [Fact]
        public void AttributeValidation_NonNegativeIntegerIsCorrect_Success () {
            var result = MethodUtility.NonNegativeIntegerAttribute (0);

            Assert.True (result.IsSuccess);
        }

        [Fact]
        public void AttributeValidation_NonNegativeIntegerIsInvalid_Error () {
            var result = MethodUtility.NonNegativeIntegerAttribute (-1);

            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentValidationError);

            var errors = ((ArgumentValidationError) result.Detail).Errors;
            Assert.Single (errors);
            Assert.Equal ("nonNegativeInteger must equal or more than 0.", errors[0]);
        }

        [Fact]
        public void AttributeValidation_PhoneNumberAttributeOnInvalidType_Error () {
            var result = MethodUtility.PhoneNumberAttributeOnInvalidType (0);

            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentValidationError);

            var errors = ((ArgumentValidationError) result.Detail).Errors;
            Assert.Single (errors);
            Assert.Equal ("phoneNumber is not valid.", errors[0]);
        }

        [Theory]
        [InlineData ("+989190000000")]
        [InlineData ("+980190000000")]
        [InlineData ("980190000000")]
        public void AttributeValidation_PhoneNumberIsCorrect_Success (string phone) {
            var result = MethodUtility.PhoneNumberAttribute (phone);

            Assert.True (result.IsSuccess);
        }

        [Theory]
        [InlineData ("aaa")]
        [InlineData ("+989190a000000")]
        [InlineData ("+980190@000000")]
        [InlineData ("9801900-00000")]
        [InlineData ("9801900 00000")]
        public void AttributeValidation_PhoneNumberIsInvalid_Error (string phone) {
            var result = MethodUtility.PhoneNumberAttribute (phone);

            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentValidationError);

            var errors = ((ArgumentValidationError) result.Detail).Errors;
            Assert.Single (errors);
            Assert.Equal ("phoneNumber is not valid.", errors[0]);
        }

        [Fact]
        public void AttributeValidation_PhoneNumberHasInvalidLength_Error () {
            var result = MethodUtility.PhoneNumberAttribute ("111");

            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentValidationError);

            var errors = ((ArgumentValidationError) result.Detail).Errors;
            Assert.Single (errors);
            Assert.Equal ("The phoneNumber length is not valid.", errors[0]);
        }

        [Fact]
        public void AttributeValidation_PositiveIntegerAttributeOnInvalidType_Error () {
            var result = MethodUtility.PositiveIntegerAttributeOnInvalidType (new List<string> ());

            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentValidationError);

            var errors = ((ArgumentValidationError) result.Detail).Errors;
            Assert.Single (errors);
            Assert.Equal ("(System.Collections.Generic.List`1[System.String] - Type of (System.Collections.Generic.List`1[System.String])) is not System.Int32", errors[0]);
        }

        [Fact]
        public void AttributeValidation_PositiveIntegerIsCorrect_Success () {
            var result = MethodUtility.PositiveIntegerAttribute (1);

            Assert.True (result.IsSuccess);
        }

        [Fact]
        public void AttributeValidation_PositiveIntegerIsInvalid_Error () {
            var result = MethodUtility.PositiveIntegerAttribute (0);

            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentValidationError);

            var errors = ((ArgumentValidationError) result.Detail).Errors;
            Assert.Single (errors);
            Assert.Equal ("positiveInteger must more than 0.", errors[0]);
        }

        //TODO: =====

        [Fact]
        public void AttributeValidation_UserNameAttributeOnInvalidType_Error () {
            var result = MethodUtility.UserNameAttributeOnInvalidType (new List<string> ());

            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentValidationError);

            var errors = ((ArgumentValidationError) result.Detail).Errors;
            Assert.Single (errors);
            Assert.Equal ("(System.Collections.Generic.List`1[System.String] - Type of (System.Collections.Generic.List`1[System.String])) is not System.String", errors[0]);
        }

        [Fact]
        public void AttributeValidation_UserNameIsCorrect_Success () {
            var result = MethodUtility.UserNameAttribute ("_user_99_name");

            Assert.True (result.IsSuccess);
        }

        [Fact]
        public void AttributeValidation_UserNameIsInvalid_Error () {
            var result = MethodUtility.UserNameAttribute ("99_username"); //Start with number.

            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentValidationError);

            var errors = ((ArgumentValidationError) result.Detail).Errors;
            Assert.Single (errors);
            Assert.Equal ("username Format is not valid.", errors[0]);
        }

        [Fact]
        public void AttributeValidation_UserNameLengthIsInvalid_Error () {
            var result = MethodUtility.UserNameAttribute ("user");

            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentValidationError);

            var errors = ((ArgumentValidationError) result.Detail).Errors;
            Assert.Single (errors);
            Assert.Equal ("The length must be between 5 and 35.", errors[0]);
        }
    }
}