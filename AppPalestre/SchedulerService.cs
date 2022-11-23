﻿using Microsoft.Extensions.Configuration;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using static AppPalestre.PalestreApi;

namespace AppPalestre
{
    public class QuartzSchedulerService
    {
        /// <summary>
        /// Cron Expression Generator online
        /// https://www.freeformatter.com/cron-expression-generator-quartz.html
        /// </summary>
        /// <param name="_configuration"></param>
        /// <returns></returns>
        public static async System.Threading.Tasks.Task StartAsync(IConfiguration _configuration)
        {
            try
            {
                List<Corsi> corsi = _configuration.GetSection("Corsi").Get<List<Corsi>>();
                string CodiceSessione = _configuration.GetSection("CodiceSessione").Get<string>();
                corsi.Where(q => string.IsNullOrEmpty(q.CodiceSessione)).ToList().ForEach(q => q.CodiceSessione = CodiceSessione);
                string IdSede = _configuration.GetSection("IdSede").Get<string>();

                var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
                if (!scheduler.IsStarted)
                {
                    await scheduler.Start();
                }
                int cont = 0;
                foreach (var corso in corsi)
                {
                    JobDataMap jobDataMap = new JobDataMap();
                    jobDataMap.Put("corso", corso);
                    jobDataMap.Put("IdSede", IdSede);
                    DateTime dt = DateTime.Today.AddDays(2).AddHours(Convert.ToInt32(corso.Orario.Split(":")[0])).AddMinutes(Convert.ToInt32(corso.Orario.Split(":")[1]));
                    string cronExpression = $"50 {dt.AddMinutes(-1).Minute} {dt.AddMinutes(-1).Hour} ? * {corso.Giorno.ToString().ToUpper().Substring(0,3)} *";
                    var job = JobBuilder.Create<QuartzTaskService>()
                        .WithIdentity($"ExecuteTaskServiceCallJob{cont}", "group1")
                        .UsingJobData(jobDataMap)
                        .Build();
                    var trigger = TriggerBuilder.Create()
                        .WithIdentity($"ExecuteTaskServiceCallTrigger{cont}", "group1")
                        .WithCronSchedule(cronExpression)
                        .Build();
                    await scheduler.ScheduleJob(job, trigger);
                    cont++;
                }

            }
            catch (Exception ex)
            {

            }
        }
    }

}