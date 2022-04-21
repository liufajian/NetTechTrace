namespace NetCoreTest.Resources
{
    [TestClass]
    public class ResourceTest
    {
        [TestMethod("测试内嵌资源")]
        public void TestManifestResource()
        {
            var names = this.GetType().Assembly.GetManifestResourceNames();

            var set = new HashSet<string> {
                "NetCoreTest.Resources.55.json",
                "NetCoreTest.Resources.66.json",
                "NetCoreTest.Resources.ver_3._13.55.json"
            };

            foreach (var n in names)
            {
                set.Remove(n);
            }

            Assert.AreEqual(set.Count, 0);
        }
    }
}
