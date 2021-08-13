using System;

// Custom attributes for testing
namespace TypeExtender.Test.TestHelpers {

    public class CustomAAttribute : Attribute {
        public CustomAAttribute(string name) {
            Name = name;
        }
        public string Name { get; }
    }

    public class CustomBAttribute : Attribute {
    }
    
    public class CustomCAttribute : CustomAAttribute {
        public CustomCAttribute(string name): base(name) {
        }
    }
}
