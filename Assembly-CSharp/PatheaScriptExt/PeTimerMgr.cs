using System.Collections.Generic;
using UnityEngine;

namespace PatheaScriptExt;

public class PeTimerMgr
{
	private static PeTimerMgr instance;

	private Dictionary<string, PETimer> mDicTimer;

	public static PeTimerMgr Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new PeTimerMgr();
			}
			return instance;
		}
	}

	public void Add(string name, PETimer timer)
	{
		if (mDicTimer == null)
		{
			mDicTimer = new Dictionary<string, PETimer>(2);
		}
		if (mDicTimer.ContainsKey(name))
		{
			Debug.LogError("timer:" + name + " exist.");
		}
		else
		{
			mDicTimer.Add(name, timer);
		}
	}

	public bool Remove(string name)
	{
		if (mDicTimer == null)
		{
			return false;
		}
		return mDicTimer.Remove(name);
	}

	public PETimer Get(string name)
	{
		if (mDicTimer == null)
		{
			return null;
		}
		if (!mDicTimer.ContainsKey(name))
		{
			return null;
		}
		return mDicTimer[name];
	}

	public void Update()
	{
		if (mDicTimer == null)
		{
			return;
		}
		foreach (PETimer value in mDicTimer.Values)
		{
			value.Update(Time.deltaTime);
		}
	}
}
