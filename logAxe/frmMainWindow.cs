//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

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

      /// <summary>
      /// This id is used to identify the primary window.
      /// </summary>
      public string FrmID { get; set; } = "Main";
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
         if(IsMainView())
            Text = $"logAxe - {ViewCommon.VersionNo} { (cntrlTextViewer1.FilterMessage== "" ? "": " - "+cntrlTextViewer1.FilterMessage)}";
         else
            Text = $"logAxe - {ViewCommon.VersionNo} - [{FrmID}] { (cntrlTextViewer1.FilterMessage == "" ? "" : " - " + cntrlTextViewer1.FilterMessage)}";
      }

      private void frmMainWindow_FormClosing(object sender, FormClosingEventArgs e)
      {

         if (IsMainView())
         {
            if (MessageBox.Show("Do you want to close the application ?", "About to close !", MessageBoxButtons.YesNo) == DialogResult.No)
            {
               e.Cancel = true;
            }
         }
         else
         {
            ViewCommon.CloseForm(FrmID);
         }
      }

      private bool IsMainView()
      {
         return "Main" == FrmID;
      }
   }
}
