using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace InteropSample
{
    static class Interop
    {
        //const string DllName = "exlibs\\InteropTest.dll";

        public const string DllName = "InteropTest.dll";

        /// <summary>
        /// 指针转为实际数据对象
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ToField<T>(IntPtr ptr)
        {
            return IntPtr.Zero == ptr ? default : Marshal.PtrToStructure<T>(ptr);
        }

        [DllImport(DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TestStringInOut([MarshalAs(UnmanagedType.AnsiBStr)] string str);

        [DllImport(DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetVersion(StringBuilder buffer, int capacity);

        //-------------------------------------------------------
        // array
        //-------------------------------------------------------

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void GetArrResult(out IntPtr outPtr, out int numPtr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void FreeArrSomeData(IntPtr ptr, int num);
    }
}
