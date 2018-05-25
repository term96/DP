using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Threading;

namespace TextProcessingLimiter
{
    class Program
    {
        static readonly String BACKEND_API_EXCHANGE = "backend_api";
        static readonly String BACKEND_QUEUE = "text_processing_limiter_backend";
        static readonly String BACKEND_ROUTING_KEY = "TextCreated";
        static readonly String TEXTRANK_API_EXCHANGE = "textrank_api";
        static readonly String TEXTRANK_QUEUE = "text_processing_limiter_textrank";
        static readonly String TEXTRANK_ROUTING_KEY_IN = "TextSuccessMarked";
        static readonly String TEXTRANK_ROUTING_KEY_OUT = "ProcessingAccepted";
        static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
        static readonly ConnectionFactory rabbit = new ConnectionFactory() { HostName = "localhost" };
        static readonly String DB_PREFIX_STATUS = "-status";

        static int textsLeft = 5;

        static void Main(string[] args)
        {
            using(var connection = rabbit.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: BACKEND_API_EXCHANGE, type: "direct");
                channel.ExchangeDeclare(exchange: TEXTRANK_API_EXCHANGE, type: "direct");

                var backendQueue = channel.QueueDeclare(queue: BACKEND_QUEUE, durable: true, exclusive: false, autoDelete: false);
                channel.QueueBind(queue: BACKEND_QUEUE, exchange: BACKEND_API_EXCHANGE, routingKey: BACKEND_ROUTING_KEY);

                var textrankQueue = channel.QueueDeclare(queue: TEXTRANK_QUEUE, durable: true, exclusive: false, autoDelete: false);
                channel.QueueBind(queue: TEXTRANK_QUEUE, exchange: TEXTRANK_API_EXCHANGE, routingKey: TEXTRANK_ROUTING_KEY_IN);

                var backendConsumer = new EventingBasicConsumer(channel);
                backendConsumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    string id = Encoding.UTF8.GetString(body);
                    Console.WriteLine("\n\nStart: {0}", id);
                    var newBody = id + (textsLeft > 0 ? ";true" : ";false");

                    Thread.Sleep(400);
                    SaveTextStatus(id, textsLeft > 0);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    channel.BasicPublish(exchange: TEXTRANK_API_EXCHANGE, routingKey: TEXTRANK_ROUTING_KEY_OUT, basicProperties: properties, body: Encoding.UTF8.GetBytes(newBody));

                    if (textsLeft > 0)
                    {
                        textsLeft--;
                    }

                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    Console.WriteLine("End: {0}\n\n", id);
                };
                channel.BasicConsume(queue: BACKEND_QUEUE, autoAck: false, consumer: backendConsumer);

                var textrankConsumer = new EventingBasicConsumer(channel);
                textrankConsumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    string status = Encoding.UTF8.GetString(body).Split(';')[1];
                    if (status == "false")
                    {
                        textsLeft++;
                    }
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };
                channel.BasicConsume(queue: TEXTRANK_QUEUE, autoAck: false, consumer: textrankConsumer);

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

        private static void SaveTextStatus(String contextId, bool accepted)
        {
            Console.WriteLine("SaveTextStatus: " + (accepted ? "accepted" : "limit_exceed"));
            IDatabase db = redis.GetDatabase(GetDBNumber(contextId));
            db.StringSet(contextId + DB_PREFIX_STATUS, accepted ? "accepted" : "limit_exceed");
        }
    }
}
