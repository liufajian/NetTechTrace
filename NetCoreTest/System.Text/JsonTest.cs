using System.Text.Json.Nodes;

namespace System.Text.Json
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

            Assert.AreEqual(ele1.ToString(), string.Empty);
            Assert.AreEqual(ele2.ToString(), "123");
        }

        [TestMethod("解析UTF8-BOM编码的JSON文本")]
        public void TestParseUTF8BomEncoding()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Resources\\System.Text\\Utf8BomEncoding.json");

            var jsonByteData = File.ReadAllBytes(path).AsSpan();

            try
            {
                _= JsonNode.Parse(jsonByteData);
            }
            catch(Exception ex)
            {
                Assert.IsTrue(ex is System.Text.Json.JsonException &&ex.Message.StartsWith("'0xEF' is an invalid start of a value"));
            }

            //var json = Encoding.UTF8.GetString(jsonByteData);

            ReadOnlySpan<byte> utf8bom = Encoding.UTF8.GetPreamble();

            if (jsonByteData.StartsWith(utf8bom))
            {
                jsonByteData = jsonByteData.Slice(utf8bom.Length);
            }

            var obj = JsonNode.Parse(jsonByteData);

            Assert.AreEqual(obj["test1"].ToString(), "test1-x");
        }
    }
}
