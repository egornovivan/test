using System;
using System.Collections.Generic;
using System.IO;
using Pathea;
using uLink;
using UnityEngine;

public class ReputationSystem : ArchivableSingleton<ReputationSystem>
{
	public enum TargetType
	{
		Puja = 5,
		Paja
	}

	public enum ReputationLevel
	{
		Fear,
		Hatred,
		Animosity,
		Cold,
		Neutral,
		Cordial,
		Amity,
		Respectful,
		Reverence,
		MAX
	}

	private class ReputationCamp
	{
		public int reputationValue = 60999;

		public bool belligerency = true;

		public ReputationLevel level = ReputationLevel.Cold;

		public int exValue;

		public void ResetState()
		{
			level = ConvntValueToLevel((exValue <= 0) ? reputationValue : exValue);
			belligerency = level < ReputationLevel.Neutral;
		}
	}

	private class ReputationData
	{
		public bool active;

		private Dictionary<int, ReputationCamp> m_ReputationCamps = new Dictionary<int, ReputationCamp>();

		public List<int> GetReputationCampIDs()
		{
			List<int> list = new List<int>();
			foreach (int key in m_ReputationCamps.Keys)
			{
				list.Add(key);
			}
			return list;
		}

		public ReputationCamp GetReputationCamp(int playerID)
		{
			ReputationCamp reputationCamp = null;
			if (m_ReputationCamps.ContainsKey(playerID))
			{
				reputationCamp = m_ReputationCamps[playerID];
			}
			else
			{
				reputationCamp = new ReputationCamp();
				m_ReputationCamps[playerID] = reputationCamp;
			}
			return reputationCamp;
		}

		public void Import(BinaryReader _in, int version)
		{
			m_ReputationCamps.Clear();
			active = _in.ReadBoolean();
			if (version < 7)
			{
				for (int i = 0; i < 2; i++)
				{
					ReputationCamp reputationCamp = new ReputationCamp();
					m_ReputationCamps[i + 5] = reputationCamp;
					reputationCamp.reputationValue = _in.ReadInt32();
					if (version < 6)
					{
						reputationCamp.belligerency = _in.ReadBoolean();
					}
					else
					{
						reputationCamp.exValue = _in.ReadInt32();
					}
					reputationCamp.ResetState();
				}
			}
			else
			{
				int num = _in.ReadInt32();
				for (int j = 0; j < num; j++)
				{
					int key = _in.ReadInt32();
					m_ReputationCamps[key] = new ReputationCamp();
					m_ReputationCamps[key].reputationValue = _in.ReadInt32();
					m_ReputationCamps[key].exValue = _in.ReadInt32();
					m_ReputationCamps[key].ResetState();
				}
			}
		}

		public void Export(BinaryWriter w)
		{
			w.Write(active);
			w.Write(m_ReputationCamps.Count);
			foreach (int key in m_ReputationCamps.Keys)
			{
				w.Write(key);
				w.Write(m_ReputationCamps[key].reputationValue);
				w.Write(m_ReputationCamps[key].exValue);
			}
		}
	}

	private const int CURRENT_VERSION = 7;

	public const float ChangeValueProportion = -1.2f;

	private const int DefaultbelligerencyValue = 60999;

	private const int DefaultReputationValue = 60999;

	private const ReputationLevel DefaultReputationLevel = ReputationLevel.Hatred;

	private static readonly int[] ReputationLevelValue = new int[9] { 999, 36999, 57999, 63999, 66999, 72999, 84999, 105999, 106998 };

	private static readonly int[] ReputationLevelValueEX = new int[9] { 999, 36000, 21000, 6000, 3000, 6000, 12000, 21000, 999 };

	private Dictionary<int, ReputationData> m_ReputationDatas = new Dictionary<int, ReputationData>();

	private List<int> m_ForceIDList = new List<int>();

	public event Action<int, int> onReputationChange;

	public static bool IsReputationTarget(int playerID)
	{
		return playerID == 5 || playerID == 6 || 20 <= playerID;
	}

	public void AddPlayerID(int playerID, bool netMsg = false)
	{
		int num = Singleton<ForceSetting>.Instance.GetForceID(playerID);
		if (num == -1 && !PeGameMgr.IsMulti)
		{
			num = 1;
		}
		AddForceID(num);
	}

