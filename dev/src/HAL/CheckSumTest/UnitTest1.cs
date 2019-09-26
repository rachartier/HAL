using HAL.CheckSum;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace CheckSumTest
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void CheckSum_HashIsAString()
        {
            using (var fs = new FileStream("test/script.pl", FileMode.Open))
            {
                var hash = CheckSumGenerator.HashOf(fs);
                Assert.IsInstanceOfType(hash, typeof(string), "Hash is supposed to be a string");
            }
        }

        [TestMethod]
        public void CheckSum_StringHashReturnIsNotNull()
        {
            using(var fs = new FileStream("test/script.pl", FileMode.Open))
            {
                var hash = CheckSumGenerator.HashOf(fs);
                Assert.IsNotNull(hash, "Hash is not supposed to be null");
            }
        }

        [TestMethod]
        public void CheckSum_HashIsEqual()
        {
            using(var fs1 = new FileStream("test/script.pl", FileMode.Open))
            {
                using (var fs2 = new FileStream("test2/script.pl", FileMode.Open))
                {
                    var hashFs1 = CheckSumGenerator.HashOf(fs1);
                    var hashFs2 = CheckSumGenerator.HashOf(fs2);

                    var equals = hashFs1.Equals(hashFs2);
                    Assert.IsTrue(equals, "Hashes are supposed to be equals");
                }
            }
        }


        [TestMethod]
        public void CheckSum_HashIsNotEqual()
        {
            using (var fs1 = new FileStream("test/script.pl", FileMode.Open))
            {
                using (var fs2 = new FileStream("test2/script.py", FileMode.Open))
                {
                    var hashFs1 = CheckSumGenerator.HashOf(fs1);
                    var hashFs2 = CheckSumGenerator.HashOf(fs2);

                    var equals = hashFs1.Equals(hashFs2);
                    Assert.IsFalse(equals, "Hashes aren't supposed to be equals");
                }
            }
        }

        [TestMethod]
        public void CheckSume_HashStringIsEqual()
        {
            string data1 = "dataTest";
            string data2 = "dataTest";

            var hash1 = CheckSumGenerator.HashOf(data1);
            var hash2 = CheckSumGenerator.HashOf(data2);

            Assert.AreEqual(hash1, hash2, "Hashes are supposed to be equals");
        }

        [TestMethod]
        public void CheckSume_HashStringIsNotEqual()
        {
            string data1 = "dataTest";
            string data2 = "data";

            var hash1 = CheckSumGenerator.HashOf(data1);
            var hash2 = CheckSumGenerator.HashOf(data2);

            Assert.AreNotEqual(hash1, hash2, "Hashes aren't supposed to be equals");
        }
    }
}
