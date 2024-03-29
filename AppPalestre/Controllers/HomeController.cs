﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AppPalestre.Models;
using Newtonsoft.Json.Linq;
using static AppPalestre.PalestreApi;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace AppPalestre.Controllers
{
    public class HomeController : Controller
    {

        private readonly ILogger<HomeController> _logger;
        IConfiguration _configuration;
        string CodiceSessione;
        string IdSede;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            CodiceSessione = _configuration.GetSection("CodiceSessione").Get<string>();
            IdSede = _configuration.GetSection("IdSede").Get<string>();
        }

        public IActionResult Index()
        {
            PalestreApi api = new PalestreApi(CodiceSessione, IdSede);
            JObject pal = api.Palinsesti();

            List<Giorno> giorni = null;

            if ((string)pal["status"] == "2")
            {
                giorni = new List<Giorno>();
                foreach (JToken giorno in (JArray)pal.SelectToken("$..lista_risultati..giorni"))
                {
                    DateTime data = Convert.ToDateTime(giorno.SelectToken("giorno"));
                    Giorno g = new Giorno { Data = data, Datas = giorno.SelectToken("nome_giorno").ToString() };
                    g.Corsi = new List<Corso>();
                    foreach (JToken orario in (JArray)giorno.SelectToken("orari_giorno"))
                    {
                        g.Corsi.Add(new Corso
                        {
                            Id = (int)orario["id_orario_palinsesto"],
                            Nome = (string)orario["nome_corso"],
                            Inizio = (string)orario["orario_inizio"],
                            Fine = (string)orario["orario_fine"],
                            IdPrenotazione = (int)orario["prenotazioni"]["utente_prenotato"],
                            Frase = (string)orario["prenotazioni"]["frase"]
                        });
                    }
                    giorni.Add(g);
                }
                ViewBag.Giorni = giorni;
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public JsonResult Prenota(int idcorso, DateTime? datacorso)
        {
            PalestreApi api = new PalestreApi(CodiceSessione, IdSede);
            string id = api.Prenota(idcorso, datacorso?.ToString("yyyy-MM-dd"));
            
            return Json(id);
        }

        public JsonResult Elimina(int idprenotazione)
        {
            PalestreApi api = new PalestreApi(CodiceSessione, IdSede);
            bool ret = api.Elimina(idprenotazione);

            return Json(ret);
        }

        public JsonResult VerificaTimer()
        {
            DirectoryInfo di = new DirectoryInfo("logs");
            FileInfo file = di.GetFiles().OrderByDescending(q => q.FullName).First();
            string row = System.IO.File.ReadLines(file.FullName).Last();
            string stato = "btn-secondary";
            string txstato = "In pausa";

            string ret = $"Ultimo log: {row.Substring(0, 19)}";
            if (row.Substring(22).StartsWith("Verifica") || row.Substring(22).StartsWith("Trovato") || row.Substring(22).StartsWith("Prenotazione"))
            {
                stato = "btn-warning";
                txstato = "In prenotazione";
            } 
            else if (row.Substring(22).StartsWith("Corso prenotato"))
            {
                stato = "btn-success";
                txstato = "Corso prenotato";
            }


            return Json(new { data = ret, stato = stato, txstato = txstato });
        }

    }
}
