using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace AppPalestre
{
    public class PalestreApi
    {
        public PalestreApi(string codiceSessione, string idSede)
        {
            CodiceSessione = codiceSessione;
            IdSede = idSede;
        }

        public string CodiceSessione { get; set; }
        public string IdSede { get; set; }

        public class Giorno
        {
            public DateTime Data { get; set; }
            public string Datas { get; set; }
            public List<Corso> Corsi { get; set; }
        }

        public class Corsi
        {
            public DayOfWeek Giorno { get; set; }
            public string Orario { get; set; }
            public string Nome { get; set; }
            public string CodiceSessione { get; set; }
            public DateTime Day { get; set; }
            public int IdCorso { get; set; }
            public bool IsPrenotato { get; set; }
        }

        public class Corso
        {
            public int Id { get; set; }
            public string Nome { get; set; }
            public string Frase { get; set; }
            public string Inizio { get; set; }
            public string Fine { get; set; }
            public int IdPrenotazione { get; set; }
        }

        public class Utente
        {
            public string Nome { get; set; }
            public string Cognome { get; set; }
            public int Stato { get; set; }
            public string UrlFoto { get; set; }
        }

        public JObject Palinsesti()
        {
            JObject obj = null;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://app.shaggyowl.com/funzioniapp/v405/palinsesti");

                var postData = $"id_sede={IdSede}&codice_sessione={CodiceSessione}&giorno={DateTime.Now.ToString("yyyy-MM-dd")}";
                var data = Encoding.ASCII.GetBytes(postData);

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                obj = JObject.Parse(responseString);
            }
            catch { }

            return obj;
        }

        public string Prenota(int idcorso, string datacorso)
        {
            string ret = null;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://app.shaggyowl.com/funzioniapp/v405/prenotazione_new");

                var postData = $"id_sede={IdSede}&codice_sessione={CodiceSessione}&id_orario_palinsesto={idcorso}&data={datacorso}";
                var data = Encoding.ASCII.GetBytes(postData);

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                JObject obj = JObject.Parse(responseString);

                if ((string)obj["status"] == "2")
                {
                    ret = (string)obj["parametri"]["prenotazione"]["id_prenotazione"];
                }
            }
            catch { }

            return (ret);
        }

        public bool Elimina(int idprenotazione)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://app.shaggyowl.com/funzioniapp/v405/cancella_prenotazione");

            var postData = $"id_sede={IdSede}&codice_sessione={CodiceSessione}O&id_prenotazione={idprenotazione}&tipo=prenotazione";
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            JObject obj = JObject.Parse(responseString);

            bool ret = (string)obj["status"] == "2";

            return (ret);
        }

        public int GetIdCorso(DayOfWeek WeekDay, int ora, int minuto, string nome)
        {
            JObject pal = Palinsesti();

            try
            {
                if ((string)pal["status"] == "2")
                {
                    foreach (JToken giorno in (JArray)pal.SelectToken("$..lista_risultati..giorni"))
                    {
                        DateTime data = Convert.ToDateTime(giorno.SelectToken("giorno"));
                        if (data.DayOfWeek == WeekDay)
                        {
                            foreach (JToken orario in (JArray)giorno.SelectToken("orari_giorno"))
                            {
                                var vorario = ((string)orario["orario_inizio"]).Split(":");
                                if ((string)orario["nome_corso"] == nome && Convert.ToInt32(vorario[0]) == ora && Convert.ToInt32(vorario[1]) == minuto)
                                {
                                    return (int)orario["id_orario_palinsesto"];
                                }

                            }
                        }
                    }
                }
            }
            catch { }

            return 0;
        }

        public List<Utente> ListaPrenotati(int idcorso, DateTime datacorso)
        {
            JObject obj = null;
            List<Utente> lista = new List<Utente>();

            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://app.shaggyowl.com/funzioniapp/v405/lista_prenotati");

                var postData = $"id_sede={IdSede}&codice_sessione={CodiceSessione}&id_orario_palinsesto={idcorso}&giorno={datacorso.ToShortDateString()}";
                var data = Encoding.ASCII.GetBytes(postData);

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                obj = JObject.Parse(responseString);

                if ((string)obj["status"] == "2")
                {

                    foreach (JToken nominativo in (JArray)obj.SelectToken("$..lista_risultati"))
                    {
                        lista.Add(new Utente
                        {
                            Nome= (string)nominativo["nome"],
                            Cognome= (string)nominativo["cognome"],
                            Stato =(int)nominativo["stato"],
                            UrlFoto=(string)nominativo["path_img_list"]
                        });
                    }
                }


            }
            catch { }

            return lista;
        }

    }


}
