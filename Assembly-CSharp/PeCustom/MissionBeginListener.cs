using System;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("MISSION BEGIN")]
public class MissionBeginListener : EventListener
{
	private int missionId;

	protected override void OnCreate()
	{
		missionId = Utility.ToEnumInt(base.parameters["mission"]);
	}

	public override void Listen()
	{
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.missionMgr != null)
		{
			MissionMgr missionMgr = PeCustomScene.Self.scenario.missionMgr;
			missionMgr.onRunMission = (Action<int>)Delegate.Combine(missionMgr.onRunMission, new Action<int>(OnResponse));
		}
	}

	public override void Close()
	{
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.missionMgr != null)
		{
			MissionMgr missionMgr = PeCustomScene.Self.scenario.missionMgr;
			missionMgr.onRunMission = (Action<int>)Delegate.Remove(missionMgr.onRunMission, new Action<int>(OnResponse));
		}
		else
		{
			Debug.LogError("Try to close eventlistener, but source has been destroyed");
		}
	}

	public void OnResponse(int mId)
	{
		if (missionId == -1 || (base.mission != null && missionId == 0 && mId == base.mission.dataId) || (base.mission != null && missionId == -2 && mId != base.mission.dataId) || missionId == mId)
		{
			Post();
		}
	}
}
