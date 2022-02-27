//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using logAxeEngine.Common;
using logAxeEngine.Interfaces;

namespace logAxe
{
   public partial class frmFileManager : Form
   {
      public frmFileManager()
      {
         InitializeComponent();
         progressBar.Visible = false;
         btnExport.Enabled = false;
         Icon = Properties.Resources.axe_icon_svg_128;
      }

      public int[] InitialSelectedFileNo { get; set; } = new int[0];

      public void RefreshList()
      {
         lstBoxFileNames.Items.Clear();

         if (ViewCommon.Engine != null)
         {
            lstBoxFileNames.DisplayMember = "DisplayName";
            lstBoxFileNames.ValueMember = "Key";
            lstBoxFileNames.Items.AddRange(ViewCommon.Engine.GetAllFileNames());
            if (InitialSelectedFileNo.Length != 0)
            {
               for (int ndx = 0; ndx < lstBoxFileNames.Items.Count; ndx++)
               {
                  if (InitialSelectedFileNo.Contains(((LogFileInfo)lstBoxFileNames.Items[ndx]).FileNo))
                  {
                     lstBoxFileNames.SetSelected(ndx, true);
                  }
               }
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

         string preFixDate = ViewCommon.UserConfig.PrefixDate ? $"{DateTime.Now:yyyyMMddHHmmss}" : "";
         saveFileBox.FileName = $"{ViewCommon.UserConfig.LogExportPrefix}{preFixDate}.zip";

         if (saveFileBox.ShowDialog() == DialogResult.OK)
         {
            progressBar.Visible = true;
            progressBar.Maximum = lst.Count;
            ViewCommon.Engine.ExportFiles(lst.ToArray(), saveFileBox.FileName);
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
            ViewCommon.Engine.AddFiles(openFileDialog.FileNames.ToArray(), processAsync: true);
         }
      }
   }
}
