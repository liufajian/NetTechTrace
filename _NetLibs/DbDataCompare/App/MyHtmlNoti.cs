using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Text;

namespace DbDataCompare.App
{
    class MyHtmlNoti : INotify
    {
        int _sameCount, _diffCount;

        private readonly StringBuilder _sb;

        private MyHtmlNoti()
        {
            _sb = new StringBuilder(500);
        }

        public static MyHtmlNoti Start()
        {
            var noti = new MyHtmlNoti();

            noti._sb.Append(@"
<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"">
  <title></title>
  <style type=""text/css"">
    html,body{width:2000px}
    p{padding:5px 0;}
    .text-info{background:#EDFBFE;}
    .text-sum{background:#F8F9FA;}
    .text-error{background:#F8D7DA;}
    .diff-info{background:#FFFAED;}
    .diff-begin{background:#FFFAED;padding:5px 0;margin-top:10px;}
    th,td{padding:8px;}
  </style>
</head>
<body>
");

            return noti;
        }

        public string End()
        {
            _sb.Append($@"
<p class='text-sum'>有{_sameCount}条数据相同,有{_diffCount}条数据不同</p>
  <script></script>
</body>
</html>
");

            return _sb.ToString();
        }

        public void Info(string message)
        {
            _sb.Append("<p class='text-info'>").Append(DateTime.Now).Append(" ").Append(message).AppendLine("</p>");
        }

        public void Error(Exception ex, string message = null)
        {
            _sb.Append("<p class='text-error'>").Append(DateTime.Now)
                .Append(" ").Append(message).Append(" ").Append(ex.Message).AppendLine("</p>");
        }

        public void Diff(string message)
        {
            _sb.Append("<p class='diff-info'>").Append(DateTime.Now).Append(" ").Append(message).AppendLine("</p>");
        }

        public void Diff(string dataKey, List<MyDifference> diffs)
        {
            if (diffs == null || diffs.Count < 1)
            {
                _sameCount++;
            }
            else
            {
                _diffCount++;

                _sb.AppendFormat("<div class='diff-begin'>{0} -------差异------ </div>", dataKey).AppendLine();

                _sb.AppendLine("<table>");

                _sb.Append("<tr>").Append("<th></th>");
                foreach (var diff in diffs)
                {
                    _sb.Append("<th>").Append(diff.Key).AppendLine("</th>");
                }
                _sb.AppendLine("</tr>");

                _sb.Append("<tr>").Append("<td>DB1</td>");
                foreach (var diff in diffs)
                {
                    _sb.Append("<td>").Append(diff.Value1).AppendLine("</td>");
                }
                _sb.AppendLine("</tr>");

                _sb.Append("<tr>").Append("<td>DB2</td>");
                foreach (var diff in diffs)
                {
                    _sb.Append("<td>").Append(diff.Value2).AppendLine("</td>");
                }
                _sb.AppendLine("</tr>");

                _sb.AppendLine("</table>");
            }
        }
    }
}
