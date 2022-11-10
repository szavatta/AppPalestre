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
        public JObject Palinsesti()
        {
            var request = (HttpWebRequest)WebRequest.Create("https://app.shaggyowl.com/funzioniapp/v405/palinsesti");

            var postData = $"id_sede=8628&codice_sessione=mfAQXc4rOBOq4twO3CaO&giorno={DateTime.Now.ToString("yyyy-MM-dd")}";
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

            return obj;
        }

        public string Prenota(int idcorso, string datacorso)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://app.shaggyowl.com/funzioniapp/v405/prenotazione_new");

            var postData = $"id_sede=8628&codice_sessione=mfAQXc4rOBOq4twO3CaO&id_orario_palinsesto={idcorso}&data={datacorso}";
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

            string ret = null;
            if ((string)obj["status"] == "2")
            {
                ret = (string)obj["parametri"]["prenotazione"]["id_prenotazione"];
            }

            return (ret);
        }

        public bool Elimina(int idprenotazione)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://app.shaggyowl.com/funzioniapp/v405/cancella_prenotazione");

            var postData = $"id_sede=8628&codice_sessione=mfAQXc4rOBOq4twO3CaO&id_prenotazione={idprenotazione}&tipo=prenotazione";
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

    }


    public static class GenreRatingFinder
    {
        public static (string? Genre, double Imdb, double Rotten) UsingDynamic(string jsonString)
        {
            var dynamicObject = JsonConvert.DeserializeObject<dynamic>(jsonString)!;
            var genre = dynamicObject.Genre;
            var imdb = dynamicObject.Rating.Imdb;
            var rotten = dynamicObject.Rating["Rotten Tomatoes"];
            return (genre, imdb, rotten);
        }
    }


}
