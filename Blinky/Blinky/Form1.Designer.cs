namespace Blinky
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.buttonTransmitTimestamp = new System.Windows.Forms.Button();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.buttonTransmitValue = new System.Windows.Forms.Button();
            this.textBoxTxValue = new System.Windows.Forms.TextBox();
            this.buttonTransmitDate = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(13, 126);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(363, 264);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // buttonTransmitTimestamp
            // 
            this.buttonTransmitTimestamp.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.buttonTransmitTimestamp.Location = new System.Drawing.Point(13, 12);
            this.buttonTransmitTimestamp.Name = "buttonTransmitTimestamp";
            this.buttonTransmitTimestamp.Size = new System.Drawing.Size(156, 23);
            this.buttonTransmitTimestamp.TabIndex = 1;
            this.buttonTransmitTimestamp.Text = "Transmit timestamp";
            this.buttonTransmitTimestamp.UseVisualStyleBackColor = false;
            this.buttonTransmitTimestamp.Click += new System.EventHandler(this.buttonTransmitTimestamp_Click);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // buttonTransmitValue
            // 
            this.buttonTransmitValue.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.buttonTransmitValue.Location = new System.Drawing.Point(12, 70);
            this.buttonTransmitValue.Name = "buttonTransmitValue";
            this.buttonTransmitValue.Size = new System.Drawing.Size(156, 23);
            this.buttonTransmitValue.TabIndex = 3;
            this.buttonTransmitValue.Text = "Transmit value";
            this.buttonTransmitValue.UseVisualStyleBackColor = false;
            this.buttonTransmitValue.Click += new System.EventHandler(this.buttonTransmitValue_Click);
            // 
            // textBoxTxValue
            // 
            this.textBoxTxValue.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.textBoxTxValue.Location = new System.Drawing.Point(174, 72);
            this.textBoxTxValue.Name = "textBoxTxValue";
            this.textBoxTxValue.Size = new System.Drawing.Size(101, 20);
            this.textBoxTxValue.TabIndex = 4;
            this.textBoxTxValue.Text = "0x12345678";
            // 
            // buttonTransmitDate
            // 
            this.buttonTransmitDate.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.buttonTransmitDate.Location = new System.Drawing.Point(13, 41);
            this.buttonTransmitDate.Name = "buttonTransmitDate";
            this.buttonTransmitDate.Size = new System.Drawing.Size(156, 23);
            this.buttonTransmitDate.TabIndex = 5;
            this.buttonTransmitDate.Text = "Transmit date";
            this.buttonTransmitDate.UseVisualStyleBackColor = false;
            this.buttonTransmitDate.Click += new System.EventHandler(this.buttonTransmitDate_Click);
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.textBox1.Location = new System.Drawing.Point(13, 396);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(131, 20);
            this.textBox1.TabIndex = 6;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.checkBox1.Location = new System.Drawing.Point(176, 17);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(53, 17);
            this.checkBox1.TabIndex = 7;
            this.checkBox1.Text = "Exact";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(388, 427);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.buttonTransmitDate);
            this.Controls.Add(this.textBoxTxValue);
            this.Controls.Add(this.buttonTransmitValue);
            this.Controls.Add(this.buttonTransmitTimestamp);
            this.Controls.Add(this.pictureBox1);
            this.Name = "Form1";
            this.Text = "Blinky";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button buttonTransmitTimestamp;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Button buttonTransmitValue;
        private System.Windows.Forms.TextBox textBoxTxValue;
        private System.Windows.Forms.Button buttonTransmitDate;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.CheckBox checkBox1;
    }
}

