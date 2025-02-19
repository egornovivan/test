using System.Collections.Generic;
using UnityEngine;

public class ItemOpBtn_N : MonoBehaviour
{
	public UILabel mButtonName;

	private string m_CmdStr = string.Empty;

	private Dictionary<string, int> m_DicCmds = new Dictionary<string, int>
	{
		{ "Turn", 8000559 },
		{ "Get", 8000560 },
		{ "Sleep", 8000561 },
		{ "Get On", 8000562 },
		{ "Open", 8000563 },
		{ "Shut", 8000564 },
		{ "Water", 8000565 },
		{ "Clean", 8000566 },
		{ "Remove", 8000567 },
		{ "Refill", 8000568 },
		{ "Rotate Pivot", 8000569 },
		{ "Turn Off", 8000570 },
		{ "Turn On", 8000571 },
		{ "Sit", 8000572 }
	};

	public void InitButton(string cmdStr, GameObject parentObj)
	{
		m_CmdStr = cmdStr;
		if (m_DicCmds.ContainsKey(cmdStr))
		{
			mButtonName.text = PELocalization.GetString(m_DicCmds[cmdStr]);
		}
		else
		{
			mButtonName.text = cmdStr;
		}
		mButtonName.MakePixelPerfect();
	}

	private void OnClick()
	{
		if (Input.GetMouseButtonUp(0))
		{
			GameUI.Instance.mItemOp.CallFunction(m_CmdStr);
		}
	}
}
