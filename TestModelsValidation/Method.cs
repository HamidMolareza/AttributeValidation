using System.Collections.Generic;
using ModelsValidation.ResultDetails;
using TestModelsValidation.Utility;
using Xunit;

namespace TestModelsValidation {
    public class Method {
        [Fact]
        public void MethodParametersMustValid_NoParameter_Success () {
            var result = MethodUtility.NoParameter ();
            Assert.True (result.IsSuccess);
        }

        [Fact]
        public void MethodParametersMustValid_NoAttribute_Success () {
            var result = MethodUtility.NoAttribute (null, null, null, null, null);
            Assert.True (result.IsSuccess);
        }

        [Fact]
        public void MethodParametersMustValid_WrongImplementation_Error () {
            var result = MethodUtility.WrongImplementation (null, null, null, null, null);
            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentError);
        }

        [Fact]
        public void MethodParametersMustValid_CorrectParametersWithAttributes_Success () {
            var result = MethodUtility.ParametersWithAttributes ("a", 5, new List<int> ());
            Assert.True (result.IsSuccess);
        }

        [Fact]
        public void MethodParametersMustValid_EnumerableType_Success () {
            var result = MethodUtility.EnumerableType (new List<int> { 1, 2 }, new List<int> { 1, 2 });
            Assert.True (result.IsSuccess);
        }

        [Fact]
        public void MethodParametersMustValid_WrongParametersWithAttributes_Error () {
            var result = MethodUtility.ParametersWithAttributes ("long string.", 5, null);
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
            var result = MethodUtility.ModelParameters (null, modelWithAttributes, complexModel, complexModel, "");
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
            var result = MethodUtility.ModelParameters (null, modelWithAttributes, complexModel, complexModel, "");
            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentValidationError);
        }
    }
}