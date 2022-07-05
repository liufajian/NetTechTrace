using System;

namespace InteropSample
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("进程:" + (Environment.Is64BitProcess ? "x64" : "x86"));

            //new InteropTest().Test();

            new MyApi.MyApiTest().Test();

            Console.Read();
        }
    }
}
