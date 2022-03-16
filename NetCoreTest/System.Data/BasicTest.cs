using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace NetCoreTest.System.Data
{
    [TestClass]
    public class BasicTest
    {
        [TestMethod]
        public void TestDataTable()
        {
            var dt = new DataTable();

            var col1 = dt.Columns.Add();
            Assert.IsTrue(col1.DataType == typeof(string));

            var col2 = dt.Columns.Add("col2");
            Assert.IsTrue(col2.DataType == typeof(string));
        }
    }
}
