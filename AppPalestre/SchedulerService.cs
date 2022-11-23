using Quartz;
using Quartz.Impl;
using System;
using System.Configuration;
namespace AppPalestre
{
    public class SchedulerService
    {
        private static readonly string ScheduleCronExpression = "";
        public static async System.Threading.Tasks.Task StartAsync()
        {
            try
            {
                var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
                if (!scheduler.IsStarted)
                {
                    await scheduler.Start();
                }
                var job1 = JobBuilder.Create<TaskService1>()
                    .WithIdentity("ExecuteTaskServiceCallJob1", "group1")
                    .Build();
                var trigger1 = TriggerBuilder.Create()
                    .WithIdentity("ExecuteTaskServiceCallTrigger1", "group1")
                    .WithCronSchedule(ScheduleCronExpression)
                    .WithCronSchedule("0 0 12 * * ?")
                    .Build();
                await scheduler.ScheduleJob(job1, trigger1);

                var job2 = JobBuilder.Create<TaskService2>()
                    .WithIdentity("ExecuteTaskServiceCallJob1", "group1")
                    .Build();
                var trigger2 = TriggerBuilder.Create()
                    .WithIdentity("ExecuteTaskServiceCallTrigger2", "group1")
                    .WithCronSchedule(ScheduleCronExpression)
                    .WithCronSchedule("30 12 18 ? * TUE *")
                    .Build();
                await scheduler.ScheduleJob(job2, trigger2);





            }
            catch (Exception ex)
            {

            }
        }
    }
}