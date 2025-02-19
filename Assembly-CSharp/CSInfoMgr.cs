using System;
using Mono.Data.SqliteClient;
using UnityEngine;

public class CSInfoMgr
{
	public static CSAssemblyInfo m_AssemblyInfo = new CSAssemblyInfo();

	public static CSPowerPlantInfo m_ppCoal = new CSPowerPlantInfo();

	public static CSStorageInfo m_StorageInfo = new CSStorageInfo();

	public static CSEnginnerInfo m_EngineerInfo = new CSEnginnerInfo();

	public static CSRepairInfo m_RepairInfo = new CSRepairInfo();

	public static CSEnhanceInfo m_EnhanceInfo = new CSEnhanceInfo();

	public static CSRecycleInfo m_RecycleInfo = new CSRecycleInfo();

	public static CSDwellingsInfo m_DwellingsInfo = new CSDwellingsInfo();

	public static CSFarmInfo m_FarmInfo = new CSFarmInfo();

	public static CSFactoryInfo m_FactoryInfo = new CSFactoryInfo();

	public static CSProcessingInfo m_ProcessingInfo = new CSProcessingInfo();

	public static CSTradeInfo m_Trade = new CSTradeInfo();

	public static CSTrainingInfo m_Train = new CSTrainingInfo();

	public static CSCheckInfo m_Check = new CSCheckInfo();

	public static CSTreatInfo m_Treat = new CSTreatInfo();

	public static CSTentInfo m_Tent = new CSTentInfo();

	public static CSPowerPlantInfo m_ppFusion = new CSPowerPlantInfo();

