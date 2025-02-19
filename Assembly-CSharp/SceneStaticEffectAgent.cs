using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

public class SceneStaticEffectAgent : SceneSerializableObjAgent
{
	private class PrototypeDesc
	{
		public int _pid;

		public string _strPrefab;

		public string _strAssetbundle;
	}

	private static List<PrototypeDesc> _protoDescs;

	public override bool NeedToActivate => false;

	public SceneStaticEffectAgent()
	{
	}

	private SceneStaticEffectAgent(string pathPrefab, string pathAssetbundle, Vector3 pos, Quaternion rotation, Vector3 scale, int id = 0)
		: base(pathPrefab, pathAssetbundle, pos, rotation, scale, id)
	{
	}

	public static void LoadData()
	{
		_protoDescs = new List<PrototypeDesc>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("PrototypeEffect");
		while (sqliteDataReader.Read())
		{
			PrototypeDesc prototypeDesc = new PrototypeDesc();
			prototypeDesc._pid = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			prototypeDesc._strAssetbundle = null;
			prototypeDesc._strPrefab = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("prefab_path"));
			_protoDescs.Add(prototypeDesc);
		}
	}

	public static SceneStaticEffectAgent Create(int protoId, Vector3 pos, Quaternion rotation, Vector3 scale, int id = 0)
	{
		if (_protoDescs == null)
		{
			LoadData();
		}
		PrototypeDesc prototypeDesc = _protoDescs.Find((PrototypeDesc it) => it._pid == protoId);
		if (prototypeDesc != null)
		{
			return new SceneStaticEffectAgent(prototypeDesc._strPrefab, prototypeDesc._strAssetbundle, pos, rotation, scale, id);
		}
		return null;
	}
}
