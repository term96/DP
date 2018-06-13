using Realms;

public class OrderItem : RealmObject
{
	public string id { get; set; }
	public string vendor { get; set; }
	public string model { get; set; }
	public int price { get; set; }
}
