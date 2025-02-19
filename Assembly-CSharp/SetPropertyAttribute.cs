using UnityEngine;

public class SetPropertyAttribute : PropertyAttribute
{
	public string Name { get; private set; }

	public bool IsDirty { get; set; }

	public SetPropertyAttribute(string name)
	{
		Name = name;
	}
}
