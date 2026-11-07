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
            this.buttonTransmitDateTime = new System.Windows.Forms.Button();
            this.buttonTransmitHeartbeat = new System.Windows.Forms.Button();
            this.buttonTransmitLocation = new System.Windows.Forms.Button();
            this.labelLatitude = new System.Windows.Forms.Label();
            this.textBoxLatitude = new System.Windows.Forms.TextBox();
            this.labelLongitude = new System.Windows.Forms.Label();
            this.textBoxLongitude = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.buttonTest = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(12, 246);
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
            // buttonTransmitDateTime
            // 
            this.buttonTransmitDateTime.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.buttonTransmitDateTime.Location = new System.Drawing.Point(13, 99);
            this.buttonTransmitDateTime.Name = "buttonTransmitDateTime";
            this.buttonTransmitDateTime.Size = new System.Drawing.Size(155, 23);
            this.buttonTransmitDateTime.TabIndex = 8;
            this.buttonTransmitDateTime.Text = "Date/time MP";
            this.buttonTransmitDateTime.UseVisualStyleBackColor = false;
            this.buttonTransmitDateTime.Click += new System.EventHandler(this.buttonTransmitDateTime_Click);
            // 
            // buttonTransmitHeartbeat
            // 
            this.buttonTransmitHeartbeat.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.buttonTransmitHeartbeat.Location = new System.Drawing.Point(13, 128);
            this.buttonTransmitHeartbeat.Name = "buttonTransmitHeartbeat";
            this.buttonTransmitHeartbeat.Size = new System.Drawing.Size(155, 23);
            this.buttonTransmitHeartbeat.TabIndex = 7;
            this.buttonTransmitHeartbeat.Text = "Heartbeat";
            this.buttonTransmitHeartbeat.UseVisualStyleBackColor = false;
            this.buttonTransmitHeartbeat.Click += new System.EventHandler(this.buttonTransmitHeartbeat_Click);
            // 
            // buttonTransmitLocation
            // 
            this.buttonTransmitLocation.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.buttonTransmitLocation.Location = new System.Drawing.Point(13, 157);
            this.buttonTransmitLocation.Name = "buttonTransmitLocation";
            this.buttonTransmitLocation.Size = new System.Drawing.Size(155, 23);
            this.buttonTransmitLocation.TabIndex = 10;
            this.buttonTransmitLocation.Text = "Transmit location";
            this.buttonTransmitLocation.UseVisualStyleBackColor = false;
            this.buttonTransmitLocation.Click += new System.EventHandler(this.buttonTransmitLocation_Click);
            // 
            // labelLatitude
            // 
            this.labelLatitude.AutoSize = true;
            this.labelLatitude.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.labelLatitude.Location = new System.Drawing.Point(174, 102);
            this.labelLatitude.Name = "labelLatitude";
            this.labelLatitude.Size = new System.Drawing.Size(45, 13);
            this.labelLatitude.TabIndex = 11;
            this.labelLatitude.Text = "Latitude";
            // 
            // textBoxLatitude
            // 
            this.textBoxLatitude.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.textBoxLatitude.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.textBoxLatitude.Location = new System.Drawing.Point(232, 99);
            this.textBoxLatitude.Name = "textBoxLatitude";
            this.textBoxLatitude.Size = new System.Drawing.Size(143, 20);
            this.textBoxLatitude.TabIndex = 12;
            // 
            // labelLongitude
            // 
            this.labelLongitude.AutoSize = true;
            this.labelLongitude.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.labelLongitude.Location = new System.Drawing.Point(174, 131);
            this.labelLongitude.Name = "labelLongitude";
            this.labelLongitude.Size = new System.Drawing.Size(54, 13);
            this.labelLongitude.TabIndex = 13;
            this.labelLongitude.Text = "Longitude";
            // 
            // textBoxLongitude
            // 
            this.textBoxLongitude.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.textBoxLongitude.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.textBoxLongitude.Location = new System.Drawing.Point(232, 128);
            this.textBoxLongitude.Name = "textBoxLongitude";
            this.textBoxLongitude.Size = new System.Drawing.Size(143, 20);
            this.textBoxLongitude.TabIndex = 14;
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.textBox1.Location = new System.Drawing.Point(12, 516);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(131, 20);
            this.textBox1.TabIndex = 6;
            // 
            // buttonTest
            // 
            this.buttonTest.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.buttonTest.Location = new System.Drawing.Point(288, 217);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(87, 23);
            this.buttonTest.TabIndex = 9;
            this.buttonTest.Text = "Test";
            this.buttonTest.UseVisualStyleBackColor = false;
            this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(388, 545);
            this.Controls.Add(this.buttonTest);
            this.Controls.Add(this.textBoxLongitude);
            this.Controls.Add(this.labelLongitude);
            this.Controls.Add(this.textBoxLatitude);
            this.Controls.Add(this.labelLatitude);
            this.Controls.Add(this.buttonTransmitLocation);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.buttonTransmitDateTime);
            this.Controls.Add(this.buttonTransmitHeartbeat);
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
        private System.Windows.Forms.Button buttonTransmitDateTime;
        private System.Windows.Forms.Button buttonTransmitHeartbeat;
        private System.Windows.Forms.Button buttonTransmitLocation;
        private System.Windows.Forms.Label labelLatitude;
        private System.Windows.Forms.TextBox textBoxLatitude;
        private System.Windows.Forms.Label labelLongitude;
        private System.Windows.Forms.TextBox textBoxLongitude;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button buttonTest;
    }
}
