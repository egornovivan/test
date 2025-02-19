using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("CUSTOM UI STANDARD", true)]
public class CustomUIStandardAction : Action
{
	private EUIType uitype;

	private Rect rect;

	private EUIAnchor anchor;

	private string uitext = string.Empty;

	private EUIStyle uistyle;

	private string uievent = string.Empty;

	protected override void OnCreate()
	{
		uitype = (EUIType)Utility.ToEnumInt(base.parameters["uitype"]);
		rect = Utility.ToRect(base.missionVars, base.parameters["rect"]);
		anchor = (EUIAnchor)Utility.ToEnumInt(base.parameters["anchor"]);
		uitext = Utility.ToText(base.missionVars, base.parameters["uitext"]);
		uistyle = (EUIStyle)Utility.ToEnumInt(base.parameters["uistyle"]);
		uievent = Utility.ToVarname(base.parameters["uievent"]);
	}

	public override bool Logic()
	{
		if (uistyle != 0)
		{
		}
		switch (anchor)
		{
		case EUIAnchor.Streched:
			rect.x = Mathf.Clamp(rect.x, -0.1f, 1.1f);
			rect.y = Mathf.Clamp(rect.y, -0.1f, 1.1f);
			rect.width = Mathf.Clamp01(rect.width);
			rect.height = Mathf.Clamp01(rect.height);
			rect = new Rect(rect.x * (float)Screen.width, rect.y * (float)Screen.height, rect.width * (float)Screen.width, rect.height * (float)Screen.height);
			break;
		case EUIAnchor.LowerLeft:
			rect.y += (float)Screen.height;
			break;
		case EUIAnchor.LowerCenter:
			rect.x += (float)(Screen.width / 2);
			rect.y += (float)Screen.height;
			break;
		case EUIAnchor.LowerRight:
			rect.x += (float)Screen.width;
			rect.y += (float)Screen.height;
			break;
		case EUIAnchor.MiddleLeft:
			rect.y += (float)(Screen.height / 2);
			break;
		case EUIAnchor.Center:
			rect.x += (float)(Screen.width / 2);
			rect.y += (float)(Screen.height / 2);
			break;
		case EUIAnchor.MiddleRight:
			rect.x += (float)Screen.width;
			rect.y += (float)(Screen.height / 2);
			break;
		case EUIAnchor.UpperCenter:
			rect.x += (float)(Screen.width / 2);
			break;
		case EUIAnchor.UpperRight:
			rect.x += (float)Screen.width;
			break;
		}
		switch (uitype)
		{
		case EUIType.Label:
			GUI.Label(rect, uitext);
			break;
		case EUIType.Button:
			if (GUI.Button(rect, uitext))
			{
				uievent = uievent.Trim();
				if (!string.IsNullOrEmpty(uievent) && PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.guiMgr != null)
				{
					PeCustomScene.Self.scenario.guiMgr.GUIResponse(uievent);
				}
			}
			break;
		case EUIType.Box:
			GUI.Box(rect, uitext);
			break;
		}
		return true;
	}
}