	public static void LoadData()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("colonyinfo");
		while (sqliteDataReader.Read())
		{
			int id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			PublicAttr(GetInfo(id), sqliteDataReader);
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_property"));
			if (@string != "0")
			{
				SplitInfo(@string, id);
			}
		}
	}

	private static void PublicAttr(CSInfo info, SqliteDataReader reader)
	{
		info.m_Durability = Convert.ToSingle(reader.GetString(reader.GetOrdinal("durability")));
		info.m_RepairTime = Convert.ToSingle(reader.GetString(reader.GetOrdinal("repairtime")));
		info.m_DeleteTime = Convert.ToSingle(reader.GetString(reader.GetOrdinal("deletetime")));
		info.m_Power = Convert.ToSingle(reader.GetString(reader.GetOrdinal("power")));
		info.m_WorkersCnt = Convert.ToInt32(reader.GetString(reader.GetOrdinal("workerscnt")));
		info.workSound = Convert.ToInt32(reader.GetString(reader.GetOrdinal("worksound")));
		if (Application.isEditor)
		{
			info.m_DeleteTime = 20f;
		}
	}

	public static CSInfo GetInfo(int id)
	{
		switch (id)
		{
		case 1127:
			return m_AssemblyInfo;
		case 1128:
			return m_ppCoal;
		case 1129:
			return m_StorageInfo;
		case 1130:
			return m_RepairInfo;
		case 1131:
			return m_DwellingsInfo;
		case 1132:
			return m_EnhanceInfo;
		case 1133:
			return m_RecycleInfo;
		case 1134:
			return m_FarmInfo;
		case 1135:
			return m_FactoryInfo;
		case 1356:
			return m_ProcessingInfo;
		case 1357:
			return m_Trade;
		case 1423:
			return m_Train;
		case 1424:
			return m_Train;
		case 1422:
			return m_Treat;
		case 1421:
			return m_Tent;
		case 1558:
			return m_ppFusion;
		default:
			Debug.LogError("ColonySystem itemid is wrong id = " + id);
			return null;
		}
	}

	private static void SplitInfo(string str, int id)
	{
		if (str.Length != 0)
		{
			switch (id)
			{
			case 1127:
				LoadAssemblyInfo(str);
				break;
			case 1128:
				LoadPPCoalInfo(str);
				break;
			case 1129:
				LoadStorageInfo(str);
				break;
			case 1130:
				LoadRepairInfo(str);
				break;
			case 1131:
				LoadDwellingsInfo(str);
				break;
			case 1132:
				LoadEnhanceInfo(str);
				break;
			case 1133:
				LoadRecycleInfo(str);
				break;
			case 1134:
				LoadFarmInfo(str);
				break;
			case 1135:
				LoadFactoryInfo(str);
				break;
			case 1356:
				LoadProcessingInfo(str);
				break;
			case 1357:
				LoadTradeInfo(str);
				break;
			case 1423:
				LoadTrainInfo(str);
				break;
			case 1424:
				LoadCheckInfo(str);
				break;
			case 1422:
				LoadTreatInfo(str);
				break;
			case 1421:
				LoadTentInfo(str);
				break;
			case 1558:
				LoadFusionInfo(str);
				break;
			default:
				Debug.LogError("ColonyMgr itemid is wrong id = " + id);
				break;
			}
		}
	}

	private static void LoadAssemblyInfo(string str)
	{
		string[] array = str.Split(';');
		for (int i = 0; i < array.Length; i++)
		{
			CSAssemblyInfo.LevelData levelData = new CSAssemblyInfo.LevelData();
			string[] array2 = array[i].Split(',');
			if (array2.Length < 19)
			{
				Debug.LogError("LoadAssemblyInfo load error");
				break;
			}
			levelData.radius = Convert.ToSingle(array2[0]);
			levelData.dwellingsCnt = Convert.ToInt32(array2[1]);
			levelData.storageCnt = Convert.ToInt32(array2[2]);
			levelData.farmCnt = Convert.ToInt32(array2[3]);
			levelData.factoryCnt = Convert.ToInt32(array2[4]);
			levelData.EngineeringCnt = Convert.ToInt32(array2[5]);
			levelData.RepairMachineCnt = Convert.ToInt32(array2[6]);
			levelData.EnhanceMachineCnt = Convert.ToInt32(array2[7]);
			levelData.RecycleMachineCnt = Convert.ToInt32(array2[8]);
			levelData.coalPlantCnt = Convert.ToInt32(array2[9]);
			levelData.processingCnt = Convert.ToInt32(array2[10]);
			levelData.tradePostCnt = Convert.ToInt32(array2[11]);
			levelData.trainCenterCnt = Convert.ToInt32(array2[12]);
			levelData.medicalCheckCnt = Convert.ToInt32(array2[13]);
			levelData.medicalTreatCnt = Convert.ToInt32(array2[14]);
			levelData.medicalTentCnt = Convert.ToInt32(array2[15]);
			levelData.fusionPlantCnt = Convert.ToInt32(array2[16]);
			string[] array3 = array2[17].Split('|');
			for (int j = 0; j < array3.Length; j++)
			{
				levelData.itemIDList.Add(Convert.ToInt32(array3[j]));
			}
			string[] array4 = array2[18].Split('|');
			for (int k = 0; k < array4.Length; k++)
			{
				levelData.itemCnt.Add(Convert.ToInt32(array4[k]));
			}
			levelData.upgradeTime = Convert.ToSingle(array2[19]);
			if (Application.isEditor)
			{
				levelData.upgradeTime = 20f;
			}
			m_AssemblyInfo.m_Levels.Add(levelData);
		}
	}

	private static void LoadDwellingsInfo(string str)
	{
	}

	private static void LoadEnhanceInfo(string str)
	{
		string[] array = str.Split(';');
		if (array.Length < 1)
		{
			Debug.LogError("LoadEnhanceInfo load error");
		}
		else
		{
			m_EnhanceInfo.m_BaseTime = Convert.ToSingle(array[0]);
		}
	}

	private static void LoadFarmInfo(string str)
	{
	}

	private static void LoadPowerPlantInfo(CSPowerPlantInfo info, string str)
	{
		string[] array = str.Split(';');
		if (array.Length < 5)
		{
			Debug.LogError("LoadPowerPlantInfo load error");
			return;
		}
		info.m_Radius = Convert.ToSingle(array[0]);
		info.m_WorkedTime = Convert.ToSingle(array[1]);
		info.m_ChargingRate = Convert.ToSingle(array[2]);
		string[] array2 = array[3].Split(',');
		for (int i = 0; i < array2.Length; i++)
		{
			info.m_WorkedTimeItemID.Add(Convert.ToInt32(array2[i]));
		}
		string[] array3 = array[4].Split(',');
		for (int j = 0; j < array3.Length; j++)
		{
			info.m_WorkedTimeItemCnt.Add(Convert.ToInt32(array3[j]));
		}
	}

	private static void LoadRecycleInfo(string str)
	{
		string[] array = str.Split(';');
		if (array.Length < 1)
		{
			Debug.LogError("LoadRecycleInfo load error");
		}
		else
		{
			m_RecycleInfo.m_BaseTime = Convert.ToSingle(array[0]);
		}
	}

	private static void LoadRepairInfo(string str)
	{
		string[] array = str.Split(';');
		if (array.Length < 1)
		{
			Debug.LogError("LoadRepairInfo load error");
		}
		else
		{
			m_RepairInfo.m_BaseTime = Convert.ToSingle(array[0]);
		}
	}

	private static void LoadStorageInfo(string str)
	{
		string[] array = str.Split(';');
		if (array.Length < 3)
		{
			Debug.LogError("LoadStorageInfo load error");
			return;
		}
		m_StorageInfo.m_MaxItem = Convert.ToInt32(array[0]);
		m_StorageInfo.m_MaxEquip = Convert.ToInt32(array[1]);
		m_StorageInfo.m_MaxRecource = Convert.ToInt32(array[2]);
		m_StorageInfo.m_MaxArmor = Convert.ToInt32(array[3]);
	}

	private static void LoadPPCoalInfo(string str)
	{
		LoadPowerPlantInfo(m_ppCoal, str);
	}

	private static void LoadFusionInfo(string str)
	{
		LoadPowerPlantInfo(m_ppFusion, str);
	}

	private static void LoadFactoryInfo(string str)
	{
	}

	private static void LoadProcessingInfo(string str)
	{
	}

	private static void LoadTradeInfo(string str)
	{
	}

	private static void LoadTrainInfo(string str)
	{
		string[] array = str.Split(';');
		if (array.Length < 1)
		{
			Debug.LogError("LoadTrainInfo load error");
		}
		else
		{
			m_Train.m_BaseTime = Convert.ToSingle(array[0]);
		}
	}

	private static void LoadCheckInfo(string str)
	{
	}

	private static void LoadTreatInfo(string str)
	{
	}

	private static void LoadTentInfo(string str)
	{
	}
}
