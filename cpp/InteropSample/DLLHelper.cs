using System;
using System.Collections.Generic;
using System.IO;

namespace InteropSample
{
    class DLLHelper
    {
        const string rootNS = "InteropSample.Resources.";

        static readonly object lockObj;
        static bool installed;

        static DLLHelper()
        {
            lockObj = new object();
        }

        /// <summary>
        /// 确保相关的C++依赖库存在
        /// </summary>
        public static void EnsureDLL()
        {
            //--------------------------------------------
            // 检查所需C++引用库是否已安装
            //--------------------------------------------

            if (installed)
            {
                return;
            }

            //--------------------------------------------
            // 安装所需的C++引用库
            //--------------------------------------------

            lock (lockObj)
            {
                if (installed)
                {
                    return;
                }

                var dllFolder = Path.GetDirectoryName(typeof(DLLHelper).Assembly.Location);

                dllFolder = Path.Combine(dllFolder, "exlibs");

                //类库手动安装
                var validFile = Path.Combine(dllFolder, "fix_dll.txt");
                if (File.Exists(validFile))
                {
                    installed = true;
                    return;
                }

                Directory.CreateDirectory(dllFolder);

                var resNamespace = rootNS + (Environment.Is64BitProcess ? "win64." : "win32.");
                var dllObjs = new List<DllResInfo>(){
                    GetDllResInfo(resNamespace,"InteropTest.dll")
                };

                foreach (var obj in dllObjs)
                {
                    var bytes = obj.ResBytes;
                    var dllPath = Path.Combine(dllFolder, obj.ResName);
                    if (!File.Exists(dllPath) || bytes.Length != new FileInfo(dllPath).Length)
                    {
                        
                    }
                    File.WriteAllBytes(dllPath, bytes);
                }

                installed = true;
            }
        }

        private static DllResInfo GetDllResInfo(string resNamespace, string resName)
        {
            using (var s = typeof(DllResInfo).Assembly.GetManifestResourceStream(resNamespace + resName))
            {
                var resBytes = new byte[s.Length];
                s.Read(resBytes, 0, resBytes.Length);
                return new DllResInfo { ResName = resName, ResBytes = resBytes };
            }
        }

        class DllResInfo
        {
            public string ResName { get; set; }

            public byte[] ResBytes { get; set; }
        }
    }

   
}
