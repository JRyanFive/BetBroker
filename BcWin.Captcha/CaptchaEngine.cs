using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using tessnet2;

namespace BcWin.Captcha
{
    public class CaptchaEngine
    {
        private static Object Lock = new Object();

        public string DeCaptcha(Bitmap img)
        {
            try
            {
                lock (Lock)
                {
                    var ocr = new Tesseract();
                    ocr.SetVariable("tessedit_char_whitelist", "0123456789"); // If digit only
                    //@"C:\OCRTest\tessdata" contains the language package, without this the method crash and app breaks
                    string path = Path.Combine(Environment.CurrentDirectory, "capdata");
                    ocr.Init(path, "eng", true);
                    var result = ocr.DoOCR(img, Rectangle.Empty);
                    return result[0].Text.Trim();
                }
              
            }
            catch (Exception)
            {
                return "";
            }
          
        }
    }
}
