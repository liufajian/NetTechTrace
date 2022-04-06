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
        }

        [TestMethod]
        public void TestEnumerator()
        {
            var list = new List<int> { 11, 22, 33 };
            var enm = list.GetEnumerator();
            
            Assert.AreEqual(enm.Current, 0);
            
            Assert.IsTrue(enm.MoveNext());
            Assert.AreEqual(enm.Current, 11);
            
            Assert.IsTrue(enm.MoveNext());
            Assert.AreEqual(enm.Current, 22);
            
            Assert.IsTrue(enm.MoveNext());
            Assert.AreEqual(enm.Current, 33);

            Assert.IsFalse(enm.MoveNext());
            Assert.IsFalse(enm.MoveNext());

            //----------------------------------

            var list2 = new List<string> { "a1", "a2", "a3" };
            var enm2 = list2.GetEnumerator();

            Assert.AreEqual(enm2.Current, default(string));

            Assert.IsTrue(enm2.MoveNext());
            Assert.AreEqual(enm2.Current, "a1");

            Assert.IsTrue(enm2.MoveNext());
            Assert.AreEqual(enm2.Current, "a2");

            Assert.IsTrue(enm2.MoveNext());
            Assert.AreEqual(enm2.Current, "a3");

            Assert.IsFalse(enm2.MoveNext());
            Assert.IsFalse(enm2.MoveNext());
        }
    }
}
