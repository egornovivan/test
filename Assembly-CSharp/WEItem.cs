[XMLObject("ITEM")]
public class WEItem : WEEntity
{
	[XMLIO(Attr = "canPickup", DefaultValue = true)]
	public bool CanPickup = true;
}
