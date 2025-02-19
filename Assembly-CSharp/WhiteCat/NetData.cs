using System;

namespace WhiteCat;

public class NetData<T> where T : struct
{
	private Func<T, bool> needSync;

	private Func<T> getData;

	private Action<T> setData;

	private T last;

	private bool finalSynced;

	public T lastData => last;

	public NetData(Func<T, bool> needSync, Func<T> getData, Action<T> setData)
	{
		this.needSync = needSync;
		this.getData = getData;
		this.setData = setData;
		last = getData();
	}

	public SyncAction GetSyncAction()
	{
		if (needSync(last))
		{
			finalSynced = false;
			return SyncAction.sync;
		}
		if (finalSynced)
		{
			return SyncAction.none;
		}
		finalSynced = true;
		return SyncAction.freeze;
	}

	public T GetData()
	{
		if (finalSynced)
		{
			return getData();
		}
		return last = getData();
	}

	public void SetData(T data)
	{
		setData(last = data);
	}
}
