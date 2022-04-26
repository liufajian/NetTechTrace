using Newtonsoft.Json.Linq;
using OfficeLib.Converters;

namespace ThirdPartyLibTest.OfficeLib
{
    [TestClass]
    public class TestJsonVar
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

            var obj = JObject.Parse(json);

            var converter = new JsonVarDicEx();

            converter.SetVar("root", obj);

            var str1 = converter.GetStringValue("root.test1");
            Assert.AreEqual(str1, "test1-x");

            var strA1 = converter.GetStringValue("root.test11.A1");
            Assert.AreEqual(strA1, "111");

            var strA3 = converter.GetStringValue("root.test11.A3");
            Assert.AreEqual(strA3, "[\"101\",\"102\",\"103\",104]");

            var strA3_1 = converter.GetStringValue("root.test11.A3:0.00");
            Assert.AreEqual(strA3_1, "[\"101\",\"102\",\"103\",\"104.00\"]");
        }

        class JsonVarDicEx : JsonVarDic
        {
            public string GetStringValue(string pathKey, JToken loopValue = null)
            {
                var (jtoken, format) = GetPathValue(pathKey, loopValue);

                return jtoken.FormatValue(format);
            }

            /// <summary>
            /// 根据变量路径获取值
            /// </summary>
            public (JToken, string format) GetPathValue(string pathKey, JToken loopValue = null)
            {
                if (pathKey is null)
                {
                    return (null, null);
                }

                if (pathKey == ".")
                {
                    return (loopValue, null);
                }

                var index = pathKey.IndexOf('.');
                var findex = pathKey.LastIndexOf(':');

                string varKey = pathKey, format = null;

                if (findex > 0)
                {
                    format = pathKey.Substring(findex + 1).Trim();
                    varKey = pathKey.Substring(0, findex).TrimEnd();
                }

                JToken jret;

                if (index < 0)
                {
                    jret = loopValue is JObject jobj && jobj.TryGetValue(varKey, out var jn1) ? jn1 : GetVar(varKey);
                }
                else if (index == 0)
                {
                    jret = JsonVarHelper.GetPropertyValue(loopValue, varKey.AsSpan().Slice(1).TrimStart());
                }
                else
                {
                    var key1 = varKey[..index];
                    jret = loopValue is JObject jobj && jobj.TryGetValue(key1, out var jn2) ? jn2 : GetVar(key1);
                    jret = JsonVarHelper.GetPropertyValue(jret, varKey.AsSpan()[(index + 1)..]);
                }

                return (jret, format);
            }
        }
    }
}
