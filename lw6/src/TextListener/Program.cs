using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System.Security.Cryptography;

namespace TextListener
{
    class Program
    {
        static readonly String EXCHANGE_NAME = "backend_api";
        static readonly String QUEUE_NAME = "text_listener";
        static readonly String ROUTING_KEY = "TextCreated";
        static readonly String DB_PREFIX_TEXT = "-text";
        static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
        static readonly ConnectionFactory rabbit = new ConnectionFactory() { HostName = "localhost" };

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
                    var id = Encoding.UTF8.GetString(body);

                    int dbNumber = GetDBNumber(id);
                    IDatabase db = redis.GetDatabase(db: dbNumber);
                    Console.WriteLine("Redis: accessed DB {0} by contextId {1}", dbNumber, id);

                    var text = db.StringGet(id + DB_PREFIX_TEXT);
                    Console.WriteLine("ID: {0}, Text: {1}", id, text);
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

        private static int GetDBNumber(String contextId)
        {
            const int databases = 16;
            using(MD5 md5 = new MD5CryptoServiceProvider())
            {
                byte[] result = md5.ComputeHash(Encoding.UTF8.GetBytes(contextId));
                return (result[0] ^ result[4] ^ result[8] ^ result[12]) % databases;
            }
        }
    }
}
