using System.Collections.Generic;
using UnityEngine;

public class TypeMonsterData
{
	public int m_TargetID;

	public int m_ScriptID;

	public string m_Desc;

	public List<NpcType> m_MonsterList;

	public List<CreMons> m_CreMonList;

	public int m_MonsterID;

	public int m_MonsterNum;

	public Vector3 m_TargetPos;

	public AdMissionRand m_mr;

	public List<int> m_ReceiveList;

	public int type;

	public bool m_mustByPlayer;

	public bool m_destroyTown;

	public List<int> m_campID;

	public List<int> m_townNum;

	public TypeMonsterData()
	{
		m_MonsterList = new List<NpcType>();
		m_CreMonList = new List<CreMons>();
		m_ReceiveList = new List<int>();
		m_campID = new List<int>();
		m_townNum = new List<int>();
	}
}
