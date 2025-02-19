using PatheaScript;
using UnityEngine;

namespace PatheaScriptExt;

public class ScriptTester : MonoBehaviour
{
	private const float axis_x_testnpc1 = 720f;

	private byte[] scriptData;

	private PsScriptMgr scriptMgr;

	private void OnGUI()
	{
		TimerMgrGui();
		ScriptToolGui();
	}

	private void Update()
	{
		PeTimerMgr.Instance.Update();
		if (scriptMgr != null)
		{
			scriptMgr.Tick();
		}
	}

	private void TimerMgrGui()
	{
		PETimer pETimer = PeTimerMgr.Instance.Get("1");
		if (pETimer != null)
		{
			GUI.Label(new Rect(500f, 50f, 100f, 20f), pETimer.FormatString("hh:mm:ss"));
		}
	}

	private void ScriptToolGui()
	{
		if (GUILayout.Button("ScriptLaunch"))
		{
			PeFactory factory = new PeFactory();
			scriptMgr = PsScriptMgr.Create(factory);
		}
		if (GUILayout.Button("ScriptStore") && scriptMgr != null)
		{
			scriptData = PsScriptMgr.Serialize(scriptMgr);
		}
		if (GUILayout.Button("ScriptRestore") && scriptData != null)
		{
			scriptMgr = PsScriptMgr.Deserialize(new PeFactory(), scriptData);
		}
	}
}
