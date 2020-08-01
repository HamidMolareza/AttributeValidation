using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using FunctionalUtility.Extensions;
using FunctionalUtility.ResultUtility;
using ModelsValidation;
using ModelsValidation.ResultDetails;
using Xunit;

namespace TestModelsValidation {
    public class TestMethod {
        [Fact]
        public void MethodParametersMustValid_NoParameter_Success () {
            var result = TestMethods.NoParameter ();
            Assert.True (result.IsSuccess);
        }

        [Fact]
        public void MethodParametersMustValid_NoAttribute_Success () {
            var result = TestMethods.NoAttribute (null, null, null, null, null);
            Assert.True (result.IsSuccess);
        }

        [Fact]
        public void MethodParametersMustValid_WrongImplementation_Error () {
            var result = TestMethods.WrongImplementation (null, null, null, null, null);
            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentError);
        }

        [Fact]
        public void MethodParametersMustValid_CorrectParametersWithAttributes_Success () {
            var result = TestMethods.ParametersWithAttributes ("a", 5, new List<int> ());
            Assert.True (result.IsSuccess);
        }

        [Fact]
        public void MethodParametersMustValid_WrongParametersWithAttributes_Error () {
            var result = TestMethods.ParametersWithAttributes ("long string.", 5, null);
            Assert.False (result.IsSuccess);
        }

        [Fact]
        public void MethodParametersMustValid_CorrectComplexModel_Success () {
            var modelWithAttributes = new ModelWithAttributes {
                A = "a",
                B = null,
                C = null,
                D = 5
            };
            var complexModel = new ComplexModel {
                SimpleModel = new SimpleModel (),
                ModelWithAttributes = new List<ModelWithAttributes> (),
                InnerClassProp = new ComplexModel.InnerClass {
                SimpleModel = new SimpleModel (),
                ModelWithAttributes = modelWithAttributes,
                ListOfModelWithAttributes = new List<ModelWithAttributes> { modelWithAttributes }
                }
            };
            var result = TestMethods.ModelParameters (null, modelWithAttributes, complexModel, complexModel, "");
            Assert.True (result.IsSuccess);
        }

        [Fact]
        public void MethodParametersMustValid_WrongComplexModel_Error () {
            var modelWithAttributes = new ModelWithAttributes {
                A = "a",
                B = null,
                C = null,
                D = 5
            };
            var complexModel = new ComplexModel {
                SimpleModel = new SimpleModel (),
                ModelWithAttributes = new List<ModelWithAttributes> (),
                InnerClassProp = new ComplexModel.InnerClass {
                SimpleModel = new SimpleModel (),
                ModelWithAttributes = new ModelWithAttributes (),
                ListOfModelWithAttributes = new List<ModelWithAttributes> { modelWithAttributes }
                }
            };
            var result = TestMethods.ModelParameters (null, modelWithAttributes, complexModel, complexModel, "");
            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentValidationError);
        }
    }

    public static class TestMethods {
        public static MethodResult NoParameter () =>
            MethodBase.GetCurrentMethod ()
            .Map (currentMethod =>
                currentMethod!.MethodParametersMustValid (new object[] { }))
            .MapMethodResult ();

        public static MethodResult NoAttribute (string a, int? b,
                IReadOnlyCollection<char> c, List<SimpleModel> d, SimpleModel e) =>
            MethodBase.GetCurrentMethod ()
            .Map (currentMethod =>
                currentMethod!.MethodParametersMustValid (
                    new object[] { a, b, c, d, e }))
            .MapMethodResult ();

        public static MethodResult WrongImplementation (string a, int? b,
                IReadOnlyCollection<char> c, List<SimpleModel> d, SimpleModel e) =>
            MethodBase.GetCurrentMethod ()
            .Map (currentMethod =>
                currentMethod!.MethodParametersMustValid (
                    new object[] { a, b, c, d }))
            .MapMethodResult ();

        public static MethodResult ParametersWithAttributes (
                [Required][StringLength (3)] string a, [Required][Range (0, 10)] int? b, [Required] List<int> c) =>
            MethodBase.GetCurrentMethod ()
            .Map (currentMethod =>
                currentMethod!.MethodParametersMustValid (
                    new object[] { a, b, c }))
            .MapMethodResult ();

        public static MethodResult ModelParameters (
                ModelWithAttributes a, [Required] ModelWithAttributes b,
                ComplexModel c, [Required] ComplexModel d, string e) =>
            MethodBase.GetCurrentMethod ()
            .Map (currentMethod =>
                currentMethod!.MethodParametersMustValid (
                    new object[] { a, b, c, d, e }))
            .MapMethodResult ();
    }

    public class SimpleModel {
        public string A { get; set; }
        public int B { get; set; }
        public IReadOnlyCollection<char> C { get; set; }
        public List<int> D { get; set; }
    }

    public class ModelWithAttributes {
        [Required][StringLength (5)] public string A { get; set; }

        [Range (0, 10)] public int? B { get; set; }
        public IReadOnlyCollection<char> C { get; set; }

        [Required][Range (0, 10)] public int? D { get; set; }
    }

    public class ComplexModel {
        [Required] public SimpleModel SimpleModel { get; set; }

        [Required] public List<ModelWithAttributes> ModelWithAttributes { get; set; }
        public InnerClass InnerClassProp { get; set; }

        public class InnerClass {
            [Required] public SimpleModel SimpleModel { get; set; }

            [Required] public ModelWithAttributes ModelWithAttributes { get; set; }

            [Required] public List<ModelWithAttributes> ListOfModelWithAttributes { get; set; }
        }
    }
}