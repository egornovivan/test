using System;

namespace DunGen.Adapters;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AdapterDisplayName : Attribute
{
	public string Name { get; private set; }

	public AdapterDisplayName(string name)
	{
		Name = name;
	}
}
