using System.Collections.Generic;
using System.IO;
using Mono.Data.SqliteClient;
using uLink;

public class ReputationSystem
{
	private const int CURRENT_VERSION = 7;

	public const int DefaultReputationValue = 60999;

	public static ReputationSystem _self = new ReputationSystem();

	private Dictionary<int, ReputationData> m_ReputationDatas = new Dictionary<int, ReputationData>();

	private ReputationData GetReputationData(int forceID)
	{
		if (!m_ReputationDatas.ContainsKey(forceID))
		{
			m_ReputationDatas[forceID] = new ReputationData();
			m_ReputationDatas[forceID].InitReputationCamp();
		}
		return m_ReputationDatas[forceID];
	}

	private void SetEXValue(int forceID, int typeIndex, int value)
	{
		ReputationData reputationData = GetReputationData(forceID);
		if (!reputationData.m_ReputationCamps.ContainsKey(typeIndex))
		{
			reputationData.m_ReputationCamps[typeIndex] = new ReputationCamp();
		}
		reputationData.m_ReputationCamps[typeIndex].exValue = value;
		NetworkManager.SyncProxy(EPacketType.PT_Reputation_SetExValue, forceID, typeIndex, value);
	}

	private void SetReputationValue(int forceID, int typeIndex, int value)
	{
		ReputationData reputationData = GetReputationData(forceID);
		if (!reputationData.m_ReputationCamps.ContainsKey(typeIndex))
		{
			reputationData.m_ReputationCamps[typeIndex] = new ReputationCamp();
		}
		reputationData.m_ReputationCamps[typeIndex].reputationValue = value;
	}

	public void ActiveReputation(int forceID)
	{
		ReputationData reputationData = GetReputationData(forceID);
		reputationData.active = true;
		NetworkManager.SyncProxy(EPacketType.PT_Reputation_SetActive, forceID);
	}

	public static void SyncData(Player player)
	{
		NetworkManager.SyncPeer(player.OwnerView.owner, EPacketType.PT_Reputation_SyncValue, _self.Export());
	}

	public static void RPC_C2S_SetValue(BitStream stream, NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		int num3 = stream.Read<int>(new object[0]);
		_self.SetReputationValue(num, num2, num3);
		NetworkManager.SyncProxy(EPacketType.PT_Reputation_SetValue, num, num2, num3);
	}

	public static void RPC_C2S_SetExValue(BitStream stream, NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		int num3 = stream.Read<int>(new object[0]);
		_self.SetEXValue(num, num2, num3);
		NetworkManager.SyncProxy(EPacketType.PT_Reputation_SetExValue, num, num2, num3);
	}

	public static void RPC_C2S_SetActive(BitStream stream, NetworkMessageInfo info)
	{
		int num = stream.Read<int>(new object[0]);
		_self.ActiveReputation(num);
		NetworkManager.SyncProxy(EPacketType.PT_Reputation_SetActive, num);
	}

	public void Import(byte[] data)
	{
		m_ReputationDatas.Clear();
		using MemoryStream input = new MemoryStream(data);
		using BinaryReader binaryReader = new BinaryReader(input);
		int num = binaryReader.ReadInt32();
		if (num == 7)
		{
			int num2 = binaryReader.ReadInt32();
			for (int i = 0; i < num2; i++)
			{
				int forceID = binaryReader.ReadInt32();
				ReputationData reputationData = GetReputationData(forceID);
				reputationData.Import(binaryReader);
			}
		}
	}

	public byte[] Export()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(7);
		binaryWriter.Write(m_ReputationDatas.Count);
		foreach (int key in m_ReputationDatas.Keys)
		{
			binaryWriter.Write(key);
			ReputationData reputationData = GetReputationData(key);
			reputationData.Export(binaryWriter);
		}
		return memoryStream.ToArray();
	}

	public void Save()
	{
		ReputationDBData reputationDBData = new ReputationDBData();
		reputationDBData.ExportData(Export());
		AsyncSqlite.AddRecord(reputationDBData);
	}

	public void Load()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT * FROM reputation;");
			pEDbOp.BindReaderHandler(LoadCompleted);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	public void LoadCompleted(SqliteDataReader reader)
	{
		while (reader.Read())
		{
			reader.GetInt32(reader.GetOrdinal("ver"));
			byte[] data = (byte[])reader.GetValue(reader.GetOrdinal("data"));
			Import(data);
		}
	}
}
