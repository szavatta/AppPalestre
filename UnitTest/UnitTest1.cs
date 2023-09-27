using AppPalestre;
using NUnit.Framework;
using System;
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

        [Test]
        public void Test1()
        {
            //4188943 spinning del lunedi
            //4281929 cicles del martedi
            //4188950 spinning del mercoledi
            //4287005 cicles del giovedi
            //4320466 spinning del venerdi
            PalestreApi api = new PalestreApi(CodiceSessione, IdSede);
            DateTime data = new DateTime(2022, 11, 3);
            int num = 0;
            while (data < DateTime.Now)
            {
                var lista = api.ListaPrenotati(4287005, data);
                var aa = lista.Select(q => q.Nome + " " + q.Cognome);
                var stefano = lista.Where(q => q.Cognome == "Zavatta");
                num += (stefano.Count() > 0) ? 1 : 0;
                data = data.AddDays(7);
            }

            Assert.Pass();
        }

        [Test]
        public void GetInfoutente()
        {
            PalestreApi api = new PalestreApi(CodiceSessione, IdSede);
            api.GetInfoUtente();
        }
    }
}