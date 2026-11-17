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
        private const int BlinkReceiverHeaderTimeSet = 1;
        private const int BlinkReceiverHeaderDateSet = 2;
        private const int BlinkReceiverHeaderHeartbeat = 3;
        private const int BlinkReceiverHeaderLatitude = 4;
        private const int BlinkReceiverHeaderLongitude = 5;
        private const int BlinkReceiverHeaderSetZone = 6;
        private const int BlinkReceiverHeaderTest = 7;
        private const int BlinkReceiverHeaderPatch = 8;

        private static readonly string[] TimeZoneNames =
        {
            "Pago Pago", "Honolulu", "Anchorage", "Los Angeles", "Denver", "Phoenix",
            "Chicago", "Regina", "New York", "Halifax", "Manaus", "Santiago",
            "Sao Paulo", "St Johns", "Nuuk", "UTC", "London", "Lagos", "Berlin",
            "Cairo", "Maputo", "Jerusalem", "Helsinki", "Nairobi", "Riyadh",
            "Moscow", "Tehran", "Dubai", "Kolkata", "Kathmandu", "Yangon",
            "Bangkok", "Shanghai", "Hong Kong", "Singapore", "Perth", "Tokyo",
            "Seoul", "Darwin", "Adelaide", "Brisbane", "Hobart", "Sydney",
            "Guam", "Tarawa", "Auckland"
        };

        double Frequency = 64.0 / 8.0;
        List<UInt32> Data;
        DateTime StartAt;
        StringBuilder sb = new StringBuilder();

        public Form1()
        {
            InitializeComponent();
            comboBoxTimeZone.Items.AddRange(TimeZoneNames);
            comboBoxTimeZone.SelectedIndex = 15;
            pictureBox1.BackColor = Color.Black;
            buttonTest.Text = "Transmit TEST";
        }
        
        private void buttonTransmitValue_Click(object sender, EventArgs e)
        {
            buttonTransmitTimestamp.Enabled = false;
            buttonTransmitDate.Enabled = false;
            buttonTransmitHeartbeat.Enabled = false;
            buttonTransmitDateTime.Enabled = false;
            buttonTransmitLocation.Enabled = false;
            buttonTransmitTimeZone.Enabled = false;
            buttonTransmitValue.Enabled = false;
            buttonTest.Enabled = false;
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
            buttonTransmitTimeZone.Enabled = false;
            buttonTransmitValue.Enabled = false;
            buttonTest.Enabled = false;
            Data = new List<UInt32> { CreateTimeData(out StartAt, BlinkReceiverHeaderTimeSet, false, CalculateBitOffset(1), false) };
            backgroundWorker1.RunWorkerAsync();
        }

        private void buttonTransmitDate_Click(object sender, EventArgs e)
        {
            buttonTransmitTimestamp.Enabled = false;
            buttonTransmitDate.Enabled = false;
            buttonTransmitHeartbeat.Enabled = false;
            buttonTransmitDateTime.Enabled = false;
            buttonTransmitLocation.Enabled = false;
            buttonTransmitTimeZone.Enabled = false;
            buttonTransmitValue.Enabled = false;
            buttonTest.Enabled = false;
            Data = new List<UInt32> { CreateDateData(BlinkReceiverHeaderDateSet) };
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
            buttonTransmitTimeZone.Enabled = false;
            buttonTransmitValue.Enabled = false;
            buttonTest.Enabled = false;
            Data = new List<UInt32> { CreateHeartbeatData(out StartAt, out _) };
            backgroundWorker1.RunWorkerAsync();
        }

        private void buttonTransmitDateTime_Click(object sender, EventArgs e)
        {
            buttonTransmitTimestamp.Enabled = false;
            buttonTransmitDate.Enabled = false;
            buttonTransmitHeartbeat.Enabled = false;
            buttonTransmitDateTime.Enabled = false;
            buttonTransmitLocation.Enabled = false;
            buttonTransmitTimeZone.Enabled = false;
            buttonTransmitValue.Enabled = false;
            buttonTest.Enabled = false;
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
            buttonTransmitTimeZone.Enabled = false;
            buttonTransmitValue.Enabled = false;
            Data = new List<UInt32>
            {
                CreateLocationData(latitudeValue, BlinkReceiverHeaderLatitude, true),
                CreateLocationData(longitudeValue, BlinkReceiverHeaderLongitude, false)
            };
            StartAt = DateTime.Now;
            backgroundWorker1.RunWorkerAsync();
        }

        private void buttonTransmitTimeZone_Click(object sender, EventArgs e)
        {
            if (comboBoxTimeZone.SelectedIndex < 0)
            {
                MessageBox.Show(
                    "Select a time zone to transmit.",
                    "Invalid time zone",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            buttonTransmitTimestamp.Enabled = false;
            buttonTransmitDate.Enabled = false;
            buttonTransmitHeartbeat.Enabled = false;
            buttonTransmitDateTime.Enabled = false;
            buttonTransmitLocation.Enabled = false;
            buttonTransmitTimeZone.Enabled = false;
            buttonTransmitValue.Enabled = false;
            buttonTest.Enabled = false;
            Data = new List<UInt32>
            {
                CreateTimeZoneData((byte)comboBoxTimeZone.SelectedIndex)
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
            textBox2.Text = sb.ToString().Substring(0, sb.Length - 1);

            buttonTransmitTimestamp.Enabled = true;
            buttonTransmitDate.Enabled = true;
            buttonTransmitHeartbeat.Enabled = true;
            buttonTransmitDateTime.Enabled = true;
            buttonTransmitLocation.Enabled = true;
            buttonTransmitTimeZone.Enabled = true;
            buttonTransmitValue.Enabled = true;
            buttonTest.Enabled = true;
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

            sb.Clear();
            sb.AppendLine("1, 1, 1, 1, 1, 1, 1, 1, //start bit" );
            sb.AppendLine();
            int cnt = 1;
            foreach (UInt32 data in Data)
            {
                for (int i = 0; i < 32; i++)
                {
                    if (((data << i) & 0x80000000) == 0)
                        sb.AppendLine("0, 0, 0, 0, 0, 0, 0, 0,");
                    else
                        sb.AppendLine("1, 1, 1, 1, 1, 1, 1, 1,");
                }

                if (cnt++ < Data.Count())
                {
                    sb.AppendLine();
                    sb.AppendLine("0, 0, 0, 0, 0, 0, 0, 0, //stop bit");
                    sb.AppendLine("1, 1, 1, 1, 1, 1, 1, 1, //start bit");
                    sb.AppendLine();
                }
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

            for (int i = -1; i < 34; i++)
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
                    //DateTime dt = DateTime.Now;
                    //back to black at end of transmission
                    Show(Color.Black);
                }
                else if (i == 33)
                {
                    break;
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

        private int CalculateBitOffset(int i)
        {
            return i * 33 + (i - 1);
        }

        private UInt32 CreateTimeData(out DateTime startAt, int header, bool multipacket, int bitOffset, bool immediate)
        {
            double ms = (1000.0 / Frequency);
            DateTime dt = DateTime.Now;

            startAt = dt.AddMilliseconds(1000 - dt.Millisecond);
            //startAt = dt.AddMilliseconds(2000 - dt.Millisecond - ((bitoffset * 1000.0 / Frequency) % 1000));
            //startAt = startAt.AddMilliseconds(1000 - startAt.Millisecond);

            dt = startAt;
            dt = dt.AddMilliseconds(bitOffset * ms);

            return CreateTimeData(dt, header, multipacket, immediate);
        }

        /// <summary>
        /// Create 32 bit containing timestamp
        /// Packet type in bits 26-29; bit 31 indicates another packet follows and bit 30 executes this packet first
        /// 6 Bit Seconds
        /// 6 Bit Minutes
        /// 5 Bit Hours
        /// 1 Bit Unused
        /// 8 Bit Checksum
        /// </summary>
        private UInt32 CreateTimeData(DateTime dt, int header, bool multipacket, bool immediate)
        {
            int sec = dt.Second;
            int min = dt.Minute;
            int hour = dt.Hour;

            UInt32 data = (UInt32)((header << 26) + (sec << 20) + (min << 14) + (hour << 9));
            if (multipacket)
                data |= 0x80000000;
            if (immediate)
                data |= 0x40000000;
            data += CalculateChecksum(data);
            return data;
        }

        private List<UInt32> CreateDateTimeData(out DateTime startAt)
        {
            //return CreateDebugData(out startAt);

            UInt32 heartbeatData= CreateHeartbeatData(out startAt, out DateTime endAt, CalculateBitOffset(3), true);
            UInt32 timeData = CreateTimeData(endAt, BlinkReceiverHeaderTimeSet, true, false);
            UInt32 dateData = CreateDateData(endAt, BlinkReceiverHeaderDateSet, false);
            return new List<UInt32> { heartbeatData, timeData, dateData};
        }

        private UInt32 CreateHeartbeatData(out DateTime startAt, out DateTime endAt, int bitOffset = 33, bool multipacket = false)
        {
            double ms = (1000.0 / Frequency);
            DateTime dt = DateTime.Now;
            double offsetMs = (dt.Millisecond + bitOffset * ms) % 1000;
            startAt = dt.AddMilliseconds(2000 - offsetMs);
            endAt = startAt.AddMilliseconds(bitOffset * ms);

            UInt32 data = (UInt32)BlinkReceiverHeaderHeartbeat << 26;
            if (multipacket)
                data |= 0x80000000;
            data += CalculateChecksum(data);
            return data;
        }

        private UInt32 CreateLocationData(short value, int header, bool multipacket)
        {
            UInt32 data = ((UInt32)header << 26) | ((UInt32)(ushort)value << 10);
            if (multipacket)
                data |= 0x80000000;
            data += CalculateChecksum(data);
            return data;
        }

        private UInt32 CreateTimeZoneData(byte index, bool multipacket = false)
        {
            UInt32 data = ((UInt32)BlinkReceiverHeaderSetZone << 26) | ((UInt32)index << 20);
            if (multipacket)
                data |= 0x80000000;
            data += CalculateChecksum(data);
            return data;
        }

        private UInt32 CreateTestData()
        {
            UInt32 data = (UInt32)BlinkReceiverHeaderTest << 26;
            data += CalculateChecksum(data);
            return data;
        }

        private UInt32 CreatePatchData(byte offset, bool isPlus, bool isSmaller, bool multipacket)
        {
            UInt32 data = ((UInt32)BlinkReceiverHeaderPatch << 26)
                | ((UInt32)(isSmaller ? 1 : 0) << 25)
                | ((UInt32)(isPlus ? 1 : 0) << 24)
                | ((UInt32)offset << 16);
            if (multipacket)
                data |= 0x80000000;
            data += CalculateChecksum(data);
            return data;
        }

        private List<UInt32> CreateDebugData(out DateTime startAt)
        {
            const int packetBitOffset = 8 * 34 - 1;
            UInt32 timeData = CreateTimeData(
                out startAt,
                BlinkReceiverHeaderTimeSet,
                true,
                packetBitOffset,
                false);
            DateTime transmissionTime = startAt.AddMilliseconds(packetBitOffset * 1000.0 / Frequency);

            return new List<UInt32>
            {
                CreateDateData(transmissionTime, BlinkReceiverHeaderDateSet, true),
                CreateLocationData(1234, BlinkReceiverHeaderLatitude, true),
                CreateLocationData(-5678, BlinkReceiverHeaderLongitude, true),
                CreateTimeZoneData(15, true),
                CreatePatchData(0, true, false, true),
                timeData,
                CreateHeartbeatData(out _, out _, packetBitOffset, multipacket: true),
                CreateTestData()
            };
        }

        /// <summary>
        /// Create 32 bit containing current date
        /// Packet type 0x2; bit 31 indicates another packet follows
        /// 5 Bit Day
        /// 4 Bit Month
        /// 6 Bit Year (current year - 2020)
        /// 3 Bit Unused
        /// 8 Bit Checksum
        /// </summary>
        private UInt32 CreateDateData(int header, bool multipacket = false)
        {
            return CreateDateData(DateTime.Now, header, multipacket);
        }

        private UInt32 CreateDateData(DateTime dt, int header, bool multipacket)
        {
            int day = dt.Day;
            int month = dt.Month;
            int year = dt.Year - 2020;

            UInt32 data = (UInt32)((header << 26) + (day << 21) + (month << 17) + (year << 11));
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
            buttonTransmitTimestamp.Enabled = false;
            buttonTransmitDate.Enabled = false;
            buttonTransmitHeartbeat.Enabled = false;
            buttonTransmitDateTime.Enabled = false;
            buttonTransmitLocation.Enabled = false;
            buttonTransmitTimeZone.Enabled = false;
            buttonTransmitValue.Enabled = false;
            buttonTest.Enabled = false;
            Data = new List<UInt32> { CreateTestData() };
            StartAt = DateTime.Now;
            backgroundWorker1.RunWorkerAsync();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Show(Color.White);
        }
    }
}
