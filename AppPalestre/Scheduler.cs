﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace AppPalestre
{
    public class Scheduler
    {
        private static IConfiguration _configuration;
        private System.Threading.Timer bTimer;
        private Dictionary<string, string> persone;

        class Corsi
        {
            public DayOfWeek Giorno { get; set; }
            public string Orario { get; set; }
            public string Nome { get; set; }
            public string CodiceSessione { get; set; }
            public DateTime Day { get; set; }
            public int IdCorso { get; set; }
            public bool IsPrenotato { get; set; }
        }

        public void Fire(IConfiguration configuration)
        {
            _configuration = configuration;
            persone = new Dictionary<string, string>()
            {
                { "mfAQXc4rOBOq4twO3CaO", "Stefano" }
            };
            TimerCallback timerDelegate = new TimerCallback(tick);

#if (DEBUG)
            bTimer = new System.Threading.Timer(timerDelegate, null, 20000, 20000);
#else
            bTimer = new System.Threading.Timer(timerDelegate, null, 60000, 60000);
#endif
            tick(null);
            //aTimer = new Timer()
            //{
            //    Interval = 60000,
            //    Enabled = true                
            //};
            //aTimer.Elapsed += OnTimedEvent;
        }

        void tick(Object obj)
        {

            bTimer.Change(Timeout.Infinite, Timeout.Infinite);

            try
            {
                ScriviLog($"{DateTime.Now} - Esecuzione timer");

                try
                {
                    WebClient client = new WebClient();
                    var test = client.DownloadStringTaskAsync("http://app.tdmitalia.it:450/home/privacy");
                }
                catch { }

                List<Corsi> corsi = _configuration.GetSection("Corsi").Get<List<Corsi>>();
                string CodiceSessione = _configuration.GetSection("CodiceSessione").Get<string>();
                string IdSede = _configuration.GetSection("IdSede").Get<string>();

                List<Corsi> corsidaprenotare = new List<Corsi>();   

                foreach (var corso in corsi)
                {
                    var orario = corso.Orario.Split(":");
                    int ora = Convert.ToInt32(orario[0]);
                    int minuto = Convert.ToInt32(orario[1]);

                    corso.CodiceSessione = string.IsNullOrEmpty(corso.CodiceSessione) ? CodiceSessione : corso.CodiceSessione;

                    DateTime dt1 = DateTime.Today.AddDays(2).AddHours(ora).AddMinutes(minuto);
                    DateTime dt2 = DateTime.Today.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute);

                    DayOfWeek giornoset = (DayOfWeek)(((int)corso.Giorno - 2 + 7) % 7);

                    //ScriviLog($"{DateTime.Now} - nome: '{corso.Nome}' giorno: '{corso.Giorno}' giorno-2: '{giornoset}' ora:{ora} minuto: {minuto}");
                    if (DateTime.Now.DayOfWeek == giornoset && dt2.Hour == dt1.AddMinutes(-1).Hour && dt2.Minute == dt1.AddMinutes(-1).Minute)
                    {
                        ScriviLog($"{DateTime.Now} - Verifica corso '{corso.Nome}' con orario {corso.Giorno} {corso.Orario}");
                        PalestreApi api = new PalestreApi(CodiceSessione, IdSede);
                        int id = api.GetIdCorso(corso.Giorno, ora, minuto, corso.Nome);
                        if (id != 0)
                        {
                            corsidaprenotare.Add(new Corsi {
                                Nome = corso.Nome,
                                CodiceSessione = corso.CodiceSessione,
                                Day = dt1,
                                IdCorso = id
                            });
                        }
                        else
                        {
                            ScriviLog($"{DateTime.Now} - Corso non trovato");
                        }
                    }

                }

                if (corsidaprenotare.Count > 0)
                {
                    ScriviLog($"{DateTime.Now} - Trovato corso");
                    bool ret = false;
                    DateTime ini = DateTime.Now;
                    while (corsidaprenotare.Where(q => q.IsPrenotato == false).Count() > 0 && (DateTime.Now - ini).TotalSeconds < 90)
                    {
                        foreach (var cdp in corsidaprenotare.Where(q => q.IsPrenotato == false))
                        {
                            PalestreApi api = new PalestreApi(cdp.CodiceSessione, IdSede);
                            ScriviLog($"{DateTime.Now} - Prenotazione corso {cdp.Nome} per {persone[cdp.CodiceSessione]}");
                            var rret = api.Prenota(cdp.IdCorso, cdp.Day.ToString("yyyy-MM-dd"));
                            cdp.IsPrenotato = rret != null && rret != "";
                            ScriviLog($"{DateTime.Now} - Corso {(!cdp.IsPrenotato ? "non " : "")}prenotato {cdp.Nome} per {persone[cdp.CodiceSessione]}!!");
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                ScriviLog($"{DateTime.Now} - Errore: {ex.Message}");
            }

#if (DEBUG)
            bTimer.Change(20000, 20000);
#else
            bTimer.Change(60000, 60000);
#endif
        }


        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            //((Timer)source).Enabled = false;

            try
            {
                ScriviLog($"{DateTime.Now} - Esecuzione timer");

                List<Corsi> corsi = _configuration.GetSection("Corsi").Get<List<Corsi>>();
                string CodiceSessione = _configuration.GetSection("CodiceSessione").Get<string>();
                string IdSede = _configuration.GetSection("IdSede").Get<string>();

                //Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);

                foreach (var corso in corsi)
                {
                    var orario = corso.Orario.Split(":");
                    int ora = Convert.ToInt32(orario[0]);
                    int minuto = Convert.ToInt32(orario[1]);
                    DateTime dt1 = DateTime.Today.AddHours(ora).AddMinutes(minuto);
                    DateTime dt2 = DateTime.Today.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute);

                    DayOfWeek giornoset = (DayOfWeek)(((int)corso.Giorno - 2 + 7) % 7);

                    if (DateTime.Now.DayOfWeek == giornoset && dt1 == dt2.AddMinutes(-1))
                    {
                        ScriviLog($"{DateTime.Now} - Verifica corso '{corso.Nome}' con orario {corso.Giorno} {corso.Orario}");
                        PalestreApi api = new PalestreApi(CodiceSessione, IdSede);
                        var id = api.GetIdCorso(corso.Giorno, ora, minuto, corso.Nome);
                        if (id != 0)
                        {
                            ScriviLog($"{DateTime.Now} - Trovato corso");
                            bool ret = false;
                            DateTime ini = DateTime.Now;
                            while (!ret && (DateTime.Now - ini).TotalSeconds < 90)
                            {
                                ScriviLog($"{DateTime.Now} - Prenotazione corso");
                                var rret = api.Prenota(id, DateTime.Now.ToString("yyyy-MM-dd"));
                                ret = rret != null && rret != "";
                                ScriviLog($"{DateTime.Now} - Corso {(!ret ? "non " : "")}prenotato!!");
                            }
                        }
                    }
                }
            }
            catch(Exception ex) 
            {
                ScriviLog($"{DateTime.Now} - Errore: {ex.Message}");
            }

            //((Timer)source).Enabled = true;
        }

        private static void ScriviLog(string testo)
        {
            try
            {
                using (StreamWriter sw = File.AppendText($"logs\\{DateTime.Now.ToString("yyyy-MM-dd")}.log"))
                {
                    sw.WriteLine(testo);
                }
            }
            catch { }
        }
    }
}
