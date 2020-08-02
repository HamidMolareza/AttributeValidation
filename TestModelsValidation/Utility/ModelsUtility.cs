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
}