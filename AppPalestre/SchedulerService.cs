using Microsoft.Extensions.Configuration;
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
                Utils.ScriviLog($"{DateTime.Now} - Application start");

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
                    PalestreApi api = new PalestreApi(corso.CodiceSessione, IdSede);
                    int ora = Convert.ToInt32(corso.Orario.Split(":")[0]);
                    int minuto = Convert.ToInt32(corso.Orario.Split(":")[1]);
                    corso.IdCorso = api.GetIdCorso(corso.Giorno, ora, minuto, corso.Nome);
                    JobDataMap jobDataMap = new JobDataMap();
                    jobDataMap.Put("corso", corso);
                    jobDataMap.Put("IdSede", IdSede);
                    DateTime dt = DateTime.Today.AddDays(2).AddHours(ora).AddMinutes(minuto);
                    DayOfWeek giornoset = (DayOfWeek)(((int)corso.Giorno - 2 + 7) % 7);
                    string cronExpression = $"50 {dt.AddMinutes(-1).Minute} {dt.AddMinutes(-1).Hour} ? * {giornoset.ToString().ToUpper().Substring(0,3)} *";
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