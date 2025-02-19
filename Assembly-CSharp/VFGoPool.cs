using System.Collections.Generic;
using UnityEngine;

public static class VFGoPool<T> where T : MonoBehaviour, IRecyclable
{
	private static Stack<T> _itemGos = new Stack<T>();

	private static List<T> _itemsToFreeInThread = new List<T>();

	private static List<T> _itemsToFreeInMain = new List<T>();

	public static void PreAlloc(int nPreAlloc)
	{
		_itemGos = new Stack<T>(nPreAlloc);
		_itemGos.Clear();
		for (int i = 0; i < nPreAlloc; i++)
		{
			GameObject gameObject = new GameObject();
			T t = gameObject.AddComponent<T>();
			gameObject.SetActive(value: false);
			_itemGos.Push(t);
		}
	}

	public static T GetGo()
	{
		if (_itemGos.Count == 0)
		{
			GameObject gameObject = new GameObject();
			T result = gameObject.AddComponent<T>();
			gameObject.SetActive(value: false);
			return result;
		}
		return _itemGos.Pop();
	}

	public static void FreeGo(T item)
	{
		item.OnRecycle();
		_itemGos.Push(item);
	}

	public static void ReqFreeGo(T item)
	{
		lock (_itemsToFreeInThread)
		{
			_itemsToFreeInThread.Add(item);
		}
	}

	public static void SwapReq()
	{
		if (_itemsToFreeInThread.Count > 0)
		{
			lock (_itemsToFreeInThread)
			{
				_itemsToFreeInMain.AddRange(_itemsToFreeInThread);
				_itemsToFreeInThread.Clear();
			}
		}
	}

	public static void ExecFreeGo()
	{
		if (_itemsToFreeInMain.Count <= 0)
		{
			return;
		}
		int count = _itemsToFreeInMain.Count;
		for (int i = 0; i < count; i++)
		{
			if (_itemsToFreeInMain[i] != null)
			{
				FreeGo(_itemsToFreeInMain[i]);
			}
		}
		_itemsToFreeInMain.Clear();
	}
}
