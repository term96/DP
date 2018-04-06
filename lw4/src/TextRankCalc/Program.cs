using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System.Collections.Generic;

namespace TextRankCalc
{
    class Program
    {
        static readonly String EXCHANGE_NAME = "backend_api";
        static readonly String QUEUE_NAME = "text_rank_calc";
        static readonly String ROUTING_KEY = "TextCreated";
        static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
        static readonly ConnectionFactory rabbit = new ConnectionFactory() { HostName = "localhost" };
        static readonly HashSet<Char> vowels = new HashSet<Char>()
        { 
            'a', 'e', 'i', 'o', 'u',
            'а', 'о', 'и', 'е', 'ё', 'э', 'ы', 'у', 'ю', 'я'
        };

        static void Main(string[] args)
        {
            using(var connection = rabbit.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(
                    exchange: EXCHANGE_NAME,
                    type: "direct"
                );
                var queue = channel.QueueDeclare(
                    queue: QUEUE_NAME,
                    durable: true,
                    exclusive: false,
                    autoDelete: false
                );
                channel.QueueBind(
                    queue: QUEUE_NAME,
                    exchange: EXCHANGE_NAME,
                    routingKey: ROUTING_KEY
                );

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    string id = Encoding.UTF8.GetString(body);
                    IDatabase db = redis.GetDatabase();
                    string text = db.StringGet(id);

                    int lettersNumber = 0;
                    int vowelsNumber = 0;
                    foreach (char ch in text)
                    {
                        if (Char.IsLetter(ch))
                        {
                            lettersNumber++;
                            vowelsNumber += vowels.Contains(Char.ToLowerInvariant(ch)) ? 1 : 0;
                        }
                    }

                    db.StringSet(id, vowelsNumber / (lettersNumber - vowelsNumber));
                    channel.BasicAck(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false
                    );
                };
                channel.BasicConsume(
                    queue: QUEUE_NAME,
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
