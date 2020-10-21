using NUnit.Framework;

namespace TypeExtender.Test {
    public class TypeExtenderShould {
        TypeExtender _typeExtender;
        [SetUp]
        public void Setup() {
            _typeExtender = new TypeExtender();
        }

        [Test]
        public void Test1() {
            var returnedType = _typeExtender.FetchType();
            Assert.AreEqual(null, returnedType);
        }
    }
}