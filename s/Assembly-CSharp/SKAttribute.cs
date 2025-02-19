using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using Pathea;
using SkillSystem;
using UnityEngine;

public class SKAttribute
{
	public static SkAttribs m_PlayerBaseData;

	public static Dictionary<int, SkAttribs> m_NpcBaseData;

	public static Dictionary<int, SkAttribs> m_MonsterBaseData;

	public static Dictionary<int, SkAttribs> m_TowerBaseData;

	public static Dictionary<int, SkAttribs> m_DoodadBaseData;

	public static bool InitPlayerBaseAttrs(SkAttribs attribs, out SkAttribs baseAttribs)
	{
		baseAttribs = null;
		for (int i = 0; i < 97; i++)
		{
			attribs.NumAttribs.SetAllAttribute((AttribType)i, m_PlayerBaseData.NumAttribs.GetAttribute((AttribType)i));
		}
		baseAttribs = m_PlayerBaseData;
		return true;
	}

	public static bool InitMonsterBaseAttrs(SkAttribs attribs, int id, out SkAttribs baseAttribs)
	{
		baseAttribs = null;
		if (m_MonsterBaseData.ContainsKey(id))
		{
			SkAttribs skAttribs = m_MonsterBaseData[id];
			if (skAttribs == null)
			{
				return false;
			}
			InitData(attribs, skAttribs);
			baseAttribs = skAttribs;
			return true;
		}
		if (LogFilter.logDebug)
		{
			Debug.LogError(" This MonsterID is invaild ,MonsterID =  " + id);
		}
		return false;
	}

	public static bool InitFlagBaseAttrs(SkAttribs attribs, out SkAttribs baseAttribs)
	{
		baseAttribs = null;
		SkAttribs skAttribs = new SkAttribs();
		if (skAttribs == null)
		{
			return false;
		}
		InitData(attribs, skAttribs);
		baseAttribs = skAttribs;
		return true;
	}

	public static bool InitTowerBaseAttrs(SkAttribs attribs, int id, out SkAttribs baseAttribs)
	{
		baseAttribs = null;
		if (m_TowerBaseData.ContainsKey(id))
		{
			SkAttribs skAttribs = m_TowerBaseData[id];
			if (skAttribs == null)
			{
				return false;
			}
			InitData(attribs, skAttribs);
			baseAttribs = skAttribs;
			return true;
		}
		if (LogFilter.logDebug)
		{
			Debug.LogError(" This tower id is invaild ,towerId =  " + id);
		}
		return false;
	}

	public static bool InitNpcBaseAttrs(SkAttribs attribs, int id, out SkAttribs baseAttribs)
	{
		baseAttribs = null;
		if (m_NpcBaseData.ContainsKey(id))
		{
			SkAttribs skAttribs = m_NpcBaseData[id];
			if (skAttribs == null)
			{
				return false;
			}
			InitData(attribs, skAttribs);
			baseAttribs = skAttribs;
			return true;
		}
		if (LogFilter.logDebug)
		{
			Debug.LogError(" This NPCID is invaild ,NPCID =  " + id);
		}
		return false;
	}

	public static bool InitDoodadBaseAttrs(SkAttribs attribs, int id, out SkAttribs baseAttribs)
	{
		baseAttribs = null;
		if (m_DoodadBaseData.ContainsKey(id))
		{
			SkAttribs skAttribs = m_DoodadBaseData[id];
			if (skAttribs == null)
			{
				return false;
			}
			InitData(attribs, skAttribs);
			baseAttribs = skAttribs;
			return true;
		}
		if (id != -1 && LogFilter.logDebug)
		{
			Debug.LogError(" This DoodadBaseData is invaild ,DoodadId =  " + id);
		}
		return false;
	}

	private static void InitData(SkAttribs attribs, SkAttribs baseAttribs)
	{
		for (int i = 0; i < 97; i++)
		{
			attribs.NumAttribs.SetAllAttribute((AttribType)i, baseAttribs.NumAttribs.GetAttribute((AttribType)i));
		}
	}

	public static void LoadData()
	{
		m_PlayerBaseData = new SkAttribs();
		m_NpcBaseData = new Dictionary<int, SkAttribs>();
		m_MonsterBaseData = new Dictionary<int, SkAttribs>();
		m_TowerBaseData = new Dictionary<int, SkAttribs>();
		m_DoodadBaseData = new Dictionary<int, SkAttribs>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("initprop");
		sqliteDataReader.Read();
		for (int i = 0; i < 97; i++)
		{
			AttribType attribType = (AttribType)i;
			string name = attribType.ToString();
			m_PlayerBaseData.NumAttribs.SetAllAttribute((AttribType)i, Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal(name))));
		}
		sqliteDataReader = LocalDatabase.Instance.ReadFullTable("prototypemonster");
		while (sqliteDataReader.Read())
		{
			int num = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			SkAttribs skAttribs = new SkAttribs();
			for (int j = 0; j < 97; j++)
			{
				AttribType attribType2 = (AttribType)j;
				string name2 = attribType2.ToString();
				skAttribs.NumAttribs.SetAllAttribute((AttribType)j, Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal(name2))));
			}
			if (!m_MonsterBaseData.ContainsKey(num))
			{
				m_MonsterBaseData.Add(num, skAttribs);
			}
			else if (LogFilter.logDebug)
			{
				Debug.LogFormat("repeat monster sk entityId:{0}", num);
			}
		}
		sqliteDataReader = LocalDatabase.Instance.ReadFullTable("prototypeNpc");
		while (sqliteDataReader.Read())
		{
			int num2 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			SkAttribs skAttribs2 = new SkAttribs();
			for (int k = 0; k < 97; k++)
			{
				AttribType attribType3 = (AttribType)k;
				string name3 = attribType3.ToString();
				skAttribs2.NumAttribs.SetAllAttribute((AttribType)k, Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal(name3))));
			}
			if (!m_NpcBaseData.ContainsKey(num2))
			{
				m_NpcBaseData.Add(num2, skAttribs2);
			}
			else if (LogFilter.logDebug)
			{
				Debug.LogFormat("repeat npc sk entityId:{0}", num2);
			}
		}
		sqliteDataReader = LocalDatabase.Instance.ReadFullTable("PrototypeTurret");
		while (sqliteDataReader.Read())
		{
			int num3 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			SkAttribs skAttribs3 = new SkAttribs();
			for (int l = 0; l < 97; l++)
			{
				AttribType attribType4 = (AttribType)l;
				string name4 = attribType4.ToString();
				skAttribs3.NumAttribs.SetAllAttribute((AttribType)l, Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal(name4))));
			}
			if (!m_TowerBaseData.ContainsKey(num3))
			{
				m_TowerBaseData.Add(num3, skAttribs3);
			}
			else if (LogFilter.logDebug)
			{
				Debug.LogFormat("repeat turret sk entityId:{0}", num3);
			}
		}
		sqliteDataReader = LocalDatabase.Instance.ReadFullTable("PrototypeDoodad");
		while (sqliteDataReader.Read())
		{
			int num4 = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			SkAttribs skAttribs4 = new SkAttribs();
			for (int m = 0; m < 97; m++)
			{
				AttribType attribType5 = (AttribType)m;
				string name5 = attribType5.ToString();
				skAttribs4.NumAttribs.SetAllAttribute((AttribType)m, Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal(name5))));
			}
			if (!m_DoodadBaseData.ContainsKey(num4))
			{
				m_DoodadBaseData.Add(num4, skAttribs4);
			}
			else if (LogFilter.logDebug)
			{
				Debug.LogFormat("repeat doodad sk entityId:{0}", num4);
			}
		}
	}
}
