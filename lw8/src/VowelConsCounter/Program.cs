using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;

namespace VowelConsCounter
{
    class Program
    {
        static readonly String TEXTRANK_API_EXCHANGE = "textrank_api";
        static readonly String TEXTRANK_QUEUE = "text-rank-tasks";
        static readonly String TEXTRANK_ROUTING_KEY_IN = "TextRankTask";
        static readonly String TEXTRANK_ROUTING_KEY_OUT = "VowelConsCounted";
        static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
        static readonly ConnectionFactory rabbit = new ConnectionFactory() { HostName = "localhost" };
        static readonly HashSet<Char> vowels = new HashSet<Char>()
        { 
            'a', 'e', 'i', 'o', 'u',
            'а', 'о', 'и', 'е', 'ё', 'э', 'ы', 'у', 'ю', 'я'
        };
        static readonly String DB_PREFIX_TEXT = "-text";
        static readonly String DB_PREFIX_STATUS = "-status";

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
                    routingKey: TEXTRANK_ROUTING_KEY_IN
                );

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    string id = Encoding.UTF8.GetString(body);
                    Console.WriteLine("\n\nStart: {0}", id);

                    Thread.Sleep(400);
                    SaveTextStatus(id);

                    int dbNumber = GetDBNumber(id);
                    IDatabase db = redis.GetDatabase(db: dbNumber);
                    Console.WriteLine("Redis: accessed DB {0} by contextId {1}", dbNumber, id);

                    string text = db.StringGet(id + DB_PREFIX_TEXT);

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

                    var publishBody = Encoding.UTF8.GetBytes(String.Format("{0};{1};{2}", id, vowelsNumber, lettersNumber - vowelsNumber));
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    channel.BasicPublish(
                        exchange: TEXTRANK_API_EXCHANGE,
                        routingKey: TEXTRANK_ROUTING_KEY_OUT,
                        basicProperties: properties,
                        body: publishBody
                    );

                    channel.BasicAck(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false
                    );
                    Console.WriteLine("End: {0}\n\n", id);
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

        private static int GetDBNumber(String contextId)
        {
            const int databases = 16;
            using(MD5 md5 = new MD5CryptoServiceProvider())
            {
                byte[] result = md5.ComputeHash(Encoding.UTF8.GetBytes(contextId));
                return (result[0] ^ result[4] ^ result[8] ^ result[12]) % databases;
            }
        }

        private static void SaveTextStatus(String contextId)
        {
            Console.WriteLine("SaveTextStatus: " + "processing");
            IDatabase db = redis.GetDatabase(GetDBNumber(contextId));
            db.StringSet(contextId + DB_PREFIX_STATUS, "processing");
        }
    }
}
