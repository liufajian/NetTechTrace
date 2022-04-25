using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace OfficeLib.Converters
{
    /// <summary>
    /// 
    /// </summary>
    public static class JsonVarHelper
    {
        /// <summary>
        /// 
        /// </summary>
        public static JToken GetPropertyValue(JToken root, ReadOnlySpan<char> pathSpan)
        {
            if (!(root is JObject jobj))
            {
                return null;
            }

            while (true)
            {
                var index = pathSpan.IndexOf('.');

                if (index < 0)
                {
                    return jobj.TryGetValue(pathSpan.ToString(), out var jnode) ? jnode : null;
                }
                else
                {
                    var substr = pathSpan[..index].ToString();
                    if (jobj.TryGetValue(substr, out var jnone) && jnone is JObject subJobj)
                    {
                        jobj = subJobj;
                    }
                    else
                    {
                        return null;
                    }
                }
                pathSpan = pathSpan.Slice(index + 1);
            }
        }

        /// <summary>
        /// 获取循环数组
        /// 如果jtoken为null或空数组或空对象或空字符串则返回null
        /// </summary>
        public static JArray GetLoopArray(JToken jtoken, string format)
        {
            if (jtoken is null)
            {
                return null;
            }

            if (jtoken is JArray jarr)
            {
                if (jarr.Count < 1)
                {
                    return null;
                }

                if (string.IsNullOrEmpty(format))
                {
                    return jarr;
                }

                jtoken = FormatValue(jarr, format);
            }
            else if (jtoken is JObject jobj)
            {
                if(jobj.Count < 1)
                {

                }
                return jobj.Count < 1 ? null : new JArray() { jobj };
            }
            else if (jtoken is JValue jval)
            {
                if (jval.Value == null || jval.Value is string str && string.IsNullOrEmpty(str)
                    || jval.Type == JTokenType.Boolean && !(bool)jval.Value)
                {
                    return null;
                }

                //带格式设置的时候转换为格式化字符串
                if (!string.IsNullOrEmpty(format))
                {
                    jtoken = jval.ToString(format, CultureInfo.InvariantCulture);
                }
            }

            return new JArray() { jtoken };
        }

        /// <summary>
        /// 如果是对象，则不应用格式设置
        /// 如果是数组，则格式设置应用于数组子项
        /// 如果是基础值类型则实现IFormattable接口的值会被格式化
        /// </summary>
        public static string FormatValue(this JToken jtoken, string format)
        {
            if (jtoken is null)
            {
                return null;
            }

            if (jtoken is JValue jval)
            {
                return jval.ToString(format, CultureInfo.InvariantCulture);
            }

            if (jtoken is JArray jarr && jarr.Count > 0 && !string.IsNullOrEmpty(format))
            {
                var jarr2 = new JArray();

                foreach (var item in jarr)
                {
                    jarr2.Add(item.FormatValue(format));
                }

                return jarr2.ToString(Formatting.None);
            }

            return jtoken.ToString(Formatting.None);
        }
    }
}
