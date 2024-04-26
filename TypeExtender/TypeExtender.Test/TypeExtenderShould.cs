using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TypeExtender.Test.TestHelpers;

namespace Extender.Test {
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
        public void AddPropertyWithAttribute() {
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

        [Test]
        public void AddPropertyWithMultipleAttributes(){
            var attributeTypeA = typeof(CustomAAttribute);
            var attributeParamsA = new object[] {"Jon Snow"};
            var attributeTypeB = typeof(CustomBAttribute);
            _typeExtender = new TypeExtender("ClassA");
            var attributesWithValues = new[]{
                new Tuple<Type, object[]>(attributeTypeA, attributeParamsA),
                new Tuple<Type, object[]>(attributeTypeB, new object[] { })
            };
            _typeExtender.AddProperty("IsAdded", typeof(bool), attributesWithValues);

            var returnedClass = _typeExtender.FetchType();
            var property = returnedClass.GetProperty("IsAdded");

            var attributesOfTypA = property.GetCustomAttributes(attributeTypeA, false);
            var attributeA = attributesOfTypA[0] as CustomAAttribute;

            Assert.AreEqual(1, attributesOfTypA.Length);
            Assert.NotNull(attributeA);
            Assert.AreEqual("Jon Snow", attributeA.Name);

            var attributesOfTypB = property.GetCustomAttributes(attributeTypeB, false);
            var attributeB = attributesOfTypB[0] as CustomBAttribute;

            Assert.AreEqual(1, attributesOfTypB.Length);
            Assert.NotNull(attributeB);
        }
		
		[Test]
        public void AddPropertyByGenericMethodWithMultipleAttributes(){
            var attributeTypeA = typeof(CustomAAttribute);
            var attributeParamsA = new object[] {"Jon Snow"};
            var attributeTypeB = typeof(CustomBAttribute);
            _typeExtender = new TypeExtender("ClassA");
            var attributesWithValues = new[]{
                new Tuple<Type, object[]>(attributeTypeA, attributeParamsA),
                new Tuple<Type, object[]>(attributeTypeB, new object[] { })
            };
            _typeExtender.AddProperty<bool>("IsAdded",  attributesWithValues);

            var returnedClass = _typeExtender.FetchType();
            var property = returnedClass.GetProperty("IsAdded");

            var attributesOfTypA = property.GetCustomAttributes(attributeTypeA, false);
            var attributeA = attributesOfTypA[0] as CustomAAttribute;

            Assert.AreEqual(1, attributesOfTypA.Length);
            Assert.NotNull(attributeA);
            Assert.AreEqual("Jon Snow", attributeA.Name);

            var attributesOfTypB = property.GetCustomAttributes(attributeTypeB, false);
            var attributeB = attributesOfTypB[0] as CustomBAttribute;

            Assert.AreEqual(1, attributesOfTypB.Length);
            Assert.NotNull(attributeB);
        }
        
        [Test]
        public void AddFieldToDerivedClass() {
            // Arrange
            _typeExtender = new TypeExtender("ClassA");
            _typeExtender.AddField("IsAdded", typeof(bool));
            _typeExtender.AddField<bool>("IsEnabled");
            
            //Act
            var returnedClass = _typeExtender.FetchType();
            var isAddedField = returnedClass.GetField("IsAdded");
            var isEnabledField = returnedClass.GetField("IsEnabled");

            //Assert
            Assert.AreEqual("IsAdded", isAddedField.Name);
            Assert.AreEqual(typeof(bool), isAddedField.FieldType);
            Assert.AreEqual("IsEnabled", isEnabledField.Name);
            Assert.AreEqual(typeof(bool), isEnabledField.FieldType);
        }
        
        [Test]
        public void AddFieldWithAttribute() {
            var attributeType1 = typeof(CustomAAttribute);
            var attributeParams1 = new object[] { "Jon Snow" };
            var attributeParams2 = new object[] { "Tyrion Lannister" };
            _typeExtender = new TypeExtender("ClassA");
            _typeExtender.AddField("IsAdded", typeof(bool), attributeType1, attributeParams1);
            _typeExtender.AddField<bool, CustomCAttribute>("IsEnabled", attributeParams2);

            var returnedClass = _typeExtender.FetchType();
            var field1 = returnedClass.GetField("IsAdded");
            var attributes1 = field1.GetCustomAttributes(attributeType1, false);
            var attribute1 = attributes1[0] as CustomAAttribute;
            var field2 = returnedClass.GetField("IsEnabled");
            var attributes2 = field2.GetCustomAttributes(typeof(CustomCAttribute), false);
            var attribute2 = attributes2[0] as CustomCAttribute;

            Assert.AreEqual(1, attributes1.Length);
            Assert.NotNull(attribute1);
            Assert.AreEqual("Jon Snow", attribute1.Name); 
            Assert.AreEqual(1, attributes2.Length);
            Assert.NotNull(attribute2);
            Assert.AreEqual("Tyrion Lannister", attribute2.Name);
        }  
        
        [Test]
        public void AddFieldWithAttributes() {
            _typeExtender = new TypeExtender("ClassA");
            var attributeTypesAndParameters = new Dictionary<Type, List<object>> {
                {typeof(CustomAAttribute), new List<object> { "Jon Snow" }},
                {typeof(CustomCAttribute), new List<object> { "Tyrion Lannister" }},
            };
            _typeExtender.AddField("IsAdded", typeof(bool), attributeTypesAndParameters);

            var returnedClass = _typeExtender.FetchType();
            var field = returnedClass.GetField("IsAdded");
            var customAAttributes = field.GetCustomAttributes(typeof(CustomAAttribute), false);
            var customCAttributes = field.GetCustomAttributes(typeof(CustomCAttribute), false);
            var attributes = customAAttributes.Concat(customCAttributes).ToArray();

            Assert.That(attributes, Has.Length.EqualTo(2));
            var attributeA = attributes.OfType<CustomAAttribute>().FirstOrDefault() ;
            var attributeC = attributes.OfType<CustomCAttribute>().FirstOrDefault();
            Assert.NotNull(attributeA);
            Assert.That(attributeA, Has.Property("Name").EqualTo("Jon Snow"));
            Assert.NotNull(attributeC);
            Assert.That(attributeC, Has.Property("Name").EqualTo("Tyrion Lannister"));
        }

        [Test]
        public void ThrowWhenBaseTypeIsNotPublic()
        {
            Assert.Throws<ArgumentException>(() => new TypeExtender("ClassA", typeof(TestInternalClass)));
        }

        [Test]
        public void ThrowWhenBaseTypeIsSealed()
        {
            Assert.Throws<ArgumentException>(() => new TypeExtender("ClassA", typeof(TestSealedClass)));
        }

        [Test]
        public void SucceedWhenInstantiatingNestedPublicClass()
        {
            _typeExtender = new TypeExtender("ClassA", typeof(TestNestedPublicClass));
            var returnedClass = _typeExtender.FetchType();

            var instance = Activator.CreateInstance(returnedClass);

            Assert.IsNotNull(instance);
        }

        public class TestNestedPublicClass { }
    }

    internal class TestInternalClass { }

    public sealed class TestSealedClass { }
}