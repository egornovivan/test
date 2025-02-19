using System.IO;
using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("WAIT")]
public class WaitAction : Action
{
	private float amt;

	private float cur;

	protected override void OnCreate()
	{
		amt = Utility.ToSingle(base.missionVars, base.parameters["amount"]);
	}

	public override bool Logic()
	{
		cur += Time.deltaTime;
		if (cur >= amt)
		{
			cur = amt;
			return true;
		}
		return false;
	}

	public override void RestoreState(BinaryReader r)
	{
		cur = r.ReadSingle();
	}

	public override void StoreState(BinaryWriter w)
	{
		w.Write(cur);
	}
}
