using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCoreTest.System
{
    [TestClass]
    public class StringValuesTest
    {
        [TestMethod]
        public void Test()
        {
            var sv1 = new StringValues(new[] { "111", "222" });
            Assert.AreEqual(sv1.ToString(), "111,222");
            Assert.IsTrue(sv1.Contains("111"));
            Assert.IsTrue(sv1.Contains("222"));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(sv1));

            string? str = null;
            var sv2 = new StringValues(str);
            Assert.AreEqual(sv2.ToString(), string.Empty);
            Assert.IsFalse(sv2.Contains("111"));
            Assert.IsTrue(string.IsNullOrWhiteSpace(sv2));


            var sv3 = new StringValues("ttt");
            Assert.IsTrue(sv3 == "ttt");
        }
    }
}
