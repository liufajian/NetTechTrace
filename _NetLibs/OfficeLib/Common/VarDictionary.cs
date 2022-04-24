using OfficeLib.JsonNodes;
using System;
using System.Collections.Generic;

namespace OfficeLib
{
    /// <summary>
    /// 变量字典
    /// </summary>
    public class VarDictionary
    {
        readonly Dictionary<string, JsonValue> _dic;

        public VarDictionary()
        {
            _dic = new Dictionary<string, JsonValue>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 
        /// </summary>
        public int Count => _dic.Count;

        /// <summary>
        /// 添加替换变量
        /// </summary>
        public void AddVariable(string varKey, JsonValue value)
        {
            if (varKey is null)
            {
                throw new ArgumentNullException(nameof(varKey));
            }

            _dic[TrimKey(varKey)] = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public JsonValue GetVarValue(string varKey)
        {
            return varKey is null ? null : _dic.TryGetValue(varKey, out var node) ? node : null;
        }

        private static string TrimKey(string varKey)
        {
            return varKey.AsSpan().Trim().TrimStart('{').TrimEnd('}').Trim().ToString();
        }
    }
}
