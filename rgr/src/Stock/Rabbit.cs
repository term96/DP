using System;
using System.Text;
using Stock.Models;
using RabbitMQ.Client;
using Newtonsoft.Json;

public class Rabbit
{
	private static readonly ConnectionFactory rabbit = new ConnectionFactory();
	private static readonly string EXCHANGE_NAME = "MobileShopExchange";
	private static readonly string ITEM_ADDED_ROUTING_KEY = "ItemAdded";
	private static readonly string ITEM_REMOVED_ROUTING_KEY = "ItemRemoved";
	private static readonly string ITEM_CHANGED_ROUTING_KEY = "ItemChanged";
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
		Console.WriteLine("Rabbit successfully started");
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
}
