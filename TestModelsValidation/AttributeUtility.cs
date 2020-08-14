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
    }
}