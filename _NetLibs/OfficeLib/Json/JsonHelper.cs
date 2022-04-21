using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JsonPair = System.Collections.Generic.KeyValuePair<string, OfficeLib.Json.JsonValue>;

namespace OfficeLib.Json
{
    public static class JsonHelper
    {
        #region----Parse Json String----

        public static JsonValue Parse(string jsonString)
        {
            if (jsonString == null)
            {
                return null;
            }

            var textReader = new StringReader(jsonString);

            return ToJsonValue(new JavaScriptReader(textReader).Read());
        }

        private static IEnumerable<JsonPair> ToJsonPairEnumerable(IEnumerable<KeyValuePair<string, object>> kvpc)
        {
            foreach (var kvp in kvpc)
            {
                yield return new KeyValuePair<string, JsonValue>(kvp.Key, ToJsonValue(kvp.Value));
            }
        }

        private static IEnumerable<JsonValue> ToJsonValueEnumerable(IEnumerable<object> arr)
        {
            foreach (var obj in arr)
            {
                yield return ToJsonValue(obj);
            }
        }

        private static JsonValue ToJsonValue(object ret)
        {
            if (ret == null)
            {
                return null;
            }

            var kvpc = ret as IEnumerable<KeyValuePair<string, object>>;
            if (kvpc != null)
            {
                return new JsonObject(ToJsonPairEnumerable(kvpc));
            }

            var arr = ret as IEnumerable<object>;
            if (arr != null)
            {
                return new JsonArray(ToJsonValueEnumerable(arr));
            }

            if (ret is bool) return new JsonBoolean((bool)ret);
            if (ret is decimal) return new JsonNumber((decimal)ret);
            if (ret is double) return new JsonNumber((double)ret);
            if (ret is int) return new JsonNumber((int)ret);
            if (ret is long) return new JsonNumber((long)ret);
            if (ret is string) return new JsonString((string)ret);

            System.Diagnostics.Debug.Assert(ret is ulong);

            return new JsonNumber((ulong)ret);
        }

        #endregion

        #region----Escape Json String----

        // Characters which have to be escaped:
        // - Required by JSON Spec: Control characters, '"' and '\\'
        // - Broken surrogates to make sure the JSON string is valid Unicode
        //   (and can be encoded as UTF8)
        // - JSON does not require U+2028 and U+2029 to be escaped, but
        //   JavaScript does require this:
        //   http://stackoverflow.com/questions/2965293/javascript-parse-error-on-u2028-unicode-character/9168133#9168133
        // - '/' also does not have to be escaped, but escaping it when
        //   preceeded by a '<' avoids problems with JSON in HTML <script> tags
        private static bool NeedEscape(string src, int i)
        {
            var c = src[i];
            return c < 32 || c == '"' || c == '\\'
                // Broken lead surrogate
                || (c >= '\uD800' && c <= '\uDBFF' &&
                    (i == src.Length - 1 || src[i + 1] < '\uDC00' || src[i + 1] > '\uDFFF'))
                // Broken tail surrogate
                || (c >= '\uDC00' && c <= '\uDFFF' &&
                    (i == 0 || src[i - 1] < '\uD800' || src[i - 1] > '\uDBFF'))
                // To produce valid JavaScript
                || c == '\u2028' || c == '\u2029'
                // Escape "</" for <script> tags
                || (c == '/' && i > 0 && src[i - 1] == '<');
        }

        public static string EscapeString(string src)
        {
            if (src != null)
            {
                for (var i = 0; i < src.Length; i++)
                {
                    if (NeedEscape(src, i))
                    {
                        var sb = new StringBuilder();
                        if (i > 0)
                        {
                            sb.Append(src, 0, i);
                        }
                        return DoEscapeString(sb, src, i);
                    }
                }
            }

            return src;
        }

        private static string DoEscapeString(StringBuilder sb, string src, int cur)
        {
            var start = cur;
            for (var i = cur; i < src.Length; i++)
                if (NeedEscape(src, i))
                {
                    sb.Append(src, start, i - start);
                    switch (src[i])
                    {
                        case '\b': sb.Append("\\b"); break;
                        case '\f': sb.Append("\\f"); break;
                        case '\n': sb.Append("\\n"); break;
                        case '\r': sb.Append("\\r"); break;
                        case '\t': sb.Append("\\t"); break;
                        case '\"': sb.Append("\\\""); break;
                        case '\\': sb.Append("\\\\"); break;
                        case '/': sb.Append("\\/"); break;
                        default:
                            sb.Append("\\u");
                            sb.Append(((int)src[i]).ToString("x04"));
                            break;
                    }
                    start = i + 1;
                }
            sb.Append(src, start, src.Length - start);
            return sb.ToString();
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public static JsonValue GetPropertyValue(JsonValue root, ReadOnlySpan<char> pathSpan)
        {
            if (!(root is JsonObject jobj))
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
                    if (jobj.TryGetValue(substr, out var jnone) && jnone is JsonObject subJobj)
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
        /// 
        /// </summary>
        public static JsonArray GetLoopArray(JsonValue jval)
        {
            if (jval is null)
            {
                return null;
            }

            switch (jval.JsonType)
            {
                case JsonType.String:
                    {
                        var str = jval.ToString();

                        if (string.IsNullOrEmpty(str))
                        {
                            return null;
                        }
                    }
                    break;
                case JsonType.Object:
                    if (jval.Count < 1)
                    {
                        return null;
                    }
                    break;
                case JsonType.Array:
                    if (jval.Count < 1)
                    {
                        return null;
                    }
                    return (JsonArray)jval;
                case JsonType.Boolean:
                    if (!((JsonBoolean)jval).Value)
                    {
                        return null;
                    }
                    break;
            }

            return new JsonArray() { jval };
        }
    }
}
