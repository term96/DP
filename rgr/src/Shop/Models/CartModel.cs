using System.Collections.Generic;
using Realms;

namespace Shop.Models
{
	public class CartModel : RealmObject
	{
		public IList<PhoneModel> items { get; }
	}
}
