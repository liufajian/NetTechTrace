using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OfficeLib
{
    static class JsonNodeHelper
    {
        /// <summary>
        /// 
        /// </summary>
        public static JsonNode GetValue(JsonNode rootNode, ReadOnlySpan<char> pathSpan)
        {
            if (!(rootNode is JsonObject jobj))
            {
                return null;
            }

            while (true)
            {
                var index = pathSpan.IndexOf('.');

                if (index < 0)
                {
                    return jobj.TryGetPropertyValue(pathSpan.ToString(), out var jnode) ? jnode : null;
                }
                else
                {
                    var substr = pathSpan.Slice(0, index).ToString();
                    if (jobj.TryGetPropertyValue(substr, out var jnone) && jnone is JsonObject subJobj)
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

        public static string GetString(JsonNode node)
        {
            if (node != null)
            {
                if (node is JsonValue)
                {
                    return node.ToString();
                }
                else
                {
                    return node.ToJsonString();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        public static JsonArray GetLoopArray(JsonNode node)
        {
            if (node is null)
            {
                return null;
            }

            if (node is JsonArray jarr)
            {
                return jarr;
            }

            if (node is JsonObject jobj)
            {
                return new JsonArray { jobj };
            }

            if (node is JsonValue)
            {
                var ele = node.GetValue<JsonElement>();

                if (ele.ValueKind == JsonValueKind.False)
                {
                    return null;
                }

                if (ele.ValueKind == JsonValueKind.String)
                {
                    var str = ele.GetString();

                    if (string.IsNullOrEmpty(str))
                    {
                        return null;
                    }

                    return new JsonArray() { JsonValue.Create(str) };
                }
            }

            return new JsonArray() { node };
        }
    }
}
