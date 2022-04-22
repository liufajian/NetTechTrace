using OfficeLib.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace OfficeLib
{
    public abstract class TemplateConverter
    {
        readonly Dictionary<string, JsonValue> _dicVar;

        public TemplateConverter()
        {
            _dicVar = new Dictionary<string, JsonValue>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 添加替换变量
        /// </summary>
        public void AddVariable(string varKey, JsonValue value)
        {
            if (varKey is null)
            {
                throw new ArgumentNullException(nameof(varKey));
            }

            _dicVar[TrimKey(varKey)] = value;
        }

        public abstract void Convert(string templatePath, string outputFilePath);

        public abstract void Convert(Stream templateStream, string outputFilePath);

        /// <summary>
        /// 
        /// </summary>
        protected JsonValue GetVarValue(string varKey)
        {
            return varKey is null ? null : _dicVar.TryGetValue(varKey, out var node) ? node : null;
        }

        private static string TrimKey(string varKey)
        {
            return varKey.AsSpan().Trim().TrimStart('{').TrimEnd('}').Trim().ToString();
        }
    }
}
