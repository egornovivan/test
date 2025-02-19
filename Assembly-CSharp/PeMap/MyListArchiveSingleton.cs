using System;
using System.Collections.Generic;
using System.IO;
using Pathea;
using PETools;

namespace PeMap;

public class MyListArchiveSingleton<T0, T> : ArchivableSingleton<T0> where T0 : class, new() where T : class, ISerializable, new()
{
	private List<T> mList = new List<T>();

	public void ForEach(Action<T> action)
	{
		mList.ForEach(action);
	}

	public T Find(Predicate<T> predicate)
	{
		return mList.Find(predicate);
	}

	public virtual void Add(T item)
	{
		mList.Add(item);
	}

	public virtual bool Remove(T item)
	{
		return mList.Remove(item);
	}

	protected override void SetData(byte[] data)
	{
		Serialize.Import(data, delegate(BinaryReader r)
		{
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				byte[] array = Serialize.ReadBytes(r);
				if (array != null)
				{
					T item = new T();
					item.Deserialize(array);
					Add(item);
				}
			}
		});
	}

	protected override void WriteData(BinaryWriter w)
	{
		w.Write(mList.Count);
		mList.ForEach(delegate(T item)
		{
			Serialize.WriteBytes(item.Serialize(), w);
		});
	}
}
