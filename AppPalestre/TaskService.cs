using Microsoft.Extensions.Configuration;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static AppPalestre.PalestreApi;

namespace AppPalestre
{

    public class QuartzTaskService : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            PalestreApi.Corsi corso = (PalestreApi.Corsi)dataMap.Get("corso");
            string IdSede = dataMap.GetString("IdSede");
            corso.Day = DateTime.Today.AddDays(2);
            string nomeUtente = PalestreApi.ListaUtenti.Where(q => q.CodiceSessione == corso.CodiceSessione).FirstOrDefault()?.Nome;

            var task = Task.Run(() =>
            {
                DateTime ini = DateTime.Now;
                while (!corso.IsPrenotato && (DateTime.Now - ini).TotalSeconds < 15)
                {
                    if (corso.IdCorso != 0)
                    {
                        PalestreApi api = new PalestreApi(corso.CodiceSessione, IdSede);
                        Utils.ScriviLog($"{DateTime.Now} - Prenotazione corso {corso.Nome} per {nomeUtente}");
                        var rret = api.Prenota(corso.IdCorso, corso.Day.ToString("yyyy-MM-dd"));
                        corso.IsPrenotato = rret != null && rret != "";
                        Utils.ScriviLog($"{DateTime.Now} - Corso {(!corso.IsPrenotato ? "non " : "")}prenotato {corso.Nome} per {nomeUtente} codice {corso.CodiceSessione}");
                    }
                    else
                    {
                        Utils.ScriviLog($"{DateTime.Now} - Corso {corso.Nome} con Id=0 per {nomeUtente} codice {corso.CodiceSessione}");
                        break;
                    }
                }

                Task.Delay(10000).ContinueWith(task =>
                {
                    Utils.ScriviLog($"{DateTime.Now} - Attesa per {nomeUtente} codice {corso.CodiceSessione}");
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