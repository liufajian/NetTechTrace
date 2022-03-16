using Microsoft.EntityFrameworkCore;
using Dapper;

namespace EFCoreTest.MySQL
{
    [TestClass]
    public class BasicTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var db = new MyDbContext();
            var guid = Guid.NewGuid().ToString("N");
            var data = db.Config.SingleOrDefault(n => n.name == guid);
            if (data == null)
            {
                data = new Config
                {
                    name = guid,
                    value = "222",
                    value_date = new DateOnly(2022, 1, 1),
                    value_double = 111.55667788
                };
                db.Config.Add(data);
                db.SaveChanges();
                db.ChangeTracker.Clear();
                data = db.Config.Single(n => n.name == guid);
            }
            Assert.AreEqual(data.value, "222");
            Assert.AreEqual(data.value_date, new DateOnly(2022, 1, 1));
            Assert.AreEqual(data.value_double, 111.5567);
            db.Config.Remove(data);
            var changes = db.SaveChanges();
            Assert.AreEqual(changes, 1);
        }

        [TestMethod]
        public void TestTimeStamp()
        {
            var db = new MyDbContext();
            var guid = Guid.NewGuid().ToString("N");

            var data = new Config
            {
                name = guid,
                value = "222",
                value_date = new DateOnly(2022, 1, 1),
                value_double = 111.55667788
            };
            db.Config.Add(data);
            db.SaveChanges();

            var timestamp1 = db.Database.GetDbConnection().QueryFirst<string>($"select timestamp from config where name='{guid}'");

            Assert.IsTrue(!string.IsNullOrEmpty(timestamp1));

            data.value = "333";
            db.SaveChanges();

            var timestamp2 = db.Database.GetDbConnection().QueryFirst<string>($"select timestamp from config where name='{guid}'");

            Assert.IsTrue(!string.IsNullOrEmpty(timestamp2) && timestamp1 == timestamp2);

            db.Config.Remove(data);
            var changes = db.SaveChanges();
            Assert.AreEqual(changes, 1);
        }
    }
}