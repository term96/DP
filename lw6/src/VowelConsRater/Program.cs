using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System.Collections.Generic;

namespace VowelConsRater
{
    class Program
    {
        static readonly String TEXTRANK_API_EXCHANGE = "textrank_api";
        static readonly String TEXTRANK_QUEUE = "vowel-cons-counter";
        static readonly String TEXTRANK_ROUTING_KEY = "VowelConsCounted";
        static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
        static readonly ConnectionFactory rabbit = new ConnectionFactory() { HostName = "localhost" };
        static readonly String DB_PREFIX_RANK = "-rank";

        static void Main(string[] args)
        {
            using(var connection = rabbit.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(
                    exchange: TEXTRANK_API_EXCHANGE,
                    type: "direct"
                );
                var queue = channel.QueueDeclare(
                    queue: TEXTRANK_QUEUE,
                    durable: true,
                    exclusive: false,
                    autoDelete: false
                );
                channel.QueueBind(
                    queue: TEXTRANK_QUEUE,
                    exchange: TEXTRANK_API_EXCHANGE,
                    routingKey: TEXTRANK_ROUTING_KEY
                );

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    string[] data = Encoding.UTF8.GetString(body).Split(';');
                    var id = data[0];
                    var vowelsNumber = double.Parse(data[1]);
                    var consNumber = int.Parse(data[2]);

                    var rank = (consNumber == 0) ? 0 : vowelsNumber / consNumber;

                    IDatabase db = redis.GetDatabase();
                    db.StringSet(id + DB_PREFIX_RANK, rank);

                    channel.BasicAck(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false
                    );
                };
                channel.BasicConsume(
                    queue: TEXTRANK_QUEUE,
                    autoAck: false,
                    consumer: consumer
                );
                Console.WriteLine("Listening started");
                while (true)
                {
                    Console.ReadKey(true);
                }
            }
        }
    }
}
