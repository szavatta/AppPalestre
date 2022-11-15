using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace AppPalestre
{
    public class Scheduler
    {
        private static IConfiguration _configuration;

        class Corsi
        {
            public DayOfWeek Giorno { get; set; }
            public string Orario { get; set; }
        }

        public void Fire(IConfiguration configuration)
        {
            _configuration = configuration;


            //Dictionary<DayOfWeek, string> corsi = new Dictionary<DayOfWeek, string>();
            //corsi.Add(DayOfWeek.Monday, "19:15");
            //corsi.Add(DayOfWeek.Wednesday, "19:15");
            //corsi.Add(DayOfWeek.Friday, "19:00");
            //corsi.Add(DayOfWeek.Thursday, "09:00");

            var aTimer = new Timer
            {
                Interval = 60000,
                AutoReset = true,
                Enabled = true
            };
            aTimer.Elapsed += OnTimedEvent;
        }

        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            ((Timer)source).Enabled = false;

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
                    ScriviLog($"{DateTime.Now} - Verifica corso con orario {corso.Giorno} {corso.Orario}");
                    PalestreApi api = new PalestreApi(CodiceSessione, IdSede);
                    var id = api.GetIdCorso(corso.Giorno, ora, minuto);
                    if (id != 0)
                    {
                        ScriviLog($"{DateTime.Now} - Trovato corso");
                        bool ret = false;
                        DateTime ini = DateTime.Now;
                        while(!ret && (DateTime.Now - ini).TotalSeconds < 90)
                        {
                            ScriviLog($"{DateTime.Now} - Prenotazione corso");
                            var rret = api.Prenota(id, DateTime.Now.ToString("yyyy-MM-dd"));
                            ret = rret != null && rret != "";
                            ScriviLog($"{DateTime.Now} - Corso prenotato!!");
                        }
                    }
                }
            }

            ((Timer)source).Enabled = true;
        }

        private static void ScriviLog(string testo)
        {
            using (StreamWriter sw = File.AppendText("log.txt"))
            {
                sw.WriteLine(testo);
            }
        }
    }
}
