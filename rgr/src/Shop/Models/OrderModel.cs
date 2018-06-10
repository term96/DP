using System.Collections.Generic;
using Realms;

namespace Shop.Models
{
	public class OrderModel : RealmObject
	{
		public IList<PhoneModel> items { get; }
		public string address { get; set; }
		public string contacts { get; set; }
	}
}
