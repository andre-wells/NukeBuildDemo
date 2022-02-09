using NUnit.Framework;

namespace NukeBuildDemo.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GetBinaryString_ValidInput_ReturnsResult()
        {
            var x = BinaryHelper.GetBinaryString(1);
            var y = BinaryHelper.GetBinaryString(24);
            Assert.Pass();
        }

        [TestCase(0,"00000000")]
        [TestCase(1, "00000001")]
        [TestCase(24, "00011000")]
        public void GetBinaryString_ValidInput_ReturnsResult(int num, string expected)
        {
            Assert.AreEqual(expected, BinaryHelper.GetBinaryString(num));
            
        }
    }
}