using System.Collections.Generic;
using System.Reflection;
using FunctionalUtility.Extensions;
using FunctionalUtility.ResultUtility;
using ModelsValidation;
using Xunit;

namespace TestModelsValidation {
    public class TestMethod {
        [Fact]
        public void MethodParametersMustValid_NoParameter_Success () {
            var result = NoParameter ();
            Assert.True (result.IsSuccess);
        }

        [Fact]
        public void MethodParametersMustValid_NoAttribute_Success () {
            var result = NoAttribute (null, null, null, null, null);
            Assert.True (result.IsSuccess);
        }

        [Fact]
        public void MethodParametersMustValid_WrongImplementation_Error () {
            var result = WrongImplementation (null, null, null, null, null);
            Assert.False (result.IsSuccess);
        }

        private static MethodResult NoParameter () =>
            MethodBase.GetCurrentMethod ()
            .Map (currentMethod =>
                currentMethod!.MethodParametersMustValid (new object[] { }))
            .MapMethodResult ();

        private static MethodResult NoAttribute (string a, int? b,
                IReadOnlyCollection<char> c, List<SimpleModel> d, SimpleModel e) =>
            MethodBase.GetCurrentMethod ()
            .Map (currentMethod =>
                currentMethod!.MethodParametersMustValid (
                    new object[] { a, b, c, d, e }))
            .MapMethodResult ();

        private static MethodResult WrongImplementation (string a, int? b,
                IReadOnlyCollection<char> c, List<SimpleModel> d, SimpleModel e) =>
            MethodBase.GetCurrentMethod ()
            .Map (currentMethod =>
                currentMethod!.MethodParametersMustValid (
                    new object[] { a, b, c, d }))
            .MapMethodResult ();
    }

    public abstract class SimpleModel {
        public string A { get; set; }
        public int B { get; set; }
        public IReadOnlyCollection<char> C { get; set; }
        public List<int> D { get; set; }
    }
}