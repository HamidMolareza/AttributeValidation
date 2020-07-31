using System.Reflection;
using FunctionalUtility.Extensions;
using FunctionalUtility.ResultUtility;
using ModelsValidation;
using Xunit;

namespace TestModelsValidation {
    public class TestModelsValidation {
        [Fact]
        public void MethodParametersMustValid_NoParameter_Success () {
            var result = NoParameter ();
            Assert.True (result.IsSuccess);
        }

        private static MethodResult NoParameter () =>
            MethodBase.GetCurrentMethod ()
            .Map (currentMethod =>
                currentMethod!.MethodParametersMustValid (new object[] { }))
            .MapMethodResult ();
    }
}