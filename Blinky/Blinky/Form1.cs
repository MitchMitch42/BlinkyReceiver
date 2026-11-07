using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Blinky
{
    public partial class Form1 : Form
    {
        double Frequency = 128.0 / 6.0;
        List<UInt32> Data;
        DateTime StartAt;

        public Form1()
        {
            InitializeComponent();      
            pictureBox1.BackColor = Color.Black;
        }
        
        private void buttonTransmitValue_Click(object sender, EventArgs e)
        {
            buttonTransmitTimestamp.Enabled = false;
            buttonTransmitDate.Enabled = false;
            buttonTransmitHeartbeat.Enabled = false;
            buttonTransmitDateTime.Enabled = false;
            buttonTransmitLocation.Enabled = false;
            buttonTransmitValue.Enabled = false;
            Data = new List<UInt32> { UInt32.Parse(textBoxTxValue.Text.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber) };
            StartAt = DateTime.Now;
            backgroundWorker1.RunWorkerAsync();
        }

        private void buttonTransmitTimestamp_Click(object sender, EventArgs e)
        {
            buttonTransmitTimestamp.Enabled = false;
            buttonTransmitDate.Enabled = false;
            buttonTransmitHeartbeat.Enabled = false;
            buttonTransmitDateTime.Enabled = false;
            buttonTransmitLocation.Enabled = false;
            buttonTransmitValue.Enabled = false;
            Data = new List<UInt32> { CreateTimeData(out StartAt, 1) };
            backgroundWorker1.RunWorkerAsync();
        }

        private void buttonTransmitDate_Click(object sender, EventArgs e)
        {
            buttonTransmitTimestamp.Enabled = false;
            buttonTransmitDate.Enabled = false;
            buttonTransmitHeartbeat.Enabled = false;
            buttonTransmitDateTime.Enabled = false;
            buttonTransmitLocation.Enabled = false;
            buttonTransmitValue.Enabled = false;
            Data = new List<UInt32> { CreateDateData(2) };
            StartAt = DateTime.Now;
            backgroundWorker1.RunWorkerAsync();
        }

        private void buttonTransmitHeartbeat_Click(object sender, EventArgs e)
        {
            buttonTransmitTimestamp.Enabled = false;
            buttonTransmitDate.Enabled = false;
            buttonTransmitHeartbeat.Enabled = false;
            buttonTransmitDateTime.Enabled = false;
            buttonTransmitLocation.Enabled = false;
            buttonTransmitValue.Enabled = false;
            Data = new List<UInt32> { CreateHeartbeatData(out StartAt) };
            backgroundWorker1.RunWorkerAsync();
        }

        private void buttonTransmitDateTime_Click(object sender, EventArgs e)
        {
            buttonTransmitTimestamp.Enabled = false;
            buttonTransmitDate.Enabled = false;
            buttonTransmitHeartbeat.Enabled = false;
            buttonTransmitDateTime.Enabled = false;
            buttonTransmitLocation.Enabled = false;
            buttonTransmitValue.Enabled = false;
            Data = CreateDateTimeData(out StartAt);
            backgroundWorker1.RunWorkerAsync();
        }

        private void buttonTransmitLocation_Click(object sender, EventArgs e)
        {
            decimal latitude;
            decimal longitude;
            if (!TryParseCoordinate(textBoxLatitude.Text, -90m, 90m, out latitude)
                || !TryParseCoordinate(textBoxLongitude.Text, -180m, 180m, out longitude))
            {
                MessageBox.Show(
                    "Enter a latitude from -90 to 90 and a longitude from -180 to 180.",
                    "Invalid location",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            short latitudeValue = (short)decimal.Round(latitude * 100m, 0, MidpointRounding.AwayFromZero);
            short longitudeValue = (short)decimal.Round(longitude * 100m, 0, MidpointRounding.AwayFromZero);

            buttonTransmitTimestamp.Enabled = false;
            buttonTransmitDate.Enabled = false;
            buttonTransmitHeartbeat.Enabled = false;
            buttonTransmitDateTime.Enabled = false;
            buttonTransmitLocation.Enabled = false;
            buttonTransmitValue.Enabled = false;
            Data = new List<UInt32>
            {
                CreateLocationData(latitudeValue, 4, true),
                CreateLocationData(longitudeValue, 5, false)
            };
            StartAt = DateTime.Now;
            backgroundWorker1.RunWorkerAsync();
        }

        private bool TryParseCoordinate(string text, decimal minimum, decimal maximum, out decimal coordinate)
        {
            return decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out coordinate)
                && coordinate >= minimum
                && coordinate <= maximum;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            buttonTransmitTimestamp.Enabled = true;
            buttonTransmitDate.Enabled = true;
            buttonTransmitHeartbeat.Enabled = true;
            buttonTransmitDateTime.Enabled = true;
            buttonTransmitLocation.Enabled = true;
            buttonTransmitValue.Enabled = true;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            textBox1.Invoke(new MethodInvoker(delegate ()
            {
                textBox1.Text = string.Join(" / ", Data.Select(data => "0x" + data.ToString("X")));
            }));
            
            double ticksPerMicrosecond = (double)Stopwatch.Frequency / 1_000_000;
            double microseconds = 1000.0 * 1000.0 / Frequency;
            long intervalTicks = (long)(microseconds * ticksPerMicrosecond);

            foreach (UInt32 data in Data)
            {
                HighResTransmission(data, intervalTicks, StartAt);
                StartAt = DateTime.Now;
            }
        }
        
        /// <summary>
        /// Flicker the screen using pictureBox1.BackColor with a high resolution (ticks)
        /// </summary>
        private void HighResTransmission(UInt32 data, long intervalTicks, DateTime startAt)
        {
            while(DateTime.Now < startAt)
            {

            }


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
                    DateTime dt = DateTime.Now;
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
            
            DateTime ft = DateTime.Now;
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
        /// Packet type 0x1; bit 31 indicates another packet follows
        /// 6 Bit Seconds
        /// 6 Bit Minutes
        /// 5 Bit Hours
        /// 3 Bit Unused
        /// 8 Bit Checksum
        /// </summary>
        private UInt32 CreateTimeData(out DateTime startAt, int header, bool multipacket = false, int bitOffset = 33)
        {
            double ms = (1000.0 / Frequency);
            DateTime dt = DateTime.Now;
            
            startAt = dt.AddMilliseconds(1000 - dt.Millisecond);
            //startAt = dt.AddMilliseconds(2000 - dt.Millisecond - ((bitoffset * 1000.0 / Frequency) % 1000));
            //startAt = startAt.AddMilliseconds(1000 - startAt.Millisecond);


            dt = startAt;
            dt = dt.AddMilliseconds(bitOffset * ms);
            int sec = dt.Second;
            int min = dt.Minute;
            int hour = dt.Hour;

            UInt32 data = (UInt32)((header << 28) + (sec << 22) + (min << 16) + (hour << 11));
            if (multipacket)
                data |= 0x80000000;
            data += CalculateChecksum(data);
            return data;
        }

        private List<UInt32> CreateDateTimeData(out DateTime startAt)
        {
            //return CreateDebugData(out startAt);
            UInt32 dateData = CreateDateData(2, true);
            UInt32 timeData = CreateTimeData(out startAt, 1, false, 33);
            return new List<UInt32> { dateData, timeData };
        }

        private UInt32 CreateHeartbeatData(out DateTime startAt, int bitOffset = 33)
        {
            double ms = (1000.0 / Frequency);
            DateTime dt = DateTime.Now;
            double offsetMs = (dt.Millisecond + bitOffset * ms) % 1000;
            startAt = dt.AddMilliseconds(2000 - offsetMs);

            UInt32 data = 3u << 28;
            data += CalculateChecksum(data);
            return data;
        }

        private UInt32 CreateLocationData(short value, int header, bool multipacket)
        {
            UInt32 data = ((UInt32)header << 28) | ((UInt32)(ushort)value << 12);
            if (multipacket)
                data |= 0x80000000;
            data += CalculateChecksum(data);
            return data;
        }

        private List<UInt32> CreateDebugData(out DateTime startAt)
        {
            startAt = DateTime.Now;
            UInt32 d1 = CreateDateData(2, true);
            UInt32 d2 = CreateDateData(2, false);
            return new List<UInt32> { d1, d2 };
        }

        /// <summary>
        /// Create 32 bit containing current date
        /// Packet type 0x2; bit 31 indicates another packet follows
        /// 5 Bit Day
        /// 4 Bit Month
        /// 6 Bit Year (current year - 2020)
        /// 5 Bit Unused
        /// 8 Bit Checksum
        /// </summary>
        private UInt32 CreateDateData(int header, bool multipacket = false)
        {
            DateTime dt = DateTime.Now;
            int day = dt.Day;
            int month = dt.Month;
            int year = dt.Year - 2020;

            UInt32 data = (UInt32)((header << 28) + (day << 23) + (month << 19) + (year << 13));
            if (multipacket)
                data |= 0x80000000;
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

        private void buttonTest_Click(object sender, EventArgs e)
        {
            pictureBox1.BackColor = Color.White;
        }
    }
}
