using System.Collections.Concurrent;
using System.Text;

namespace DbDataCompare.App
{
    /// <summary>
    /// StringBuilder对象池
    /// </summary>
    public static class StringBuilderPool
    {
        static readonly ConcurrentQueue<StringBuilder> _Pool;

        static StringBuilderPool()
        {
            _Pool = new ConcurrentQueue<StringBuilder>();
        }

        static int _Max = 100;

        /// <summary>
        /// 对象池最大数量
        /// </summary>
        public static int MaxPoolCount
        {
            get { return _Max; }
            set { _Max = value < 10 ? 10 : value; }
        }

        /// <summary>
        /// 请求一个StringBuilder对象
        /// </summary>
        public static StringBuilder Acquire()
        {
            return _Pool.TryDequeue(out var sb) ? sb : new StringBuilder();
        }

        /// <summary>
        /// 释放StringBuilder对象到对象池中并返回StringBuilder中的值
        /// </summary>
        /// <param name="sb"></param>
        /// <returns></returns>
        public static string Release(this StringBuilder sb)
        {
            var str = sb.ToString();
            if (_Pool.Count < _Max && sb.Length < 300)
            {
                sb.Clear();
                _Pool.Enqueue(sb);
            }
            return str;
        }
    }
}