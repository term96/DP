using System.Collections.Generic;
using Realms;

namespace Shop.Models
{
	public class OrderModel : RealmObject
	{
		[PrimaryKey]
		public string id { get; set; }
		public IList<PhoneModel> items { get; }
		public string address { get; set; }
		public string contacts { get; set; }
		public string status { get; set; }
	}
}
