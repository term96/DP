using System;
using System.Threading;
using System.Text;
using System.Linq;
using Delivery.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using Realms;

public class Rabbit
{
	private static readonly ConnectionFactory rabbit = new ConnectionFactory();
	private const string EXCHANGE_NAME = "MobileShopExchange";
	private const string QUEUE_NAME = "DeliveryQueue";
	private const string NEW_ORDER_ROUTING_KEY = "NewOrder";
	private const string ORDER_CHANGED_ROUTING_KEY = "OrderChanged";
	private static Rabbit instance;
	private static IConnection connection;
	private static IModel channel;

	private Rabbit()
	{
		connection = rabbit.CreateConnection();
		channel = connection.CreateModel();
		channel.ExchangeDeclare
		(
			exchange: EXCHANGE_NAME,
			type: "direct",
			durable: true,
			autoDelete: false
		);
		channel.QueueDeclare
		(
			queue: QUEUE_NAME,
			durable: true,
			exclusive: false,
			autoDelete: false
		);
		channel.QueueBind(queue: QUEUE_NAME, exchange: EXCHANGE_NAME, routingKey: NEW_ORDER_ROUTING_KEY);
		CreateConsumer();

		Console.WriteLine("Rabbit successfully started");
	}

	public void StopRabbit()
	{
		channel.Close();
		connection.Close();
		Console.WriteLine("Rabbit successfully stopped");
	}

	public void BroadcastOrderChangedEvent(OrderModel order)
	{
		var properties = channel.CreateBasicProperties();
		properties.Persistent = true;

		var serialized = JsonConvert.SerializeObject(order);
		var body = Encoding.UTF8.GetBytes(serialized);

		channel.BasicPublish
		(
			exchange: EXCHANGE_NAME,
			routingKey: ORDER_CHANGED_ROUTING_KEY,
			mandatory: true,
			basicProperties: properties,
			body: body
		);
	}

	private void CreateConsumer()
	{
		var consumer = new EventingBasicConsumer(channel);
		consumer.Received += (model, ea) =>
		{
			var body = ea.Body;
			var serialized = Encoding.UTF8.GetString(body);
			OrderModel order = null;
			try
			{
				order = JsonConvert.DeserializeObject<OrderModel>(serialized);
				var db = GetRealmInstance();
				db.Write(() =>
				{
					db.Add(order);
				});
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}

			channel.BasicAck(ea.DeliveryTag, false);
		};
		channel.BasicConsume
		(
			queue: QUEUE_NAME,
			autoAck: false,
			consumer: consumer
		);
	}

	private Realm GetRealmInstance()
	{
		return Realm.GetInstance("delivery");
	}

	public static Rabbit GetInstance()
	{
		if (instance == null)
		{
			instance = new Rabbit();
		}
		return instance;
	}
}
