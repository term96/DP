using System.Collections.Generic;
using Realms;

namespace Delivery.Models
{
	public class OrderModel : RealmObject
	{
		[PrimaryKey]
		public string id { get; set; }
		public IList<OrderItem> items { get; }
		public string address { get; set; }
		public string contacts { get; set; }
		public string status { get; set; }
	}
}
