using System;
using System.IO;
using System.Net.Mime;

namespace BcWin.Core.CssStyle
{
    public class Css
    {
        public static string IbetStatements { get; set; }
        public static string SboStatements { get; set; }

        static Css()
        {
            TextReader reader = new StreamReader(Path.Combine(Environment.CurrentDirectory, "CssStyle/table_w.css"));
            TextReader reader1 = new StreamReader(Path.Combine(Environment.CurrentDirectory, "CssStyle/oddsFamily.css"));
            IbetStatements = reader.ReadToEnd();
            IbetStatements += reader1.ReadToEnd();

            TextReader readerSbo = new StreamReader(Path.Combine(Environment.CurrentDirectory, "CssStyle/global.css"));
            TextReader reader1Sbo = new StreamReader(Path.Combine(Environment.CurrentDirectory, "CssStyle/maincontent.css"));
            SboStatements = readerSbo.ReadToEnd();
            SboStatements += reader1Sbo.ReadToEnd();
        }
    }
}
