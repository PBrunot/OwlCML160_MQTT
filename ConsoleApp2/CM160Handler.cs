using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OwlCM160_MQTT
{
    internal class CM160Handler
    {
        public const int PACKET_LENGTH = 11;
        private const string ID_REPLY = "IDTCMV001";

        SerialPort? _serialPort;

        public bool UnitFound { get; set; }
        public bool HistoricalDone { get; set; }

        public bool init(Options opt)
        {
            UnitFound = false;
            HistoricalDone = false;

            // Create a new SerialPort object with default settings.
            _serialPort = new SerialPort();

            if (string.IsNullOrEmpty(opt.SerialPort))
                return false;

            // Allow the user to set the appropriate properties.
            _serialPort.PortName = opt.SerialPort;
            _serialPort.BaudRate = 250000;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;

            // Set the read/write timeouts
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            try
            {
                _serialPort.Open();
                if (opt.Verbose)
                {
                    Console.WriteLine($"Port {opt.SerialPort} successfully opened");
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while opening {opt.SerialPort} : {ex}");
                return false;
            }
        }

        /// <summary>
        /// Gets a 11-bytes packet from CM160.
        /// </summary>
        /// <returns></returns>
        public List<byte> GetPacket()
        {
            var empty = new List<byte>();

            if (_serialPort == null)
            {
                return empty;
            }

            try
            {
                List<byte> bytes = new List<byte>();
                while (bytes.Count() != 11)
                {
                    var value = _serialPort.ReadByte();
                    if (value == -1) // Was not able to read a byte in time.
                    {
                        return empty;
                    }
                    bytes.Add((byte)value);
                }
                // Packet is completed
                return bytes;
            }
            catch (Exception)
            {
                // Swallow Timeout exceptions
                return empty;
            }
        }

        /// <summary>
        /// Parses answers from CM160, handles replies, until realtime measurements arrive.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="opt"></param>
        /// <returns></returns>
        public RealtimeMeasurement? Parse(List<byte> packet, Options opt)
        {
            if (packet.Count != PACKET_LENGTH || _serialPort == null)
            {
                return null;
            }

            var strbuffer = Encoding.UTF8.GetString(packet.ToArray(), 1, 10);
            if (opt.Verbose)
            {
                Console.WriteLine("-> " + BitConverter.ToString(packet.ToArray()));
            }

            if (strbuffer.Contains(ID_REPLY))
            {
                this.UnitFound = true;
                if (opt.Verbose)
                {
                    Console.WriteLine("CM160 unit recognized");
                }
            }

            if (UnitFound && strbuffer.Contains("IDTWAITPCR"))
            {
                _serialPort.Write([0xa5], 0, 1);
                HistoricalDone = true;
                if (opt.Verbose)
                {
                    Console.WriteLine("<- a5");
                    Console.WriteLine("End of historical data");
                }
            }

            switch (packet[0])
            {
                case 0xa9:
                    if (UnitFound)
                    {
                        HistoricalDone = false;
                        _serialPort.Write([0x5a], 0, 1);
                        if (opt.Verbose)
                        {
                            Console.WriteLine("Starting historical data request...");
                            Console.WriteLine("<- 5a");
                        }
                    }
                    break;
                case 0x51:
                    if (opt.Verbose)
                    {
                        Console.WriteLine("Live data received");
                    }

                    var amps = (packet[8] + (packet[9] << 8)) * 0.07;
                    var kw = amps * opt.Volts / 1000;
                    var realtime = new RealtimeMeasurement() { Current_A = amps, Power_kW = kw };
                    return realtime;
                default:
                    break;
            }

            return null;
        }
    }
}
