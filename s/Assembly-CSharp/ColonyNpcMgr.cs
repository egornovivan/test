using System.Collections.Generic;
using System.IO;
using Mono.Data.SqliteClient;
using UnityEngine;

public class ColonyNpcMgr
{
	private static Dictionary<int, ColonyNpc> _colonyNpcList = new Dictionary<int, ColonyNpc>();

	private static Dictionary<int, List<ColonyNpc>> _colonyNpcTeamList = new Dictionary<int, List<ColonyNpc>>();

	public static void Add(int teamId, int id, ColonyNpc npc)
	{
		_colonyNpcList[id] = npc;
		npc._npcID = id;
		npc._refNpc = ObjNetInterface.Get<AiAdNpcNetwork>(id);
		if (npc._refNpc != null)
		{
			npc._refNpc.SetTeamId(teamId);
			if (!_colonyNpcTeamList.ContainsKey(teamId))
			{
				_colonyNpcTeamList.Add(teamId, new List<ColonyNpc>());
			}
			_colonyNpcTeamList[teamId].Add(npc);
		}
		else if (LogFilter.logDebug)
		{
			Debug.LogError("npc._npcID = " + npc._npcID + " _refNpc is null");
		}
	}

	public static void RemoveAt(int teamId, int id)
	{
		_colonyNpcList.Remove(id);
		if (_colonyNpcTeamList.ContainsKey(teamId))
		{
			_colonyNpcTeamList[teamId].RemoveAll((ColonyNpc it) => it._npcID == id);
		}
		Delete(id);
	}

	public static ColonyNpc GetNpcByID(int id)
	{
		if (_colonyNpcList.ContainsKey(id))
		{
			return _colonyNpcList[id];
		}
		return null;
	}

	public static List<ColonyNpc> GetTeamNpcs(int teamId)
	{
		if (_colonyNpcTeamList.ContainsKey(teamId))
		{
			return _colonyNpcTeamList[teamId];
		}
		return null;
	}

	public static bool IsColonyNpc(int id)
	{
		if (GetNpcByID(id) == null)
		{
			return false;
		}
		return true;
	}

	public static void LoadComplete(SqliteDataReader reader)
	{
		while (reader.Read())
		{
			int @int = reader.GetInt32(reader.GetOrdinal("ver"));
			ColonyNpc colonyNpc = new ColonyNpc();
			colonyNpc._npcID = reader.GetInt32(reader.GetOrdinal("id"));
			int int2 = reader.GetInt32(reader.GetOrdinal("teamid"));
			if (int2 == -1)
			{
				continue;
			}
			colonyNpc.m_State = reader.GetInt32(reader.GetOrdinal("state"));
			colonyNpc.m_DwellingsID = reader.GetInt32(reader.GetOrdinal("dwellingsid"));
			colonyNpc.m_Occupation = reader.GetInt32(reader.GetOrdinal("occupation"));
			colonyNpc.m_WorkRoomID = reader.GetInt32(reader.GetOrdinal("workroomid"));
			colonyNpc.m_WorkMode = reader.GetInt32(reader.GetOrdinal("workmode"));
			byte[] buffer = (byte[])reader.GetValue(reader.GetOrdinal("blobdata"));
			using (MemoryStream input = new MemoryStream(buffer))
			{
				using BinaryReader reader2 = new BinaryReader(input);
				colonyNpc.m_ProcessingIndex = BufferHelper.ReadInt32(reader2);
				colonyNpc.m_IsProcessing = BufferHelper.ReadBoolean(reader2);
				BufferHelper.ReadVector3(reader2, out var _value);
				colonyNpc.m_GuardPos = _value;
				colonyNpc.trainerType = (ETrainerType)BufferHelper.ReadInt32(reader2);
				colonyNpc.trainingType = (ETrainingType)BufferHelper.ReadInt32(reader2);
				colonyNpc.IsTraining = BufferHelper.ReadBoolean(reader2);
			}
			Add(int2, colonyNpc._npcID, colonyNpc);
			if (colonyNpc._refNpc == null)
			{
				continue;
			}
			ColonyDwellings colonyDwellings = (ColonyDwellings)ColonyMgr.GetColonyItemByObjId(colonyNpc.m_DwellingsID);
			if (colonyDwellings != null)
			{
				colonyDwellings.AddNpcs(colonyNpc._npcID);
				colonyNpc._refNpc.SetTeamId(colonyDwellings._Network.TeamId);
			}
			ColonyBase colonyItemByObjId = ColonyMgr.GetColonyItemByObjId(colonyNpc.m_WorkRoomID);
			if (colonyItemByObjId != null)
			{
				colonyItemByObjId.AddWorker(colonyNpc);
				if (colonyNpc.m_ProcessingIndex >= 0 && colonyNpc.m_Occupation == 5 && colonyItemByObjId is ColonyProcessing)
				{
					ColonyProcessing colonyProcessing = colonyItemByObjId as ColonyProcessing;
					colonyProcessing.InitNpcData(colonyNpc);
				}
				if (colonyNpc.m_Occupation == 7 && colonyNpc.trainerType != 0)
				{
					ColonyTrain colonyTrain = colonyItemByObjId as ColonyTrain;
					colonyTrain.InitNpcData(colonyNpc);
				}
			}
		}
		ColonyBase.IsNewPutOut = true;
		if (VersionMgr.colonyRecordVersion < VersionMgr.currentColonyVersion)
		{
			Save(isSync: false);
			VersionMgr.SaveColonyVersion();
			if (LogFilter.logDebug)
			{
				Debug.Log("save currentVersion colonyNpc");
			}
		}
	}

	public static void Load()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT * FROM colonynpc;");
			pEDbOp.BindReaderHandler(LoadComplete);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	public static void Save(bool isSync)
	{
		if (_colonyNpcList.Count > 0)
		{
			Dictionary<int, ColonyNpc>.ValueCollection values = _colonyNpcList.Values;
			ColonyNpcMgrData colonyNpcMgrData = new ColonyNpcMgrData();
			colonyNpcMgrData.ExportData(values);
			AsyncSqlite.AddRecord(colonyNpcMgrData);
		}
	}

	public static void Delete(int id)
	{
		ColonyNpcData colonyNpcData = new ColonyNpcData();
		colonyNpcData.DeleteData(id);
		AsyncSqlite.AddRecord(colonyNpcData);
	}
}
