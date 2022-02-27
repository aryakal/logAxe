//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace logAxe
{
   public partial class frmNotepad : Form
   {
      public frmNotepad()
      {
         InitializeComponent();         
         Icon = Properties.Resources.axe_icon_svg_128;
      }

      public void SetTitle(string text)
      {
         Text = $"logAxe - {text}";
      }

      public string WindowTitle { get; set; } = "logAxe - <version> - ";
      private string _name = "";      
      public string NotepadName { 
         get { 
            return _name; 
         } 
         set { 
            _name = value;            
            cntrlTextViewer1.NotepadName = _name;
         } 
      }

      private void frmNotepad_FormClosing(object sender, FormClosingEventArgs e)
      {
         ViewCommon.CloseNotepad(NotepadName);
      }
   }
}
