namespace NetCoreTest.System
{
    [TestClass]
    public class ListTest
    {
        [TestMethod]
        public void TestIncrement()
        {
            var aa = new int[10];
            var bb = aa[0];
            aa[0]++;
            bb++;
            Assert.AreEqual(expected: 1, actual: aa[0]);
            Assert.AreEqual(1, bb);

            aa[1] = 11;
            aa[2] = 20;
            aa[5] = 5;
            Array.Clear(aa, 0, 3);
            Assert.AreEqual(0, aa[0]);
            Assert.AreEqual(0, aa[1]);
            Assert.AreEqual(0, aa[2]);
            Assert.AreEqual(5, aa[5]);

            var list = new List<int> { 0 };
            list[0]++;
            Assert.AreEqual(1, list[0]);
        }

        [TestMethod]
        public void TestBinarySearch()
        {
            int index;

            var listEmpty = new List<int> ();
            index = listEmpty.BinarySearch(1);
            Assert.AreEqual(~0, index);

            var list = new List<int> { 3, 5, 8, 13, 21 };

            index = list.BinarySearch(1);
            Assert.AreEqual(~0, index);

            index = list.BinarySearch(3);
            Assert.AreEqual(0, index);

            index = list.BinarySearch(5);
            Assert.AreEqual(1, index);

            index = list.BinarySearch(4);
            Assert.AreEqual(~1, index);

            index = list.BinarySearch(6);
            Assert.AreEqual(~2, index);

            index = list.BinarySearch(26);
            Assert.AreEqual(~list.Count, index);
        }
    }
}
