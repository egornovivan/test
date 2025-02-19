using System.Collections.Generic;

namespace Pathfinding.Util;

public static class ListPool<T>
{
	private const int MaxCapacitySearchLength = 8;

	private static List<List<T>> pool;

	static ListPool()
	{
		pool = new List<List<T>>();
	}

	public static List<T> Claim()
	{
		lock (pool)
		{
			if (pool.Count > 0)
			{
				List<T> result = pool[pool.Count - 1];
				pool.RemoveAt(pool.Count - 1);
				return result;
			}
			return new List<T>();
		}
	}

	public static List<T> Claim(int capacity)
	{
		lock (pool)
		{
			if (pool.Count > 0)
			{
				List<T> list = null;
				int i;
				for (i = 0; i < pool.Count && i < 8; i++)
				{
					list = pool[pool.Count - 1 - i];
					if (list.Capacity >= capacity)
					{
						pool.RemoveAt(pool.Count - 1 - i);
						return list;
					}
				}
				if (list == null)
				{
					list = new List<T>(capacity);
				}
				else
				{
					list.Capacity = capacity;
					pool[pool.Count - i] = pool[pool.Count - 1];
					pool.RemoveAt(pool.Count - 1);
				}
				return list;
			}
			return new List<T>(capacity);
		}
	}

	public static void Warmup(int count, int size)
	{
		lock (pool)
		{
			List<T>[] array = new List<T>[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = Claim(size);
			}
			for (int j = 0; j < count; j++)
			{
				Release(array[j]);
			}
		}
	}

	public static void Release(List<T> list)
	{
		list.Clear();
		lock (pool)
		{
			pool.Add(list);
		}
	}

	public static void Clear()
	{
		lock (pool)
		{
			pool.Clear();
		}
	}

	public static int GetSize()
	{
		return pool.Count;
	}
}
