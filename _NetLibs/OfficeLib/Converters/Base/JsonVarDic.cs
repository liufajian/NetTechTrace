using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OfficeLib.Converters
{
    /// <summary>
    /// 变量字典
    /// </summary>
    public class JsonVarDic
    {
        readonly Dictionary<string, JToken> _dic;

        public JsonVarDic()
        {
            _dic = new Dictionary<string, JToken>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 
        /// </summary>
        public int Count => _dic.Count;

        /// <summary>
        /// 添加变量
        /// </summary>
        public void SetVar(string varKey, JToken value)
        {
            if (varKey is null)
            {
                throw new ArgumentNullException(nameof(varKey));
            }

            _dic[TrimKey(varKey)] = value;
        }

        /// <summary>
        /// 获取变量值
        /// </summary>
        public JToken GetVar(string varKey)
        {
            return varKey is null ? null : _dic.TryGetValue(varKey, out var node) ? node : null;
        }

        private static string TrimKey(string varKey)
        {
            return varKey.AsSpan().Trim().TrimStart('{').TrimEnd('}').Trim().ToString();
        }
    }
}
