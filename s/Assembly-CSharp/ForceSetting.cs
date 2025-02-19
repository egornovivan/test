using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using PETools;
using UnityEngine;

public class ForceSetting
{
	private static List<ForceDesc> m_Forces = new List<ForceDesc>();

	private static List<PlayerDesc> m_Players = new List<PlayerDesc>();

	public static List<ForceDesc> humanForces { get; private set; }

	public static List<PlayerDesc> humanPlayers { get; private set; }

	public static void InitCustomHumanForces()
	{
		humanForces = new List<ForceDesc>();
		humanPlayers = new List<PlayerDesc>();
		foreach (PlayerDesc player in m_Players)
		{
			if (player.Type != EPlayerType.Human)
			{
				continue;
			}
			if (!humanPlayers.Contains(player))
			{
				humanPlayers.Add(player);
			}
			foreach (ForceDesc force in m_Forces)
			{
				if (force.ID == player.Force)
				{
					if (!humanForces.Contains(force))
					{
						humanForces.Add(force);
					}
					break;
				}
			}
		}
		foreach (ForceDesc force2 in m_Forces)
		{
			if (force2.JoinablePlayerCount != 0 && !humanForces.Contains(force2))
			{
				humanForces.Add(force2);
			}
		}
	}

	public static void AddForceDesc(ForceDesc desc)
	{
		if (!HasForce(desc.ID))
		{
			m_Forces.Add(desc);
		}
		else if (LogFilter.logDebug)
		{
			Debug.LogError("Can't add the same id : ForceDesc");
		}
	}

	public static void AddPlayerDesc(PlayerDesc desc)
	{
		if (!HasPlayer(desc.ID))
		{
			m_Players.Add(desc);
		}
		else if (LogFilter.logDebug)
		{
			Debug.LogError("Can't add the same id : PlayerDesc");
		}
	}

	public static bool AddAllyPlayer(int srcPlayerID, int dstPlayerID)
	{
		if (srcPlayerID == dstPlayerID)
		{
			return false;
		}
		int num = m_Players.FindIndex((PlayerDesc iter) => iter.ID == srcPlayerID);
		int num2 = m_Players.FindIndex((PlayerDesc iter) => iter.ID == dstPlayerID);
		if (num != -1 && num2 != -1)
		{
			return AddAllyForce(m_Players[num].Force, m_Players[num2].Force);
		}
		return false;
	}

	public static bool AddAllyForce(int srcFroce, int dstForce)
	{
		if (srcFroce == dstForce)
		{
			return false;
		}
		int num = m_Forces.FindIndex((ForceDesc iter) => iter.ID == srcFroce);
		int num2 = m_Forces.FindIndex((ForceDesc iter) => iter.ID == dstForce);
		if (num != -1 && num2 != -1 && num != num2)
		{
			if (!m_Forces[num].Allies.Contains(dstForce))
			{
				m_Forces[num].Allies.Add(dstForce);
			}
			if (!m_Forces[num2].Allies.Contains(srcFroce))
			{
				m_Forces[num2].Allies.Add(srcFroce);
			}
			return true;
		}
		return false;
	}

	public static bool RemoveAllyPlayer(int srcPlayerID, int dstPlayerID)
	{
		if (srcPlayerID == dstPlayerID)
		{
			return false;
		}
		int num = m_Players.FindIndex((PlayerDesc iter) => iter.ID == srcPlayerID);
		int num2 = m_Players.FindIndex((PlayerDesc iter) => iter.ID == dstPlayerID);
		if (num != -1 && num2 != -1)
		{
			return RemoveAllyForce(m_Players[num].Force, m_Players[num2].Force);
		}
		return false;
	}

	public static bool RemoveAllyForce(int srcFroce, int dstForce)
	{
		if (srcFroce == dstForce)
		{
			return false;
		}
		int num = m_Forces.FindIndex((ForceDesc iter) => iter.ID == srcFroce);
		int num2 = m_Forces.FindIndex((ForceDesc iter) => iter.ID == dstForce);
		if (num != -1 && num2 != -1 && num != num2)
		{
			if (m_Forces[num].Allies.Contains(dstForce))
			{
				m_Forces[num].Allies.Remove(dstForce);
			}
			if (m_Forces[num2].Allies.Contains(srcFroce))
			{
				m_Forces[num2].Allies.Remove(srcFroce);
			}
			return true;
		}
		return false;
	}

	public static bool AllyPlayer(int srcPlayerID, int dstPlayerID)
	{
		int num = m_Players.FindIndex((PlayerDesc iter) => iter.ID == srcPlayerID);
		int num2 = m_Players.FindIndex((PlayerDesc iter) => iter.ID == dstPlayerID);
		if (num != -1 && num2 != -1)
		{
			return Ally(m_Players[num].Force, m_Players[num2].Force);
		}
		return false;
	}

	public static bool Ally(int srcForceID, int dstForceID)
	{
		int num = m_Forces.FindIndex((ForceDesc iter) => iter.ID == srcForceID);
		if (num != -1)
		{
			return m_Forces[num].Allies.Contains(dstForceID);
		}
		return false;
	}

	public static bool Ally(ForceDesc f1, ForceDesc f2)
	{
		if (f1 != null && f2 != null)
		{
			return f1.Allies.Contains(f2.ID);
		}
		return false;
	}

	public static int GetForceID(int playerID)
	{
		int num = m_Players.FindIndex((PlayerDesc iter) => iter.ID == playerID);
		if (num != -1)
		{
			return m_Players[num].Force;
		}
		return -1;
	}

