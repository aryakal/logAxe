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
            this.txtTimeTravel = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.BackColor = System.Drawing.Color.White;
            this.textBox1.Location = new System.Drawing.Point(0, 1);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(636, 179);
            this.textBox1.TabIndex = 0;
            this.textBox1.Text = "Time: 2020-01-30 11:30:35.002, ThreadId: 14, ProcId: 1, Category:  categor1 ";
            // 
            // btnForward
            // 
            this.btnForward.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnForward.Location = new System.Drawing.Point(549, 186);
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
            this.btnBack.Location = new System.Drawing.Point(468, 186);
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
            this.lblLineNo.Location = new System.Drawing.Point(125, 188);
            this.lblLineNo.Name = "lblLineNo";
            this.lblLineNo.Size = new System.Drawing.Size(83, 17);
            this.lblLineNo.TabIndex = 3;
            this.lblLineNo.Text = "of TotalLine";
            // 
            // textLineNo
            // 
            this.textLineNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textLineNo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textLineNo.Location = new System.Drawing.Point(49, 190);
            this.textLineNo.Name = "textLineNo";
            this.textLineNo.Size = new System.Drawing.Size(72, 15);
            this.textLineNo.TabIndex = 4;
            this.textLineNo.Text = "999999999";
            this.textLineNo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textLineNo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.handle_enter_key);
            // 
            // txtTimeTravel
            // 
            this.txtTimeTravel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTimeTravel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtTimeTravel.Location = new System.Drawing.Point(286, 190);
            this.txtTimeTravel.Name = "txtTimeTravel";
            this.txtTimeTravel.Size = new System.Drawing.Size(147, 15);
            this.txtTimeTravel.TabIndex = 6;
            this.txtTimeTravel.Text = "2020-10-11 21:20:31.445";
            this.txtTimeTravel.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtTimeTravel.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.handle_enter_key_for_time);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(241, 188);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 17);
            this.label1.TabIndex = 7;
            this.label1.Text = "Time";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 189);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 17);
            this.label2.TabIndex = 8;
            this.label2.Text = "Lino";
            // 
            // frmLineData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(636, 212);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtTimeTravel);
            this.Controls.Add(this.textLineNo);
            this.Controls.Add(this.lblLineNo);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.btnForward);
            this.Controls.Add(this.textBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmLineData";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "frmLineData";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmLineData_FormClosing);
            this.Shown += new System.EventHandler(this.frmLineData_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnForward;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Label lblLineNo;
        private System.Windows.Forms.TextBox textLineNo;
        private System.Windows.Forms.TextBox txtTimeTravel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}