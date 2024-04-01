using MQTTnet.Client;
using MQTTnet;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Text;
using OwlCM160_MQTT;
using CommandLine;
using MQTTnet.Server;

internal class Program
{
   
    static async Task Main(string[] args)
    {
        // add unhandled exception handler
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            Console.WriteLine("Unhandled exception", e.ExceptionObject);
        };

        var opt_result = Parser.Default.ParseArguments<Options>(args);
        await MainLoop(opt_result.Value);
    }

    static async Task<bool> MainLoop(Options opt)
    {
        MQTTHandler mqtt = new MQTTHandler();
        CM160Handler cm160 = new CM160Handler();

        if (!cm160.init(opt))
        {
            Console.WriteLine("Failure to connect to serial port, exiting.");
            return false;
        }

        if (!await mqtt.TryConnect(opt))
        {
            Console.WriteLine($"Failure to connect to MQTT broker ({opt.Broker}), exiting.");
            return false;
        }

        Console.WriteLine("MQTT and Serial connections established, starting main loop.");

        while (true)
        {
            try
            {
                var packet = cm160.GetPacket();
                var realtime = cm160.Parse(packet, opt);
                if (realtime != null)
                {
                    Console.WriteLine($"{DateTime.Now} : received realtime data: {realtime}");
                    await mqtt.PublishRealtime(realtime, opt);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled exception", ex);
            }
        }
    }
}