using Microsoft.AspNetCore.Http;
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


        class Corsi
        {
            public DayOfWeek Giorno { get; set; }
            public string Orario { get; set; }
            public string Nome { get; set; }
        }

        public void Fire(IConfiguration configuration)
        {
            _configuration = configuration;

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

                foreach (var corso in corsi)
                {
                    var orario = corso.Orario.Split(":");
                    int ora = Convert.ToInt32(orario[0]);
                    int minuto = Convert.ToInt32(orario[1]);

                    DayOfWeek giornoset = (DayOfWeek)(((int)corso.Giorno - 2 + 7) % 7);

                    //ScriviLog($"{DateTime.Now} - nome: '{corso.Nome}' giorno: '{corso.Giorno}' giorno-2: '{giornoset}' ora:{ora} minuto: {minuto}");
                    if (DateTime.Now.DayOfWeek == giornoset && DateTime.Now.Hour == ora && DateTime.Now.Minute == minuto - 1)
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
                        else
                        {
                            ScriviLog($"{DateTime.Now} - Corso non trovato");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ScriviLog($"{DateTime.Now} - Errore: {ex.Message}");
            }

#if (DEBUG)
            bTimer.Change(60000, 60000);
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

                    DayOfWeek giornoset = (DayOfWeek)(((int)corso.Giorno - 2 + 7) % 7);

                    if (DateTime.Now.DayOfWeek == giornoset && DateTime.Now.Hour == ora && DateTime.Now.Minute == minuto - 1)
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
