#r "nuget: System.Reactive, 5.0.0"
#r "nuget: System.Net.Mqtt, 0.6.16-beta"

using System.Net.Http;
using System.Net.Mqtt;

string mqttServer = "192.168.178.50";

var configuration = new MqttConfiguration();
configuration.Port = 1883;

var client = await MqttClient.CreateAsync(mqttServer, configuration);
var sessionState = await client.ConnectAsync();

await client.SubscribeAsync("stat/+/STATUS5", MqttQualityOfService.AtMostOnce);

client
      .MessageStream
      .Subscribe(msg => Console.WriteLine($"Message received in topic: {msg.Topic}\n{Encoding.UTF8.GetString(msg.Payload)}"));


var call = new MqttApplicationMessage("cmnd/tasmotas/status", Encoding.UTF8.GetBytes("5"));

await client.PublishAsync(call, MqttQualityOfService.AtMostOnce); //QoS0


// await downloadConfig("192.168.178.39");

await client.DisconnectAsync();