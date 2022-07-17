using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Text;
using System.Threading.Tasks;

namespace MQTT.Client.ConsoleApp
{
    class ConsoleApplication
    {
        public static IMqttClient MqttClient { get; set; }

        static async Task Main(string[] args)
        {
            Console.WriteLine("MQTT Topics Watcher \n" +
                "\n" +
                "\t Connect to MQTT sever & topic, follow the below questions\n");

            Console.WriteLine("What is websocket ULR?");
            var host = Console.ReadLine();

            Console.WriteLine("What is the topic?");
            var topic = Console.ReadLine();

            var mqttFactory = new MqttFactory();
            MqttClient = mqttFactory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
                .WithClientId(Guid.NewGuid().ToString())
                .WithWebSocketServer(host)
                .WithCleanSession()
                .Build();

            MqttClient.UseConnectedHandler(async e =>
            {
                Console.WriteLine("Connected to MQTT broker");
                var topicFilter = new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .Build();

                await MqttClient.SubscribeAsync(topicFilter);
            });

            MqttClient.UseDisconnectedHandler(e =>
            {
                Console.WriteLine("Disconnected to MQTT broker");
            });


            MqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                Console.WriteLine();
                Console.WriteLine("------------Receive Message From Topic: Start -----------");
                Console.WriteLine($"Message: {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                Console.WriteLine("------------Receive Message From Topic: END ------------");
            });

            await MqttClient.ConnectAsync(options);


            var i = 1;
            while (i > 0)
            {
                Console.WriteLine("\nType anything to publish the message to the topic  \n" +
                    "To Terminate Console App, type \'CLOSE\'\n");

                var messagePayload = Console.ReadLine();
                if (messagePayload == "CLOSE")
                    i = 0;
                else
                {
                    await PublishlMessageAsync(MqttClient, messagePayload, topic);
                }
            }
        }

        private static async Task PublishlMessageAsync(IMqttClient client, string messagePayload, string topic)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(messagePayload)
                .WithAtLeastOnceQoS()
                .Build();

            if (client.IsConnected)
            {
                await client.PublishAsync(message);
            }
        }
    }
}
