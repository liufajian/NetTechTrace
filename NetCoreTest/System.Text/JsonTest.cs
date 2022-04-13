using System.Text.Json;
using System.Text.Json.Nodes;

namespace NetCoreTest.System.Text
{
    [TestClass]
    public class JsonTest
    {
        [TestMethod]
        public void TestNode()
        {
            string str = null;

            JsonNode node1 = str;
            JsonNode node2 = "123";

            var arr1 = new JsonArray { node1, node2 };

            var ele1 = node1.GetValue<JsonElement>();
            var ele2 = node2.GetValue<JsonElement>();

            Assert.AreEqual(ele1.ValueKind, JsonValueKind.Null);
            Assert.AreEqual(ele2.ValueKind, JsonValueKind.String);

            Assert.AreEqual(ele1.ToString(), String.Empty);
            Assert.AreEqual(ele2.ToString(), "123");
        }
    }
}
