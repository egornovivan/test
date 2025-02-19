using ItemAsset;
using Railway;

public class LoadManager
{
	internal static void Load()
	{
		VersionMgr.LoadVersion();
		SteamWorks.Load();
		ItemManager.LoadItems();
		SceneObjMgr.Load();
		SceneObjMgr.LoadSceneIds();
		DropItemManager.LoadNetworkObj();
		GameWorld.LoadAll();
		NpcManager.Load();
		MissionManager.Manager.LoadPlayerMission();
		MissionManager.Manager.LoadTeamMission();
		RailwayManager.Instance.LoadData();
		FarmManager.LoadData();
		ColonyNpcMgr.Load();
		ColonyMgr.Load();
		AISpawnPoint.LoadData();
		ReputationSystem._self.Load();
		ServerAdministrator.ReadConfig();
		switch (ServerConfig.SceneMode)
		{
		case ESceneMode.Story:
			if (ServerConfig.IsNewServer)
			{
				NpcMissionDataRepository.CreateStoryNpc();
				StoryDoodadMap.CreateAllStoryDoodad();
			}
			else
			{
				DoodadMgr.LoadData();
			}
			AISpawnPoint.CreateKillMonsterFix();
			PublicData.Self.LoadData();
			break;
		case ESceneMode.Adventure:
			if (!ServerConfig.IsNewServer)
			{
				DoodadMgr.LoadData();
			}
			break;
		case ESceneMode.Custom:
			if (ServerConfig.IsNewServer)
			{
				CustomGameData.Mgr.LoadGameData();
				SPTerrainEvent.SaveCustomNpc();
				break;
			}
			DoodadMgr.LoadData();
			CustomGameData.Mgr.LoadMonster();
			CustomGameData.Mgr.LoadCustomDialogs();
			SPTerrainEvent.LoadCustomNpc();
			break;
		case ESceneMode.Build:
		case ESceneMode.TowerDefense:
			break;
		}
	}

	public static void Init()
	{
		GameTime.Instance.Init();
		FarmManager.Init();
	}

	public static void Save()
	{
		if (uLinkNetwork.HasServerInitialized)
		{
			GameWorld.SaveAll();
			ServerConfig.SyncSave();
			ItemManager.SaveItems();
			SceneObjMgr.SaveSceneIds();
			RailwayManager.Instance.SaveData();
			FarmManager.SyncSave();
			AiAdNpcNetwork.SaveAll();
			MissionManager.Manager.SaveMissions();
			PublicData.Self.Save();
			Player.SavePlayers();
			DoodadMgr.SaveAll();
			ThreadHelper.Instance.Reset();
			ReputationSystem._self.Save();
			ServerAdministrator.CreateConfig();
			ESceneMode sceneMode = ServerConfig.SceneMode;
			if (sceneMode == ESceneMode.Custom)
			{
				CustomGameData.Mgr.SaveCustomDialogs();
			}
		}
	}
}
