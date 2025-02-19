using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("SHOW TITLE", true)]
public class ShowTitleAction : Action
{
	private string title;

	private string subtitle;

	private float time;

	private Color color;

	protected override void OnCreate()
	{
		title = Utility.ToText(base.missionVars, base.parameters["title"]);
		subtitle = Utility.ToText(base.missionVars, base.parameters["subtitle"]);
		time = Utility.ToSingle(base.missionVars, base.parameters["time"]);
		color = Utility.ToColor(base.missionVars, base.parameters["color"]);
	}

	public override bool Logic()
	{
		if (time < 1f)
		{
			time = 1f;
		}
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.guiMgr != null)
		{
			PeCustomScene.Self.scenario.guiMgr.ShowTitle(title, subtitle, time, color);
		}
		return true;
	}
}
