using NetLib.Helpers;

namespace NetCoreTest.System.Reflection
{
    [TestClass]
    public class TypeGeneratorTest
    {
        public string ABC { get; set; }

        [TestMethod]
        public void Test()
        {
            var cc = new TypeGeneratorGeneric<TypeGeneratorTest>(new Dictionary<string, Type> {
                {"AA",typeof(string) },
                {"AB",typeof(string) },
            });

            var instance = cc.CreateInstance(new Dictionary<string, object> {
                {"AA","1111" },
                {"AB","222" }
            });

            instance.ABC = "111c";

            Console.WriteLine(instance.ABC);
            var json = @"
{
""AA"":""AA1"",
""AB"":""AB1""
}
";
            dynamic t = global::System.Text.Json.JsonSerializer.Deserialize(json, cc.GeneratedType);

            Assert.AreEqual(t.AA, "AA1");
        }

        [TestMethod]
        public void Test2()
        {
            var t = TypeHelper.CreateTypeFromPocoInterface(typeof(ITest));
            dynamic instance = Activator.CreateInstance(t);
            instance.AA = "12222";
            Assert.AreEqual(instance.AA, "12222");
        }

        interface ITest
        {
            string AA { get; }
            string AB { get; }
        }
    }
}
