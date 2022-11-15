using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DbDataCompare.App
{
    class RequestParser
    {
        /// <summary>
        /// 
        /// </summary>
        public static MyRequest ParseRequestText(string text)
        {
            var req = new MyRequest();

            if (string.IsNullOrWhiteSpace(text))
            {
                return req;
            }

            var sr = new StringReader(text);

            var lineIndex = 0;

            Action<string> setter = null;

            var lineList = new List<string>();

            void setPropValue()
            {
                if (setter == null)
                {
                    return;
                }

                setter(string.Join(Environment.NewLine, lineList));

                lineList.Clear();
            }

            while (sr.Peek() > 0)
            {
                lineIndex++;

                var line = sr.ReadLine().Trim();

                if (line.Length < 1)
                {
                    continue;
                }

                if (line[0] == '[')
                {
                    setPropValue();

                    setter = ParseProp(line, lineIndex, req);
                }
                else
                {
                    lineList.Add(line);
                }
            }

            setPropValue();

            return req;
        }

        static Action<string> ParseProp(string line, int lineIndex, MyRequest req)
        {
            var str = line;
            var index = line.IndexOf(']', 1);

            if (index > 0)
            {
                str = line.Substring(1, index - 1);
            }

            var ss = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (ss.Length != 2)
            {
                throw new Exception($"第{lineIndex}行,未能理解:{str}");
            }

            if (ss[0] == "DB1")
            {
                if (ss[1] == "连接字符串")
                {
                    return m => req.Db1ConnectionString = m;
                }

                if (ss[1] == "查询语句")
                {
                    return m => req.Db1SqlCommand = m;
                }

                if (ss[1] == "数据标识字段")
                {
                    return m =>
                    {
                        if (string.IsNullOrEmpty(m))
                        {
                            return;
                        }
                        req.Db1ResultKeys = m.Split(new[] { ',', '，' })
                            .Where(n => !string.IsNullOrWhiteSpace(n))
                            .Select(n => n.Trim()).ToArray();
                    };
                }
            }
            else if (ss[0] == "DB2")
            {
                if (ss[1] == "连接字符串")
                {
                    return m => req.Db2ConnectionString = m;
                }

                if (ss[1] == "查询语句")
                {
                    return m => req.Db2SqlCommand = m;
                }

                if (ss[1] == "数据标识字段")
                {
                    return m =>
                    {
                        if (string.IsNullOrEmpty(m))
                        {
                            return;
                        }
                        req.Db2ResultKeys = m.Split(new[] { ',', '，' })
                            .Where(n => !string.IsNullOrWhiteSpace(n))
                            .Select(n => n.Trim()).ToArray();
                    };
                }
            }
            else if (ss[0] == "比较选项")
            {
                if (ss[1] == "忽略比较的字段")
                {
                    return m =>
                    {
                        if (string.IsNullOrEmpty(m))
                        {
                            return;
                        }

                        var arr = m.Split(new[] { ',', '，' }).Where(n => !string.IsNullOrWhiteSpace(n))
                                    .Select(n => n.Trim());

                        req.IgnoreFields = new HashSet<string>(arr, System.StringComparer.OrdinalIgnoreCase);
                    };
                }
                else if (ss[1] == "小数精度")
                {
                    return m =>
                    {
                        if (string.IsNullOrEmpty(m))
                        {
                            return;
                        }
                        if (!int.TryParse(m, out var d))
                        {
                            throw new Exception($"未能正确解析 小数精度:{m}");
                        }
                        req.DoublePrecision = d;
                    };
                }
            }

            throw new Exception($"第{lineIndex}行,未能理解:{str}");
        }
    }
}
