using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitMQChat;

public static class ChatRoom
{
    public static void Start()
    {
        Console.WriteLine("RabbitMQ Chat!");
        Console.WriteLine("Please type your name");
        var userName = Console.ReadLine();
        Console.WriteLine("Please type the room name");
        var roomName = Console.ReadLine();
        var routingKey = $"message.room.{roomName}";

        var factory = new ConnectionFactory
        {
            Uri = new Uri("amqp://guest:guest@localhost:5672")
        };
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        const string exchange = "RabbitMQ_ChatRoom_Exchange";
        var queue = $"RabbitMQ_ChatRoom_Queue_{userName}";

        channel.ExchangeDeclare(exchange, ExchangeType.Direct, durable: true);
        channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(queue, exchange, routingKey: routingKey);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (_, eventArgs) =>
        {
            var message = Encoding.UTF8.GetString(eventArgs.Body);
            Console.WriteLine(message);
        };
        channel.BasicConsume(queue, autoAck: true, consumer);

        Console.WriteLine("Write a message:");
        var message = Console.ReadLine() ?? string.Empty;
        while (message != string.Empty)
        {
            var body = Encoding.UTF8.GetBytes($"{userName}:{message}");
            channel.BasicPublish(exchange, routingKey, basicProperties: null, body);
            message = Console.ReadLine() ?? string.Empty;
        }

        channel.Close();
        connection.Close();
    }
}