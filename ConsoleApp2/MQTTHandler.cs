using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwlCM160_MQTT
{
    internal class MQTTHandler
    {
        MqttFactory mqttFactory = new MqttFactory();

        /// <summary>
        /// Publishes the realtime data, power is conditional to topic2 being defined.
        /// </summary>
        /// <param name="rm">Realtime data</param>
        /// <param name="opt">Options</param>
        /// <returns></returns>
        public async Task PublishRealtime(RealtimeMeasurement rm, Options opt)
        {
            if (!String.IsNullOrEmpty(opt.Topic))
            {
                await Publish(opt.Topic, rm.Current_A, opt);
            }
            if (!String.IsNullOrEmpty(opt.Topic2))
            {
                await Publish(opt.Topic2, rm.Power_kW, opt);
            }
        }

        private MqttClientOptions? GetClientOptions(Options opt)
        {
            if (String.IsNullOrEmpty(opt.Broker))
            {
                Console.WriteLine("ERROR : MQTT broker name is empty");
                return null;
            }
            var builder = new MqttClientOptionsBuilder()
                            .WithClientId(Guid.NewGuid().ToString())
                            .WithTcpServer(opt.Broker, 1883);

            if (!String.IsNullOrEmpty(opt.User))
                builder = builder.WithCredentials(opt.User, opt.Password);

            return builder.Build();
        }

        /// <summary>
        /// Try to establish connection to MQTT broker
        /// </summary>
        /// <param name="opt"></param>
        /// <returns></returns>
        public async Task<bool> TryConnect(Options opt)
        {
            try
            {
                using (var mqttClient = mqttFactory.CreateMqttClient())
                {
                    var opts = GetClientOptions(opt);
                    if (opts == null)
                    {
                        return false;
                    }

                    if (opt.Verbose)
                    {
                        Console.WriteLine($"Connecting to {opt.Broker} as {opt.User}...");
                    }

                    var result = await mqttClient.ConnectAsync(opts, CancellationToken.None);
                    return (result.ResultCode == MqttClientConnectResultCode.Success);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("ERROR : Unhandled exception during MQTT connection", ex);
                return false;
            }
        }

        /// <summary>
        /// Publish the value on the MQTT broker
        /// </summary>
        /// <param name="topic">Topic where to publish (e.g. home/mains/current)</param>
        /// <param name="value">Value to be published (e.g. 2.324 A)</param>
        /// <param name="opt">Settings to be used for MQTT broker connection</param>
        /// <returns></returns>
        private async Task Publish(string topic, double value, Options opt)
        {
            try
            {
                using (var mqttClient = mqttFactory.CreateMqttClient())
                {
                    var opts = GetClientOptions(opt);
                    if (opts == null)
                    {
                        return;
                    }
                    await mqttClient.ConnectAsync(opts, CancellationToken.None);
                    var str_val = Math.Round(value, 3).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    var applicationMessage = new MqttApplicationMessageBuilder()
                        .WithTopic(topic)
                        .WithPayload(str_val)
                        .Build();

                    var result = await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                    if (result.IsSuccess)
                    {
                        if (opt.Verbose)
                        {
                            Console.WriteLine($"Published on {topic} value {str_val}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("ERROR : cannot publish MQTT packet", result.ToString());
                    }

                    await mqttClient.DisconnectAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR : Unhandled exception during MQTT publish", ex);
            }
        }
    }
}
