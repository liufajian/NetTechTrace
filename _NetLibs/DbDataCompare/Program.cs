using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DbDataCompare
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += new ThreadExceptionEventHandler(UIThreadException);

            Application.Run(new Form1());
        }

        private static void UIThreadException(object sender, ThreadExceptionEventArgs t)
        {
            MessageBox.Show(t.Exception.Message, "未处理的异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
