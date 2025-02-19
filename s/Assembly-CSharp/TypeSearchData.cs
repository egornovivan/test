using System.Collections.Generic;
using UnityEngine;

public class TypeSearchData
{
	public int m_TargetID;

	public int m_ScriptID;

	public string m_Desc;

	public string m_NpcName;

	public Vector3 m_DistPos;

	public int m_DistRadius;

	public List<int> m_Prompt;

	public List<int> m_TalkID;

	public int m_AdDist;

	public int m_AdRadius;

	public int m_AdNpcRadius;

	public List<int> m_ReceiveList;

	public List<AdTalkInfo> m_AdTalkID;

	public List<AdTalkInfo> m_AdPrompt;

	public List<int> m_CreateNpcList;

	public AdMissionRand m_mr;

	public TypeSearchData()
	{
		m_NpcName = "0";
		m_Prompt = new List<int>();
		m_TalkID = new List<int>();
		m_AdPrompt = new List<AdTalkInfo>();
		m_AdTalkID = new List<AdTalkInfo>();
		m_ReceiveList = new List<int>();
		m_CreateNpcList = new List<int>();
	}
}
