using System.Collections.Generic;
using UnityEngine;

public class TypeSearchData
{
	public int m_TargetID;

	public int m_ScriptID;

	public int m_SceneType;

	public string m_Desc;

	public int m_NpcID;

	public Vector3 m_DistPos;

	public int m_DistRadius;

	public int m_TrackRadius;

	public List<int> m_Prompt;

	public List<int> m_TalkID;

	public AdMissionRand m_mr;

	public int m_AdNpcRadius;

	public List<int> m_ReceiveList;

	public List<AdTalkInfo> m_AdTalkID;

	public List<AdTalkInfo> m_AdPrompt;

	public List<int> m_CreateNpcList;

	public bool m_notForDungeon = true;

	public TypeSearchData()
	{
		m_Prompt = new List<int>();
		m_TalkID = new List<int>();
		m_AdPrompt = new List<AdTalkInfo>();
		m_AdTalkID = new List<AdTalkInfo>();
		m_ReceiveList = new List<int>();
		m_CreateNpcList = new List<int>();
	}
}
