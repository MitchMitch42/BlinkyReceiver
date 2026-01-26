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
            this.label1 = new System.Windows.Forms.Label();
            this.buttonTransmitValue = new System.Windows.Forms.Button();
            this.textBoxTxValue = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(13, 88);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(363, 264);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // buttonTransmitTimestamp
            // 
            this.buttonTransmitTimestamp.Location = new System.Drawing.Point(13, 12);
            this.buttonTransmitTimestamp.Name = "buttonTransmitTimestamp";
            this.buttonTransmitTimestamp.Size = new System.Drawing.Size(156, 23);
            this.buttonTransmitTimestamp.TabIndex = 1;
            this.buttonTransmitTimestamp.Text = "Transmit timestamp";
            this.buttonTransmitTimestamp.UseVisualStyleBackColor = true;
            this.buttonTransmitTimestamp.Click += new System.EventHandler(this.buttonTransmitTimestamp_Click);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(175, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(24, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "0x0";
            // 
            // buttonTransmitValue
            // 
            this.buttonTransmitValue.Location = new System.Drawing.Point(13, 41);
            this.buttonTransmitValue.Name = "buttonTransmitValue";
            this.buttonTransmitValue.Size = new System.Drawing.Size(156, 23);
            this.buttonTransmitValue.TabIndex = 3;
            this.buttonTransmitValue.Text = "Transmit value";
            this.buttonTransmitValue.UseVisualStyleBackColor = true;
            this.buttonTransmitValue.Click += new System.EventHandler(this.buttonTransmitValue_Click);
            // 
            // textBoxTxValue
            // 
            this.textBoxTxValue.Location = new System.Drawing.Point(175, 43);
            this.textBoxTxValue.Name = "textBoxTxValue";
            this.textBoxTxValue.Size = new System.Drawing.Size(101, 20);
            this.textBoxTxValue.TabIndex = 4;
            this.textBoxTxValue.Text = "0x12345678";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(388, 361);
            this.Controls.Add(this.textBoxTxValue);
            this.Controls.Add(this.buttonTransmitValue);
            this.Controls.Add(this.label1);
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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonTransmitValue;
        private System.Windows.Forms.TextBox textBoxTxValue;
    }
}

