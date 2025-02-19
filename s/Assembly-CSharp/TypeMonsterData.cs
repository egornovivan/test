using System.Collections.Generic;
using UnityEngine;

public class TypeMonsterData
{
	public int m_TargetID;

	public int m_ScriptID;

	public string m_Desc;

	public List<MissionIDNum> m_MonsterList;

	public List<int> m_CommonMonsterIds = new List<int>();

	public int m_MonsterID;

	public int m_MonsterNum;

	public Vector3 m_TargetPos;

	public int m_AdDist;

	public int m_AdRadius;

	public List<MissionIDNum> m_CreateMonsterList;

	public List<int> m_ReceiveList;

	public int type;

	public TypeMonsterData()
	{
		m_MonsterList = new List<MissionIDNum>();
		m_CreateMonsterList = new List<MissionIDNum>();
		m_ReceiveList = new List<int>();
	}
}
