using System;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("MISSION END")]
public class MissionEndListener : EventListener
{
	private int missionId;

	private EMissionResult result;

	protected override void OnCreate()
	{
		missionId = Utility.ToEnumInt(base.parameters["mission"]);
		result = (EMissionResult)Utility.ToEnumInt(base.parameters["result"]);
	}

	public override void Listen()
	{
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.missionMgr != null)
		{
			MissionMgr missionMgr = PeCustomScene.Self.scenario.missionMgr;
			missionMgr.onCloseMission = (Action<int, EMissionResult>)Delegate.Combine(missionMgr.onCloseMission, new Action<int, EMissionResult>(OnResponse));
		}
	}

	public override void Close()
	{
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.missionMgr != null)
		{
			MissionMgr missionMgr = PeCustomScene.Self.scenario.missionMgr;
			missionMgr.onCloseMission = (Action<int, EMissionResult>)Delegate.Remove(missionMgr.onCloseMission, new Action<int, EMissionResult>(OnResponse));
		}
		else
		{
			Debug.LogError("Try to close eventlistener, but source has been destroyed");
		}
	}

	public void OnResponse(int mId, EMissionResult res)
	{
		if ((missionId == -1 || (base.mission != null && missionId == 0 && mId == base.mission.dataId) || (base.mission != null && missionId == -2 && mId != base.mission.dataId) || missionId == mId) && (result == res || result == EMissionResult.Any))
		{
			Post();
		}
	}
}
