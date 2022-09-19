//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Windows.Forms;

namespace logAxe
{
   public partial class frmMainWindow : Form
   {
      public frmMainWindow()
      {
         InitializeComponent();      
         if (ViewCommon.Channel != null)
         {
            
            new HelperAttachFileDrop(this);
            SetTitle();
         }
         this.Icon = Properties.Resources.axe_icon_svg_128;         
         cntrlTextViewer1.OnNewNotepadChange += TitleChangeEvent;
      }

      /// <summary>
      /// This id is used to identify the primary window.
      /// </summary>
      public string FrmID { get; set; } = "Main";
      public string WindowTitle { get; set; } = "logAxe - <version> - ";

      void TitleChangeEvent(CntrlTextViewerMsg msg)
      {
         this.Invoke(new Action(() =>
         {
            switch (msg)
            {
               case CntrlTextViewerMsg.SetTitle:
                  SetTitle();
                  break;
               case CntrlTextViewerMsg.AwakeWindows:
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
            else {
               cntrlTextViewer1.UnRegister();
            }
         }
         else
         {
            cntrlTextViewer1.UnRegister();
            ViewCommon.CloseForm(FrmID);
         }
      }
      private bool IsMainView()
      {
         return "Main" == FrmID;
      }
      private void frmMainWindow_Load(object sender, EventArgs e)
      {
         if (IsMainView()) {
            cntrlTextViewer1.EnableFileAppInfo();
         }
         
      }
      public void Register(bool isMainView) {
         cntrlTextViewer1.Register(isMainView: isMainView);
      }
   }
}
