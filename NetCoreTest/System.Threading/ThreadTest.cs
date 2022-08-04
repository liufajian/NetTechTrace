using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace System.Threading
{
    [TestClass]
    public class ThreadTest
    {
        [ThreadStatic]
        public static string TestStatic;

        [TestMethod]
        public void Test1()
        {
            void innerWaitTasks(params Task<int>[] tasks)
            {
                if (tasks is null || tasks.Length < 1)
                {
                    return;
                }
                if (tasks.Any(n => n == null))
                {
                    tasks = tasks.Where(n => n != null).ToArray();

                    if (tasks.Length < 1)
                    {
                        return;
                    }
                }

                foreach (var task in tasks)
                {
                    task.ContinueWith(t =>
                    {
                        Console.WriteLine($"结束任务:{task.Result}::{Thread.CurrentThread.ManagedThreadId}::{TestStatic}");
                    });
                }

                Task.WaitAll(tasks);
            }

            var taskList = new List<Task<int>>(20);

            for (var i = 0; i < 10; i++)
            {
                var d = i;
                taskList.Add(Task.Run(() =>
                {
                    TestStatic = "hello" + d;
                    Console.WriteLine($"{d}::{Thread.CurrentThread.ManagedThreadId}::{TestStatic}");
                    Thread.Sleep(10000);
                    return d;
                }));
            }

            innerWaitTasks(taskList.ToArray());

            Console.WriteLine("end!");
        }
    }
}
