//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System.Windows.Forms;

using logAxeCommon;
using libACommunication;

namespace logAxe
{
   public sealed class HelperAttachFileDrop
   {
      public HelperAttachFileDrop(Control cntrl)
      {
         cntrl.AllowDrop = true;
         cntrl.DragDrop += OnDragDrop;
         cntrl.DragEnter += OnDragEnger;
      }
      private void OnDragEnger(object sender, DragEventArgs e)
      {
         //https://docs.microsoft.com/en-us/dotnet/desktop/winforms/advanced/walkthrough-performing-a-drag-and-drop-operation-in-windows-forms?view=netframeworkdesktop-4.8
         if (e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effect = DragDropEffects.Copy;
         else
            e.Effect = DragDropEffects.None;
      }
      private void OnDragDrop(object sender, DragEventArgs e)
      {
         string[] fileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
         ViewCommon.Channel.SendMsg(new UnitMsg(opCode: WebFrameWork.CMD_PUT_FILES, name: WebFrameWork.CMD_PUT_ALL_VIEW_UPDATE, value: new UnitCmdAddDiskFiles() { FilePaths = fileList }));
      }
   }
}
