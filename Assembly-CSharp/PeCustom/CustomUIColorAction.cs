using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("CUSTOM UI COLOR", true)]
public class CustomUIColorAction : Action
{
	private Color uicolor;

	protected override void OnCreate()
	{
		uicolor = Utility.ToColor(base.missionVars, base.parameters["uicolor"]);
	}

	public override bool Logic()
	{
		GUI.color = uicolor;
		return true;
	}
}
