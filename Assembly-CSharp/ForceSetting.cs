using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Pathea;
using PETools;
using UnityEngine;

public class ForceSetting : Singleton<ForceSetting>
{
	public List<ForceDesc> m_Forces;

	public List<PlayerDesc> m_Players;

	public List<PlayerDesc> HumanPlayer = new List<PlayerDesc>();

	public List<ForceDesc> HumanForce = new List<ForceDesc>();

	private static int m_TeamNum;

	private static int m_NumPerTeam;

	public ForceSetting()
	{
		m_Forces = new List<ForceDesc>();
		m_Players = new List<PlayerDesc>();
	}

	public int GetForceIndex(int id)
	{
		int count = m_Forces.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_Forces[i].ID == id)
			{
				return i;
			}
		}
		return -1;
	}

	public void AddForceDesc(ForceDesc desc)
	{
		if (desc == null)
		{
			return;
		}
		int forceIndex = GetForceIndex(desc.ID);
		if (forceIndex == -1)
		{
			m_Forces.Add(desc);
		}
		else
		{
			m_Forces[forceIndex] = desc;
			if (LogFilter.logDebug)
			{
				Debug.LogWarningFormat("replace force id:{0}", desc.ID);
			}
		}
		if (PeGameMgr.IsMulti)
		{
			PeSingleton<ReputationSystem>.Instance.AddForceID(desc.ID);
		}
	}

	public int GetPlayerIndex(int id)
	{
		int count = m_Players.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_Players[i].ID == id)
			{
				return i;
			}
		}
		return -1;
	}

	public void AddPlayerDesc(PlayerDesc desc)
	{
		if (desc == null)
		{
			return;
		}
		int playerIndex = GetPlayerIndex(desc.ID);
		if (playerIndex == -1)
		{
			m_Players.Add(desc);
		}
		else
		{
			m_Players[playerIndex] = desc;
			if (LogFilter.logDebug)
			{
				Debug.LogWarningFormat("replace player id:{0}", desc.ID);
			}
		}
		if (PeGameMgr.IsMulti)
		{
			PeSingleton<ReputationSystem>.Instance.AddForceID(desc.Force);
		}
	}

	public bool AllyPlayer(int srcPlayerID, int dstPlayerID)
	{
		int playerIndex = GetPlayerIndex(srcPlayerID);
		int playerIndex2 = GetPlayerIndex(dstPlayerID);
		if (playerIndex < 0 || playerIndex2 < 0)
		{
			return false;
		}
		return AllyForce(m_Players[playerIndex].Force, m_Players[playerIndex2].Force);
	}

	public bool AllyForce(int srcForceID, int dstForceID)
	{
		int forceIndex = GetForceIndex(srcForceID);
		if (forceIndex < 0)
		{
			return false;
		}
		return m_Forces[forceIndex].Allies.Contains(dstForceID);
	}

	public bool Ally(ForceDesc f1, ForceDesc f2)
	{
		if (f1 != null && f2 != null)
		{
			return f1.Allies.Contains(f2.ID);
		}
		return false;
	}

	public int GetForceID(int playerID)
	{
		int playerIndex = GetPlayerIndex(playerID);
		if (playerIndex == -1)
		{
			return -1;
		}
		return m_Players[playerIndex].Force;
	}

	public Color32 GetForceColor(int forceId)
	{
		int forceIndex = GetForceIndex(forceId);
		if (forceIndex >= 0)
		{
			return m_Forces[forceIndex].Color;
		}
		return default(Color32);
	}

	public EPlayerType GetForceType(int playerID)
	{
		int playerIndex = GetPlayerIndex(playerID);
		if (playerIndex == -1)
		{
			return EPlayerType.Neutral;
		}
		return m_Players[playerIndex].Type;
	}

	public bool Conflict(int srcID, int dstID)
	{
		int playerIndex = GetPlayerIndex(srcID);
		int playerIndex2 = GetPlayerIndex(dstID);
		if (playerIndex >= 0 && playerIndex2 >= 0)
		{
			int forceIndex = GetForceIndex(m_Players[playerIndex].Force);
			int forceIndex2 = GetForceIndex(m_Players[playerIndex2].Force);
			if (forceIndex >= 0 && forceIndex2 >= 0)
			{
				if (AllyForce(m_Forces[forceIndex].ID, m_Forces[forceIndex2].ID))
				{
					if (m_Forces[forceIndex].ID == m_Forces[forceIndex2].ID)
					{
						return m_Forces[forceIndex].InternalConflict;
					}
					return m_Forces[forceIndex].AllyConflict;
				}
				return m_Forces[forceIndex].EnemyConflict;
			}
		}
		return false;
	}

	public void Load(string s)
	{
		m_Forces.Clear();
		m_Players.Clear();
		XmlDocument xmlDocument = new XmlDocument();
		StringReader stringReader = new StringReader(s);
		xmlDocument.Load(stringReader);
		XmlNode xmlNode = xmlDocument.SelectSingleNode("ForceSetting");
		XmlNode xmlNode2 = xmlNode.SelectSingleNode("Force");
		XmlNodeList elementsByTagName = ((XmlElement)xmlNode2).GetElementsByTagName("ForceDesc");
		foreach (XmlNode item in elementsByTagName)
		{
			XmlElement e = item as XmlElement;
			ForceDesc forceDesc = new ForceDesc();
			forceDesc.ID = XmlUtil.GetAttributeInt32(e, "ID");
			forceDesc.Name = XmlUtil.GetAttributeString(e, "Name");
			forceDesc.Color = XmlUtil.GetNodeColor32(e, "Color");
			forceDesc.Allies = XmlUtil.GetNodeInt32List(e, "Ally");
			forceDesc.JoinablePlayerCount = XmlUtil.GetNodeInt32(e, "JoinablePlayerCount");
			forceDesc.JoinStr = XmlUtil.GetNodeString(e, "JoinLocation");
			forceDesc.ItemUseShare = XmlUtil.GetNodeBool(e, "ItemUseShare");
			forceDesc.ItemShare = XmlUtil.GetNodeBool(e, "ItemShare");
			forceDesc.InternalConflict = XmlUtil.GetNodeBool(e, "InternalConflict");
			forceDesc.AllyConflict = XmlUtil.GetNodeBool(e, "AllyConflict");
			forceDesc.EnemyConflict = XmlUtil.GetNodeBool(e, "EnemyConflict");
			forceDesc.Color.a = byte.MaxValue;
			AddForceDesc(forceDesc);
		}
		XmlNode xmlNode4 = xmlNode.SelectSingleNode("Player");
		XmlNodeList elementsByTagName2 = ((XmlElement)xmlNode4).GetElementsByTagName("PlayerDesc");
		foreach (XmlNode item2 in elementsByTagName2)
		{
			XmlElement e2 = item2 as XmlElement;
			PlayerDesc playerDesc = new PlayerDesc();
			playerDesc.ID = XmlUtil.GetAttributeInt32(e2, "ID");
			playerDesc.Name = XmlUtil.GetAttributeString(e2, "Name");
			playerDesc.Force = XmlUtil.GetAttributeInt32(e2, "Force");
			playerDesc.StartStr = XmlUtil.GetAttributeString(e2, "Start");
			playerDesc.Type = (EPlayerType)(int)Enum.Parse(typeof(EPlayerType), XmlUtil.GetAttributeString(e2, "Type"));
			AddPlayerDesc(playerDesc);
		}
		stringReader.Close();
	}

	public void Load(TextAsset text)
	{
		Load(text.text);
	}

	public void OnLevelWasLoaded(int level)
	{
	}

	private bool HasForce(int id)
	{
		return Singleton<ForceSetting>.Instance.GetForceIndex(id) != -1;
	}

	public void InitRoomForces(int teamNum, int numPerTeam)
	{
		m_TeamNum = teamNum;
		m_NumPerTeam = numPerTeam;
		HumanForce.Clear();
		HumanPlayer.Clear();
		if (PeGameMgr.IsCustom)
		{
			m_Forces.Clear();
			m_Players.Clear();
			m_Forces.AddRange(PeSingleton<CustomGameData.Mgr>.Instance.curGameData.mForceDescs);
			m_Players.AddRange(PeSingleton<CustomGameData.Mgr>.Instance.curGameData.mPlayerDescs);
			foreach (PlayerDesc player in m_Players)
			{
				if (player.Type != EPlayerType.Human)
				{
					continue;
				}
				int forceIndex = GetForceIndex(player.Force);
				if (forceIndex != -1)
				{
					if (!HumanPlayer.Contains(player))
					{
						HumanPlayer.Add(player);
					}
					if (!HumanForce.Contains(m_Forces[forceIndex]))
					{
						HumanForce.Add(m_Forces[forceIndex]);
					}
				}
			}
			{
				foreach (ForceDesc force in m_Forces)
				{
					if (force.JoinablePlayerCount != 0 && !HumanForce.Contains(force))
					{
						HumanForce.Add(force);
					}
				}
				return;
			}
		}
		if (PeGameMgr.IsCooperation)
		{
			ForceDesc forceDesc = new ForceDesc();
			forceDesc.Allies = new List<int>();
			forceDesc.ID = 1;
			forceDesc.Allies.Add(forceDesc.ID);
			forceDesc.AllyConflict = true;
			forceDesc.EnemyConflict = true;
			forceDesc.InternalConflict = false;
			forceDesc.ItemShare = true;
			forceDesc.ItemUseShare = true;
			forceDesc.JoinablePlayerCount = m_NumPerTeam;
			forceDesc.Name = "Cooperation";
			forceDesc.PublicInventory = true;
			forceDesc.Color = Color.green;
			HumanForce.Add(forceDesc);
		}
		else if (PeGameMgr.IsSurvive)
		{
			int num = -1;
			ForceDesc forceDesc2 = new ForceDesc();
			forceDesc2.ID = num;
			forceDesc2.Allies = new List<int>();
			forceDesc2.Allies.Add(num);
			forceDesc2.AllyConflict = false;
			forceDesc2.EnemyConflict = true;
			forceDesc2.InternalConflict = false;
			forceDesc2.ItemShare = false;
			forceDesc2.ItemUseShare = false;
			forceDesc2.JoinablePlayerCount = m_NumPerTeam;
			forceDesc2.Name = "Survive";
			forceDesc2.PublicInventory = false;
			forceDesc2.Color = Color.red;
			HumanForce.Add(forceDesc2);
		}
		else if (PeGameMgr.IsVS)
		{
			int num2 = 10000;
			for (int i = 0; i < teamNum; i++)
			{
				num2++;
				ForceDesc forceDesc3 = new ForceDesc();
				forceDesc3.ID = num2;
				forceDesc3.Allies = new List<int>();
				forceDesc3.Allies.Add(num2);
				forceDesc3.AllyConflict = false;
				forceDesc3.EnemyConflict = true;
				forceDesc3.InternalConflict = false;
				forceDesc3.ItemShare = true;
				forceDesc3.ItemUseShare = true;
				forceDesc3.JoinablePlayerCount = m_NumPerTeam;
				forceDesc3.Name = "Team" + num2;
				forceDesc3.PublicInventory = true;
				forceDesc3.Color = Color.red;
				HumanForce.Add(forceDesc3);
			}
		}
	}

	public void InitGameForces(int curSurviveTeamId)
	{
		if (PeGameMgr.IsCooperation)
		{
			int num = 1;
			AddPlayer(num, num, EPlayerType.Human, "Team" + num);
			AddForce(num, m_NumPerTeam, PeGameMgr.EGameType.Cooperation);
			return;
		}
		if (PeGameMgr.IsVS)
		{
			int num2 = 10000;
			for (int i = 0; i < m_TeamNum; i++)
			{
				num2++;
				AddPlayer(num2, num2, EPlayerType.Human, "Team" + num2);
				AddForce(num2, m_NumPerTeam, PeGameMgr.EGameType.VS);
			}
			return;
		}
		if (PeGameMgr.IsCustom)
		{
			foreach (ForceDesc item in HumanForce)
			{
				AddForceDesc(item);
			}
			{
				foreach (PlayerDesc item2 in HumanPlayer)
				{
					AddPlayerDesc(item2);
				}
				return;
			}
		}
		if (PeGameMgr.IsSurvive)
		{
			for (int j = 10001; j <= curSurviveTeamId; j++)
			{
				AddForce(j, PeGameMgr.EGameType.Survive);
				AddPlayer(j, j, EPlayerType.Human, "Team" + j);
			}
		}
	}

	public static bool AddAllyPlayer(int srcPlayerID, int dstPlayerID)
	{
		if (srcPlayerID == dstPlayerID || null == Singleton<ForceSetting>.Instance)
		{
			return false;
		}
		int num = Singleton<ForceSetting>.Instance.m_Players.FindIndex((PlayerDesc iter) => iter.ID == srcPlayerID);
		int num2 = Singleton<ForceSetting>.Instance.m_Players.FindIndex((PlayerDesc iter) => iter.ID == dstPlayerID);
		if (num != -1 && num2 != -1)
		{
			return AddAllyForce(Singleton<ForceSetting>.Instance.m_Players[num].Force, Singleton<ForceSetting>.Instance.m_Players[num2].Force);
		}
		return false;
	}

	public static bool AddAllyForce(int srcFroce, int dstForce)
	{
		if (srcFroce == dstForce || null == Singleton<ForceSetting>.Instance)
		{
			return false;
		}
		int num = Singleton<ForceSetting>.Instance.m_Forces.FindIndex((ForceDesc iter) => iter.ID == srcFroce);
		int num2 = Singleton<ForceSetting>.Instance.m_Forces.FindIndex((ForceDesc iter) => iter.ID == dstForce);
		if (num != -1 && num2 != -1 && num != num2)
		{
			if (!Singleton<ForceSetting>.Instance.m_Forces[num].Allies.Contains(dstForce))
			{
				Singleton<ForceSetting>.Instance.m_Forces[num].Allies.Add(dstForce);
			}
			if (!Singleton<ForceSetting>.Instance.m_Forces[num2].Allies.Contains(srcFroce))
			{
				Singleton<ForceSetting>.Instance.m_Forces[num2].Allies.Add(srcFroce);
			}
			return true;
		}
		return false;
	}

	public static bool RemoveAllyPlayer(int srcPlayerID, int dstPlayerID)
	{
		if (srcPlayerID == dstPlayerID || null == Singleton<ForceSetting>.Instance)
		{
			return false;
		}
		int num = Singleton<ForceSetting>.Instance.m_Players.FindIndex((PlayerDesc iter) => iter.ID == srcPlayerID);
		int num2 = Singleton<ForceSetting>.Instance.m_Players.FindIndex((PlayerDesc iter) => iter.ID == dstPlayerID);
		if (num != -1 && num2 != -1)
		{
			return RemoveAllyForce(Singleton<ForceSetting>.Instance.m_Players[num].Force, Singleton<ForceSetting>.Instance.m_Players[num2].Force);
		}
		return false;
	}

	public static bool RemoveAllyForce(int srcFroce, int dstForce)
	{
		if (srcFroce == dstForce || null == Singleton<ForceSetting>.Instance)
		{
			return false;
		}
		int num = Singleton<ForceSetting>.Instance.m_Forces.FindIndex((ForceDesc iter) => iter.ID == srcFroce);
		int num2 = Singleton<ForceSetting>.Instance.m_Forces.FindIndex((ForceDesc iter) => iter.ID == dstForce);
		if (num != -1 && num2 != -1 && num != num2)
		{
			if (Singleton<ForceSetting>.Instance.m_Forces[num].Allies.Contains(dstForce))
			{
				Singleton<ForceSetting>.Instance.m_Forces[num].Allies.Remove(dstForce);
			}
			if (Singleton<ForceSetting>.Instance.m_Forces[num2].Allies.Contains(srcFroce))
			{
				Singleton<ForceSetting>.Instance.m_Forces[num2].Allies.Remove(srcFroce);
			}
			return true;
		}
		return false;
	}

	public static ForceDesc AddForce(int id, PeGameMgr.EGameType type)
	{
		return AddForce(id, m_NumPerTeam, type);
	}

	public static ForceDesc AddForce(int id, int maxNum, PeGameMgr.EGameType type)
	{
		if (PeGameMgr.IsMultiCoop)
		{
			id = 1;
		}
		if (!Singleton<ForceSetting>.Instance.HasForce(id))
		{
			ForceDesc forceDesc = new ForceDesc();
			forceDesc.ID = id;
			forceDesc.Allies = new List<int>();
			forceDesc.Allies.Add(id);
			forceDesc.EnemyConflict = true;
			forceDesc.InternalConflict = false;
			forceDesc.JoinablePlayerCount = maxNum;
			forceDesc.Name = id.ToString();
			switch (type)
			{
			case PeGameMgr.EGameType.Cooperation:
				forceDesc.AllyConflict = true;
				forceDesc.ItemShare = true;
				forceDesc.ItemUseShare = true;
				forceDesc.PublicInventory = true;
				forceDesc.Color = Color.green;
				break;
			case PeGameMgr.EGameType.Survive:
				forceDesc.AllyConflict = false;
				forceDesc.ItemShare = false;
				forceDesc.ItemUseShare = false;
				forceDesc.PublicInventory = false;
				forceDesc.Color = Color.red;
				break;
			case PeGameMgr.EGameType.VS:
				forceDesc.AllyConflict = false;
				forceDesc.ItemShare = true;
				forceDesc.ItemUseShare = true;
				forceDesc.PublicInventory = true;
				forceDesc.Color = Color.red;
				break;
			}
			Singleton<ForceSetting>.Instance.AddForceDesc(forceDesc);
			return forceDesc;
		}
		int forceIndex = Singleton<ForceSetting>.Instance.GetForceIndex(id);
		if (forceIndex != -1)
		{
			return Singleton<ForceSetting>.Instance.m_Forces[forceIndex];
		}
		return null;
	}

	private bool HasPlayer(int id)
	{
		return Singleton<ForceSetting>.Instance.GetPlayerIndex(id) != -1;
	}

	public static PlayerDesc AddPlayer(int id, int force, EPlayerType type, string name)
	{
		if (PeGameMgr.IsMultiCoop)
		{
			force = 1;
		}
		int playerIndex = Singleton<ForceSetting>.Instance.GetPlayerIndex(id);
		if (playerIndex == -1)
		{
			PlayerDesc playerDesc = new PlayerDesc();
			playerDesc.ID = id;
			playerDesc.Force = force;
			playerDesc.Type = type;
			playerDesc.Name = name;
			Singleton<ForceSetting>.Instance.AddPlayerDesc(playerDesc);
			return playerDesc;
		}
		Singleton<ForceSetting>.Instance.m_Players[playerIndex].Force = force;
		return Singleton<ForceSetting>.Instance.m_Players[playerIndex];
	}

	public static bool GetScenarioPos(int scenarioId, out Vector3 pos)
	{
		int num = Singleton<ForceSetting>.Instance.m_Players.FindIndex((PlayerDesc iter) => iter.ID == scenarioId);
		if (num != -1)
		{
			pos = Singleton<ForceSetting>.Instance.m_Players[num].StartLocation;
			return true;
		}
		pos = Vector3.zero;
		return false;
	}

	public static bool GetForcePos(int forceId, out Vector3 pos)
	{
		int num = Singleton<ForceSetting>.Instance.m_Forces.FindIndex((ForceDesc iter) => iter.ID == forceId);
		if (num != -1)
		{
			pos = Singleton<ForceSetting>.Instance.m_Forces[num].JoinLocation;
			return true;
		}
		pos = Vector3.zero;
		return false;
	}

	public static PlayerDesc GetPlayerDesc(int id)
	{
		return Singleton<ForceSetting>.Instance.m_Players.Find((PlayerDesc iter) => iter.ID == id);
	}

	public static ForceDesc GetForceDesc(int id)
	{
		return Singleton<ForceSetting>.Instance.m_Forces.Find((ForceDesc iter) => iter.ID == id);
	}
}
