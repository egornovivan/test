using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class StoryDoodadMap
{
	public static Dictionary<int, SceneDoodadDesc> s_dicDoodadData = new Dictionary<int, SceneDoodadDesc>(320);

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("sceneAssetList");
		int ordinal = sqliteDataReader.GetOrdinal("ID");
		int ordinal2 = sqliteDataReader.GetOrdinal("type");
		int ordinal3 = sqliteDataReader.GetOrdinal("IsProduce");
		int ordinal4 = sqliteDataReader.GetOrdinal("IsDamage");
		int ordinal5 = sqliteDataReader.GetOrdinal("PrototypeDoodad_Id");
		int ordinal6 = sqliteDataReader.GetOrdinal("PosXYZ");
		int ordinal7 = sqliteDataReader.GetOrdinal("RotXYZW");
		int ordinal8 = sqliteDataReader.GetOrdinal("ScaleXYZ");
		while (sqliteDataReader.Read())
		{
			SceneDoodadDesc sceneDoodadDesc = new SceneDoodadDesc();
			sceneDoodadDesc._id = sqliteDataReader.GetInt32(ordinal);
			sceneDoodadDesc._type = sqliteDataReader.GetInt32(ordinal2);
			sceneDoodadDesc._protoId = sqliteDataReader.GetInt32(ordinal5);
			sceneDoodadDesc._isShown = 0 != sqliteDataReader.GetInt32(ordinal3);
			SceneDoodadDesc.GetCampDamageId(0 != sqliteDataReader.GetInt32(ordinal4), out sceneDoodadDesc._campId, out sceneDoodadDesc._damageId);
			string[] array = sqliteDataReader.GetString(ordinal6).Split(',');
			string[] array2 = sqliteDataReader.GetString(ordinal7).Split(',');
			string[] array3 = sqliteDataReader.GetString(ordinal8).Split(',');
			sceneDoodadDesc._pos = new Vector3(Convert.ToSingle(array[0]), Convert.ToSingle(array[1]), Convert.ToSingle(array[2]));
			sceneDoodadDesc._rot = new Quaternion(Convert.ToSingle(array2[0]), Convert.ToSingle(array2[1]), Convert.ToSingle(array2[2]), Convert.ToSingle(array2[3]));
			if (sceneDoodadDesc._rot.w > 2f)
			{
				sceneDoodadDesc._rot.eulerAngles = new Vector3(sceneDoodadDesc._rot.x, sceneDoodadDesc._rot.y, sceneDoodadDesc._rot.z);
			}
			sceneDoodadDesc._scl = new Vector3(Convert.ToSingle(array3[0]), Convert.ToSingle(array3[1]), Convert.ToSingle(array3[2]));
			s_dicDoodadData.Add(sceneDoodadDesc._id, sceneDoodadDesc);
		}
	}

	public static SceneDoodadDesc Get(int id)
	{
		if (s_dicDoodadData.ContainsKey(id))
		{
			return s_dicDoodadData[id];
		}
		return null;
	}
}