	public void AddForceID(int forceID)
	{
		if (!m_ForceIDList.Contains(forceID))
		{
			m_ForceIDList.Add(forceID);
		}
	}

	public bool HasReputation(int p1, int p2)
	{
		return GetActiveState(p1) && IsReputationTarget(p2);
	}

	private bool HasReputation(int playerID)
	{
		int forceID = Singleton<ForceSetting>.Instance.GetForceID(playerID);
		return m_ForceIDList.Contains(forceID);
	}

	public bool GetActiveState(int playerID)
	{
		int forceID = Singleton<ForceSetting>.Instance.GetForceID(playerID);
		if (!m_ForceIDList.Contains(forceID))
		{
			return false;
		}
		return GetReputationData(forceID).active;
	}

	public void ActiveReputation(int playerID)
	{
		int forceID = Singleton<ForceSetting>.Instance.GetForceID(playerID);
		if (m_ForceIDList.Contains(forceID))
		{
			if (PeGameMgr.IsMulti)
			{
				NetworkManager.SyncServer(EPacketType.PT_Reputation_SetActive, forceID);
			}
			else
			{
				ReputationData reputationData = GetReputationData(forceID);
				reputationData.active = true;
			}
		}
	}

	public void SetEXValue(int playerID, int targetPlayerID, int exValue)
	{
		int forceID = Singleton<ForceSetting>.Instance.GetForceID(playerID);
		if (m_ForceIDList.Contains(forceID))
		{
			if (PeGameMgr.IsMulti)
			{
				NetworkManager.SyncServer(EPacketType.PT_Reputation_SetExValue, forceID, targetPlayerID, exValue);
			}
			else
			{
				SetEXValueByForce(forceID, targetPlayerID, exValue);
			}
		}
	}

	public void SetReputationValue(int playerID, int targetPlayerID, int value)
	{
		int forceID = Singleton<ForceSetting>.Instance.GetForceID(playerID);
		if (m_ForceIDList.Contains(forceID))
		{
			if (PeGameMgr.IsMulti)
			{
				NetworkManager.SyncServer(EPacketType.PT_Reputation_SetValue, forceID, targetPlayerID, value);
			}
			else
			{
				SetReputationValueByForce(forceID, targetPlayerID, value);
			}
		}
	}

	public void CancelEXValue(int playerID, int targetPlayerID)
	{
		int forceID = Singleton<ForceSetting>.Instance.GetForceID(playerID);
		if (m_ForceIDList.Contains(forceID))
		{
			if (PeGameMgr.IsMulti)
			{
				NetworkManager.SyncServer(EPacketType.PT_Reputation_SetExValue, forceID, targetPlayerID, 0);
			}
			else
			{
				SetEXValueByForce(forceID, targetPlayerID, 0);
			}
		}
	}

	public int GetReputationValue(int playerID, int targetPlayerID)
	{
		int forceID = Singleton<ForceSetting>.Instance.GetForceID(playerID);
		if (!m_ForceIDList.Contains(forceID))
		{
			return 60999;
		}
		return GetReputationValueByForce(forceID, targetPlayerID);
	}

	public int GetExValue(int playerID, int targetPlayerID)
	{
		int forceID = Singleton<ForceSetting>.Instance.GetForceID(playerID);
		if (!m_ForceIDList.Contains(forceID))
		{
			return 60999;
		}
		ReputationData reputationData = GetReputationData(forceID);
		return reputationData.GetReputationCamp(targetPlayerID).exValue;
	}

	public int GetShowReputationValue(int playerID, int targetPlayerID)
	{
		int reputationValue = GetReputationValue(playerID, targetPlayerID);
		ReputationLevel reputationLevel = ConvntValueToLevel(reputationValue);
		if (reputationLevel == ReputationLevel.Fear)
		{
			return reputationValue;
		}
		int num = ReputationLevelValue[(int)(reputationLevel - 1)];
		return reputationValue - num;
	}

