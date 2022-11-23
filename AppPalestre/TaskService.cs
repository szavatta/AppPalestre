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

            PalestreApi.Corsi cdp = (PalestreApi.Corsi)dataMap.Get("corso");
            string IdSede = dataMap.GetString("IdSede");

            var task = Task.Run(() =>
            {
                DateTime ini = DateTime.Now;
                while (!cdp.IsPrenotato && (DateTime.Now - ini).TotalSeconds < 15)
                {
                    PalestreApi api = new PalestreApi(cdp.CodiceSessione, IdSede);
                    Utils.ScriviLog($"{DateTime.Now} - Prenotazione corso {cdp.Nome} per {persone[cdp.CodiceSessione]}");
                    var rret = api.Prenota(cdp.IdCorso, cdp.Day.ToString("yyyy-MM-dd"));
                    cdp.IsPrenotato = rret != null && rret != "";
                    Utils.ScriviLog($"{DateTime.Now} - Corso {(!cdp.IsPrenotato ? "non " : "")}prenotato {cdp.Nome} per {persone[cdp.CodiceSessione]}!!");
                }
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