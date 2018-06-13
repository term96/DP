using System;
using System.Text;
using Stock.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using Realms;
using System.Linq;

public class Rabbit
{
	private static readonly ConnectionFactory rabbit = new ConnectionFactory();
	private static readonly string EXCHANGE_NAME = "MobileShopExchange";
	private static readonly string QUEUE_NAME = "StockQueue";
	private static readonly string ITEM_ADDED_ROUTING_KEY = "ItemAdded";
	private static readonly string ITEM_REMOVED_ROUTING_KEY = "ItemRemoved";
	private static readonly string ITEM_CHANGED_ROUTING_KEY = "ItemChanged";
	private const string ITEM_SOLD_ROUTING_KEY = "ItemSold";
	private const string ITEM_RETURNED_ROUTING_KEY = "ItemReturned";
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
		channel.QueueBind(queue: QUEUE_NAME, exchange: EXCHANGE_NAME, routingKey: ITEM_SOLD_ROUTING_KEY);
		channel.QueueBind(queue: QUEUE_NAME, exchange: EXCHANGE_NAME, routingKey: ITEM_RETURNED_ROUTING_KEY);

		var consumer = new EventingBasicConsumer(channel);
		consumer.Received += (model, ea) =>
		{
			var body = ea.Body;
			var id = Encoding.UTF8.GetString(body);

			try
			{
				var db = GetRealmInstance();
				db.Write(() =>
				{
					var items = db.All<PhoneModel>().Where(i => i.id.Equals(id)).ToList();
					PhoneModel item = null;
					if (items.Count() > 0)
					{
						item = items[0];
					}
					if (item != null)
					{
						if (ea.RoutingKey.Equals(ITEM_SOLD_ROUTING_KEY))
						{
							item.available--;
						}
						else
						{
							item.available++;
						}
						db.Add(item, update: true);
						BroadcastEvent(EventType.ITEM_CHANGED, item);
					}
				});
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}

			channel.BasicAck(ea.DeliveryTag, false);
			return;
		};
		channel.BasicConsume
		(
			queue: QUEUE_NAME,
			autoAck: false,
			consumer: consumer
		);
		Console.WriteLine("Rabbit successfully started");
	}

	public void StopRabbit()
	{
		channel.Close();
		connection.Close();
		Console.WriteLine("Rabbit successfully stopped");
	}

	public static Rabbit GetInstance()
	{
		if (instance == null)
		{
			instance = new Rabbit();
		}
		return instance;
	}

	public enum EventType
	{
		ITEM_ADDED, ITEM_REMOVED, ITEM_CHANGED
	}

	public void BroadcastEvent(EventType type, PhoneModel item)
	{
		switch (type)
		{
			case EventType.ITEM_ADDED:
				BroadcastEvent(ITEM_ADDED_ROUTING_KEY, item);
				return;
			case EventType.ITEM_REMOVED:
				BroadcastEvent(ITEM_REMOVED_ROUTING_KEY, item);
				return;
			case EventType.ITEM_CHANGED:
				BroadcastEvent(ITEM_CHANGED_ROUTING_KEY, item);
				return;
		}
	}

	private void BroadcastEvent(string routingKey, PhoneModel item)
	{
		using (var connection = rabbit.CreateConnection())
		using (var channel = connection.CreateModel())
		{
			var properties = channel.CreateBasicProperties();
			properties.Persistent = true;

			var serialized = JsonConvert.SerializeObject(item);
			var body = Encoding.UTF8.GetBytes(serialized);

			channel.BasicPublish
			(
				exchange: EXCHANGE_NAME,
				routingKey: routingKey,
				mandatory: true,
				basicProperties: properties,
				body: body
			);
		}
	}
	
	private Realm GetRealmInstance()
	{
		return Realm.GetInstance("stock");
	}
}
