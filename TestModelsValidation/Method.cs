using System.Collections.Generic;
using System.Security.Claims;
using FunctionalUtility.ResultDetails.Errors;
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
            Assert.True (result.Detail is BadRequestError);
        }

        [Fact]
        public void MethodParametersMustValid_CorrectParametersWithAttributes_Success () {
            var result = MethodUtility.ParametersWithAttributes ("a", 5, new List<int> ());
            Assert.True (result.IsSuccess);
        }

        [Fact]
        public void MethodParametersMustValid_WrongParametersWithAttributes_Error () {
            var result = MethodUtility.ParametersWithAttributes ("long string.", 5, null);
            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentValidationError);
        }

        [Fact]
        public void MethodParametersMustValid_EnumerableType_Success () {
            var result = MethodUtility.EnumerableType (new List<int> { 1, 2 }, new List<int> { 1, 2 });
            Assert.True (result.IsSuccess);
        }

        [Fact]
        public void MethodParametersMustValid_InputIsClaimType_Success () {
            var result = MethodUtility.InputIsClaimType (new [] {
                new Claim (ClaimTypes.NameIdentifier, "value")
            });
            Assert.True (result.IsSuccess);
        }

        [Fact]
        public void MethodParametersMustValid_CheckDepth1_Success () {
            var model = new ModelDepth0 {
                ModelDepth1 = new ModelDepth1 {
                ModelDepth2 = new ModelDepth2 { A = "1" }
                }
            };
            var result = MethodUtility.CheckDepth (model, 1);
            Assert.True (result.IsSuccess);
        }

        [Fact]
        public void MethodParametersMustValid_CheckDepth2_Fail () {
            var model = new ModelDepth0 {
                ModelDepth1 = new ModelDepth1 {
                ModelDepth2 = new ModelDepth2 { A = "1" }
                }
            };

            var result = MethodUtility.CheckDepth (model, 2);

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
            var result = MethodUtility.CheckDepthSingleModel (model, 0);
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
            var result = MethodUtility.CheckDepthSingleModel (model, 0);
            Assert.False (result.IsSuccess);
            Assert.True (result.Detail is ArgumentValidationError);
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