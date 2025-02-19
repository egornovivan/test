using System.Collections.Generic;
using UnityEngine;

public class TypeFollowData
{
	public int m_TargetID;

	public int m_ScriptID;

	public int m_SceneType;

	public string m_Desc;

	public List<int> m_iNpcList;

	public int m_EMode;

	public int m_isAttack;

	public int m_BuildID;

	public int m_LookNameID;

	public Vector3 m_DistPos;

	public int m_DistRadius;

	public int m_TrackRadius;

	public List<int> m_WaitDist;

	public Vector3 m_ResetPos;

	public Vector3 m_FailResetPos;

	public List<TalkInfo> m_TalkInfo;

	public List<MonsterIDNum> m_Monster;

	public List<int> m_ComTalkID;

	public List<int> m_iFailNpc;

	public AdMissionRand m_AdDistPos;

	public AdNpcInfo m_AdNpcRadius;

	public List<AdTalkInfo1> m_AdTalkInfo;

	public List<AdTalkInfo> m_AdTalkID;

	public List<int> m_ReceiveList;

	public List<int> m_CreateNpcList;

	public bool m_isNeedPlayer;

	public List<Vector3> m_PathList;

	public bool m_isNeedReturn;

	public Dictionary<int, int[]> npcid_behindTalk_forwardTalk;

	public TypeFollowData()
	{
		m_iNpcList = new List<int>();
		m_TalkInfo = new List<TalkInfo>();
		m_Monster = new List<MonsterIDNum>();
		m_ComTalkID = new List<int>();
		m_iFailNpc = new List<int>();
		m_AdTalkInfo = new List<AdTalkInfo1>();
		m_AdTalkID = new List<AdTalkInfo>();
		m_ReceiveList = new List<int>();
		m_CreateNpcList = new List<int>();
		m_WaitDist = new List<int>();
		m_PathList = new List<Vector3>();
		npcid_behindTalk_forwardTalk = new Dictionary<int, int[]>();
	}
}
