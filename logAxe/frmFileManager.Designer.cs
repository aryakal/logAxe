namespace logAxe
{
   partial class frmFileManager
   {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
         if (disposing && (components != null))
         {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmFileManager));
         this.lstBoxFileNames = new System.Windows.Forms.ListBox();
         this.label1 = new System.Windows.Forms.Label();
         this.btnExport = new System.Windows.Forms.Button();
         this.progressBar = new System.Windows.Forms.ProgressBar();
         this.saveFileBox = new System.Windows.Forms.SaveFileDialog();
         this.SuspendLayout();
         // 
         // lstBoxFileNames
         // 
         this.lstBoxFileNames.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.lstBoxFileNames.FormattingEnabled = true;
         this.lstBoxFileNames.ItemHeight = 16;
         this.lstBoxFileNames.Location = new System.Drawing.Point(12, 29);
         this.lstBoxFileNames.Name = "lstBoxFileNames";
         this.lstBoxFileNames.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
         this.lstBoxFileNames.Size = new System.Drawing.Size(682, 276);
         this.lstBoxFileNames.TabIndex = 0;
         this.lstBoxFileNames.SelectedIndexChanged += new System.EventHandler(this.lstBoxFileNames_SelectedIndexChanged);
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(12, 6);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(194, 17);
         this.label1.TabIndex = 1;
         this.label1.Text = "Filenames currently in system";
         // 
         // btnExport
         // 
         this.btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnExport.Location = new System.Drawing.Point(520, 316);
         this.btnExport.Name = "btnExport";
         this.btnExport.Size = new System.Drawing.Size(174, 34);
         this.btnExport.TabIndex = 2;
         this.btnExport.Text = "Export File";
         this.btnExport.UseVisualStyleBackColor = true;
         this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
         // 
         // progressBar
         // 
         this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.progressBar.Location = new System.Drawing.Point(12, 320);
         this.progressBar.Name = "progressBar";
         this.progressBar.Size = new System.Drawing.Size(502, 10);
         this.progressBar.TabIndex = 3;
         // 
         // frmFileManager
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(700, 355);
         this.Controls.Add(this.progressBar);
         this.Controls.Add(this.btnExport);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.lstBoxFileNames);
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "frmFileManager";
         this.Text = "FileManager";
         this.Shown += new System.EventHandler(this.FileManager_Shown);
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.ListBox lstBoxFileNames;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.Button btnExport;
      private System.Windows.Forms.ProgressBar progressBar;
      private System.Windows.Forms.SaveFileDialog saveFileBox;
   }
}