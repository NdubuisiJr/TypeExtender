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

            var returnedClass = _typeExtender.FetchType();
            var name = returnedClass.Name;

            Assert.AreEqual(className, name);
        }

        [Test]
        public void ReturnATypeWithThePassedNameAndBaseClass() {
            var className = "ClassA";
            var baseType = typeof(List<string>);
            _typeExtender = new TypeExtender(className, baseType);

            var returnedClass = _typeExtender.FetchType();
            var name = returnedClass.Name;
            var basetypeReturned = returnedClass.BaseType;

            Assert.AreEqual(className, name);
            Assert.AreEqual(baseType, basetypeReturned);
        }

        [Test]
        public void AddAttributesWithoutParamsToDerivedClass() {
            _typeExtender = new TypeExtender("ClassA");
            _typeExtender.AddAttribute(typeof(CustomBAttribute));

            var returnedClass = _typeExtender.FetchType();
            var attributes = returnedClass.GetCustomAttributes(typeof(CustomBAttribute), false);
            var attribute = attributes.Single().GetType();

            Assert.AreEqual(typeof(CustomBAttribute).Name, attribute.Name);
            Assert.AreEqual(typeof(CustomBAttribute).FullName, attribute.FullName);
        }

        [Test]
        public void AddAttributesWithParamsToDerivedClass() {
            _typeExtender = new TypeExtender("ClassA");
            _typeExtender.AddAttribute<CustomAAttribute>(new object[] { "Jon Snow" });

            var returnedClass = _typeExtender.FetchType();
            var attributes = returnedClass.GetCustomAttributes(typeof(CustomAAttribute), false);
            var attribute = attributes.Single().GetType();

            Assert.AreEqual(typeof(CustomAAttribute).Name, attribute.Name);
            Assert.AreEqual(typeof(CustomAAttribute).FullName, attribute.FullName);
        }

        [Test]
        public void AddPropertyToDerivedClass() {
            // Arrange
            _typeExtender = new TypeExtender("ClassA");
            _typeExtender.AddProperty("IsAdded", typeof(bool));
            _typeExtender.AddProperty("IsEnabled", typeof(bool), true);
            _typeExtender.AddProperty<double>("Length");
            _typeExtender.AddProperty<double>("Width", true);

            //Act
            var returnedClass = _typeExtender.FetchType();
            var isAddedProperty = returnedClass.GetProperty("IsAdded");
            var isEnabledProperty = returnedClass.GetProperty("IsEnabled");
            var lengthProperty = returnedClass.GetProperty("Length");
            var widthProperty = returnedClass.GetProperty("Width");

            //Assert
            Assert.AreEqual("IsAdded", isAddedProperty.Name);
            Assert.AreEqual(typeof(bool), isAddedProperty.PropertyType);

            Assert.AreEqual("IsEnabled", isEnabledProperty.Name);
            Assert.AreEqual(typeof(bool), isEnabledProperty.PropertyType);
            Assert.AreEqual(false, isEnabledProperty.CanWrite);

            Assert.AreEqual("Length", lengthProperty.Name);
            Assert.AreEqual(typeof(double), lengthProperty.PropertyType);

            Assert.AreEqual("Width", widthProperty.Name);
            Assert.AreEqual(typeof(double), widthProperty.PropertyType);
            Assert.AreEqual(false, widthProperty.CanWrite);
        }

        [Test]
        public void AddACollectionOfPropertiesWithSameType() {
            _typeExtender = new TypeExtender("ClassA");
            var properites1 = new string[] { "IsEnabled", "CanFollowUp", "IsAdded" };
            var properties2 = new string[] { "Length", "Width", "Height" };

            _typeExtender.AddProperty(properites1, typeof(bool));
            _typeExtender.AddProperty<double>(properties2);
            var returnedClass = _typeExtender.FetchType();

            var properties = returnedClass.GetProperties();
            var all = properites1.Union(properties2);
            foreach (var prop in all) {
                Assert.Contains(prop, properties.Select(x => x.Name).ToList());
            }
        }

        [Test]
        public void AddPropertyWithAddributes() {
            var attributeType = typeof(CustomAAttribute);
            var attributeParams = new object[] { "Jon Snow" };
            _typeExtender = new TypeExtender("ClassA");
            _typeExtender.AddProperty("IsAdded", typeof(bool), attributeType, attributeParams);

            var returnedClass = _typeExtender.FetchType();
            var property = returnedClass.GetProperty("IsAdded");
            var attributes = property.GetCustomAttributes(attributeType, false);
            var attribute = attributes[0] as CustomAAttribute;

            Assert.AreEqual(1, attributes.Length);
            Assert.NotNull(attribute);
            Assert.AreEqual("Jon Snow", attribute.Name);
        }
    }
}