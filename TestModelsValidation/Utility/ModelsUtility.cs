using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TestModelsValidation.Utility {
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

    public class ModelDepth0 {
        [Required]
        public ModelDepth1 ModelDepth1 { get; set; }
    }

    public class ModelDepth1 {
        [Required]
        public ModelDepth2 ModelDepth2 { get; set; }
    }

    public class ModelDepth2 {
        [Required][StringLength (10, MinimumLength = 5)]
        public string A { get; set; }
    }
}