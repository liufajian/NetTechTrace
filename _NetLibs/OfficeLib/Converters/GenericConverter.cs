using OfficeLib.Converters.Template;
using System;

namespace OfficeLib.Converters
{
    /// <summary>
    /// 通用转换器
    /// </summary>
    public class GenericConverter
    {
        /// <summary>
        /// doc模板转换
        /// </summary>
        public void DocTemplateConvert(string templatePath, string outputFilePath, JsonVarDic varDic)
        {
            var docOutFile = outputFilePath;
            var toPDF = outputFilePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);

            if (toPDF)
            {
                docOutFile = outputFilePath.Substring(0, outputFilePath.Length - 4) + ".docx";
            }

            new NpDocTemplateConverter().Convert(templatePath, docOutFile, varDic);

            if (toPDF)
            {

            }
        }
    }
}
