using System.Collections.Generic;
using UnityEngine;

public class TypeUseItemData
{
	public int m_TargetID;

	public int m_ScriptID;

	public string m_Desc;

	public int m_Type;

	public int m_ItemID;

	public int m_UseNum;

	public Vector3 m_Pos;

	public int m_Radius;

	public List<int> m_UsedPrompt;

	public List<int> m_TalkID;

	public List<int> m_FailPrompt;

	public List<int> m_ReceiveList;

	public AdMissionRand m_AdDistPos;

	public bool m_comMission;

	public bool m_allowOld;

	public TypeUseItemData()
	{
		m_UsedPrompt = new List<int>();
		m_TalkID = new List<int>();
		m_FailPrompt = new List<int>();
		m_ReceiveList = new List<int>();
		m_AdDistPos = default(AdMissionRand);
	}
}
