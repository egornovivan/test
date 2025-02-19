using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace RedGrass;

public sealed class RGListPool<T> where T : class, new()
{
	private Stack<List<T>> mPool;

	public RGListPool(int capacity = 20, int per_list_cap = 100)
	{
		mPool = new Stack<List<T>>(capacity);
		for (int i = 0; i < mPool.Count; i++)
		{
			mPool.Push(new List<T>(per_list_cap));
		}
	}

	public List<T> Get()
	{
		lock (mPool)
		{
			if (mPool.Count == 0)
			{
				return new List<T>();
			}
			return mPool.Pop();
		}
	}

	public void Recycle(List<T> item)
	{
		lock (mPool)
		{
			mPool.Push(item);
		}
	}
}
public sealed class RGListPool
{
	private Queue<Dictionary<int, List<RedGrassInstance>>> mTempPool;

	private Stack<List<RedGrassInstance>> mPool;

	private bool mRunning;

	private int mCapacity = 10;

	private Thread mThread;

	public RGListPool(int capacity, int per_list_cap, string name = "None")
	{
		mPool = new Stack<List<RedGrassInstance>>(capacity);
		for (int i = 0; i < capacity; i++)
		{
			mPool.Push(new List<RedGrassInstance>(per_list_cap));
		}
		mTempPool = new Queue<Dictionary<int, List<RedGrassInstance>>>();
		mCapacity = per_list_cap;
		if (mThread == null)
		{
			mRunning = true;
			mThread = new Thread(Run);
			mThread.Name = name.ToString() + " Pool ";
			mThread.Start();
		}
	}

	public void Destroy()
	{
		mRunning = false;
	}

	~RGListPool()
	{
		mRunning = false;
	}

	public List<RedGrassInstance> Get()
	{
		lock (mPool)
		{
			if (mPool.Count == 0)
			{
				return new List<RedGrassInstance>(mCapacity);
			}
			return mPool.Pop();
		}
	}

	public void Recycle(Dictionary<int, List<RedGrassInstance>> grasses)
	{
		lock (mTempPool)
		{
			mTempPool.Enqueue(grasses);
		}
	}

	public void Recycle(List<RedGrassInstance> rgi)
	{
		lock (mPool)
		{
			mPool.Push(rgi);
		}
	}

	private void Run()
	{
		try
		{
			while (mRunning)
			{
				while (mTempPool.Count != 0)
				{
					Dictionary<int, List<RedGrassInstance>> dictionary = null;
					lock (mTempPool)
					{
						dictionary = mTempPool.Dequeue();
					}
					foreach (List<RedGrassInstance> value in dictionary.Values)
					{
						if (value != null)
						{
							lock (mPool)
							{
								mPool.Push(value);
							}
						}
					}
				}
				Thread.Sleep(10);
			}
		}
		catch (SystemException ex)
		{
			Debug.LogError("<<<< Data Pool Thread error >>>> \r\n" + ex);
		}
	}
}
