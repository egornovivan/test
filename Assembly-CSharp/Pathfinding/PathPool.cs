using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

public static class PathPool<T> where T : Path, new()
{
	private static Stack<T> pool;

	private static int totalCreated;

	static PathPool()
	{
		pool = new Stack<T>();
	}

	public static void Recycle(T path)
	{
		lock (pool)
		{
			path.recycled = true;
			path.OnEnterPool();
			pool.Push(path);
		}
	}

	public static void Warmup(int count, int length)
	{
		ListPool<GraphNode>.Warmup(count, length);
		ListPool<Vector3>.Warmup(count, length);
		Path[] array = new Path[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = GetPath();
			array[i].Claim(array);
		}
		for (int j = 0; j < count; j++)
		{
			array[j].Release(array);
		}
	}

	public static int GetTotalCreated()
	{
		return totalCreated;
	}

	public static int GetSize()
	{
		return pool.Count;
	}

	public static T GetPath()
	{
		lock (pool)
		{
			T val;
			if (pool.Count > 0)
			{
				val = pool.Pop();
			}
			else
			{
				val = new T();
				totalCreated++;
			}
			val.recycled = false;
			val.Reset();
			return val;
		}
	}
}
