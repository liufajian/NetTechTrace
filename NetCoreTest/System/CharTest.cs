using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCoreTest.System
{
    [TestClass]
    public class CharTest
    {
        [TestMethod]
        public void Test1()
        {
            var b = (byte)'1';
            Assert.AreEqual("10" + b, "1049");
            Assert.AreEqual("10" + b, "101");
        }
    }
}
