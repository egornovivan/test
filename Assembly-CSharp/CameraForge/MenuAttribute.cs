using System;

namespace CameraForge;

public class MenuAttribute : Attribute
{
	public string Name;

	public int Order;

	public MenuAttribute(string name, int order = 0)
	{
		Name = name;
		Order = order;
	}
}
