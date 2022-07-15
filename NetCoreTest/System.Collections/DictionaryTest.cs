using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCoreTest.System.Collections
{
    [TestClass]
    public class DictionaryTest
    {
        [TestMethod]
        public void Test1()
        {
            var dic = new Dictionary<string, string>() {
                {"abc","111" },
                {"ABC","111" },
            };

            var dic2 = new Dictionary<string, string>(dic,StringComparer.OrdinalIgnoreCase);

            Assert.IsTrue(true);
        }
    }
}
