using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Blinky
{
    public partial class Form1 : Form
    {
        private Image img_white = Image.FromFile("white.jpg");
        private Image img_black = Image.FromFile("black.jpg");
        
        int Frequency = 8;
        bool HighRes = Stopwatch.IsHighResolution; //more precise transmission using ticks instead of ms
        UInt32 Data;

        public Form1()
        {
            InitializeComponent();

            HighRes = false; //TODO: high res is currently not working stable

            if (HighRes)
                pictureBox1.BackColor = Color.Black;
            else
                pictureBox1.Image = img_black;
        }
        
        private void buttonTransmitValue_Click(object sender, EventArgs e)
        {
            buttonTransmitTimestamp.Enabled = false;
            buttonTransmitValue.Enabled = false;
            Data = UInt32.Parse(textBoxTxValue.Text.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
            backgroundWorker1.RunWorkerAsync();
        }

        private void buttonTransmitTimestamp_Click(object sender, EventArgs e)
        {
            buttonTransmitTimestamp.Enabled = false;
            buttonTransmitValue.Enabled = false;
            Data = CreateTimeData();
            backgroundWorker1.RunWorkerAsync();
        }
        
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            buttonTransmitTimestamp.Enabled = true;
            buttonTransmitValue.Enabled = true;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            label1.Invoke(new MethodInvoker(delegate ()
            {
                label1.Text = "0x" + Data.ToString("X");
            }));
            
            if (HighRes)
            {
                double ticksPerMicrosecond = (double)Stopwatch.Frequency / 1_000_000;
                double microseconds = 1000.0 * 1000.0 / (float)Frequency;
                long intervalTicks = (long)(microseconds * ticksPerMicrosecond);

                HighResTransmission(Data, intervalTicks);
            }
            else
            {
                LowResTransmission(Data, 1000.0F / Frequency);
            }
        }
        
        /// <summary>
        /// Flicker the screen using pictureBox1.BackColor with a high resolution (ticks)
        /// </summary>
        private void HighResTransmission(UInt32 data, long intervalTicks)
        {
            long nextTick = Stopwatch.GetTimestamp();

            for (int i = -1; i < 33; i++)
            {
                // Busy-wait until the next microsecond interval is reached
                while (Stopwatch.GetTimestamp() < nextTick)
                {
                    // Optional: Thread.SpinWait(1) to reduce power consumption slightly
                }

                if (i == -1)
                {
                    //rising edge to start transmission
                    Show(Color.White);
                }
                else if(i == 32)
                {
                    //back to black at end of transmission
                    Show(Color.Black);
                }
                else
                {
                    //transmit data bit by bit
                    if (((data << i) & 0x80000000) == 0)
                        Show(Color.Black);
                    else
                        Show(Color.White);
                }

                nextTick += intervalTicks; // Schedule next execution
            }
        }


        /// <summary>
        /// Flicker the screen using pictureBox1.Image with a low resolution (ms)
        /// </summary>
        private void LowResTransmission(UInt32 data, float interval_ms)
        {
            float nextTimestamp = 0;
            int index = 0;
            bool start = true;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (true)
            {
                if (sw.ElapsedMilliseconds > nextTimestamp)
                {
                    if (start)
                    {
                        pictureBox1.Image = img_white;
                        start = false;
                        nextTimestamp += interval_ms / 2;
                    }
                    else
                    {
                        if (((data << index) & 0x80000000) == 0)
                            pictureBox1.Image = img_black;
                        else
                            pictureBox1.Image = img_white;
                        
                        index++;
                        if (index == 32)
                            break;

                        nextTimestamp += interval_ms;
                    }
                }
            }

            System.Threading.Thread.Sleep((int)interval_ms); //wait for last bit to be received

            pictureBox1.Image = img_black;
        }

        private void Show(Color color)
        {
            pictureBox1.Invoke(new MethodInvoker(delegate ()
            {
                pictureBox1.BackColor = color;
            }));
        }

        /// <summary>
        /// Create 32 bit containing timestamp
        /// 4 Bit Header (0x1)
        /// 6 Bit Seconds
        /// 6 Bit Minutes
        /// 5 Bit Hours
        /// 3 Bit Unused
        /// 8 Bit Checksum
        /// </summary>
        private UInt32 CreateTimeData()
        {
            DateTime dt = DateTime.Now;
            int header = 1;
            int sec = dt.Second;
            int min = dt.Minute;
            int hour = dt.Hour;

            UInt32 data = (UInt32)((header << 28) + (sec << 22) + (min << 16) + (hour << 11));
            data += CalculateChecksum(data);
            return data;
        }

        /// <summary>
        /// Create 32 bit containing current date
        /// 4 Bit Header (0x2)
        /// 5 Bit Day
        /// 4 Bit Month
        /// 6 Bit Year (current year - 2020)
        /// 5 Bit Unused
        /// 8 Bit Checksum
        /// </summary>
        private UInt32 CreateDateData()
        {
            DateTime dt = DateTime.Now;
            int header = 2;
            int day = dt.Day;
            int month = dt.Month;
            int year = dt.Year - 2020;

            UInt32 data = (UInt32)((header << 28) + (day << 23) + (month << 19) + (year << 13));
            data += CalculateChecksum(data);
            return data;
        }

        /// <summary>
        /// calculate checksum from first 3 byte in data
        /// </summary>
        private byte CalculateChecksum(UInt32 data)
        {
            UInt32 checksum = (data >> 24 & 0xFF) + (data >> 16 & 0xFF) + (data >> 8 & 0xFF);
            return (byte)(checksum & 0xFF);
        }
    }
}
