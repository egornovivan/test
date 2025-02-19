using System;
using System.Collections.Generic;

namespace PeMap;

public class MyBaseList<T> where T : class, new()
{
	protected List<T> mList;

	public MyBaseList(int capacity)
	{
		mList = new List<T>(capacity);
	}

	public void ForEach(Action<T> action)
	{
		mList.ForEach(action);
	}

	public T Find(Predicate<T> predicate)
	{
		return mList.Find(predicate);
	}

	public List<T> FindAll(Predicate<T> predicate)
	{
		return mList.FindAll(predicate);
	}

	public void Add(T item)
	{
		mList.Add(item);
	}

	public bool Remove(T item)
	{
		return mList.Remove(item);
	}
}
