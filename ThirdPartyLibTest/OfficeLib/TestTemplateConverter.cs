using OfficeLib;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ThirdPartyLibTest.OfficeLib
{
    [TestClass]
    public class TestTemplateConverter
    {
        [TestMethod]
        public void Test1()
        {
            var jsonByteData = Properties.Resources.MyTemplateConverter.AsSpan();

            var json = Encoding.UTF8.GetString(jsonByteData);

            // Check BOM
            bool isBOM = false;
            ReadOnlySpan<byte> utf8bom = Encoding.UTF8.GetPreamble();
            if (jsonByteData.StartsWith(utf8bom))
            {
                isBOM = true;
                jsonByteData = jsonByteData.Slice(utf8bom.Length);
            }

            var obj = JsonNode.Parse(jsonByteData);

            var converter = new MyTemplateConverter();
            converter.AddVariable("root", obj);

            var test1 = converter.GetJsonNodeTest("root.test1");
            Assert.AreEqual(test1.ToString(), "test1-x");

            var a1 = converter.GetJsonNodeTest("root.test11.A1");
            Assert.AreEqual(a1.ToString(), "111");

            var a3 = converter.GetStringTest("root.test11.A3");
            Assert.AreEqual(a3, "[\"101\",\"102\",\"103\",104]");
        }

        class MyTemplateConverter : TemplateConverter
        {
            public string GetStringTest(string key)
            {
                return GetString(key);
            }

            public JsonNode GetJsonNodeTest(string key)
            {
                return GetJsonNode(key);
            }
        }
    }
}
