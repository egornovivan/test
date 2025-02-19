using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using Pathea;
using PETools;
using UnityEngine;

public class AISpawnTDWavesData
{
	public class TDMonsterData
	{
		private int _type;

		public int ProtoId;

		public bool IsGrp => _type == 1;

		public bool IsAirbornePaja => _type == 2;

		public bool IsAirbornePuja => _type == 3;

		public static List<TDMonsterData> GetMonsterDataLst(string strMonsterDatasDesc)
		{
			string[] array = strMonsterDatasDesc.Split(',');
			int num = array.Length;
			List<TDMonsterData> list = new List<TDMonsterData>(num);
			for (int i = 0; i < num; i++)
			{
				string[] array2 = array[i].Split('_');
				TDMonsterData tDMonsterData = new TDMonsterData();
				tDMonsterData._type = Convert.ToInt32(array2[0]);
				tDMonsterData.ProtoId = Convert.ToInt32(array2[1]);
				list.Add(tDMonsterData);
			}
			return list;
		}
	}

	public class TDWaveData
	{
		public int _delayTime;

		public List<int> _plotID = new List<int>();

		public List<int> _monsterTypes = new List<int>();

		public List<int> _minNums = new List<int>();

		public List<int> _maxNums = new List<int>();

		public List<int> _minDegs = new List<int>();

		public List<int> _maxDegs = new List<int>();

		public static List<TDWaveData> GetWaveDataLst(string strWaveDatasDesc)
		{
			string[] array = strWaveDatasDesc.Split(';');
			int num = array.Length;
			List<TDWaveData> list = new List<TDWaveData>(num);
			for (int i = 0; i < num; i++)
			{
				string[] array2 = array[i].Split('_', ',');
				TDWaveData tDWaveData = new TDWaveData();
				tDWaveData._delayTime = Convert.ToInt32(array2[0]);
				int num2 = 1;
				while (array2.Length > num2)
				{
					tDWaveData._monsterTypes.Add(Convert.ToInt32(array2[num2++]));
					tDWaveData._minNums.Add(Convert.ToInt32(array2[num2++]));
					tDWaveData._maxNums.Add(Convert.ToInt32(array2[num2++]));
					tDWaveData._minDegs.Add(Convert.ToInt32(array2[num2++]));
					tDWaveData._maxDegs.Add(Convert.ToInt32(array2[num2++]));
				}
				list.Add(tDWaveData);
			}
			return list;
		}
	}

	public class TDMonsterSpData
	{
		public int _spType;

		public int _spawnType;

		public int _areaTypeRandTer;

		public List<int> _areaTypeStoryTer;

		public int _dps;

		public int _rhp;

		public int _diflv;

		public List<TDMonsterData>[] _terMonsterDatas = new List<TDMonsterData>[4];

		public static List<int> GetAreaTypes(string strAreas)
		{
			string[] array = strAreas.Split(',');
			int num = array.Length;
			List<int> list = new List<int>(num);
			for (int i = 0; i < num; i++)
			{
				int item = Convert.ToInt32(array[i]);
				list.Add(item);
			}
			return list;
		}
	}

	public class TDWaveSpData
	{
		public int _dif;

		public float _weight;

		public int _spawnType;

		public int _timeToCool;

		public int _timeToStart;

		public int _timeToDelete;

		public List<TDWaveData> _waveDatas = new List<TDWaveData>();
	}

	private static List<TDMonsterSpData> _lstMonsterSpData;

	private static List<TDWaveSpData> _lstWaveSpData;

