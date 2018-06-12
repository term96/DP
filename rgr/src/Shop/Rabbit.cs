using System;
using System.Threading;
using System.Text;
using System.Linq;
using Shop.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using Realms;

public class Rabbit
{
	private static readonly ConnectionFactory rabbit = new ConnectionFactory();
	private const string EXCHANGE_NAME = "MobileShopExchange";
	private const string QUEUE_NAME = "ShopQueue";
	private const string ITEM_ADDED_ROUTING_KEY = "ItemAdded";
	private const string ITEM_REMOVED_ROUTING_KEY = "ItemRemoved";
	private const string ITEM_CHANGED_ROUTING_KEY = "ItemChanged";
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
		channel.QueueBind(queue: QUEUE_NAME, exchange: EXCHANGE_NAME, routingKey: ITEM_ADDED_ROUTING_KEY);
		channel.QueueBind(queue: QUEUE_NAME, exchange: EXCHANGE_NAME, routingKey: ITEM_REMOVED_ROUTING_KEY);
		channel.QueueBind(queue: QUEUE_NAME, exchange: EXCHANGE_NAME, routingKey: ITEM_CHANGED_ROUTING_KEY);
		CreateConsumer();
	}

	public void StopRabbit()
	{
		channel.Close();
		connection.Close();
		Console.WriteLine("Rabbit successfully stopped");
	}

	private void CreateConsumer()
	{
		var consumer = new EventingBasicConsumer(channel);
		consumer.Received += (model, ea) =>
		{
			var body = ea.Body;
			var serialized = Encoding.UTF8.GetString(body);
			PhoneModel item = null;
			try
			{
				item = JsonConvert.DeserializeObject<PhoneModel>(serialized);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}

			switch (ea.RoutingKey)
			{
				case ITEM_ADDED_ROUTING_KEY:
					AddOrUpdateItem(item, update: false);
					break;
				case ITEM_REMOVED_ROUTING_KEY:
					RemoveItem(item.id);
					break;
				case ITEM_CHANGED_ROUTING_KEY:
					AddOrUpdateItem(item, update: true);
					break;
			}

			channel.BasicAck(ea.DeliveryTag, false);
		};
		channel.BasicConsume
		(
			queue: QUEUE_NAME,
			autoAck: false,
			consumer: consumer
		);
		Console.WriteLine("Rabbit successfully started");
	}

	private void AddOrUpdateItem(PhoneModel item, bool update)
	{
		try
		{
			var db = GetRealmInstance();
			db.Write(() =>
			{
				db.Add(item, update);
			});
		}
		catch (Exception e)
		{
			Console.WriteLine(e.Message);
		}
	}

	private void RemoveItem(string id)
	{
		try
		{
			var db = GetRealmInstance();
			var allItems = db.All<PhoneModel>().Where(i => i.id.Equals(id));
			var item = allItems.Count() > 0 ? allItems.First() : null;
			if (item == null)
			{
				return;
			}

			db.Write(() => 
			{
				db.Remove(item);
			});
		}
		catch (Exception e)
		{
			Console.WriteLine(e.Message);
		}
	}

	private Realm GetRealmInstance()
	{
		return Realm.GetInstance("shop");
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
