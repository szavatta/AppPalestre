using AppPalestre;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTest
{
    public class Tests
    {
        string CodiceSessione = "rmUQCNU3Fsl3ZlgFtAUs";// "2NJLK2EmAqxg69s5kgxw";
        string IdSede = "8628";

        [SetUp]
        public void Setup()
        {
        }

        //[Test]
        //public void Test1()
        //{
        //    //4188943 spinning del lunedi
        //    //4281929 cicles del martedi
        //    //4188950 spinning del mercoledi
        //    //4287005 cicles del giovedi
        //    //4320466 spinning del venerdi
        //    PalestreApi api = new PalestreApi(CodiceSessione, IdSede);
        //    DateTime data = new DateTime(2022, 10, 1);
        //    int num = 0;
        //    List<DateTime> listastefano = new List<DateTime>();
        //    while (data < DateTime.Now)
        //    {
        //        int idcorso;
        //        switch (data.DayOfWeek)
        //        {
        //            case DayOfWeek.Monday:
        //                idcorso = 4188943;
        //                break;
        //            case DayOfWeek.Tuesday:
        //                idcorso = 4281929;
        //                break;
        //            case DayOfWeek.Wednesday:
        //                idcorso = 4188950;
        //                break;
        //            case DayOfWeek.Thursday:
        //                idcorso = 4287005;
        //                break;
        //            case DayOfWeek.Friday:
        //                idcorso = 4320466;
        //                break;
        //            default:
        //                idcorso = 0;
        //                break;
        //        }

        //        if (idcorso > 0)
        //        {
        //            var lista = api.ListaPrenotati(idcorso, data);
        //            var aa = lista.Select(q => q.Nome + " " + q.Cognome);
        //            var stefano = lista.FirstOrDefault(q => q.Cognome == "Zavatta" && q.Nome == "Stefano");
        //            if (stefano != null)
        //                listadate.Add(data);
        //        }
        //        data = data.AddDays(1);
        //    }

        //    var bb = listadate.Select(q => q.ToString("dd-MM-yyyy") + " " + q.DayOfWeek).ToList();

        //    Assert.Pass();
        //}

        [Test]
        public void GetInfoutente()
        {
            PalestreApi api = new PalestreApi(CodiceSessione, IdSede);
            api.GetInfoUtente();
        }
        //    Assert.Pass();
        //}
    }
}