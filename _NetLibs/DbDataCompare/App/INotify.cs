using System;
using System.Collections.Generic;

namespace DbDataCompare.App
{
    public interface INotify
    {
        void Info(string message);

        void Error(Exception ex, string message = null);

        void Diff(string message);

        void Diff(string dataKey, List<MyDifference> diffs);
    }
}
