using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using Pathea;
using UnityEngine;

public class StoryDoodadMap
{
	public static Dictionary<int, StoryDoodadDesc> s_dicDoodadData = new Dictionary<int, StoryDoodadDesc>(320);

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("sceneAssetList");
		int ordinal = sqliteDataReader.GetOrdinal("ID");
		int ordinal2 = sqliteDataReader.GetOrdinal("type");
		int ordinal3 = sqliteDataReader.GetOrdinal("IsDamage");
		int ordinal4 = sqliteDataReader.GetOrdinal("PrototypeDoodad_Id");
		int ordinal5 = sqliteDataReader.GetOrdinal("PosXYZ");
		int ordinal6 = sqliteDataReader.GetOrdinal("RotXYZW");
		int ordinal7 = sqliteDataReader.GetOrdinal("ScaleXYZ");
		int ordinal8 = sqliteDataReader.GetOrdinal("DoodadType");
		int ordinal9 = sqliteDataReader.GetOrdinal("Param");
		while (sqliteDataReader.Read())
		{
			StoryDoodadDesc storyDoodadDesc = new StoryDoodadDesc();
			storyDoodadDesc._id = Convert.ToInt32(sqliteDataReader.GetString(ordinal));
			storyDoodadDesc._type = Convert.ToInt32(sqliteDataReader.GetString(ordinal2));
			storyDoodadDesc._protoId = Convert.ToInt32(sqliteDataReader.GetString(ordinal4));
			storyDoodadDesc._isDamagable = 0 != Convert.ToInt32(sqliteDataReader.GetString(ordinal3));
			storyDoodadDesc._doodadType = Convert.ToInt32(sqliteDataReader.GetString(ordinal8));
			storyDoodadDesc._param = sqliteDataReader.GetString(ordinal9);
			string[] array = sqliteDataReader.GetString(ordinal5).Split(',');
			string[] array2 = sqliteDataReader.GetString(ordinal6).Split(',');
			string[] array3 = sqliteDataReader.GetString(ordinal7).Split(',');
			storyDoodadDesc._pos = new Vector3(Convert.ToSingle(array[0]), Convert.ToSingle(array[1]), Convert.ToSingle(array[2]));
			storyDoodadDesc._rot = new Quaternion(Convert.ToSingle(array2[0]), Convert.ToSingle(array2[1]), Convert.ToSingle(array2[2]), Convert.ToSingle(array2[3]));
			if (storyDoodadDesc._rot.w > 2f)
			{
				storyDoodadDesc._rot.eulerAngles = new Vector3(storyDoodadDesc._rot.x, storyDoodadDesc._rot.y, storyDoodadDesc._rot.z);
			}
			storyDoodadDesc._scl = new Vector3(Convert.ToSingle(array3[0]), Convert.ToSingle(array3[1]), Convert.ToSingle(array3[2]));
			s_dicDoodadData.Add(storyDoodadDesc._id, storyDoodadDesc);
		}
	}

	public static StoryDoodadDesc Get(int id)
	{
		if (s_dicDoodadData.ContainsKey(id))
		{
			return s_dicDoodadData[id];
		}
		return null;
	}

	public static StoryDoodadDesc GetByProtoId(int protoId)
	{
		foreach (StoryDoodadDesc value in s_dicDoodadData.Values)
		{
			if (value._protoId == protoId)
			{
				return value;
			}
		}
		return null;
	}

	public static void CreateAllStoryDoodad()
	{
		foreach (KeyValuePair<int, StoryDoodadDesc> s_dicDoodadDatum in s_dicDoodadData)
		{
			DoodadMgr.CreateDoodad(-1, -1, 200, s_dicDoodadDatum.Key, IdGenerator.NewDoodadId, s_dicDoodadDatum.Value._protoId, s_dicDoodadDatum.Value._pos, s_dicDoodadDatum.Value._doodadType, s_dicDoodadDatum.Value._param, s_dicDoodadDatum.Value._scl);
		}
	}
}
