using System;
using System.Collections.Generic;
using System.IO;
using Mono.Data.SqliteClient;
using PETools;
using UnityEngine;

public class NpcManager : MonoBehaviour
{
	private const string npcPrefabPath = "Prefab/Npc/";

	private static NpcManager _instance;

	private static Dictionary<Vector2, int> townNpcs = new Dictionary<Vector2, int>();

	private static Dictionary<Vector3, int> buildingNpcs = new Dictionary<Vector3, int>();

	private static Dictionary<int, Vector3> storyNpcs = new Dictionary<int, Vector3>();

	private static Dictionary<int, Vector3> storyRDNpcs = new Dictionary<int, Vector3>();

	public static NpcManager Instance => _instance;

	private void Awake()
	{
		_instance = this;
	}

	public static bool IsRandomNpc(int npc_ID)
	{
		return npc_ID / 92 == 100 || npc_ID < 9000;
	}

	public static void LoadComplete(SqliteDataReader reader)
	{
		while (reader.Read())
		{
			int @int = reader.GetInt32(reader.GetOrdinal("ver"));
			int id = reader.GetInt32(reader.GetOrdinal("id"));
			int worldId = reader.GetInt32(reader.GetOrdinal("worldid"));
			byte[] buff = (byte[])reader.GetValue(reader.GetOrdinal("basedata"));
			Serialize.Import(buff, delegate(BinaryReader r)
			{
				int templateId = BufferHelper.ReadInt32(r);
				int num = BufferHelper.ReadInt32(r);
				float scale = BufferHelper.ReadSingle(r);
				BufferHelper.ReadVector3(r, out var _value);
				bool forcedServant = BufferHelper.ReadBoolean(r);
				string npcName = BufferHelper.ReadString(r);
				switch (num)
				{
				case 3:
					if (HasBuildingNpc(_value))
					{
						return;
					}
					break;
				case 2:
					if (HasTownNpc(new Vector2(_value.x, _value.z)))
					{
						return;
					}
					break;
				}
				id = SPTerrainEvent.CreateNpcWithoutLimit(-1, worldId, _value, templateId, num, scale, id, isStand: false, 0f, forcedServant, npcName);
				if (id != -1)
				{
					switch (num)
					{
					case 3:
						AddBuildingNpc(_value, id);
						break;
					case 2:
					{
						Vector2 pos = new Vector2(_value.x, _value.z);
						AddTownNpc(pos, id);
						break;
					}
					}
				}
			});
		}
	}

	public static void Load()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT ver,id,worldid,basedata FROM npcdata;");
			pEDbOp.BindReaderHandler(LoadComplete);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	public static void LoadData()
	{
		LoadNormalNpcNameIdFromDB();
		StoryNpcMgr.LoadStoryNpc();
	}

	internal static void MapNpcNameToId(string npcName, int npcId)
	{
		if (string.IsNullOrEmpty(npcName))
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("npc name is invalid.");
			}
		}
		else if (npcId <= 0 && LogFilter.logDebug)
		{
			Debug.LogError("npc id is invalid[" + npcId + "]");
		}
	}

	private static void LoadNormalNpcNameIdFromDB()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("NPC");
		while (sqliteDataReader.Read())
		{
			int num = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPC_ID")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("NPC_name"));
		}
	}

	internal static void AddTownNpc(Vector2 pos, int objID)
	{
		townNpcs.Add(pos, objID);
	}

	internal static bool HasTownNpc(Vector2 pos)
	{
		return townNpcs.ContainsKey(pos);
	}

	internal static void AddBuildingNpc(Vector3 pos, int objID)
	{
		buildingNpcs.Add(pos, objID);
	}

	internal static bool HasBuildingNpc(Vector3 pos)
	{
		return buildingNpcs.ContainsKey(pos);
	}

	public static string GetNpcNameById(int npcId)
	{
		AiAdNpcNetwork aiAdNpcNetwork = ObjNetInterface.Get<AiAdNpcNetwork>(npcId);
		if (null != aiAdNpcNetwork)
		{
			return aiAdNpcNetwork.NpcName;
		}
		return string.Empty;
	}
}
