using System;

namespace DbDataCompare.App
{
    internal static class NumberHelper
    {
        /// <summary>
        /// 非四舍五入情况下截取最大小数位数字
        /// </summary>
        public static double Truncate(double value, int minDecimals, int maxDecimals, bool percent)
        {
            var decimals = maxDecimals > minDecimals ? maxDecimals : minDecimals;

            if (percent)
            {
                decimals += 2;
            }

            if (decimals > 0)
            {
                var pow = Math.Pow(10, decimals);

                return Math.Truncate(value * pow) / pow;
            }
            else
            {
                return Math.Truncate(value);
            }
        }

        /// <summary>
        /// 获取数字格式化字符串
        /// </summary>
        /// <param name="minDecimals">最小小数位</param>
        /// <param name="maxDecimals">最大小数位</param>
        /// <param name="percent">是否百分比格式</param>
        /// <param name="grouping">是否千分位分组</param>
        public static string GetFormat(int minDecimals, int maxDecimals, bool percent, bool grouping)
        {
            var sb = StringBuilderPool.Acquire();

            sb.Append(grouping ? "#,##0" : "0");

            if (minDecimals > 0)
            {
                sb.Append('.').Append('0', minDecimals);

                if (maxDecimals > minDecimals)
                {
                    sb.Append('#', maxDecimals - minDecimals);
                }
            }
            else if (maxDecimals > 0)
            {
                sb.Append('.').Append('#', maxDecimals);
            }

            if (percent)
            {
                sb.Append('%');
            }

            return sb.Release();
        }

        /// <summary>
        /// 数字格式化
        /// </summary>
        public static string OtcFormatFlex(double? d, int minDecimals, int maxDecimals = 6
            , bool percent = false, bool grouping = false, bool rounded = true)
        {
            return d.HasValue ? OtcFormatFlex(d.Value, minDecimals: minDecimals, maxDecimals: maxDecimals, percent: percent, grouping: grouping, rounded: rounded) : string.Empty;
        }

        /// <summary>
        /// 数字格式化
        /// </summary>
        public static string OtcFormatFlex(double d, int minDecimals, int maxDecimals = 6
            , bool percent = false, bool grouping = false, bool rounded = true)
        {
            if (double.IsNaN(d) || double.IsInfinity(d))
            {
                return string.Empty;
            }

            if (!rounded)
            {
                d = Truncate(d, minDecimals: minDecimals, maxDecimals: maxDecimals, percent: percent);
            }

            return d.ToString(GetFormat(minDecimals: minDecimals, maxDecimals: maxDecimals, percent: percent, grouping: grouping));
        }
    }
}
