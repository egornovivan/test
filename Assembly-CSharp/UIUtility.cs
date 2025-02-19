using System;
using System.Collections.Generic;
using UnityEngine;

public static class UIUtility
{
	public static T CreateItem<T>(T prefab, Transform parent) where T : MonoBehaviour
	{
		T result = UnityEngine.Object.Instantiate(prefab);
		Transform transform = result.transform;
		transform.parent = parent;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one;
		transform.gameObject.SetActive(value: true);
		return result;
	}

	public static void UpdateListGos(List<GameObject> goList, GameObject prefab, Transform parent, int count, Action<int, GameObject> setItemContent, Action<GameObject> destroyItem)
	{
		if (count > goList.Count)
		{
			for (int i = 0; i < goList.Count; i++)
			{
				setItemContent?.Invoke(i, goList[i]);
			}
			for (int j = goList.Count; j < count; j++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
				Transform transform = gameObject.transform;
				gameObject.transform.parent = parent.transform;
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
				transform.localScale = Vector3.one;
				transform.gameObject.SetActive(value: true);
				goList.Add(gameObject);
				setItemContent?.Invoke(j, gameObject);
			}
		}
		else
		{
			for (int k = 0; k < count; k++)
			{
				setItemContent?.Invoke(k, goList[k]);
			}
			for (int num = goList.Count - 1; num >= count; num--)
			{
				destroyItem?.Invoke(goList[num]);
				UnityEngine.Object.Destroy(goList[num]);
				goList[num].transform.parent = null;
				goList.RemoveAt(num);
			}
		}
	}

	public static void UpdateListItems<T>(List<T> itemList, T prefab, Transform parent, int count, Action<int, T> setItemContent, Action<T> destroyItem) where T : MonoBehaviour
	{
		count = ((count >= 0) ? count : 0);
		if (count > itemList.Count)
		{
			for (int i = 0; i < itemList.Count; i++)
			{
				setItemContent?.Invoke(i, itemList[i]);
			}
			int num = count;
			for (int j = itemList.Count; j < num; j++)
			{
				T val = UnityEngine.Object.Instantiate(prefab);
				Transform transform = val.transform;
				val.transform.parent = parent.transform;
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
				transform.localScale = Vector3.one;
				transform.gameObject.SetActive(value: true);
				itemList.Add(val);
				setItemContent?.Invoke(j, val);
			}
		}
		else
		{
			for (int k = 0; k < count; k++)
			{
				setItemContent?.Invoke(k, itemList[k]);
			}
			for (int num2 = itemList.Count - 1; num2 >= count; num2--)
			{
				destroyItem?.Invoke(itemList[num2]);
				T val2 = itemList[num2];
				UnityEngine.Object.Destroy(val2.gameObject);
				T val3 = itemList[num2];
				val3.transform.parent = null;
				itemList.RemoveAt(num2);
			}
		}
	}
}
