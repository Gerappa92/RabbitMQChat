using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitMQChat;

public static class AuthChat
{
    public static void Start()
    {
        Console.WriteLine("RabbitMQ Chat!");
        Console.WriteLine("Please type your name");
        var userName = Console.ReadLine();
        Console.WriteLine("Please type your password");
        var password = Console.ReadLine();
        var routingKey = $"chat.message";

        var factory = new ConnectionFactory
        {
            Uri = new Uri($"amqp://{userName}:{password}@localhost:5672")
        };
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        const string exchange = "RabbitMQ_AuthChat_Exchange";
        var queue = $"RabbitMQ_AuthChat_Queue_{userName}";

        channel.ExchangeDeclare(exchange, ExchangeType.Direct, durable: true);
        channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(queue, exchange, routingKey: routingKey);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (_, eventArgs) =>
        {
            if (eventArgs.BasicProperties.UserId == userName) return;
            var message = Encoding.UTF8.GetString(eventArgs.Body);
            Console.WriteLine(message);
        };
        channel.BasicConsume(queue, autoAck: true, consumer);

        Console.WriteLine("Enter empty message to quit.");
        Console.WriteLine("Write a message:");
        var message = Console.ReadLine() ?? string.Empty;
        while (message != string.Empty)
        {
            var body = Encoding.UTF8.GetBytes($"{userName}:{message}");
            var bp = channel.CreateBasicProperties();
            bp.UserId = userName;
            channel.BasicPublish(exchange, routingKey, bp, body);
            message = Console.ReadLine() ?? string.Empty;
        }

        channel.Close();
        connection.Close();
    }
}