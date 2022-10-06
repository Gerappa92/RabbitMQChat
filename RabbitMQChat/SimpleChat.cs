using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitMQChat;

public static class SimpleChat
{
    public static void Start()
    {
        const string messageCreated = "message.created";

        Console.WriteLine("RabbitMQ Chat!");
        Console.WriteLine("Please type your name");
        var userName = Console.ReadLine();

        var factory = new ConnectionFactory
        {
            Uri = new Uri("amqp://guest:guest@localhost:5672")
        };
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        const string exchange = "RabbitMQ_Chat_Exchange";
        var queue = $"RabbitMQ_Chat_Queue_{userName}";

        channel.ExchangeDeclare(exchange, ExchangeType.Fanout, durable: true);
        channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(queue, exchange, routingKey: messageCreated);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (_, eventArgs) =>
        {

            var message = Encoding.UTF8.GetString(eventArgs.Body);
            Console.WriteLine($"{userName}:{message}");
        };
        channel.BasicConsume(queue, autoAck: true, consumer);

        Console.WriteLine("Write a message:");
        var message = Console.ReadLine() ?? string.Empty;
        while (message != string.Empty)
        {
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange, messageCreated, basicProperties: null, body);
            message = Console.ReadLine() ?? string.Empty;
        }

        channel.Close();
        connection.Close();
    }
}