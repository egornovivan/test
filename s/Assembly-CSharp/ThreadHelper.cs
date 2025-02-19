using System;
using System.Collections.Generic;
using System.Threading;

public class ThreadHelper
{
	private static ThreadHelper mInstance;

	private List<Thread> mThreadPool;

	private Mutex mMutex;

	public static ThreadHelper Instance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = new ThreadHelper();
				mInstance.Init();
			}
			return mInstance;
		}
	}

	private void Init()
	{
		mThreadPool = new List<Thread>();
	}

	public void StartThread(ThreadStart action)
	{
		Thread thread = new Thread(action);
		thread.Start();
		mThreadPool.Add(thread);
	}

	public void CheckSingletonServer()
	{
		bool flag = false;
		try
		{
			Mutex mutex = Mutex.OpenExisting(ServerConfig.ServerName);
			if (!object.ReferenceEquals(mutex, mMutex))
			{
				mutex.Close();
			}
		}
		catch (WaitHandleCannotBeOpenedException)
		{
			mMutex = new Mutex(initiallyOwned: true, ServerConfig.ServerName);
			flag = true;
		}
		if (!flag)
		{
			throw new Exception("Server " + ServerConfig.ServerName + " is already existed.");
		}
	}

	private void ReleaseMutex()
	{
		if (mMutex != null)
		{
			mMutex.Close();
			mMutex = null;
		}
	}

	public void Reset()
	{
		for (int i = 0; i < mThreadPool.Count; i++)
		{
			if (mThreadPool[i] != null)
			{
				mThreadPool[i].Abort();
			}
		}
		mThreadPool.Clear();
		ReleaseMutex();
	}
}
