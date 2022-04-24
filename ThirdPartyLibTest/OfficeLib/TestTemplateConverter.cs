using OfficeLib;
using OfficeLib.JsonNodes;

namespace ThirdPartyLibTest.OfficeLib
{
    [TestClass]
    public class TestTemplateConverter
    {
        [TestMethod]
        public void Test1()
        {
            var jsonByteData = Properties.Resources.MyTemplateConverter.AsSpan();

            ReadOnlySpan<byte> utf8bom = Encoding.UTF8.GetPreamble();

            if (jsonByteData.StartsWith(utf8bom))
            {
                jsonByteData = jsonByteData.Slice(utf8bom.Length);
            }

            var json = Encoding.UTF8.GetString(jsonByteData);

            var obj = JsonHelper.Parse(json);

            var converter = new MyTemplateConverter();
            converter.AddVariable("root", obj);

            var test1 = converter.GetValue("root.test1");
            Assert.AreEqual(test1.ToString(), "test1-x");

            var a1 = converter.GetValue("root.test11.A1");
            Assert.AreEqual(a1.ToString(), "111");

            var a3 = converter.GetStringValue("root.test11.A3");
            Assert.AreEqual(a3, "[\"101\", \"102\", \"103\", 104]");
        }

        class MyTemplateConverter : TemplateConverter
        {
            public JsonValue LoopValue { get; set; }

            public string GetStringValue(string key)
            {
                return GetValue(key).ToString();
            }

            public JsonValue GetValue(string pathKey)
            {
                if (pathKey is null)
                {
                    return null;
                }

                if (pathKey == ".")
                {
                    return LoopValue;
                }

                var index = pathKey.IndexOf('.');

                if (index < 0)
                {
                    return LoopValue is JsonObject jobj1 && jobj1.TryGetValue(pathKey, out var jn1) ? jn1 : base.GetVarValue(pathKey);
                }

                if (index == 0)
                {
                    return JsonHelper.GetPropertyValue(LoopValue, pathKey.AsSpan().Slice(1).TrimStart());
                }

                var key1 = pathKey[..index];
                var jnode = LoopValue is JsonObject jobj2 && jobj2.TryGetValue(key1, out var jn2) ? jn2 : base.GetVarValue(key1);
                return JsonHelper.GetPropertyValue(jnode, pathKey.AsSpan()[(index + 1)..]);
            }
        }
    }
}
