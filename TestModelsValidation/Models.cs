using System.Collections.Generic;
using ModelsValidation;
using ModelsValidation.ResultDetails;
using TestModelsValidation.Utility;
using Xunit;

namespace TestModelsValidation {
    public class Models {
        [Fact]
        public void ModelsMustValid_EmptyArray_Success () {
            var input = new object[] { };
            var result = input.ModelsMustValid (false);
            Assert.True (result.IsSuccess);
        }

        [Fact]
        public void ModelsMustValid_CorrectComplexModel_Success () {
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
            var result = new object[] { modelWithAttributes, complexModel }.ModelsMustValid (false);
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
            var result = new object[] { modelWithAttributes, complexModel }.ModelsMustValid (false);
            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentValidationError);
        }
    }
}