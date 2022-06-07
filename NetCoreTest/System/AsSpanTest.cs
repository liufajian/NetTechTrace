namespace NetCoreTest.System
{
    [TestClass]
    public class AsSpanTest
    {
        [TestMethod]
        public void Test1()
        {
            var str = "1,2,3,4,5,6,7";
            var span = str.AsSpan();
            while (true)
            {
                var index = span.IndexOf(",");
                if (index < 0)
                {
                    break;
                }
                Assert.AreEqual(index, 1);
                span = span.Slice(index + 1);
            }
        }
    }
}
