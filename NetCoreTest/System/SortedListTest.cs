namespace NetCoreTest.System
{
    [TestClass]
    public class SortedListTest
    {
        [TestMethod]
        public void Test1()
        {
            var slist = new SortedList<DateTime, InnerDataClass>() {
                {new DateTime(2022,2,1),new InnerDataClass{ Message ="2022-02-01"} },
                {new DateTime(2022,3,1),new InnerDataClass{ Message ="2022-03-01"} },
                {new DateTime(2022,5,1),new InnerDataClass{ Message ="2022-05-01"} },
                {new DateTime(2022,7,1),new InnerDataClass{ Message ="2022-07-01"} },
                {new DateTime(2022,10,1),new InnerDataClass{ Message ="2022-10-01"} },
            };
        }

        class InnerDataClass
        {
            public string Message { get; set; }
        }
    }
}
