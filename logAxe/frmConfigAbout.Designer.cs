namespace logAxe
{
   partial class frmConfigAbout
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmConfigAbout));
         this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
         this.tabControl1 = new System.Windows.Forms.TabControl();
         this.tabPage1 = new System.Windows.Forms.TabPage();
         this.tabPage2 = new System.Windows.Forms.TabPage();
         this.txtAbout = new System.Windows.Forms.TextBox();
         this.tabControl1.SuspendLayout();
         this.tabPage1.SuspendLayout();
         this.tabPage2.SuspendLayout();
         this.SuspendLayout();
         // 
         // propertyGrid1
         // 
         this.propertyGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
         | System.Windows.Forms.AnchorStyles.Left)
         | System.Windows.Forms.AnchorStyles.Right)));
         this.propertyGrid1.Location = new System.Drawing.Point(6, 3);
         this.propertyGrid1.Name = "propertyGrid1";
         this.propertyGrid1.Size = new System.Drawing.Size(448, 538);
         this.propertyGrid1.TabIndex = 0;
         this.propertyGrid1.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid1_PropertyValueChanged);
         // 
         // tabControl1
         // 
         this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
         | System.Windows.Forms.AnchorStyles.Left)
         | System.Windows.Forms.AnchorStyles.Right)));
         this.tabControl1.Controls.Add(this.tabPage1);
         this.tabControl1.Controls.Add(this.tabPage2);
         this.tabControl1.Location = new System.Drawing.Point(5, 2);
         this.tabControl1.Name = "tabControl1";
         this.tabControl1.SelectedIndex = 0;
         this.tabControl1.Size = new System.Drawing.Size(468, 578);
         this.tabControl1.TabIndex = 1;
         // 
         // tabPage1
         // 
         this.tabPage1.Controls.Add(this.propertyGrid1);
         this.tabPage1.Location = new System.Drawing.Point(4, 25);
         this.tabPage1.Name = "tabPage1";
         this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
         this.tabPage1.Size = new System.Drawing.Size(460, 549);
         this.tabPage1.TabIndex = 0;
         this.tabPage1.Text = "Config";
         this.tabPage1.UseVisualStyleBackColor = true;
         // 
         // tabPage2
         // 
         this.tabPage2.Controls.Add(this.txtAbout);
         this.tabPage2.Location = new System.Drawing.Point(4, 25);
         this.tabPage2.Name = "tabPage2";
         this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
         this.tabPage2.Size = new System.Drawing.Size(460, 549);
         this.tabPage2.TabIndex = 1;
         this.tabPage2.Text = "About";
         this.tabPage2.UseVisualStyleBackColor = true;
         // 
         // txtAbout
         // 
         this.txtAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
         | System.Windows.Forms.AnchorStyles.Left)
         | System.Windows.Forms.AnchorStyles.Right)));
         this.txtAbout.BackColor = System.Drawing.Color.White;
         this.txtAbout.Font = new System.Drawing.Font("Consolas", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.txtAbout.Location = new System.Drawing.Point(6, 6);
         this.txtAbout.Multiline = true;
         this.txtAbout.Name = "txtAbout";
         this.txtAbout.ReadOnly = true;
         this.txtAbout.Size = new System.Drawing.Size(448, 535);
         this.txtAbout.TabIndex = 0;
         // 
         // frmConfigAbout
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(475, 580);
         this.Controls.Add(this.tabControl1);
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "frmConfigAbout";
         this.Text = "Config/About - Dialog";
         this.Load += new System.EventHandler(this.frmConfiguration_Load);
         this.tabControl1.ResumeLayout(false);
         this.tabPage1.ResumeLayout(false);
         this.tabPage2.ResumeLayout(false);
         this.tabPage2.PerformLayout();
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.PropertyGrid propertyGrid1;
      private System.Windows.Forms.TabControl tabControl1;
      private System.Windows.Forms.TabPage tabPage1;
      private System.Windows.Forms.TabPage tabPage2;
      private System.Windows.Forms.TextBox txtAbout;
   }
}