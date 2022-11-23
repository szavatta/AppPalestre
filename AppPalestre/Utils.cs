using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AppPalestre
{
    public class Utils
    {
        public static void ScriviLog(string testo)
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