	public static EPlayerType GetForceType(int playerID)
	{
		int num = m_Players.FindIndex((PlayerDesc iter) => iter.ID == playerID);
		if (num != -1)
		{
			return m_Players[num].Type;
		}
		return EPlayerType.Neutral;
	}

	public static bool Conflict(int srcID, int dstID)
	{
		if (m_Players.Count <= 0 && m_Forces.Count <= 0)
		{
			return true;
		}
		int num = m_Players.FindIndex((PlayerDesc iter) => iter.ID == srcID);
		int num2 = m_Players.FindIndex((PlayerDesc iter) => iter.ID == dstID);
		if (num != -1 && num2 != -1)
		{
			PlayerDesc p1 = m_Players[num];
			PlayerDesc p2 = m_Players[num2];
			int num3 = m_Forces.FindIndex((ForceDesc iter) => iter.ID == p1.Force);
			int num4 = m_Forces.FindIndex((ForceDesc iter) => iter.ID == p2.Force);
			if (num3 != -1 && num4 != -1)
			{
				ForceDesc forceDesc = m_Forces[num3];
				ForceDesc forceDesc2 = m_Forces[num4];
				if (Ally(forceDesc, forceDesc2))
				{
					if (forceDesc.ID == forceDesc2.ID)
					{
						return forceDesc.InternalConflict;
					}
					return forceDesc.AllyConflict;
				}
				return forceDesc.EnemyConflict;
			}
			if (LogFilter.logDebug)
			{
				Debug.LogError("Can't find force id : " + p1.Force + " & " + p2.Force);
			}
		}
		return false;
	}

	public static void Load(string s)
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

	public static bool HasForce(int id)
	{
		return m_Forces.Exists((ForceDesc iter) => iter.ID == id);
	}

	public static void AddForce(int id, int max, EGameType type)
	{
		if (ServerConfig.IsCooperation)
		{
			id = 1;
		}
		if (!HasForce(id))
		{
			ForceDesc forceDesc = new ForceDesc();
			forceDesc.ID = id;
			forceDesc.Allies = new List<int>();
			forceDesc.Allies.Add(id);
			switch (type)
			{
			case EGameType.Cooperation:
				forceDesc.AllyConflict = false;
				forceDesc.EnemyConflict = true;
				forceDesc.InternalConflict = false;
				forceDesc.ItemShare = true;
				forceDesc.ItemUseShare = true;
				forceDesc.JoinablePlayerCount = max;
				forceDesc.Name = id.ToString();
				forceDesc.PublicInventory = true;
				forceDesc.Color = Color.green;
				break;
			case EGameType.Survive:
				forceDesc.AllyConflict = false;
				forceDesc.EnemyConflict = true;
				forceDesc.InternalConflict = false;
				forceDesc.ItemShare = false;
				forceDesc.ItemUseShare = false;
				forceDesc.JoinablePlayerCount = max;
				forceDesc.Name = id.ToString();
				forceDesc.PublicInventory = false;
				forceDesc.Color = Color.green;
				break;
			case EGameType.VS:
				forceDesc.AllyConflict = true;
				forceDesc.EnemyConflict = true;
				forceDesc.InternalConflict = false;
				forceDesc.ItemShare = true;
				forceDesc.ItemUseShare = true;
				forceDesc.JoinablePlayerCount = max;
				forceDesc.Name = id.ToString();
				forceDesc.PublicInventory = true;
				forceDesc.Color = Color.green;
				break;
			}
			AddForceDesc(forceDesc);
		}
	}

	public static bool HasPlayer(int id)
	{
		return m_Players.Exists((PlayerDesc iter) => iter.ID == id);
	}

	public static void AddPlayer(int id, int force, EPlayerType type, string name)
	{
		if (ServerConfig.IsCooperation)
		{
			force = 1;
		}
		int num = m_Players.FindIndex((PlayerDesc iter) => iter.ID == id);
		if (num != -1)
		{
			PlayerDesc playerDesc = m_Players[num];
			playerDesc.Force = force;
			playerDesc.Type = type;
			playerDesc.Name = name;
		}
		else
		{
			PlayerDesc playerDesc2 = new PlayerDesc();
			playerDesc2.ID = id;
			playerDesc2.Force = force;
			playerDesc2.Type = type;
			playerDesc2.Name = name;
			AddPlayerDesc(playerDesc2);
		}
	}

	public static void SetForcePos(int id, Vector3 pos)
	{
		int num = m_Forces.FindIndex((ForceDesc iter) => iter.ID == id);
		if (num != -1)
		{
			m_Forces[num].JoinLocation = pos;
		}
	}

	public static Vector3 GetPlayerPos(int id)
	{
		int num = m_Players.FindIndex((PlayerDesc iter) => iter.ID == id);
		if (num != -1)
		{
			return m_Players[num].StartLocation;
		}
		return Vector3.zero;
	}

	public static Vector3 GetForcePos(int id)
	{
		int num = m_Forces.FindIndex((ForceDesc iter) => iter.ID == id);
		if (num != -1)
		{
			return m_Forces[num].JoinLocation;
		}
		return Vector3.zero;
	}

	public static int GetForceWorld(int id)
	{
		int num = m_Forces.FindIndex((ForceDesc iter) => iter.ID == id);
		if (num != -1)
		{
			return m_Forces[num].JoinWorld;
		}
		return 200;
	}

	public static void SetForceJoinableNum(int id, int num)
	{
		int num2 = m_Forces.FindIndex((ForceDesc iter) => iter.ID == id);
		if (num2 != -1)
		{
			m_Forces[num2].JoinablePlayerCount = num;
		}
	}
}
