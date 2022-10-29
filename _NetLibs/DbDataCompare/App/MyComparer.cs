using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DbDataCompare.App
{
    class MyComparer
    {
        public void Execute(MyRequest req, INotify noti)
        {
            if (req is null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            req.CheckAndRepair();

            IDictionary<string, object>[] result1, result2;

            //--------------------------

            noti.Info("开始查询DB1数据");

            using (var conn = new MySqlConnection(req.Db1ConnectionString))
            {
                result1 = conn.Query(req.Db1SqlCommand)
                    .Select(n => (IDictionary<string, object>)n).ToArray();
            }

            if (!result1.Any())
            {
                noti.Info("从DB1获取到0条数据，不需要进行比较");
                return;
            }

            noti.Info($"从DB1获取到{result1.Length}条数据");

            //--------------------------

            noti.Info("开始查询DB2数据");

            using (var conn = new MySqlConnection(req.Db2ConnectionString))
            {
                result2 = conn.Query(req.Db2SqlCommand)
                    .Select(n => (IDictionary<string, object>)n).ToArray();
            }

            if (!result2.Any())
            {
                noti.Info("从DB2获取到0条数据，不需要进行比较");
                return;
            }

            noti.Info($"从DB2获取到{result2.Length}条数据");

            //--------------------------

            noti.Info("开始比较数据");

            var db2Lookup = result2.ToDictionary(dic2 =>
            {
                var arr = req.Db2ResultKeys.Select(m =>
                    string.Concat(m, "：", dic2.TryGetValue(m, out var val) ? val?.ToString() : string.Empty)).ToArray();

                return string.Join("，", arr);
            });

            foreach (var dic1 in result1)
            {
                var arr = req.Db1ResultKeys.Select(m =>
                    string.Concat(m, "：", dic1.TryGetValue(m, out var val) ? val?.ToString() : string.Empty)).ToArray();

                var datakey1 = string.Join("，", arr);

                if (db2Lookup.TryGetValue(datakey1, out var dic2))
                {
                    db2Lookup[datakey1] = null;

                    var differences = CompareDics(dic1, dic2, req.DoublePrecision, req.IgnoreFields);

                    noti.Diff(datakey1, differences);
                }
                else
                {
                    noti.Diff($"[DB1:{datakey1}]没有找到对应的DB2数据");
                }
            }

            foreach (var kv2 in db2Lookup)
            {
                if (kv2.Value != null)
                {
                    noti.Diff($"[DB2:{kv2.Key}]没有找到对应的DB1数据");
                }
            }
        }

        private List<MyDifference> CompareDics(IDictionary<string, object> dic1, IDictionary<string, object> dic2
            , int doublePrecision, HashSet<string> ignoreFields)
        {
            var list = new List<MyDifference>();

            string getValue(object val)
            {
                if (val is null)
                {
                    return string.Empty;
                }
                else if (val is double dd)
                {
                    return NumberHelper.OtcFormatFlex(dd, 0, doublePrecision);
                }
                else
                {
                    return val.ToString().Trim();
                }
            }

            foreach (var item in dic1)
            {
                if (ignoreFields != null && ignoreFields.Contains(item.Key))
                {
                    continue;
                }

                var value1 = getValue(item.Value);

                if (dic2.TryGetValue(item.Key, out var val))
                {
                    var value2 = getValue(val);

                    if (!value1.Equals(value2, StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(new MyDifference
                        {
                            Key = item.Key,
                            Value1 = value1,
                            Value2 = value2,
                        });
                    }
                }
                else
                {
                    list.Add(new MyDifference
                    {
                        Key = item.Key,
                        Value1 = value1
                    });
                }
            }

            return list;
        }
    }
}
