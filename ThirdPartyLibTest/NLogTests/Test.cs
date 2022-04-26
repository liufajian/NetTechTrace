using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyLibTest.NLogTests
{
    [TestClass]
    public class Test
    {
        [TestMethod]
        public void Test1()
        {
            NLog.LogManager.Setup();
            //var config = NLog.LogManager.Configuration;
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("init main");
        }
    }
}
