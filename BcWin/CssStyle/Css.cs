using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BcWin.CssStyle
{
    public class Css
    {
        public static string IbetStatements { get; set; }
        public static string SboStatements { get; set; }

        static Css()
        {
            TextReader reader = new StreamReader(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "CssStyle/table_w.css"));
            TextReader reader1 = new StreamReader(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "CssStyle/oddsFamily.css"));
            IbetStatements = reader.ReadToEnd();
            IbetStatements += reader1.ReadToEnd();

            TextReader readerSbo = new StreamReader(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "CssStyle/global.css"));
            TextReader reader1Sbo = new StreamReader(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "CssStyle/maincontent.css"));
            SboStatements = readerSbo.ReadToEnd();
            SboStatements += reader1Sbo.ReadToEnd();
        }
    }
}
