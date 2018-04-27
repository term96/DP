using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using RabbitMQ.Client;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Security.Cryptography;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        static readonly String EXCHANGE_NAME = "backend_api";
        static readonly String ROUTING_KEY = "TextCreated";
        static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
        static readonly ConnectionFactory rabbit = new ConnectionFactory() { HostName = "localhost" };
        static readonly int MAX_ATTEMPTIONS = 5;
        static readonly int SLEEP_MS = 50;
        static readonly String DB_PREFIX_TEXT = "-text";
        static readonly String DB_PREFIX_RANK = "-rank";


        // GET api/values/<id>
        [HttpGet("{id}")]
        public string Get(string id)
        {
            int dbNumber = GetDBNumber(id);
            IDatabase db = redis.GetDatabase(db: dbNumber);
            Console.WriteLine("Redis: accessed DB {0} by contextId {1}", dbNumber, id);

            for (int i = 0; i < MAX_ATTEMPTIONS; i++)
            {
                if (db.KeyExists(id + DB_PREFIX_RANK))
                {
                    Response.StatusCode = (int) HttpStatusCode.OK;
                    return db.StringGet(id + DB_PREFIX_RANK);
                }
                Thread.Sleep(SLEEP_MS);
            }
            Response.StatusCode = (int) HttpStatusCode.NotFound;
            return "";
        }

        // POST api/values
        [HttpPost]
        public string Post(string value)
        {
            var id = Guid.NewGuid().ToString();

            int dbNumber = GetDBNumber(id);
            IDatabase db = redis.GetDatabase(db: dbNumber);
            Console.WriteLine("Redis: accessed DB {0} by contextId {1}", dbNumber, id);

            db.StringSet(id + DB_PREFIX_TEXT, value);
            SendMessage(id);

            return id;
        }

        private void SendMessage(String message)
        {
            using(var connection = rabbit.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(
                    exchange: EXCHANGE_NAME,
                    type: "direct"
                );

                var body = Encoding.UTF8.GetBytes(message);
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(
                    exchange: EXCHANGE_NAME,
                    routingKey: ROUTING_KEY,
                    basicProperties: properties,
                    body: body
                );
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