	public int GetShowLevelThreshold(int playerID, int targetPlayerID)
	{
		int forceID = Singleton<ForceSetting>.Instance.GetForceID(playerID);
		if (!m_ForceIDList.Contains(forceID))
		{
			return 60999;
		}
		int reputationValue = GetReputationValue(playerID, targetPlayerID);
		ReputationLevel level = ConvntValueToLevel(reputationValue);
		return GetLevelThreshold(level);
	}

	public ReputationLevel GetReputationLevel(int playerID, int targetPlayerID)
	{
		int forceID = Singleton<ForceSetting>.Instance.GetForceID(playerID);
		if (!m_ForceIDList.Contains(forceID))
		{
			return ReputationLevel.Hatred;
		}
		return GetReputationLevelByForce(forceID, targetPlayerID);
	}

	public ReputationLevel GetShowLevel(int playerID, int targetPlayerID)
	{
		int forceID = Singleton<ForceSetting>.Instance.GetForceID(playerID);
		if (!m_ForceIDList.Contains(forceID))
		{
			return ReputationLevel.Hatred;
		}
		int reputationValue = GetReputationValue(playerID, targetPlayerID);
		return ConvntValueToLevel(reputationValue);
	}

	public bool GetBelligerency(int playerID, int targetPlayerID)
	{
		int forceID = Singleton<ForceSetting>.Instance.GetForceID(playerID);
		if (!m_ForceIDList.Contains(forceID))
		{
			return false;
		}
		return GetBelligerencyByForce(forceID, playerID);
	}

	public void ChangeReputationValue(int playerID, int targetPlayerID, int addValue, bool changeOther = false)
	{
		int forceID = Singleton<ForceSetting>.Instance.GetForceID(playerID);
		if (m_ForceIDList.Contains(forceID))
		{
			ChangeReputationValueByForce(forceID, targetPlayerID, addValue, changeOther);
		}
	}

	public bool TryChangeBelligerencyState(int playerID, int targetPlayerID, bool state)
	{
		int forceID = Singleton<ForceSetting>.Instance.GetForceID(playerID);
		if (!m_ForceIDList.Contains(forceID))
		{
			return false;
		}
		return TryChangeBelligerencyStateByForce(forceID, targetPlayerID, state);
	}

