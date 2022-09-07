namespace logAxe
{
   partial class frmMainWindow
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMainWindow));
         this.cntrlTextViewer1 = new logAxe.CntrlTextViewer();
         this.SuspendLayout();
         // 
         // cntrlTextViewer1
         // 
         this.cntrlTextViewer1.AllowDrop = true;
         this.cntrlTextViewer1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
         this.cntrlTextViewer1.BackColor = System.Drawing.Color.White;
         this.cntrlTextViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.cntrlTextViewer1.IsViewNotepad = false;
         this.cntrlTextViewer1.Location = new System.Drawing.Point(0, 0);
         this.cntrlTextViewer1.Name = "cntrlTextViewer1";
         this.cntrlTextViewer1.NotepadName = null;
         this.cntrlTextViewer1.OnNewNotepadChange = null;
         this.cntrlTextViewer1.Size = new System.Drawing.Size(1327, 456);
         this.cntrlTextViewer1.TabIndex = 0;
         // 
         // frmMainWindow
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
         this.ClientSize = new System.Drawing.Size(1327, 456);
         this.Controls.Add(this.cntrlTextViewer1);
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.Name = "frmMainWindow";
         this.Text = "logAxe 0.1";
         this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMainWindow_FormClosing);
         this.Load += new System.EventHandler(this.frmMainWindow_Load);
         this.ResumeLayout(false);

      }


      #endregion

      private CntrlTextViewer cntrlTextViewer1;
   }
}

