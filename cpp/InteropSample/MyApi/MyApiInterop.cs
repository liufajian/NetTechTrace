using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace InteropSample.MyApi
{
    static class MyApiInterop
    {
        const string DllName = Interop.DllName;

        /// <summary>
        /// 指针转为实际数据对象
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ToField<T>(IntPtr ptr)
        {
            return IntPtr.Zero == ptr ? default : Marshal.PtrToStructure<T>(ptr);
        }

        /// <summary>
        /// 
        /// </summary>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void TapQuoteAPIEvent(EventType type, int errorCode, TAPIYNFLAG isLast, IntPtr info);

        [DllImport(DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr CreateAPI();

        [DllImport(DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void RegisterEventCallBack(IntPtr pApi, TapQuoteAPIEvent onEvent);

        [DllImport(DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void API_Test1(IntPtr pApi);

        [DllImport(DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void API_Test2(IntPtr pApi);

        [DllImport(DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void API_Test3(IntPtr pApi);

        [DllImport(DllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr TestTapAPIPositionProfitNotice();
    }

    public enum TAPIYNFLAG : byte
    {
        /// <summary>
        /// 是
        /// </summary>
        YES = (byte)'Y',

        /// <summary>
        /// 否
        /// </summary>
        NO = (byte)'N'
    }

    public enum EventType
    {
        Test1 = 1,
        Test2 = 2,
        Test3 = 3,
    }
}
