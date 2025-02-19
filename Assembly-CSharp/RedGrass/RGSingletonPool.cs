using System.Collections.Generic;

namespace RedGrass;

public sealed class RGSingletonPool<T> where T : class, new()
{
	private Stack<T> mPool;

	public RGSingletonPool(int capacity = 100)
	{
		mPool = new Stack<T>(capacity);
		for (int i = 0; i < capacity; i++)
		{
			mPool.Push(new T());
		}
	}

	public T Get()
	{
		lock (mPool)
		{
			if (mPool.Count == 0)
			{
				return new T();
			}
			return mPool.Pop();
		}
	}

	public void Recycle(T item)
	{
		lock (mPool)
		{
			mPool.Push(item);
		}
	}
}