	public static void LoadData()
	{
		if (_lstWaveSpData == null)
		{
			_lstWaveSpData = new List<TDWaveSpData>();
			SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("MonsterBesiege");
			while (sqliteDataReader.Read())
			{
				TDWaveSpData tDWaveSpData = new TDWaveSpData();
				tDWaveSpData._dif = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("dif_coef")));
				tDWaveSpData._weight = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("weights")));
				tDWaveSpData._spawnType = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("spawn_type")));
				tDWaveSpData._timeToCool = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("cd_time")));
				tDWaveSpData._timeToStart = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("start_time")));
				tDWaveSpData._timeToDelete = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("remaining_time")));
				tDWaveSpData._waveDatas = TDWaveData.GetWaveDataLst(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("sp_type")));
				_lstWaveSpData.Add(tDWaveSpData);
			}
		}
		if (_lstMonsterSpData == null)
		{
			_lstMonsterSpData = new List<TDMonsterSpData>();
			SqliteDataReader sqliteDataReader2 = LocalDatabase.Instance.ReadFullTable("MonsterBesiegeSpawn");
			while (sqliteDataReader2.Read())
			{
				TDMonsterSpData tDMonsterSpData = new TDMonsterSpData();
				tDMonsterSpData._diflv = Convert.ToInt32(sqliteDataReader2.GetString(sqliteDataReader2.GetOrdinal("dif_lv")));
				tDMonsterSpData._spType = Convert.ToInt32(sqliteDataReader2.GetString(sqliteDataReader2.GetOrdinal("sp_type")));
				tDMonsterSpData._spawnType = Convert.ToInt32(sqliteDataReader2.GetString(sqliteDataReader2.GetOrdinal("spawn_type")));
				tDMonsterSpData._dps = Convert.ToInt32(sqliteDataReader2.GetString(sqliteDataReader2.GetOrdinal("dps")));
				tDMonsterSpData._rhp = Convert.ToInt32(sqliteDataReader2.GetString(sqliteDataReader2.GetOrdinal("rhp")));
				tDMonsterSpData._areaTypeRandTer = Convert.ToInt32(sqliteDataReader2.GetString(sqliteDataReader2.GetOrdinal("area_type_R")));
				tDMonsterSpData._areaTypeStoryTer = TDMonsterSpData.GetAreaTypes(sqliteDataReader2.GetString(sqliteDataReader2.GetOrdinal("area_type_S")));
				tDMonsterSpData._terMonsterDatas[0] = TDMonsterData.GetMonsterDataLst(sqliteDataReader2.GetString(sqliteDataReader2.GetOrdinal("land")));
				tDMonsterSpData._terMonsterDatas[1] = TDMonsterData.GetMonsterDataLst(sqliteDataReader2.GetString(sqliteDataReader2.GetOrdinal("water")));
				tDMonsterSpData._terMonsterDatas[2] = TDMonsterData.GetMonsterDataLst(sqliteDataReader2.GetString(sqliteDataReader2.GetOrdinal("hole")));
				tDMonsterSpData._terMonsterDatas[3] = TDMonsterData.GetMonsterDataLst(sqliteDataReader2.GetString(sqliteDataReader2.GetOrdinal("sky")));
				_lstMonsterSpData.Add(tDMonsterSpData);
			}
		}
	}

	public static TDWaveSpData GetWaveSpData(int dif, float weight, List<int> spawnTypes)
	{
		if (dif >= 500)
		{
			return _lstWaveSpData.Find((TDWaveSpData w) => dif == w._dif);
		}
		List<TDWaveSpData> list = _lstWaveSpData.FindAll((TDWaveSpData w) => dif == w._dif && spawnTypes.Contains(w._spawnType));
		if (list != null && list.Count > 0)
		{
			int num = 0;
			TDWaveSpData tDWaveSpData = list[num];
			do
			{
				tDWaveSpData = list[num];
				weight -= tDWaveSpData._weight;
				if (++num >= list.Count)
				{
					num = 0;
				}
			}
			while (weight > 0f);
			return tDWaveSpData;
		}
		return null;
	}

	public static TDMonsterSpData GetMonsterSpData(bool bRandTer, int spType, int diflv, int spawnType, int areaType = -1, int terType = 0)
	{
		TDMonsterSpData tDMonsterSpData = null;
		tDMonsterSpData = ((areaType < 0) ? _lstMonsterSpData.Find((TDMonsterSpData m) => m._spType == spType && m._spawnType == spawnType) : ((!bRandTer) ? _lstMonsterSpData.Find((TDMonsterSpData m) => m._spType == spType && m._spawnType == spawnType && m._areaTypeStoryTer.Contains(areaType)) : _lstMonsterSpData.Find((TDMonsterSpData m) => m._spType == spType && m._spawnType == spawnType && m._areaTypeRandTer == areaType)));
		if (tDMonsterSpData == null)
		{
			tDMonsterSpData = _lstMonsterSpData.Find((TDMonsterSpData m) => m._spType == -1 && m._diflv == diflv && m._spawnType == spawnType);
		}
		if (tDMonsterSpData == null)
		{
			tDMonsterSpData = _lstMonsterSpData.Find((TDMonsterSpData m) => m._spType == -1 && m._diflv == diflv && m._spawnType == -1);
		}
		return tDMonsterSpData;
	}

	public static TDMonsterData GetMonsterProtoId(bool bRandTer, int spType, int diflv, int spawnType, int areaType = -1, int terType = 0, int opPlayerId = -1)
	{
		TDMonsterSpData monsterSpData = GetMonsterSpData(bRandTer, spType, diflv, spawnType, areaType, terType);
		if (monsterSpData != null)
		{
			List<TDMonsterData> list = monsterSpData._terMonsterDatas[terType];
			int num = 0;
			while (num < 5)
			{
				TDMonsterData tDMonsterData = list[UnityEngine.Random.Range(0, list.Count)];
				if (opPlayerId >= 0 && !tDMonsterData.IsGrp)
				{
					MonsterProtoDb.Item item = MonsterProtoDb.Get(tDMonsterData.ProtoId);
					if (item != null)
					{
						int pid = (int)item.dbAttr.attributeArray[91];
						if (!PEUtil.CanDamageReputation(pid, opPlayerId))
						{
							num++;
							continue;
						}
					}
				}
				return tDMonsterData;
			}
		}
		return null;
	}
}
