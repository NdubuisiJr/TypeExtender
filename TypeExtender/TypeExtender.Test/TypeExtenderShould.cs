using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TypeExtender.Test.TestHelpers;

namespace TypeExtender.Test {
    public class TypeExtenderShould {
        TypeExtender _typeExtender;
      
        [Test]
        public void HaveAConstructorThatTakesTypeName() {
            var className = "ClassA";
            _typeExtender = new TypeExtender(className);

            Assert.AreEqual(className, _typeExtender.TypeName);
        }

        [Test]
        public void ReplaceSpaceInClassNameWithUnderScore() {
            var className = "Class A";
            _typeExtender = new TypeExtender(className);

            Assert.AreEqual("Class_A", _typeExtender.TypeName);
        }

        [Test]
        public void HaveAConstructorThatTakesTypeNameAndBaseType() {
            var className = "ClassA";
            var baseType = typeof(List<string>);

            _typeExtender = new TypeExtender(className, baseType);

            Assert.AreEqual(className, _typeExtender.TypeName);
            Assert.AreEqual(baseType, _typeExtender.BaseType);
        }

        [Test]
        public void ReturnATypeWithThePassedName() {
            var className = "ClassA";
            _typeExtender = new TypeExtender(className);

            var returnedType = _typeExtender.FetchType();
            var name = returnedType.Name;

            Assert.AreEqual(className, name);
        }

        [Test]
        public void ReturnATypeWithThePassedNameAndBaseClass() {
            var className = "ClassA";
            var baseType = typeof(List<string>);
            _typeExtender = new TypeExtender(className,baseType);

            var returnedType = _typeExtender.FetchType();
            var name = returnedType.Name;
            var basetypeReturned = returnedType.BaseType;

            Assert.AreEqual(className, name);
            Assert.AreEqual(baseType, basetypeReturned);
        }

        [Test]
        public void AddAttributesWithoutParamsToDerivedClass() {
            _typeExtender = new TypeExtender("ClassA");
            _typeExtender.AddAttribute(typeof(CustomBAttribute));

            var returnedType = _typeExtender.FetchType();
            var attributes = returnedType.GetCustomAttributes(typeof(CustomBAttribute), false);
            var attribute = attributes.Single().GetType();

            Assert.AreEqual(typeof(CustomBAttribute).Name, attribute.Name);
            Assert.AreEqual(typeof(CustomBAttribute).FullName, attribute.FullName);
        }
        
        [Test]
        public void AddAttributesWithParamsToDerivedClass() {
            _typeExtender = new TypeExtender("ClassA");
            _typeExtender.AddAttribute<CustomAAttribute>(new object[] { "Jon Snow" });

            var returnedType = _typeExtender.FetchType();
            var attributes = returnedType.GetCustomAttributes(typeof(CustomAAttribute), false);
            var attribute = attributes.Single().GetType();

            Assert.AreEqual(typeof(CustomAAttribute).Name, attribute.Name);
            Assert.AreEqual(typeof(CustomAAttribute).FullName, attribute.FullName);
        }
    }
}