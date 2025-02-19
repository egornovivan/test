using System;
using System.IO;
using AiAsset;
using CustomCharactor;
using Mono.Data.SqliteClient;
using NaturalResAsset;
using Pathea;
using UnityEngine;

public class LocalDatabase : MonoBehaviour
{
	private static string s_tmpDbFileName;

	private static SqliteAccessCS s_localDatabase;

	public static SqliteAccessCS Instance => s_localDatabase;

	private static SqliteAccessCS LoadDb()
	{
		TextAsset textAsset = Resources.Load("DataBase/localData", typeof(TextAsset)) as TextAsset;
		s_tmpDbFileName = Path.GetTempFileName();
		using (FileStream fileStream = new FileStream(s_tmpDbFileName, FileMode.Open, FileAccess.Write, FileShare.None))
		{
			fileStream.Write(textAsset.bytes, 0, textAsset.bytes.Length);
		}
		return new SqliteAccessCS(s_tmpDbFileName);
	}

	private void Awake()
	{
		if (s_localDatabase == null)
		{
			Debug.Log("Mem size after App start :" + GC.GetTotalMemory(forceFullCollection: true));
			s_localDatabase = LoadDb();
			PELocalization.LoadData();
			NaturalRes.LoadData();
			StoreRepository.LoadData();
			ShopRespository.LoadData();
			AiHatredData.LoadData();
			AiHarmData.LoadData();
			AiDamageTypeData.LoadData();
			AIResource.LoadData();
			SKAttribute.LoadData();
			RandomNpcDb.Load();
			MonsterProtoDb.Load();
			MonsterGroupProtoDb.Load();
			NpcProtoDb.Load();
			PlayerProtoDb.Load();
			TowerProtoDb.Load();
			ItemDropData.LoadData();
			MonsterRandomDb.Load();
			NpcManager.LoadData();
			NpcMissionDataRepository.LoadData();
			BlockBuilding.LoadBuilding();
			BSBlockMatMap.Load();
			BSVoxelMatMap.Load();
			SceneBoxMgr.LoadData();
			MissionManager.LoadData();
			NameGenerater.Load();
			CustomMetaData.LoadData();
			BattleConstData.LoadData();
			LifeFormRule.LoadData();
			SkillTreeInfo.LoadData();
			PlantInfo.LoadData();
			ActionProcess._self.LoadInfo();
			ActionEventsMgr._self.LoadInfo();
			RandomItemDataMgr.LoadData();
			FecesData.LoadData();
			RandomDungeonDataBase.LoadData();
			ColonyMgr.LoadInfo();
			CSTradeInfoData.LoadData();
			ProcessingObjInfo.LoadData();
			CSMedicineSupport.LoadData();
			AbnormalData.LoadData();
			AbnormalTypeTreatData.LoadData();
			NpcAbility.Load();
			StoryDoodadMap.LoadData();
			DoodadProtoDb.Load();
			AttPlusNPCData.Load();
			AttPlusBuffDb.Load();
			NpcEatDb.LoadData();
			MonsterHandbookData.LoadData();
		}
	}

	private void OnApplicationQuit()
	{
		Debug.Log("Mem size before ocl CleanUp :" + GC.GetTotalMemory(forceFullCollection: true));
		NaturalRes.s_tblNaturalRes = null;
		s_localDatabase.CloseDB();
		File.Delete(s_tmpDbFileName);
	}

	public static string findGrid(string listName, int id, string lineName)
	{
		SqliteDataReader sqliteDataReader = Instance.ReadFullTable(listName);
		string result = null;
		int num = 0;
		while (sqliteDataReader.Read())
		{
			if (num > 1)
			{
				int num2 = Convert.ToInt32(sqliteDataReader.GetString(0));
				if (num2 == id)
				{
					return sqliteDataReader.GetString(sqliteDataReader.GetOrdinal(lineName));
				}
			}
			num++;
		}
		return result;
	}
}
