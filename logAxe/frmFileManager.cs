//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

using logAxeCommon;

namespace logAxe
{
   public partial class frmFileManager : Form
   {
      public LogFileInfo[] FileInfo { get; set; } = new LogFileInfo[0];
      public int[] InitialSelectedFileNo { get; set; } = new int[0];

      public frmFileManager()
      {
         InitializeComponent();
         progressBar.Visible = false;
         btnExport.Enabled = false;
         Icon = Properties.Resources.axe_icon_svg_128;
      }
      public void RefreshList()
      {
         lstBoxFileNames.Items.Clear();
         lstBoxFileNames.DisplayMember = "DisplayName";
         lstBoxFileNames.ValueMember = "Key";
         lstBoxFileNames.Items.AddRange(FileInfo);

         if (InitialSelectedFileNo.Length == 0)
            return;
         
         for (int ndx = 0; ndx < lstBoxFileNames.Items.Count; ndx++)
         {
            if (InitialSelectedFileNo.Contains(((LogFileInfo)lstBoxFileNames.Items[ndx]).FileNo))
            {
               lstBoxFileNames.SetSelected(ndx, true);
            }
         }
      }

      private void btnExport_Click(object sender, EventArgs e)
      {
         if (lstBoxFileNames.SelectedItems.Count == 0) return;

         var lst = new List<LogFileInfo>();
         {
            foreach (var item in lstBoxFileNames.SelectedItems)
            {
               lst.Add((LogFileInfo)item);
            }
         }
         
         string preFixDate = ViewCommon.ConfigOfSystem.PrefixDate ? $"{DateTime.Now:yyyyMMddHHmmss}" : "";
         saveFileBox.FileName = $"{ViewCommon.ConfigOfSystem.LogExportPrefix}{preFixDate}.zip";

         if (saveFileBox.ShowDialog() == DialogResult.OK)
         {
            //progressBar.Visible = true;
            //progressBar.Maximum = lst.Count;
            //TODO : now we need to send the index of the files only !            
            ViewCommon.ExportFiles(WebFrameWork.CMD_PUT_ALL_VIEW_UPDATE,  lst.ToArray(), saveFileBox.FileName);
         }

      }

      private void lstBoxFileNames_SelectedIndexChanged(object sender, EventArgs e)
      {
         btnExport.Enabled = lstBoxFileNames.SelectedItems.Count != 0;
      }

      private void FileManager_Shown(object sender, EventArgs e)
      {
         RefreshList();
      }

      private void btnImportFile_Click(object sender, EventArgs e)
      {
         
         if (openFileDialog.ShowDialog() == DialogResult.OK)
         {
            ViewCommon.AddFiles(WebFrameWork.CMD_PUT_ALL_VIEW_UPDATE, openFileDialog.FileNames.ToArray());
         }
      }
   }
}
