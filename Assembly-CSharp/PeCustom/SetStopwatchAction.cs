using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("SET STOPWATCH", true)]
public class SetStopwatchAction : Action
{
	private int id;

	private string name = string.Empty;

	private EFunc func_time;

	private double amt;

	private EFunc func_speed;

	private float speed;

	protected override void OnCreate()
	{
		id = Utility.ToInt(base.missionVars, base.parameters["id"]);
		name = Utility.ToText(base.missionVars, base.parameters["string"]);
		func_time = Utility.ToFunc(base.parameters["funct"]);
		amt = Utility.ToDouble(base.missionVars, base.parameters["amount"]);
		func_speed = Utility.ToFunc(base.parameters["funcs"]);
		speed = Utility.ToSingle(base.missionVars, base.parameters["speed"]);
	}

	public override bool Logic()
	{
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.stopwatchMgr != null)
		{
			PeCustomScene.Self.scenario.stopwatchMgr.SetStopwatch(id, name, func_time, amt, func_speed, speed);
		}
		else
		{
			Debug.LogError("SetStopwatch - target is null");
		}
		return true;
	}
}
