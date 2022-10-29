using System.Collections.Generic;
using System.Linq;

namespace DbDataCompare.App
{
    class MyRequest
    {
        public string Db1ConnectionString { get; set; }

        public string Db2ConnectionString { get; set; }

        public string Db1SqlCommand { get; set; }

        public string Db2SqlCommand { get; set; }

        public string[] Db1ResultKeys { get; set; }

        public string[] Db2ResultKeys { get; set; }

        public HashSet<string> IgnoreFields { get; set; }

        public int DoublePrecision { get; set; } = 5;

        public void CheckAndRepair()
        {
            //连接字符串

            if (string.IsNullOrWhiteSpace(Db1ConnectionString))
            {
                throw new System.Exception("[DB1 连接字符串]未赋值");
            }

            if (string.IsNullOrWhiteSpace(Db2ConnectionString))
            {
                Db2ConnectionString = Db1ConnectionString;
            }

            //查询语句

            if (string.IsNullOrWhiteSpace(Db1SqlCommand))
            {
                throw new System.Exception("[DB1 查询语句]未赋值");
            }

            if (string.IsNullOrWhiteSpace(Db2SqlCommand))
            {
                Db2SqlCommand = Db1SqlCommand;
            }

            //数据标识字段

            if (Db1ResultKeys == null || !Db1ResultKeys.Any())
            {
                throw new System.Exception("[DB1 数据标识字段]未赋值");
            }

            if (Db2ResultKeys == null || !Db2ResultKeys.Any())
            {
                Db2ResultKeys = Db1ResultKeys;
            }

            //比较选项

            if (DoublePrecision < 0)
            {
                throw new System.Exception("[比较选项 小数精度]不能为负值");
            }

            //IgnoreFields

            if (IgnoreFields == null)
            {
                IgnoreFields = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            }

            foreach (var item in Db1ResultKeys)
            {
                IgnoreFields.Add(item);
            }

            foreach (var item in Db2ResultKeys)
            {
                IgnoreFields.Add(item);
            }
        }
    }
}
