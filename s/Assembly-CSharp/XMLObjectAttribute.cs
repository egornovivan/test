using System;

[AttributeUsage(AttributeTargets.Class)]
public class XMLObjectAttribute : Attribute
{
	public string Name = "OBJECT";

	public XMLObjectAttribute(string name)
	{
		Name = name;
	}
}
