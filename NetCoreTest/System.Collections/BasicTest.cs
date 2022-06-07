using System.Collections.Specialized;

namespace NetCoreTest.System.Collections
{
    [TestClass]
    public class BasicTest
    {
        [TestMethod]
        public void TestNameValueCollection()
        {
            const string key1 = "key1";

            var nv = new NameValueCollection();

            nv.Add(key1, "111");
            nv.Add(key1, "222");
            Assert.AreEqual(nv[key1], "111,222");

            nv.Set(key1, "333");
            Assert.AreEqual(nv[key1], "333");
            nv.Add(key1, "444");
            Assert.AreEqual(nv[key1], "333,444");
            nv.Add(key1, "444");
            Assert.AreEqual(nv[key1], "333,444,444");

            nv.Set(key1, "333");
            Assert.AreEqual(nv[key1], "333");
            nv.Set(key1, "444");
            Assert.AreEqual(nv[key1], "444");

            nv[key1] = "333";
            Assert.AreEqual(nv[key1], "333");
            nv[key1] = "444";
            Assert.AreEqual(nv[key1], "444");
        }
    }
}
