using System.Collections;
using System.Collections.Generic;
using PeEvent;
using UnityEngine;

namespace Pathea;

public class PeLauncher : MonoBehaviour
{
	private class LaunchInfo
	{
		public float mTime;

		public ILaunchable mTarget;

		public LaunchInfo(float time, ILaunchable target)
		{
			mTime = time;
			mTarget = target;
		}
	}

	public class LoadFinishedArg : EventArg
	{
	}

	public interface ILaunchable
	{
		void Launch();
	}

	public delegate bool EndLaunch();

	private static PeLauncher instance;

	private List<LaunchInfo> mList = new List<LaunchInfo>(10);

	private EndLaunch mEndLaunch;

	public bool isLoading;

	private Event<LoadFinishedArg> mEventor = new Event<LoadFinishedArg>();

	public static PeLauncher Instance
	{
		get
		{
			if (null == instance)
			{
				instance = new GameObject("PeLauncher").AddComponent<PeLauncher>();
			}
			return instance;
		}
	}

	public Event<LoadFinishedArg> eventor => mEventor;

	public EndLaunch endLaunch
	{
		get
		{
			return mEndLaunch;
		}
		set
		{
			mEndLaunch = value;
		}
	}

	private void UpdateIndicator(float percent, string info)
	{
		Debug.Log(info + ", progress:" + percent);
		UILoadScenceEffect.Instance.SetProgress((int)(100f * percent));
	}

	private IEnumerator Load()
	{
		float curTime = 0f;
		float totalTime = 0f;
		mList.ForEach(delegate(LaunchInfo launch)
		{
			totalTime += launch.mTime;
		});
		foreach (LaunchInfo launch2 in mList)
		{
			launch2.mTarget.Launch();
			curTime += launch2.mTime;
			UpdateIndicator(curTime / totalTime, launch2.mTarget.GetType().ToString());
			yield return 0;
		}
		isLoading = false;
		while (mEndLaunch != null && !mEndLaunch())
		{
			yield return new WaitForSeconds(1f);
		}
		endLaunch = null;
		yield return new WaitForSeconds(3f);
		Object.Destroy(base.gameObject);
		UILoadScenceEffect.Instance.EnableProgress(enable: false);
		UILoadScenceEffect.Instance.BeginScence(null);
		eventor.Dispatch(new LoadFinishedArg());
		FastTravel.bTraveling = false;
	}

	public void Add(ILaunchable target, float time = 1f)
	{
		mList.Add(new LaunchInfo(time, target));
	}

	public void StartLoad()
	{
		isLoading = true;
		UILoadScenceEffect.Instance.EnableProgress(enable: true);
		StartCoroutine(Load());
	}
}
