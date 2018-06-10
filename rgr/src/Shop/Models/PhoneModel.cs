using Realms;

namespace Shop.Models
{
	public class PhoneModel : RealmObject
	{
		[PrimaryKey]
		public string id { get; set; }
		public string vendor { get; set; }
		public string model { get; set; }
		public string system { get; set; }
		public string processor { get; set; }
		public string screen { get; set; }
		public string camera { get; set; }
		public float ram { get; set; }
		public int battery { get; set; }
		public int available { get; set; }
		public int price { get; set; }
		public string imgUrl { get; set; }
	}
}
