using System.Runtime.InteropServices;

namespace InteropSample.MyApi
{
    [StructLayout(LayoutKind.Sequential)]
    public class TapAPIPositionProfit
    {
        public string PositionNo;
        public int PositionStreamId;
        public double PositionProfit;
        public double LMEPositionProfit;
        public double OptionMarketValue;
        public double CalculatePrice;
        public double FloatingPL;

        public override string ToString()
        {
            return $"{PositionNo}--{PositionStreamId}--{PositionProfit}";
        }
    }
}
