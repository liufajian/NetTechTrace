namespace NetCoreTest.System
{
    [TestClass]
    public class ListTest
    {
        [TestMethod]
        public void TestIncrement()
        {
            var aa = new int[10];
            aa[0]++;
            Assert.AreEqual(aa[0], 1);

            aa[1] = 11;
            aa[2] = 20;
            aa[5] = 5;
            Array.Clear(aa, 0, 3);
            Assert.AreEqual(aa[0], 0);
            Assert.AreEqual(aa[1], 0);
            Assert.AreEqual(aa[2], 0);
            Assert.AreEqual(aa[5], 5);

            var list = new List<int> { 0 };
            list[0]++;
            Assert.AreEqual(list[0], 1);
        }
    }
}
