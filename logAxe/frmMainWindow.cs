using logAxeEngine.Interfaces;
using System;
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
            SetTitle();
         }
         this.Icon = Properties.Resources.axe_icon_svg_128;
         cntrlTextViewer1.SetMasterView();
         cntrlTextViewer1.OnNewNotepadChange += TitleChangeEvent;
      }

      public int UniqueFrmId { get; set; } = 1;
      public string WindowTitle { get; set; } = "logAxe - <version> - ";

      void TitleChangeEvent(ILogAxeMessage message)
      {
         this.Invoke(new Action(() =>
         {
            switch (message.MessageType)
            {
               case LogAxeMessageEnum.NewMainFrmAddRemoved:

                  SetTitle();

                  break;
               case LogAxeMessageEnum.AwakeAllWindows:
                  WindowState = WindowState == FormWindowState.Minimized ? FormWindowState.Normal : FormWindowState.Minimized;
                  break;
            }
         }));

      }

      public void SetTitle()
      {

         Text = $"logAxe - {ViewCommon.VersionNo} - [{UniqueFrmId} of {ViewCommon.NewWindowCount}] { (cntrlTextViewer1.FilterMessage== "" ? "": " - "+cntrlTextViewer1.FilterMessage)}";

      }

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
