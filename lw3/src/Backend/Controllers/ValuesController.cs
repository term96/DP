using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using RabbitMQ.Client;
using System.Text;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        static readonly String EXCHANGE_NAME = "backend_api";
        static readonly String MESSAGE_TYPE = "TextCreated";
        static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
        static readonly ConnectionFactory rabbit = new ConnectionFactory() { HostName = "localhost" };

        // GET api/values/<id>
        [HttpGet("{id}")]
        public string Get(string id)
        {
            IDatabase db = redis.GetDatabase();
            return db.StringGet(id);
        }

        // POST api/values
        [HttpPost]
        public string Post(string value)
        {
            var id = Guid.NewGuid().ToString();
            IDatabase db = redis.GetDatabase();
            db.StringSet(id, value);
            SendMessage(id);

            return id;
        }

        private void SendMessage(String message)
        {
            using(var connection = rabbit.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(EXCHANGE_NAME, type: "direct");
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(
                    exchange: EXCHANGE_NAME,
                    routingKey: MESSAGE_TYPE,
                    basicProperties: null,
                    body: body
                );
            }
        }
    }
}
