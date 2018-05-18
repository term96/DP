using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System.Security.Cryptography;

namespace TextStatistics
{
    class Program
    {
        static readonly String TEXTRANK_API_EXCHANGE = "textrank_api";
        static readonly String TEXTRANK_QUEUE = "text-statistics";
        static readonly String TEXTRANK_ROUTING_KEY = "TextSuccessMarked";
        static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
        static readonly ConnectionFactory rabbit = new ConnectionFactory() { HostName = "localhost" };
        static readonly String DB_TEXTNUM_KEY = "TextNum";
        static readonly String DB_HIGHRANKPART_KEY = "HighRankPart";
        static readonly String DB_AVGRANK_KEY = "AvgRank";
        static readonly String DB_PREFIX_RANK = "-rank";
        static readonly String DB_PREFIX_STATUS = "-status";

        static int textNum;
        static int highRankPart;
        static double avgRank;

        static void Main(string[] args)
        {
            IDatabase db = redis.GetDatabase(0);
            textNum = db.KeyExists(DB_TEXTNUM_KEY) ? int.Parse(db.StringGet(DB_TEXTNUM_KEY)) : 0;
            highRankPart = db.KeyExists(DB_HIGHRANKPART_KEY) ? int.Parse(db.StringGet(DB_HIGHRANKPART_KEY)) : 0;
            avgRank = db.KeyExists(DB_AVGRANK_KEY) ? double.Parse(db.StringGet(DB_AVGRANK_KEY)) : 0;

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
                    string id = data[0];
                    string status = data[1];

                    int dbNumber = GetDBNumber(id);
                    IDatabase database = redis.GetDatabase(db: dbNumber);
                    Console.WriteLine("Redis: accessed DB {0} by contextId {1}", dbNumber, id);

                    double rank = double.Parse(database.StringGet(id + DB_PREFIX_RANK));

                    textNum++;
                    highRankPart += status == "true" ? 1 : 0;
                    avgRank = (avgRank * (textNum - 1) + rank) / textNum;
                    saveData();
                    SaveTextStatus(id);

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

        static void saveData() {
            IDatabase db = redis.GetDatabase(0);
            db.StringSet(DB_TEXTNUM_KEY, textNum);
            db.StringSet(DB_HIGHRANKPART_KEY, highRankPart);
            db.StringSet(DB_AVGRANK_KEY, avgRank.ToString());
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
            IDatabase db = redis.GetDatabase(GetDBNumber(contextId));
            db.StringSet(contextId + DB_PREFIX_STATUS, "done");
        }
    }
}
