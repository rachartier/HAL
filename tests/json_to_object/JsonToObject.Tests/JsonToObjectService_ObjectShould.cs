using NUnit.Framework;
using System;
using Newtonsoft.Json.Linq;

namespace Tests
{
    [TestFixture]
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Object_InputIsFloat_ReturnFalse()
        {
            Assert.IsFalse(isObjectGoodType("3.14", JTokenType.Integer));
        }

        [Test]
        public void Object_InputIsFloat_ReturnTrue()
        {
            Assert.IsTrue(isObjectGoodType("3.14", JTokenType.Float));
        }

        [Test]
        public void Object_InputIsInt_ReturnTrue()
        {
            Assert.IsTrue(isObjectGoodType("3", JTokenType.Integer));
        }

        [Test]
        public void Object_InputIsString_ReturnTrue()
        {
            Assert.IsTrue(isObjectGoodType("'string aaaa bbbb'", JTokenType.String));
        }

        [Test]
        public void Object_InputIsDateUTC_ReturnTrue()
        {
            Assert.IsTrue(isObjectGoodType("'2019-09-05T11:35:57'", JTokenType.Date));
        }

        [Test]
        public void Object_InputIsObject_ReturnTrue()
        {
            Assert.IsTrue(isObjectGoodType("{a:1, b:2, c:3}", JTokenType.Object));
        }

        [Test]
        public void Object_InputIsNestedObject_ReturnTrue()
        {
            Assert.IsTrue(isObjectGoodType("{a:{aa: 1, bb:2}, b:2, c:3}", JTokenType.Object));
        }

        [Test]
        public void Object_InputIsArray_ReturnTrue()
        {
            Assert.IsTrue(isObjectGoodType("[1,2,3,'a']", JTokenType.Array));
        }

        private bool isObjectGoodType(string val, JTokenType type)
        {
            Json j = new Json();
            JObject result = j.ToObject($"{{ attribute: {val} }}");

            return result["attribute"].Type == type;
        }
    }
}
