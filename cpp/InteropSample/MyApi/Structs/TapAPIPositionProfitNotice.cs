using System.Runtime.InteropServices;

namespace InteropSample.MyApi
{
    /// <summary>
    /// 客户持仓盈亏通知
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class TapAPIPositionProfitNotice
    {
        /// <summary>
        /// 是否最后一包
        /// </summary>
        public TAPIYNFLAG IsLast;

        ///<summary>
        /// 本地持仓号，服务器编写
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 71)]
        public string PositionNo;

        /// <summary>
        /// 持仓流号
        /// </summary>
        public uint PositionStreamId;

        /// <summary>
        /// 持仓盈亏
        /// </summary>
        public double PositionProfit;

        /// <summary>
        /// LME持仓盈亏
        /// </summary>
        public double LMEPositionProfit;

        /// <summary>
        /// 期权市值
        /// </summary>
        public double OptionMarketValue;

        /// <summary>
        /// 计算价格
        /// </summary>
        public double CalculatePrice;

        /// <summary>
        /// 逐笔浮盈
        /// </summary>
        public double FloatingPL;

        /// <summary>
        /// 
        /// </summary>
        public override string ToString()
        {
            return $"IsLast:{IsLast},PositionNo:{PositionNo},PositionStreamId:{PositionStreamId},PositionProfit:{PositionProfit},LMEPositionProfit:{LMEPositionProfit},OptionMarketValue:{OptionMarketValue},CalculatePrice:{CalculatePrice},FloatingPL:{FloatingPL}";
        }
    }
}
