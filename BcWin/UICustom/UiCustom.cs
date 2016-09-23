using BcWin.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BcWin.UICustom
{
    public class BcTabPage : TabPage
    {
        private Font _font;
        public override Font Font 
        {
            get 
            {
                return FontHelper.DefaultFont();
            }
          set{
              this._font = value;
          }
        }
    }

    public class NumericTextBox : TextBox
    {
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            // Check if the pressed character was a backspace or numeric.
            if (e.KeyChar != (char)8 && !char.IsNumber(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}
