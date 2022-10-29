using KellermanSoftware.CompareNetObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyLibTest.CompareNETObjectsTests
{
    [TestClass]
    public class Test
    {
        [TestMethod]
        public void Test1()
        {
            CompareLogic compareLogic = new CompareLogic(new ComparisonConfig
            {
                IgnoreObjectTypes = true
            });
            var ta = new TestA { S1 = DateTime.Now };
            var tb = new TestB { S1 = ta.ToString() };
            ComparisonResult result = compareLogic.Compare(ta, tb);

            if (result.AreEqual)
            {
                Console.WriteLine("henhao，xiangtong");
            }
            else
            {
                Console.WriteLine(result.DifferencesString);
            }
        }

        class TestA
        {
            public DateTime S1 { get; set; }
        }

        class TestB
        {
            public string S1 { get; set; }
        }
    }
}
