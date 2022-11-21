using AppPalestre;
using NUnit.Framework;
using System;

namespace UnitTest
{
    public class Tests
    {
        string CodiceSessione = "2NJLK2EmAqxg69s5kgxw";
        string IdSede = "8628";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            PalestreApi api = new PalestreApi(CodiceSessione, IdSede);
            var lista = api.ListaPrenotati(4188943, new DateTime(2022, 11, 21));

            Assert.Pass();
        }
    }
}