using System;
using System.Collections.Generic;
using Pathea;
using UnityEngine;

namespace PeMap;

public class MyBaseListSingleton<T0, T> : MonoLikeSingleton<T0> where T0 : class, new()
{
	public List<T> mList = new List<T>();

	public void ForEach(Action<T> action)
	{
		mList.ForEach(action);
	}

	public void RemoveAll(Predicate<T> match)
	{
		mList.RemoveAll(match);
	}

	public T Find(Predicate<T> predicate)
	{
		return mList.Find(predicate);
	}

	public List<T> FindAll(Predicate<T> predicate)
	{
		return mList.FindAll(predicate);
	}

	public virtual bool Add(T item)
	{
		if (item != null)
		{
			int num = mList.FindIndex((T a) => ((ILabel)(object)item).CompareTo((ILabel)(object)a));
			if (num >= 0)
			{
				return false;
			}
			mList.Add(item);
			return true;
		}
		Debug.Log("MapLabel Item is null: " + item);
		return false;
	}

	public virtual bool Remove(T item)
	{
		if (item == null)
		{
			return false;
		}
		int num = mList.FindIndex((T a) => ((ILabel)(object)item).CompareTo((ILabel)(object)a));
		if (num >= 0)
		{
			mList.RemoveAt(num);
			return true;
		}
		return false;
	}
}
