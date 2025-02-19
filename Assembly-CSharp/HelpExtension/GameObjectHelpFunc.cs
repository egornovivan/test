using System.Collections.Generic;
using UnityEngine;

namespace HelpExtension;

public static class GameObjectHelpFunc
{
	public static void RefreshItem(this List<GameObject> old_item, int new_count, GameObject prefab, Transform parent = null)
	{
		int count = old_item.Count;
		if (new_count <= count)
		{
			for (int num = count - 1; num >= new_count; num--)
			{
				Object.Destroy(old_item[num]);
				GameObject gameObject = old_item[num];
				gameObject.transform.parent = null;
				old_item.RemoveAt(num);
			}
		}
		else
		{
			for (int i = count; i < new_count; i++)
			{
				GameObject item = prefab.CreateNew(parent);
				old_item.Add(item);
			}
		}
	}

	public static GameObject CreateNew(this GameObject obj, Transform parent = null)
	{
		GameObject gameObject = Object.Instantiate(obj);
		gameObject.transform.parent = parent;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localRotation = Quaternion.identity;
		return gameObject;
	}
}
