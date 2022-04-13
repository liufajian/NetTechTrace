using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace OfficeLib
{
    public abstract class TemplateConverter
    {
        readonly Dictionary<string, JsonNode> _dicVar;

        public TemplateConverter()
        {
            _dicVar = new Dictionary<string, JsonNode>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 添加替换变量
        /// </summary>
        public void AddVariable(string key, JsonNode value)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            _dicVar[TrimKey(key)] = value;
        }

        protected virtual JsonNode GetJsonNode(string key)
        {
            return key is null ? null : _dicVar.TryGetValue(key, out var node) ? node : null;
        }

        private static string TrimKey(string key)
        {
            return key.AsSpan().Trim().TrimStart('{').TrimEnd('}').Trim().ToString();
        }
    }
}
