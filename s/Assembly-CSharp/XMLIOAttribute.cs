using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class XMLIOAttribute : Attribute
{
	public int Order;

	public string Attr = "xmlattr";

	public bool Necessary;

	public object DefaultValue;

	public XMLIOAttribute()
	{
	}

	public XMLIOAttribute(string attr)
	{
		Order = 0;
		Attr = attr;
		Necessary = false;
		DefaultValue = null;
	}
}
