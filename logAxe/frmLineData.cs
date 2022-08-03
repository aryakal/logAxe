//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using logAxeCommon;
using System;
using System.Globalization;
using System.Windows.Forms;

namespace logAxe
{
   public partial class frmLineData : Form
   {
      private int _currentLine = LogLine.INVALID;
      //private string _lasttimeStamp = string.Empty;


      public frmLineData()
      {
         InitializeComponent();
         ChangeFont();
         this.Icon = Properties.Resources.axe_icon_svg_128;
      }

      public void ChangeFont()
      {
         //TODO : we can prefetc the config.
         //if (null != ViewCommon.Engine)
         //{
         //   textBox1.Font = ViewCommon.UserConfig.TableBodyFont;
         //}
      }

      public void Clear()
      {
         textBox1.Text = "";
         lblLineNo.Text = "";
      }

      public void SetCurrentLine(int currentLine, int totalLine, LogLine lineInfo)
      {
         _currentLine = currentLine;
         var timeStamp = lineInfo.TimeStamp.ToString(ViewCommon.DefaultDateTimeFmt);
         Text = $"{timeStamp} [{currentLine}]";
         textBox1.Text =
                 $"Time: {timeStamp}, ThreadId: {lineInfo.ThreadNo}, ProcId: {lineInfo.ProcessId}, Category: {lineInfo.Category} {Environment.NewLine}{Environment.NewLine}" +                 
                 $"{lineInfo.Msg} {Environment.NewLine}{Environment.NewLine}-------------------{Environment.NewLine}" +
                 $"{lineInfo.StackTrace} {Environment.NewLine}";
         textLineNo.Text = _currentLine.ToString();
         lblLineNo.Text = $" of {totalLine}";         
         textBox1.Select(0, 0);
         //txtTimeCntrl.Text = timeStamp;
      }

      public Action<int> MoveLine { get; set; }

      private void btnForward_Click(object sender, EventArgs e)
      {
         MoveLine?.Invoke(1);
      }

      private void btnBack_Click(object sender, EventArgs e)
      {
         MoveLine?.Invoke(-1);
      }

      private void frmLineData_FormClosing(object sender, FormClosingEventArgs e)
      {
         Hide();
         e.Cancel = true;
      }

      private void frmLineData_Shown(object sender, EventArgs e)
      {
         textBox1.Select(0, 0);
      }

      private void handle_enter_key(object sender, KeyPressEventArgs e)
      {
         if (e.KeyChar == (char)Keys.Return)
         {
            try
            {
               var lineNo = Convert.ToInt32(textLineNo.Text);
               MoveLine?.Invoke(lineNo - _currentLine);
            }
            catch
            {
               textLineNo.Text = _currentLine.ToString();
            }
         }
      }

      //private void handle_enter_key_for_time(object sender, KeyPressEventArgs e)
      //{
      //   if (e.KeyChar == (char)Keys.Return)
      //   {
      //      try
      //      {
      //         DateTime.ParseExact(txtTimeTravel.Text, ViewCommon.DefaultDateTimeFmt, CultureInfo.InvariantCulture);
      //      }
      //      catch
      //      {
      //         txtTimeTravel.Text = _lasttimeStamp;
      //      }
      //   }
      //}

      //private void lblCopyTime_Click(object sender, EventArgs e)
      //{

      //}
   }

}
