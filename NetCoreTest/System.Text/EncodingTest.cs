using System.Text;

namespace NetCoreTest.System.Text
{
    [TestClass]
    public class EncodingTest
    {
        [TestMethod]
        public void TestUtf8Bom()
        {
            //utf8 with bom
            var preamble = Encoding.UTF8.GetPreamble();
            Assert.AreEqual(preamble[0].ToString("X2"), "EF");
            Assert.AreEqual(preamble[1].ToString("X2"), "BB");
            Assert.AreEqual(preamble[2].ToString("X2"), "BF");
        }
    }
}
