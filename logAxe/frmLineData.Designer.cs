namespace logAxe
{
    partial class frmLineData
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLineData));
         this.textBox1 = new System.Windows.Forms.TextBox();
         this.btnForward = new System.Windows.Forms.Button();
         this.btnBack = new System.Windows.Forms.Button();
         this.lblLineNo = new System.Windows.Forms.Label();
         this.textLineNo = new System.Windows.Forms.TextBox();
         this.lblLineFrmLineData = new System.Windows.Forms.Label();
         this.tbLineData = new System.Windows.Forms.TabControl();
         this.tabPage1 = new System.Windows.Forms.TabPage();
         this.tbLineData.SuspendLayout();
         this.tabPage1.SuspendLayout();
         this.SuspendLayout();
         // 
         // textBox1
         // 
         this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.textBox1.BackColor = System.Drawing.Color.White;
         this.textBox1.Location = new System.Drawing.Point(6, 6);
         this.textBox1.Multiline = true;
         this.textBox1.Name = "textBox1";
         this.textBox1.ReadOnly = true;
         this.textBox1.Size = new System.Drawing.Size(642, 155);
         this.textBox1.TabIndex = 0;
         this.textBox1.Text = "Time:  2020-01-30 11:30:35.002, ThreadId: 14, ProcId: 1, Category:  categor1 ";
         // 
         // btnForward
         // 
         this.btnForward.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnForward.Location = new System.Drawing.Point(552, 165);
         this.btnForward.Name = "btnForward";
         this.btnForward.Size = new System.Drawing.Size(75, 23);
         this.btnForward.TabIndex = 1;
         this.btnForward.Text = ">>";
         this.btnForward.UseVisualStyleBackColor = true;
         this.btnForward.Click += new System.EventHandler(this.btnForward_Click);
         // 
         // btnBack
         // 
         this.btnBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnBack.Location = new System.Drawing.Point(471, 165);
         this.btnBack.Name = "btnBack";
         this.btnBack.Size = new System.Drawing.Size(75, 23);
         this.btnBack.TabIndex = 2;
         this.btnBack.Text = "<<";
         this.btnBack.UseVisualStyleBackColor = true;
         this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
         // 
         // lblLineNo
         // 
         this.lblLineNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.lblLineNo.AutoSize = true;
         this.lblLineNo.Location = new System.Drawing.Point(142, 165);
         this.lblLineNo.Name = "lblLineNo";
         this.lblLineNo.Size = new System.Drawing.Size(83, 17);
         this.lblLineNo.TabIndex = 3;
         this.lblLineNo.Text = "of TotalLine";
         // 
         // textLineNo
         // 
         this.textLineNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.textLineNo.BorderStyle = System.Windows.Forms.BorderStyle.None;
         this.textLineNo.Location = new System.Drawing.Point(64, 165);
         this.textLineNo.Name = "textLineNo";
         this.textLineNo.Size = new System.Drawing.Size(72, 15);
         this.textLineNo.TabIndex = 4;
         this.textLineNo.Text = "999999999";
         this.textLineNo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
         this.textLineNo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.handle_enter_key);
         // 
         // lblLineFrmLineData
         // 
         this.lblLineFrmLineData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.lblLineFrmLineData.AutoSize = true;
         this.lblLineFrmLineData.Location = new System.Drawing.Point(6, 165);
         this.lblLineFrmLineData.Name = "lblLineFrmLineData";
         this.lblLineFrmLineData.Size = new System.Drawing.Size(57, 17);
         this.lblLineFrmLineData.TabIndex = 8;
         this.lblLineFrmLineData.Text = "Line No";
         // 
         // tbLineData
         // 
         this.tbLineData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.tbLineData.Controls.Add(this.tabPage1);
         this.tbLineData.Location = new System.Drawing.Point(0, 0);
         this.tbLineData.Name = "tbLineData";
         this.tbLineData.SelectedIndex = 0;
         this.tbLineData.Size = new System.Drawing.Size(659, 220);
         this.tbLineData.TabIndex = 9;
         // 
         // tabPage1
         // 
         this.tabPage1.Controls.Add(this.textBox1);
         this.tabPage1.Controls.Add(this.lblLineFrmLineData);
         this.tabPage1.Controls.Add(this.btnForward);
         this.tabPage1.Controls.Add(this.textLineNo);
         this.tabPage1.Controls.Add(this.btnBack);
         this.tabPage1.Controls.Add(this.lblLineNo);
         this.tabPage1.Location = new System.Drawing.Point(4, 25);
         this.tabPage1.Name = "tabPage1";
         this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
         this.tabPage1.Size = new System.Drawing.Size(651, 191);
         this.tabPage1.TabIndex = 0;
         this.tabPage1.Text = "LineData";
         this.tabPage1.UseVisualStyleBackColor = true;
         // 
         // frmLineData
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.BackColor = System.Drawing.Color.White;
         this.ClientSize = new System.Drawing.Size(660, 221);
         this.Controls.Add(this.tbLineData);
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "frmLineData";
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "frmLineData";
         this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmLineData_FormClosing);
         this.Shown += new System.EventHandler(this.frmLineData_Shown);
         this.tbLineData.ResumeLayout(false);
         this.tabPage1.ResumeLayout(false);
         this.tabPage1.PerformLayout();
         this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnForward;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Label lblLineNo;
        private System.Windows.Forms.TextBox textLineNo;
        private System.Windows.Forms.Label lblLineFrmLineData;
      private System.Windows.Forms.TabControl tbLineData;
      private System.Windows.Forms.TabPage tabPage1;
   }
}