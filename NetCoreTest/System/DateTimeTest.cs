using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCoreTest.System
{
    [TestClass]
    public class DateTimeTest
    {
        [TestMethod]
        public void TestDateTimeOffset()
        {
            Assert.AreEqual(DateTimeOffset.Now.ToUnixTimeSeconds(), DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            var now1 = DateTimeOffset.Now;
            var uxs1 = now1.ToUnixTimeSeconds();
            var ut1 = DateTimeOffset.FromUnixTimeSeconds(uxs1);
            Assert.AreEqual(now1.Ticks, ut1.ToLocalTime().Ticks,TimeSpan.FromSeconds(1).Ticks);

            var now2 = DateTimeOffset.UtcNow;
            var uxs2 = now2.ToUnixTimeMilliseconds();
            var ut2 = DateTimeOffset.FromUnixTimeMilliseconds(uxs2);
            Assert.AreEqual(now2.Ticks, ut2.Ticks, TimeSpan.FromMilliseconds(1).Ticks);
        }
    }
}
