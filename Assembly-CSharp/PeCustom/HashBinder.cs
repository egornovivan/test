using System;
using System.Collections.Generic;
using UnityEngine;

namespace PeCustom;

public class HashBinder
{
	private Dictionary<Type, object> mBinders = new Dictionary<Type, object>();

	public void Bind<T>(T obj) where T : class
	{
		Type type = obj.GetType();
		if (mBinders.ContainsKey(type))
		{
			Debug.LogError("this type is already exist");
		}
		else
		{
			mBinders.Add(type, obj);
		}
	}

	public T Get<T>() where T : class
	{
		Type typeFromHandle = typeof(T);
		if (mBinders.ContainsKey(typeFromHandle))
		{
			return mBinders[typeFromHandle] as T;
		}
		return (T)null;
	}

	public void Unbind<T>() where T : class
	{
		mBinders.Remove(typeof(T));
	}
}
