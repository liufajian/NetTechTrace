using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace OfficeLib.Converters.Format
{
    public class LibreOfficeFailedException : Exception
    {
        public LibreOfficeFailedException(int exitCode)
            : base(string.Format("LibreOffice has failed with " + exitCode))
        { }
    }

    public class LibreOfficeConverter
    {
        /// <summary>
        /// convert(html->pdf,docx->pdf,docx->html,html->docx)
        /// </summary>
        /// <param name="libreOfficePath"></param>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        /// <exception cref="LibreOfficeFailedException"></exception>
        public static void Convert(string libreOfficePath, string inputFile, string outputFile)
        {
            var convertedFile = "";

            //Create tmp folder
            var tmpFolder = Path.Combine(Path.GetDirectoryName(outputFile), Guid.NewGuid().ToString("n"));

            Directory.CreateDirectory(tmpFolder);

            var commandArgs = new List<string>
            {
                "-env:UserInstallation=file:///d:/temp/p0/",
                "--convert-to"
            };

            if ((inputFile.EndsWith(".html") || inputFile.EndsWith(".htm")) && outputFile.EndsWith(".pdf"))
            {
                commandArgs.Add("pdf:writer_pdf_Export");
                convertedFile = Path.Combine(tmpFolder, Path.GetFileNameWithoutExtension(inputFile) + ".pdf");
            }
            else if (inputFile.EndsWith(".docx") && outputFile.EndsWith(".pdf"))
            {
                commandArgs.Add("pdf:writer_pdf_Export");
                convertedFile = Path.Combine(tmpFolder, Path.GetFileNameWithoutExtension(inputFile) + ".pdf");
            }
            else if (inputFile.EndsWith(".docx") && (outputFile.EndsWith(".html") || outputFile.EndsWith(".htm")))
            {
                commandArgs.Add("html:HTML:EmbedImages");
                convertedFile = Path.Combine(tmpFolder, Path.GetFileNameWithoutExtension(inputFile) + ".html");
            }
            else if ((inputFile.EndsWith(".html") || inputFile.EndsWith(".htm")) && (outputFile.EndsWith(".docx")))
            {
                commandArgs.Add("docx:\"Office Open XML Text\"");
                convertedFile = Path.Combine(tmpFolder, Path.GetFileNameWithoutExtension(inputFile) + ".docx");
            }

            commandArgs.AddRange(new[] { inputFile, "--norestore", "--writer", "--headless", "--outdir", tmpFolder });

            var procStartInfo = new ProcessStartInfo(libreOfficePath);
            foreach (var arg in commandArgs)
            {
                procStartInfo.ArgumentList.Add(arg);
            }
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;
            procStartInfo.WorkingDirectory = Environment.CurrentDirectory;

            var process = new Process() { StartInfo = procStartInfo };
            var pname = Process.GetProcessesByName("soffice");

            //Supposedly, only one instance of Libre Office can be run simultaneously
            while (pname.Length > 0)
            {
                Thread.Sleep(5000);
                pname = Process.GetProcessesByName("soffice");
            }

            process.Start();
            process.WaitForExit();

            // Check for failed exit code.
            if (process.ExitCode != 0)
            {
                throw new LibreOfficeFailedException(process.ExitCode);
            }
            else
            {
                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                }

                if (File.Exists(convertedFile))
                {
                    File.Move(convertedFile, outputFile);
                }

                Directory.Delete(tmpFolder, true);
            }
        }
    }
}
