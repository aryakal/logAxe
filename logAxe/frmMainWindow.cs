using System.Windows.Forms;

namespace logAxe
{
   public partial class frmMainWindow : Form
   {
      public frmMainWindow()
      {
         InitializeComponent();
         if (ViewCommon.Engine != null)
         {
            new HelperAttachFileDrop(this);
            Text = $"logAxe - {ViewCommon.VersionNo} - {cntrlTextViewer1.UniqueId}";
         }
         this.Icon = Properties.Resources.axe_icon_svg_128;

      }

      public int UniqueFrmId { get; set; } = 1;
      public string WindowTitle { get; set; } = "logAxe - <version> - ";
      private void frmMainWindow_FormClosing(object sender, FormClosingEventArgs e)
      {
         if (1 == UniqueFrmId)
         {
            if (MessageBox.Show("Do you want to close the application ?", "Really !", MessageBoxButtons.YesNo) == DialogResult.No)
            {
               e.Cancel = true;
            }
         }
         else
         {
            ViewCommon.CloseForm(UniqueFrmId);
         }
      }
   }
}
