using System;

namespace OfficeLib.Converts
{
    /// <summary>
    /// 文件格式转换异常
    /// </summary>
    public class FileFormatConvertException : Exception
    {
        public FileFormatConvertException(string message) : base(message)
        {

        }
    }
}
