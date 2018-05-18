using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;

namespace TextStatistics
{
    class Program
    {
        static readonly String TEXTRANK_API_EXCHANGE = "textrank_api";
        static readonly String TEXTRANK_QUEUE = "text-rank-calc";
        static readonly String TEXTRANK_ROUTING_KEY = "TextRankCalculated";
        static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
        static readonly ConnectionFactory rabbit = new ConnectionFactory() { HostName = "localhost" };
        static readonly String DB_TEXTNUM_KEY = "TextNum";
        static readonly String DB_HIGHRANKPART_KEY = "HighRankPart";
        static readonly String DB_AVGRANK_KEY = "AvgRank";

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
                    var id = data[0];
                    var rank = double.Parse(data[1]);

                    textNum++;
                    if (rank > 0.5) {
                        highRankPart++;
                    }
                    avgRank = (avgRank * (textNum - 1) + rank) / textNum;
                    saveData();

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
    }
}
