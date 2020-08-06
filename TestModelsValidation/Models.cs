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
        public void MethodParametersMustValid_CheckDepth1_Success () {
            var model = new ModelDepth0 {
                ModelDepth1 = new ModelDepth1 {
                ModelDepth2 = new ModelDepth2 { A = "1" }
                }
            };
            var result = model.ModelMustValid (1, false);
            Assert.True (result.IsSuccess);
        }

        [Fact]
        public void MethodParametersMustValid_CheckDepth2_Fail () {
            var model = new ModelDepth0 {
                ModelDepth1 = new ModelDepth1 {
                ModelDepth2 = new ModelDepth2 { A = "1" }
                }
            };
            var result = model.ModelMustValid (2, false);
            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentValidationError);
        }

        [Fact]
        public void MethodParametersMustValid_CheckDepth1Array_Success () {
            var model = new ModelDepth0 {
                ModelDepth1 = new ModelDepth1 {
                ModelDepth2 = new ModelDepth2 { A = "1" }
                }
            };
            var result = new object[] { model }.ModelsMustValid (maximumDepth: 1, showDefaultMessageToUser: false);
            Assert.True (result.IsSuccess);
        }

        [Fact]
        public void MethodParametersMustValid_CheckDepth2Array_Fail () {
            var model = new ModelDepth0 {
                ModelDepth1 = new ModelDepth1 {
                ModelDepth2 = new ModelDepth2 { A = "1" }
                }
            };
            var result = new object[] { model }.ModelsMustValid (maximumDepth: 2, showDefaultMessageToUser: false);
            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentValidationError);
        }

        [Fact]
        public void MethodParametersMustValid_CheckDepthSingleModel_Success () {
            var model = new ModelWithAttributes {
                A = "1",
                B = 5,
                C = null,
                D = 5
            };
            var result = new object[] { model }.ModelsMustValid (maximumDepth: 0, showDefaultMessageToUser: false);
            Assert.True (result.IsSuccess);
        }

        [Fact]
        public void MethodParametersMustValid_CheckDepthSingleModel_Fail () {
            var model = new ModelWithAttributes {
                A = "TheLongText",
                B = 5,
                C = null,
                D = 5
            };
            var result = new object[] { model }.ModelsMustValid (maximumDepth: 0, showDefaultMessageToUser: false);
            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentValidationError);
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