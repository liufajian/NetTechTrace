using System;
using System.IO;
using System.Text;

namespace NetCoreTest.System.IO
{
    [TestClass]
    public class FileTest
    {
        [TestMethod]
        public void TestEncoding()
        {
            var s = new MemoryStream();
            var sw = new StreamWriter(s);
            Assert.IsTrue(sw.Encoding is UTF8Encoding);
            Assert.AreNotEqual(sw.Encoding ,Encoding.UTF8);

            byte[] preamble = sw.Encoding.GetPreamble();

            //utf8 without bom
            Assert.IsTrue(preamble.Length < 1);

            //utf8 with bom
            preamble = Encoding.UTF8.GetPreamble();
            Assert.AreEqual(preamble[0].ToString("X2"), "EF");
            Assert.AreEqual(preamble[1].ToString("X2"), "BB");
            Assert.AreEqual(preamble[2].ToString("X2"), "BF");
        }
    }
}
