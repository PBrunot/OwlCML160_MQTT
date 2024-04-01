using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwlCM160_MQTT
{
    public class Options
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('s', "serial", Required = true, HelpText = "Serial port for communication with CM160.")]
        public string? SerialPort { get; set; }

        [Option('b', "broker", Required = false, Default="homeassistant.local", HelpText = "MQTT broker name or IP Address")]
        public string? Broker { get; set; }

        [Option('t', "topic", Required = false, Default="homeassistant/mains/current", HelpText = "MQTT Topic to publish the realtime current")]
        public string? Topic { get; set; }

        [Option("topic2", Required = false, Default = "homeassistant/mains/power", HelpText = "MQTT Topic to publish the realtime power, facultative.")]
        public string? Topic2 { get; set; }

        [Option('u', "user", Required = false, Default = "", HelpText = "Username to log on the MQTT broker")]
        public string? User { get; set; }

        [Option('p', "password", Required = false, Default = "", HelpText = "Password to log on the MQTT broker")]
        public string? Password { get; set; }

        [Option("volts", Required = false, Default =230, HelpText = "Average voltage (used for Power publication, P=U.I)")]
        public float Volts { get; set; }

    }
}
