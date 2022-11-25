using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
namespace AppPalestre
{

    public class QuartzTaskService : IJob
    {
        private Dictionary<string, string> persone = new Dictionary<string, string>()
        {
            { "mfAQXc4rOBOq4twO3CaO", "Stefano" }
        };
        public Task Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            PalestreApi.Corsi corso = (PalestreApi.Corsi)dataMap.Get("corso");
            string IdSede = dataMap.GetString("IdSede");
            corso.Day = DateTime.Today.AddDays(2);

            var task = Task.Run(() =>
            {
                DateTime ini = DateTime.Now;
                while (!corso.IsPrenotato && (DateTime.Now - ini).TotalSeconds < 15)
                {
                    if (corso.IdCorso != 0)
                    {
                        PalestreApi api = new PalestreApi(corso.CodiceSessione, IdSede);
                        Utils.ScriviLog($"{DateTime.Now} - Prenotazione corso {corso.Nome} per {persone[corso.CodiceSessione]}");
                        var rret = api.Prenota(corso.IdCorso, corso.Day.ToString("yyyy-MM-dd"));
                        corso.IsPrenotato = rret != null && rret != "";
                        Utils.ScriviLog($"{DateTime.Now} - Corso {(!corso.IsPrenotato ? "non " : "")}prenotato {corso.Nome} per {persone[corso.CodiceSessione]}!!");
                    }
                    else
                        Utils.ScriviLog($"{DateTime.Now} - Corso {corso.Nome} con Id=0");
                }

                Task.Delay(10000).ContinueWith(task =>
                {
                    Utils.ScriviLog($"{DateTime.Now} - Attesa");
                });

            });
            
            return task;
        }

    }


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