using System;
using System.Diagnostics;

namespace OfficeLib.Converts
{
    /// <summary>
    /// 
    /// </summary>
    public class DocxToPdfConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="libreOfficePath"></param>
        /// <param name="sourceDocFilePath"></param>
        /// <param name="targetDir"></param>
        public static void ConvertByLibreOffice(string libreOfficePath, string sourceDocFilePath, string targetDir)
        {
            // FIXME: file name escaping: I have not idea how to do it in .NET.
            var procStartInfo = new ProcessStartInfo(libreOfficePath, string.Format("--convert-to pdf --outdir {0} --nologo {1} ", targetDir, sourceDocFilePath))
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Environment.CurrentDirectory
            };

            var process = new Process() { StartInfo = procStartInfo, };
            process.Start();
            process.WaitForExit();

            // Check for failed exit code.
            if (process.ExitCode != 0)
            {
                throw new FileFormatConvertException("转换为PDF格式文件失败,ExitCode:" + process.ExitCode);
            }
        }
    }
}
