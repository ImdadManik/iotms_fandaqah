﻿using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using Org.BouncyCastle.Security;
using System;

namespace iotms.Emqx
{
    public static class cMsgPublisher
    {
        private static readonly string broker = "pms-db003.fandaqah.com";
        private static readonly int port = 1883;

        public static void PublishMessage(string _payload, string _username, string _topics, bool isRetained = true)
        {
            Random random = new Random();
            int randomNumber = random.Next(1000, 1000000);
            GenerateAES aes = new GenerateAES("098pub+1key+0pri", 256, "ABCXYZ123098");
            string clientId = string.Format("python-mqtt-{0}", randomNumber);
            string topic = _topics;
            string username = _username;
            string password = aes.Encrypt(_username);
            var factory = new MqttFactory();

            // Create a MQTT client instance
            var mqttClient = factory.CreateMqttClient();

            // Create MQTT client options
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port) // MQTT broker address and port
                .WithCredentials(username, password) // Set username and password
                .WithClientId(clientId)
                .Build();
            try
            {
                //Connect to MQTT broker
                var connectResult = mqttClient.ConnectAsync(options).GetAwaiter().GetResult();
                if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
                {
                    Console.WriteLine("Connected to MQTT broker successfully.");
                    MqttApplicationMessage mqttMessage = null;

                    if (isRetained)
                    {
                        mqttMessage = new MqttApplicationMessageBuilder()
                                        .WithTopic(topic)
                                        .WithPayload(_payload)
                                        .WithRetainFlag()
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .Build();
                    }
                    else
                    {
                        mqttMessage = new MqttApplicationMessageBuilder()
                                        .WithTopic(topic)
                                        .WithPayload(_payload) 
                                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                        .Build(); 
                    }

                    mqttClient.PublishAsync(mqttMessage).GetAwaiter().GetResult();

                    // Disconnect from the MQTT broker
                    mqttClient.DisconnectAsync().GetAwaiter().GetResult();
                }
                else
                {
                    Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
                }
            }
            catch (MQTTnet.Adapter.MqttConnectingFailedException ex)
            {
                Console.WriteLine($"Failed to connect to MQTT broker due to exception: {ex.Message}");
            }
        }

    }
}
