using Quartz;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
namespace AppPalestre
{
    public class TaskService1 : IJob
    {
        //public static readonly string SchedulingStatus = "0 0/1 * 1/1 * ? *";
        public Task Execute(IJobExecutionContext context)
        {
            var task = Task.Run(() =>
            {
                //if (SchedulingStatus.Equals("ON"))
                //{
                int a = 1;
                //}
            });
            return task;
        }
    }

    public class TaskService2 : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var task = Task.Run(() =>
            {
                int a = 1;
            });
            return task;
        }
    }



}