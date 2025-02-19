using System;
using System.IO;
using AiAsset;
using CustomCharactor;
using Mono.Data.SqliteClient;
using NaturalResAsset;
using Pathea;
using Pathea.Effect;
using Pathea.Projectile;
using SkillSystem;
using SoundAsset;
using UnityEngine;

public class LocalDatabase : MonoBehaviour
{
	private static string s_tmpDbFileName;

	private static SqliteAccessCS s_localDatabase;

	public static SqliteAccessCS Instance => s_localDatabase;

	public static SqliteAccessCS PureInstance => s_localDatabase;

	private static SqliteAccessCS LoadDb()
	{
		try
		{
			PELocalization.LoadData(GameConfig.DataBaseI18NPath);
			Debug.Log("[I18N]Succeed to load data:" + GameConfig.DataBaseI18NPath);
		}
		catch
		{
			Debug.LogError("[I18N]Failed to load data:" + GameConfig.DataBaseI18NPath);
		}
		TextAsset textAsset = Resources.Load("DataBase/localData", typeof(TextAsset)) as TextAsset;
		try
		{
			s_tmpDbFileName = Path.GetTempFileName();
			using FileStream fileStream = new FileStream(s_tmpDbFileName, FileMode.Open, FileAccess.Write, FileShare.None);
			fileStream.Write(textAsset.bytes, 0, textAsset.bytes.Length);
		}
		catch
		{
			string tempPath = Path.GetTempPath();
			if (!Directory.Exists(tempPath))
			{
				Directory.CreateDirectory(tempPath);
			}
			do
			{
				s_tmpDbFileName = Path.Combine(tempPath, Path.GetRandomFileName());
			}
			while (File.Exists(s_tmpDbFileName));
			Debug.LogWarning("Failed to create temp file!  Retry with " + s_tmpDbFileName);
			try
			{
				using FileStream fileStream2 = new FileStream(s_tmpDbFileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
				fileStream2.Write(textAsset.bytes, 0, textAsset.bytes.Length);
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to create temp file!  Try running the game as administrator. \r\n\r\n" + ex.ToString());
			}
		}
		return new SqliteAccessCS(s_tmpDbFileName);
	}

	public static void LoadAllData()
	{
		if (s_localDatabase == null)
		{
			s_localDatabase = LoadDb();
			SkData.LoadData();
			EffectData.LoadData();
			ProjectileData.LoadData();
			RequestRelation.LoadData();
			CampData.LoadData();
			ThreatData.LoadData();
			DamageData.LoadData();
			HumanSoundData.LoadData();
			ItemDropData.LoadData();
			PELocalization.LoadData();
			NaturalRes.LoadData();
			AiData.LoadData();
			SESoundBuff.LoadData();
			SESoundStory.LoadData();
			StoryDoodadMap.LoadData();
			StoreRepository.LoadData();
			NpcMissionDataRepository.LoadData();
			MissionRepository.LoadData();
			TalkRespository.LoadData();
			ShopRespository.LoadData();
			WareHouseManager.LoadData();
			MutiPlayRandRespository.LoadData();
			PromptRepository.LoadData();
			CampPatrolData.LoadDate();
			Camp.LoadData();
			RepProcessor.LoadData();
			CloudManager.LoadData();
			TutorialData.LoadData();
			MapMaskData.LoadDate();
			MessageData.LoadData();
			MonsterHandbookData.LoadData();
			StoryRepository.LoadData();
			RMRepository.LoadRandMission();
			MisInitRepository.LoadData();
			CameraRepository.LoadCameraPlot();
			AdRMRepository.LoadData();
			VCConfig.InitConfig();
			Cutscene.LoadData();
			BSPattern.LoadBrush();
			BSVoxelMatMap.Load();
			BSBlockMatMap.Load();
			BlockBuilding.LoadBuilding();
			LifeFormRule.LoadData();
			PlantInfo.LoadData();
			MetalScanData.LoadData();
			BattleConstData.LoadData();
			CustomMetaData.LoadData();
			SkillTreeInfo.LoadData();
			VArtifactUtil.LoadData();
			ActionRelationData.LoadActionRelation();
			CSInfoMgr.LoadData();
			ProcessingObjInfo.LoadData();
			CSTradeInfoData.LoadData();
			CampTradeIdData.LoadData();
			AbnormalTypeTreatData.LoadData();
			CSMedicineSupport.LoadData();
			RandomItemDataMgr.LoadData();
			FecesData.LoadData();
			RandomDungeonDataBase.LoadData();
			AbnormalData.LoadData();
			PEAbnormalNoticeData.LoadData();
			RelationInfo.LoadData();
			EquipSetData.LoadData();
			SuitSetData.LoadData();
			CheatData.LoadData();
			NpcProtoDb.Load();
			MonsterProtoDb.Load();
			MonsterRandomDb.Load();
			MonsterGroupProtoDb.Load();
			RandomNpcDb.Load();
			PlayerProtoDb.Load();
			TowerProtoDb.Load();
			DoodadProtoDb.Load();
			AttPlusNPCData.Load();
			AttPlusBuffDb.Load();
			NpcTypeDb.Load();
			NpcRandomTalkDb.Load();
			NpcThinkDb.LoadData();
			NpcEatDb.LoadData();
			NpcRobotDb.Load();
			NPCScheduleData.Load();
			NpcVoiceDb.LoadData();
			InGameAidData.LoadData();
			MountsSkillDb.LoadData();
			Debug.Log("Database Loaded");
		}
	}

	public static void FreeAllData()
	{
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
