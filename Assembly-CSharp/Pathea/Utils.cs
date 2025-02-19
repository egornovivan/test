using UnityEngine;

namespace Pathea;

public static class Utils
{
	public static Transform GetChild(Transform parent, string childName)
	{
		if (childName == string.Empty)
		{
			return null;
		}
		foreach (Transform item in parent)
		{
			if (item.name.Equals(childName))
			{
				return item;
			}
			Transform child = GetChild(item, childName);
			if (child != null)
			{
				return child;
			}
		}
		return null;
	}
}