	public void Import(byte[] data)
	{
		m_ReputationDatas.Clear();
		MemoryStream memoryStream = new MemoryStream(data);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		int version = binaryReader.ReadInt32();
		int num = binaryReader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int forceID = binaryReader.ReadInt32();
			AddForceID(forceID);
			ReputationData reputationData = GetReputationData(forceID);
			reputationData.Import(binaryReader, version);
		}
		binaryReader.Close();
		memoryStream.Close();
	}

	public void Export(BinaryWriter w)
	{
		w.Write(7);
		w.Write(m_ReputationDatas.Count);
		foreach (int key in m_ReputationDatas.Keys)
		{
			w.Write(key);
			ReputationData reputationData = GetReputationData(key);
			reputationData.Export(w);
		}
	}

	protected override bool GetYird()
	{
		return false;
	}

	protected override void WriteData(BinaryWriter bw)
	{
		Export(bw);
	}

	protected override void SetData(byte[] data)
	{
		Import(data);
	}

	public static ReputationLevel ConvntValueToLevel(int value)
	{
		for (int i = 0; i < ReputationLevelValue.Length; i++)
		{
			if (value < ReputationLevelValue[i])
			{
				return (ReputationLevel)i;
			}
		}
		return ReputationLevel.Reverence;
	}

	public static int GetLevelThreshold(ReputationLevel level)
	{
		return ReputationLevelValueEX[(int)level];
	}

	private ReputationData GetReputationData(int forceID)
	{
		if (!m_ReputationDatas.ContainsKey(forceID))
		{
			m_ReputationDatas[forceID] = new ReputationData();
		}
		return m_ReputationDatas[forceID];
	}

	private int GetReputationValueByForce(int forceID, int targetPlayerID)
	{
		ReputationData reputationData = GetReputationData(forceID);
		return reputationData.GetReputationCamp(targetPlayerID).reputationValue;
	}

	private int GetLevelThresholdByForce(int forceID, int targetPlayerID)
	{
		ReputationData reputationData = GetReputationData(forceID);
		return GetLevelThreshold(reputationData.GetReputationCamp(targetPlayerID).level);
	}

	private ReputationLevel GetReputationLevelByForce(int forceID, int targetPlayerID)
	{
		ReputationData reputationData = GetReputationData(forceID);
		return reputationData.GetReputationCamp(targetPlayerID).level;
	}

	private bool GetBelligerencyByForce(int forceID, int targetPlayerID)
	{
		ReputationData reputationData = GetReputationData(forceID);
		return reputationData.GetReputationCamp(targetPlayerID).belligerency;
	}

	private void ChangeReputationValueByForce(int forceID, int targetPlayerID, int addValue, bool changeOther = false)
	{
		ReputationData reputationData = GetReputationData(forceID);
		ReputationCamp reputationCamp = reputationData.GetReputationCamp(targetPlayerID);
		SetReputationValueByForce(forceID, targetPlayerID, addValue + reputationCamp.reputationValue);
		if (!reputationData.active || !changeOther || !PeGameMgr.IsStory)
		{
			return;
		}
		int num = (int)((float)addValue * ((addValue <= 0) ? (-5f / 6f) : (-1.2f)));
		List<int> reputationCampIDs = reputationData.GetReputationCampIDs();
		for (int i = 0; i < reputationCampIDs.Count; i++)
		{
			if (reputationCampIDs[i] != targetPlayerID)
			{
				ReputationCamp reputationCamp2 = reputationData.GetReputationCamp(reputationCampIDs[i]);
				SetReputationValueByForce(forceID, reputationCampIDs[i], num + reputationCamp2.reputationValue);
			}
		}
	}

	private bool TryChangeBelligerencyStateByForce(int forceID, int targetPlayerID, bool state)
	{
		ReputationData reputationData = GetReputationData(forceID);
		if (reputationData.GetReputationCamp(targetPlayerID).belligerency == state)
		{
			return false;
		}
		if (!state)
		{
			return false;
		}
		SetReputationValueByForce(forceID, targetPlayerID, 60999);
		return true;
	}

	private void SetEXValueByForce(int forceID, int targetPlayerID, int value)
	{
		ReputationData reputationData = GetReputationData(forceID);
		ReputationCamp reputationCamp = reputationData.GetReputationCamp(targetPlayerID);
		reputationCamp.exValue = Mathf.Clamp(value, 0, ReputationLevelValue[ReputationLevelValue.Length - 1]);
		reputationCamp.ResetState();
	}

	private void SetReputationValueByForce(int forceID, int targetPlayerID, int value)
	{
		ReputationData reputationData = GetReputationData(forceID);
		ReputationCamp reputationCamp = reputationData.GetReputationCamp(targetPlayerID);
		reputationCamp.reputationValue = Mathf.Clamp(value, 0, ReputationLevelValue[ReputationLevelValue.Length - 1]);
		reputationCamp.ResetState();
		if (this.onReputationChange != null)
		{
			this.onReputationChange(forceID, targetPlayerID);
		}
	}

	public void Active(int forceID)
	{
		if (m_ForceIDList.Contains(forceID))
		{
			ReputationData reputationData = GetReputationData(forceID);
			reputationData.active = true;
		}
	}

	public static void RPC_S2C_SyncValue(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] data = stream.Read<byte[]>(new object[0]);
		PeSingleton<ReputationSystem>.Instance.Import(data);
	}

	public static void RPC_S2C_SetValue(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int forceID = stream.Read<int>(new object[0]);
		int targetPlayerID = stream.Read<int>(new object[0]);
		int value = stream.Read<int>(new object[0]);
		PeSingleton<ReputationSystem>.Instance.SetReputationValueByForce(forceID, targetPlayerID, value);
	}

	public static void RPC_S2C_SetExValue(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int forceID = stream.Read<int>(new object[0]);
		int targetPlayerID = stream.Read<int>(new object[0]);
		int value = stream.Read<int>(new object[0]);
		PeSingleton<ReputationSystem>.Instance.SetEXValueByForce(forceID, targetPlayerID, value);
	}

	public static void RPC_S2C_SetActive(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int forceID = stream.Read<int>(new object[0]);
		PeSingleton<ReputationSystem>.Instance.Active(forceID);
	}
}
