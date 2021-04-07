namespace logAxe
{
   partial class frmNotepad
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
         this.cntrlTextViewer1 = new logAxe.CntrlTextViewer();
         this.SuspendLayout();
         // 
         // cntrlTextViewer1
         // 
         this.cntrlTextViewer1.AllowDrop = true;
         this.cntrlTextViewer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.cntrlTextViewer1.BackColor = System.Drawing.Color.White;
         this.cntrlTextViewer1.IsViewNotepad = true;
         this.cntrlTextViewer1.Location = new System.Drawing.Point(1, 0);
         this.cntrlTextViewer1.Name = "cntrlTextViewer1";
         this.cntrlTextViewer1.Size = new System.Drawing.Size(1432, 552);
         this.cntrlTextViewer1.TabIndex = 0;
         // 
         // frmNotepad
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(1432, 553);
         this.Controls.Add(this.cntrlTextViewer1);
         this.Name = "frmNotepad";
         this.Text = "logAxe - Notepad";
         this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmNotepad_FormClosing);
         this.ResumeLayout(false);

      }

      #endregion

      private CntrlTextViewer cntrlTextViewer1;
   }
}