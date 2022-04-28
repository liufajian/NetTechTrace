using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace OfficeLib.Converters.Format
{
    public static class LibreOfficeConverter
    {
        const int MaxRun = 10;
        const int MaxFlag = 60;

        private static readonly string _installDir;
        private static readonly string _libreOfficePath;
        private static readonly RunCheck[] _runChecks;

        static LibreOfficeConverter()
        {
            _runChecks = new RunCheck[MaxRun + 1];

            _libreOfficePath = Environment.GetEnvironmentVariable("LibreOffice_ExePath");

            _installDir = Environment.GetEnvironmentVariable("LibreOffice_UserInstallation")?.Trim(new char[] { ' ', '/', '\\' });

            if (!string.IsNullOrEmpty(_installDir))
            {
                _installDir = _installDir.Replace('\\', '/');
            }
        }

        private static RunCheck GetRunCheck(out int enableIndex)
        {
            lock (_runChecks)
            {
                var timeoutIndex = 0;

                for (var s = 0; s < MaxFlag; s++)
                {
                    if (Process.GetProcessesByName("soffice").Length == 0)
                    {
                        Array.Clear(_runChecks, 0, MaxRun);
                    }

                    enableIndex = -1;
                    timeoutIndex = -1;

                    _runChecks[MaxRun] = null;

                    for (var i = 0; i < MaxRun; i++)
                    {
                        var run = _runChecks[i];

                        if (run == null)
                        {
                            enableIndex = i;

                            _runChecks[i] = new RunCheck();

                            break;
                        }

                        if (run.Flag > 0)
                        {
                            run.Flag++;

                            if (_runChecks[MaxRun] == null || run.Flag > _runChecks[MaxRun].Flag)
                            {
                                _runChecks[MaxRun] = run;

                                timeoutIndex = i;
                            }
                        }
                        else if (enableIndex < 0)
                        {
                            enableIndex = i;
                        }
                    }

                    if (enableIndex >= 0)
                    {
                        _runChecks[enableIndex].Flag = 1;

                        return _runChecks[enableIndex];
                    }
                    else if (_runChecks[MaxRun].Flag > MaxFlag)
                    {
                        enableIndex = timeoutIndex;

                        return _runChecks[enableIndex] = new RunCheck { Flag = 1 };
                    }

                    System.Threading.Thread.Sleep(1000);
                }

                enableIndex = timeoutIndex;

                return _runChecks[enableIndex] = new RunCheck { Flag = 1 };
            }
        }

        /// <summary>
        /// convert(html->pdf,docx->pdf,docx->html,html->docx)
        /// </summary>
        /// <param name="libreOfficePath"></param>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        /// <exception cref="FormatConvertException"></exception>
        public static void Convert(string inputFile, string outputFile)
        {
            if (string.IsNullOrEmpty(_libreOfficePath))
            {
                throw new FormatConvertException("unset environment variable 'LibreOffice_ExePath'");
            }

            if (!File.Exists(_libreOfficePath))
            {
                throw new FormatConvertException("libre office exe path not exists:" + _libreOfficePath);
            }

            if (string.IsNullOrEmpty(_installDir))
            {
                throw new FormatConvertException("unset environment variable 'LibreOffice_UserInstallation'");
            }

            if (!File.Exists(inputFile))
            {
                throw new FileNotFoundException("需要转换格式的文件不存在:" + inputFile);
            }

            var runCheck = GetRunCheck(out var index);

            Debug.WriteLine("convert to pdf,index:" + index);

            try
            {
                //Create tmp folder
                var tmpFolder = Path.Combine(Path.GetDirectoryName(outputFile), "yilian_" + index);

                Directory.CreateDirectory(tmpFolder);

                var installPath = _installDir + "/p" + index;

                Directory.CreateDirectory(installPath);

                var commandArgs = new List<string>
                {
                    "-env:UserInstallation=file:///" + installPath,
                    "--convert-to"
                };

                string convertedFile;

                if (outputFile.EndsWith(".pdf"))
                {
                    commandArgs.Add("pdf:writer_pdf_Export");
                    convertedFile = Path.Combine(tmpFolder, Path.GetFileNameWithoutExtension(inputFile) + ".pdf");

                    if (!(inputFile.EndsWith(".html") || inputFile.EndsWith(".htm")
                        || inputFile.EndsWith(".docx") || inputFile.EndsWith(".xlsx")))
                    {
                        throw new FormatConvertException("文件格式转换不支持:" + inputFile);
                    }
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
                else
                {
                    throw new FormatConvertException("文件格式转换不支持:" + inputFile);
                }

                commandArgs.AddRange(new[] { inputFile, "--norestore", "--writer", "--headless", "--outdir", tmpFolder });

                var procStartInfo = new ProcessStartInfo(_libreOfficePath)
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Environment.CurrentDirectory
                };

                foreach (var arg in commandArgs)
                {
                    procStartInfo.ArgumentList.Add(arg);
                }

                var process = new Process() { StartInfo = procStartInfo };

                process.Start();

                if (!process.WaitForExit(180000))
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(30000);
                    }
                    catch { }
                }

                // Check for failed exit code.
                if (process.ExitCode != 0)
                {
                    throw new FormatConvertException("LibreOffice has failed with " + process.ExitCode);
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
                }
            }
            finally
            {
                runCheck.Flag = 0;
            }
        }

        class RunCheck
        {
            public int Flag { get; set; }
        }
    }
}
