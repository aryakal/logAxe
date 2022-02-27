//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace logAxe
{
   public class UserConfig
   {
      public bool ShowTableHeader { get; set; } = true;
      public bool ShowLineNo { get; set; } = true;

      public int FontHeightPad { get; set; } = 2;
      public int PadBetweenCol { get; set; } = 4;
      public int GlobalLineSelectedWidth { get; set; } = 4;

      public Color BackgroundColor { get; set; } = Color.White;
      public Color TableBackgroundColor { get; set; } = Color.White;
      public Color TableHeaderBackgroundColor { get; set; } = Color.White;
      public Color TableHeaderForeGroundColor { get; set; } = Color.Black;
      public Color MsgTraceFontColor { get; set; } = Color.Black;
      public Color MsgErrorFontColor { get; set; } = Color.OrangeRed;
      public Color MsgWarningFontColor { get; set; } = Color.Orange;
      public Color MsgInfoFontColor { get; set; } = Color.Green;
      public Color GlobalLineSelected { get; set; } = Color.FromArgb(30, Color.Green);
      

      [Browsable(false)]
      public Font TableHeaderFont { get; set; } = new Font("Consolas", 9.2F);
      public Font TableBodyFont { get; set; } = new Font("Consolas", 9.2F);

      public float Column0_LineOffset { get; set; } = 0;
      public float Column1_TimeStamp { get; set; } = 100;

      public string Column1TimeStampFormat { get; set; } = "yy-MM-dd HH:mm:ss.fff";

      public bool DebugUI { get; set; } = false;

      public string LogExportPrefix { get; set; } = "logs-";
      public bool PrefixDate { get; set; } = true;

   }
}
