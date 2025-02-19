using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DunGen;
using DunGen.Graph;
using Pathea;
using PeMap;
using PETools;
using SkillSystem;
using UnityEngine;

public class RandomDungenMgr : MonoBehaviour
{
	private const string dungeonFlowPath = "Prefab/Item/Rdungeon/DungeonFlow_Main";

	private const string EntrancePath0 = "Prefab/RandomDunGen/RandomDunEntrance_Native";

	private const string EntrancePath1 = "Prefab/RandomDunGen/RandomDunEntrance_Cave";

	private const string dungeonWaterPath = "Prefab/RandomDunGen/DungeonWater";

	public const float PASSTIME_T = 360f;

	public const float PASSDIST_T = 1000f;

	public const float GEN_T = 1.5f;

	public const int DISTANCE_MAX = 20;

	public const int INDEX256MAX = 1;

	public const int GEN_RADIUS_MIN = 50;

	public const int GEN_RADIUS_MAX = 100;

	private const int AREA_RADIUS = 8;

	private static RandomDungenMgr mInstance;

	private DungeonGenerator generator;

	public bool isActive;

	public GameObject manager;

	private UnityEngine.Object entrancePrefab0;

	private UnityEngine.Object entrancePrefab1;

	private GameObject dungeonWaterPrefab;

	private GameObject dungeonWater;

	public static List<Vector3> entrancesToAdd = new List<Vector3>();

	public bool generateSwitch;

	public float multiFactor = 4f;

	public PeTrans playerTrans;

	public Vector3 born_pos;

	public Vector3 start_pos;

	public Vector3 last_pos;

	public float timeCounter;

	public float last_time;

	public float timePassed;

	public float distancePassed;

	public int counter;

	public static Dictionary<IntVector2, DunEntranceObj> entranceArea = new Dictionary<IntVector2, DunEntranceObj>();

	public static Dictionary<Vector3, DunEntranceObj> allEntrance = new Dictionary<Vector3, DunEntranceObj>();

	public static RandomDungenMgr Instance => mInstance;

	private DungeonBaseData dungeonData
	{
		get
		{
			return RandomDungenMgrData.dungeonBaseData;
		}
		set
		{
			RandomDungenMgrData.dungeonBaseData = value;
		}
	}

	public bool IsInIronDungeon => dungeonData.IsIron;

	public DungeonType Dungeontype => dungeonData.Type;

	private void Awake()
	{
		mInstance = this;
	}

	private void Start()
	{
	}

	public void Init()
	{
		manager = base.gameObject;
		entrancePrefab0 = Resources.Load("Prefab/RandomDunGen/RandomDunEntrance_Native");
		entrancePrefab1 = Resources.Load("Prefab/RandomDunGen/RandomDunEntrance_Cave");
		dungeonWaterPrefab = Resources.Load("Prefab/RandomDunGen/DungeonWater") as GameObject;
		allEntrance = new Dictionary<Vector3, DunEntranceObj>();
		entranceArea = new Dictionary<IntVector2, DunEntranceObj>();
	}

	public void CreateInitTaskEntrance()
	{
		if (RandomDungenMgrData.initTaskEntrance.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<IntVector2, int> item in RandomDungenMgrData.initTaskEntrance)
		{
			GenTaskEntrance(item.Key, item.Value);
		}
	}

	public void EnterDungen(Vector3 entrancePos, int dungeonDataId)
	{
		RandomDungenMgrData.Clear();
		MissionManager.Instance.m_PlayerMission.AbortFollowMission();
		RandomDungenMgrData.AddServants();
		LoadDataFromId(dungeonDataId);
		SetWaterType(entrancePos);
		if (PeGameMgr.IsMulti)
		{
			MessageBox_N.ShowMaskBox(MsgInfoType.DungeonGeneratingMask, "Generating", 20f);
			RandomDungenMgrData.entrancePos = entrancePos;
			RandomDungenMgrData.enterPos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
			PlayerNetwork.mainPlayer.RequestEnterDungeon(entrancePos);
		}
		else
		{
			isActive = true;
			MissionManager.Instance.yirdName = AdventureScene.Dungen.ToString();
			RandomDungenMgrData.SetPosByEnterPos(entrancePos);
			MissionManager.Instance.transPoint = RandomDungenMgrData.revivePos;
			MissionManager.Instance.SceneTranslate();
		}
	}

	public void SaveInDungeon()
	{
		MissionManager.Instance.TransPlayerAndMissionFollower(RandomDungenMgrData.enterPos);
		TransFollower(RandomDungenMgrData.enterPos);
		SinglePlayerTypeLoader singleScenario = PeSingleton<SinglePlayerTypeArchiveMgr>.Instance.singleScenario;
		singleScenario.SetYirdName(AdventureScene.MainAdventure.ToString());
	}

	public void TransFromDungeon(Vector3 pos)
	{
		MissionManager.Instance.TransMissionFollower(pos);
		TransFollower(pos);
		SinglePlayerTypeLoader singleScenario = PeSingleton<SinglePlayerTypeArchiveMgr>.Instance.singleScenario;
		singleScenario.SetYirdName(AdventureScene.MainAdventure.ToString());
	}

	public void TransBackToDungeon(Vector3 pos)
	{
		PeTrans peTrans = PeSingleton<PeCreature>.Instance.mainPlayer.peTrans;
		if (!(peTrans == null))
		{
			peTrans.position = pos;
			MissionManager.Instance.TransMissionFollower(pos);
			TransFollower(pos);
			SinglePlayerTypeLoader singleScenario = PeSingleton<SinglePlayerTypeArchiveMgr>.Instance.singleScenario;
			singleScenario.SetYirdName(AdventureScene.Dungen.ToString());
		}
	}

	public void TransFromDungeonMultiMode(Vector3 pos)
	{
		MissionManager.Instance.TransMissionFollower(pos);
		TransFollower(pos);
	}

	public void TransFollower(Vector3 pos)
	{
		List<PeEntity> allFollower = GetAllFollower();
		if (PeGameMgr.IsSingle)
		{
			foreach (PeEntity item in allFollower)
			{
				item.position = pos;
			}
			return;
		}
		foreach (PeEntity item2 in allFollower)
		{
			item2.position = pos;
		}
	}

	private List<PeEntity> GetAllFollower()
	{
		List<PeEntity> list = new List<PeEntity>();
		NpcCmpt[] servants = PeSingleton<PeCreature>.Instance.mainPlayer.GetComponent<ServantLeaderCmpt>().GetServants();
		NpcCmpt[] array = servants;
		foreach (NpcCmpt npcCmpt in array)
		{
			if (npcCmpt != null)
			{
				list.Add(npcCmpt.Entity);
			}
		}
		return list;
	}

	public void ExitDungen()
	{
		if (PeGameMgr.IsMulti)
		{
			if (allEntrance.ContainsKey(RandomDungenMgrData.entrancePos))
			{
				DunEntranceObj dunEntranceObj = allEntrance[RandomDungenMgrData.entrancePos];
				dunEntranceObj.ShowEnterOrNot = true;
			}
			PlayerNetwork.mainPlayer.RequestExitDungeon();
		}
		else
		{
			isActive = true;
			MissionManager.Instance.yirdName = AdventureScene.MainAdventure.ToString();
			MissionManager.Instance.transPoint = RandomDungenMgrData.enterPos;
			TransFollower(RandomDungenMgrData.enterPos);
			DestroyDungeon();
			MissionManager.Instance.SceneTranslate();
		}
	}

	public void LoadPathFinding()
	{
		StartCoroutine(LoadPathFinder());
	}

	public void ResetPathFinding()
	{
		StartCoroutine(ResetPathFinder());
	}

	private IEnumerator ResetPathFinder()
	{
		if (AstarPath.active != null)
		{
			if (AstarPath.active.transform.parent != null)
			{
				UnityEngine.Object.Destroy(AstarPath.active.transform.parent.gameObject);
			}
			else
			{
				UnityEngine.Object.Destroy(AstarPath.active.gameObject);
			}
		}
		yield return null;
		GameObject obj = Resources.Load("Prefab/PathfinderStd") as GameObject;
		if (obj != null)
		{
			UnityEngine.Object.Instantiate(obj);
		}
		yield return null;
		long tickStart = DateTime.UtcNow.Ticks;
		if (AstarPath.active != null)
		{
			AstarPath.active.Scan();
			Debug.Log("AstarPath.active.Scan(): " + (DateTime.UtcNow.Ticks - tickStart) / 10000 + "ms");
		}
	}

	private IEnumerator LoadPathFinder()
	{
		if (AstarPath.active != null)
		{
			if (AstarPath.active.transform.parent != null)
			{
				UnityEngine.Object.Destroy(AstarPath.active.transform.parent.gameObject);
			}
			else
			{
				UnityEngine.Object.Destroy(AstarPath.active.gameObject);
			}
		}
		yield return null;
		GameObject obj = Resources.Load("Prefab/Pathfinder_Dungeon") as GameObject;
		if (obj != null)
		{
			UnityEngine.Object.Instantiate(obj);
		}
		yield return null;
		long tickStart = DateTime.UtcNow.Ticks;
		if (AstarPath.active != null)
		{
			AstarPath.active.Scan();
			Debug.Log("AstarPath.active.Scan(): " + (DateTime.UtcNow.Ticks - tickStart) / 10000 + "ms");
		}
	}

	public bool GenDungeon(int seed = -1)
	{
		if (PeGameMgr.IsSingle)
		{
			RandomDungenMgrData.DungeonId++;
		}
		generator = new DungeonGenerator(dungeonData.dungeonFlowPath);
		manager.transform.position = RandomDungenMgrData.genDunPos;
		if (manager == null || manager.transform == null)
		{
			Debug.LogError("manager==null||manager.transform==null!");
			return false;
		}
		bool flag = false;
		if (seed == -1)
		{
			flag = generator.Generate(manager);
			if (!flag)
			{
				return false;
			}
			Debug.Log("gen success without seed");
			GenWater_SafeBottom(RandomDungenMgrData.waterType);
			GeneralSet(enter: true);
			GenContent();
		}
		else
		{
			flag = generator.GenerateWithSeed(manager, seed);
			if (!flag)
			{
				return false;
			}
			Debug.Log("gen success with seed");
			GenWater_SafeBottom(RandomDungenMgrData.waterType);
			GeneralSet(enter: true);
		}
		if (PeGameMgr.IsSingle)
		{
		}
		if (PeGameMgr.IsMulti)
		{
			seed = generator.ChosenSeed;
			PlayerNetwork.mainPlayer.RequestUploadDungeonSeed(RandomDungenMgrData.entrancePos, seed);
			Debug.Log("dun Seed: " + seed);
			ChangeOther(isEnter: true);
		}
		Debug.Log("RemoveTerrainDependence");
		SceneMan.RemoveTerrainDependence();
		return flag;
	}

	public void GenContent()
	{
		int seed = (int)(DateTime.UtcNow.Ticks % int.MaxValue);
		System.Random rand = new System.Random(seed);
		RandomDungenMgrData.allMonsters = manager.GetComponentsInChildren<MonsterGenerator>().ToList();
		GenMonsters(RandomDungenMgrData.allMonsters, rand);
		RandomDungenMgrData.allMinBoss = manager.GetComponentsInChildren<MinBossGenerator>().ToList();
		GenMinBoss(RandomDungenMgrData.allMinBoss, rand);
		RandomDungenMgrData.allBoss = manager.GetComponentsInChildren<BossMonsterGenerator>().ToList();
		GenBoss(RandomDungenMgrData.allBoss, rand);
		RandomDungenMgrData.allItems = manager.GetComponentsInChildren<DunItemGenerator>().ToList();
		GenItems(RandomDungenMgrData.allItems, rand);
		RandomDungenMgrData.allRareItems = manager.GetComponentsInChildren<DunRareItemGenerator>().ToList();
		List<ItemIdCount> specifiedItems = RandomDungenMgrData.dungeonBaseData.specifiedItems;
		List<IdWeight> rareItemTags = RandomDungenMgrData.dungeonBaseData.rareItemTags;
		GenRareItem(RandomDungenMgrData.allRareItems, rareItemTags, rand, specifiedItems);
	}

	public void DestroyDungeon()
	{
		GeneralSet(enter: false);
		SceneMan.AddTerrainDependence();
		if (generator != null && generator.CurrentDungeon != null && generator.CurrentDungeon.gameObject != null)
		{
			UnityUtil.Destroy(generator.CurrentDungeon.gameObject);
		}
		RandomDungenMgrData.Clear();
		if (PeGameMgr.IsMulti)
		{
			ChangeOther(isEnter: false);
			ResetPathFinding();
		}
		if (dungeonWater != null)
		{
			UnityEngine.Object.Destroy(dungeonWater);
		}
		FBSafePlane.instance.DeleteCol();
		DragItemAgent.DestroyAllInDungeon();
	}

	private void GeneralSet(bool enter)
	{
		if (enter)
		{
			if (PeGameMgr.IsSingle)
			{
				SaveOutOfDungeon();
			}
			PeEnv.CanRain(canRain: false);
			if (dungeonWater != null)
			{
				RandomMapConfig.SetGlobalFogHeight(dungeonWater.transform.position.y);
			}
		}
		else
		{
			PeEnv.CanRain(canRain: true);
			RandomMapConfig.SetGlobalFogHeight();
		}
	}

	public void SaveOutOfDungeon()
	{
		PeTrans peTrans = PeSingleton<PeCreature>.Instance.mainPlayer.peTrans;
		if (!(peTrans == null))
		{
			Vector3 position = peTrans.position;
			SaveInDungeon();
			PeSingleton<ArchiveMgr>.Instance.Save(ArchiveMgr.ESave.Min);
			TransBackToDungeon(position);
		}
	}

	private void DeactiveObjNotUse()
	{
		List<int> allFollowers = RandomDungenMgrData.GetAllFollowers();
		int mainPlayerId = PeSingleton<PeCreature>.Instance.mainPlayerId;
		IEnumerable<PeEntity> all = PeSingleton<EntityMgr>.Instance.All;
		foreach (PeEntity item in all)
		{
			if (item.Id != mainPlayerId && !allFollowers.Contains(item.Id) && item.gameObject != null)
			{
				item.gameObject.SetActive(value: false);
				MapCmpt cmpt = item.GetCmpt<MapCmpt>();
				item.Remove(cmpt);
			}
		}
		List<ILabel> list = PeSingleton<LabelMgr>.Instance.FindAll((ILabel itr) => itr.GetType() == ELabelType.Mission);
		foreach (ILabel item2 in list)
		{
			PeSingleton<LabelMgr>.Instance.Remove(item2);
		}
	}

	public void ChangeOther(bool isEnter)
	{
		UIMinMapCtrl.Instance.UpdateCameraPos();
		if (isEnter)
		{
			UIMinMapCtrl.Instance.CameraNear = 200f;
			UIMinMapCtrl.Instance.CameraFar = 400f;
		}
		else
		{
			UIMinMapCtrl.Instance.CameraNear = 1f;
			UIMinMapCtrl.Instance.CameraFar = 1000f;
		}
	}

	public void ReceiveIsoObj(int dungeonId, ulong isoCode, int instanceId)
	{
		RandomDungenMgrData.RareItemReady(instanceId, dungeonId);
	}

	public void OpenLockedDoor(SkEntity caster, SkEntity monster)
	{
		LockedDoor lockedDoor = generator.CurrentDungeon.LockedDoorList.Find((LockedDoor it) => !it.IsOpen);
		if (lockedDoor != null)
		{
			lockedDoor.Open();
		}
	}

	public void PickUpKey(int keyId)
	{
		RandomDungenMgrData.pickedKeys.Add(keyId);
	}

	public bool HasKey(int keyId)
	{
		return RandomDungenMgrData.pickedKeys.Contains(keyId);
	}

	public void RemoveKey(int keyId)
	{
		RandomDungenMgrData.pickedKeys.Remove(keyId);
	}

	public void LoadDataFromDataBase(int level)
	{
		DungeonBaseData dataFromLevel = RandomDungeonDataBase.GetDataFromLevel(level);
		DungeonFlow dungeonFlow = Resources.Load(dataFromLevel.dungeonFlowPath) as DungeonFlow;
		if (dungeonFlow == null)
		{
			Debug.LogError("flow null: " + dataFromLevel.dungeonFlowPath);
			dataFromLevel.dungeonFlowPath = "Prefab/Item/Rdungeon/DungeonFlow_Main";
		}
		dungeonData = dataFromLevel;
	}

	public void LoadDataFromId(int id)
	{
		DungeonBaseData dataFromId = RandomDungeonDataBase.GetDataFromId(id);
		DungeonFlow dungeonFlow = Resources.Load(dataFromId.dungeonFlowPath) as DungeonFlow;
		if (dungeonFlow == null)
		{
			Debug.LogError("flow null: " + dataFromId.dungeonFlowPath);
			dataFromId.dungeonFlowPath = "Prefab/Item/Rdungeon/DungeonFlow_Main";
		}
		dungeonData = dataFromId;
	}

	public void GenMonsters(List<MonsterGenerator> allPoints, System.Random rand)
	{
		if (allPoints != null && allPoints.Count != 0)
		{
			float num = 1f / dungeonData.monsterAmount;
			if (num < 1f)
			{
				num = 1f;
			}
			for (float num2 = 0f; num2 < (float)allPoints.Count; num2 += num)
			{
				int index = Mathf.FloorToInt(num2);
				allPoints[index].GenerateMonster(dungeonData.landMonsterId, dungeonData.waterMonsterId, dungeonData.monsterBuff, rand, dungeonData.IsTaskDungeon);
			}
		}
	}

	public void GenItems(List<DunItemGenerator> allPoints, System.Random rand)
	{
		if (allPoints != null && allPoints.Count != 0)
		{
			float num = 1f / dungeonData.itemAmount;
			if (num < 1f)
			{
				num = 1f;
			}
			for (float num2 = 0f; num2 < (float)allPoints.Count; num2 += num)
			{
				int index = Mathf.FloorToInt(num2);
				allPoints[index].GenItem(dungeonData.itemId, rand);
			}
		}
	}

	public void GenMinBoss(List<MinBossGenerator> allPoints, System.Random rand)
	{
		if (allPoints != null && allPoints.Count != 0)
		{
			MinBossGenerator.GenAllBoss(allPoints, dungeonData.minBossId, dungeonData.minBossWaterId, dungeonData.minBossMonsterBuff, rand, dungeonData.IsTaskDungeon);
		}
	}

	public void GenBoss(List<BossMonsterGenerator> allPoints, System.Random rand)
	{
		if (allPoints != null && allPoints.Count != 0)
		{
			BossMonsterGenerator.GenAllBoss(allPoints, dungeonData.bossId, dungeonData.minBossWaterId, dungeonData.bossMonsterBuff, rand, dungeonData.IsTaskDungeon);
		}
	}

	public void GenRareItem(List<DunRareItemGenerator> allPoints, List<IdWeight> rareItemTags, System.Random rand, List<ItemIdCount> specifiedItems = null)
	{
		if (allPoints != null && allPoints.Count != 0)
		{
			DunRareItemGenerator.GenAllItem(allPoints, dungeonData.rareItemId, dungeonData.rareItemChance, rareItemTags, rand, specifiedItems);
		}
	}

	public void SetWaterType(Vector3 entrancePos)
	{
		if (dungeonData.IsTaskDungeon)
		{
			RandomDungenMgrData.waterType = DungeonWaterType.None;
		}
		else if (VFVoxelWater.self != null)
		{
			if (VFVoxelWater.self.IsInWater(entrancePos))
			{
				RandomDungenMgrData.waterType = DungeonWaterType.Deep;
			}
			else
			{
				RandomDungenMgrData.waterType = DungeonWaterType.None;
			}
		}
		else
		{
			RandomDungenMgrData.waterType = DungeonWaterType.None;
		}
	}

	public void GenWater_SafeBottom(DungeonWaterType waterType)
	{
		Vector3 vector = RandomDungenMgrData.genDunPos + new Vector3(-1000f, -50f, -1000f);
		Vector3 max = vector + new Vector3(2000f, 5f, 2000f);
		switch (waterType)
		{
		case DungeonWaterType.Deep:
			dungeonWater = UnityEngine.Object.Instantiate(dungeonWaterPrefab);
			dungeonWater.transform.position = RandomDungenMgrData.genDunPos - new Vector3(0f, 4f, 0f);
			break;
		case DungeonWaterType.Shallow:
			dungeonWater = UnityEngine.Object.Instantiate(dungeonWaterPrefab);
			dungeonWater.transform.position = RandomDungenMgrData.genDunPos - new Vector3(0f, 10f, 0f);
			break;
		}
		if (generator.CurrentDungeon.gameObject != null)
		{
			Bounds wordColliderBoundsInChildren = PEUtil.GetWordColliderBoundsInChildren(generator.CurrentDungeon.gameObject);
			vector = wordColliderBoundsInChildren.min + new Vector3(-80f, 0f, -80f);
			max = new Vector3(wordColliderBoundsInChildren.max.x + 80f, vector.y + 5f, wordColliderBoundsInChildren.max.z + 80f);
			if (dungeonWater != null)
			{
				dungeonWater.transform.position = new Vector3(wordColliderBoundsInChildren.center.x, dungeonWater.transform.position.y, wordColliderBoundsInChildren.center.z);
				dungeonWater.transform.localScale = new Vector3((wordColliderBoundsInChildren.size.x + 100f) / 10f, 1f, (wordColliderBoundsInChildren.size.z + 100f) / 10f);
			}
		}
		FBSafePlane.instance.ResetCol(vector, max, RandomDungenMgrData.revivePos);
	}

	private void Update()
	{
		if (!PeGameMgr.IsAdventure)
		{
			return;
		}
		counter++;
		timeCounter += Time.deltaTime;
		if (counter <= 240)
		{
			return;
		}
		if (playerTrans == null)
		{
			if (PeSingleton<PeCreature>.Instance.mainPlayer != null && PeSingleton<PeCreature>.Instance.mainPlayer.peTrans.position != Vector3.zero)
			{
				playerTrans = PeSingleton<PeCreature>.Instance.mainPlayer.peTrans;
				born_pos = playerTrans.position;
				if (Application.isEditor)
				{
					Debug.Log(string.Concat("<color=yellow>born_pos", born_pos, "</color>"));
				}
				start_pos = born_pos;
				last_pos = start_pos;
				last_time = timeCounter;
			}
			counter = 0;
			return;
		}
		if (RandomDungenMgrData.InDungeon)
		{
			last_pos = RandomDungenMgrData.enterPos;
			last_time = timeCounter;
			return;
		}
		if (!generateSwitch)
		{
			timePassed += timeCounter - last_time;
			float num = Vector2.Distance(new Vector2(playerTrans.position.x, playerTrans.position.z), new Vector2(last_pos.x, last_pos.z));
			if (num > 20f)
			{
				num = 20f;
			}
			distancePassed += num;
			last_pos = playerTrans.position;
			last_time = timeCounter;
			if (!CheckGenerate(timePassed, distancePassed))
			{
				counter = 0;
				return;
			}
			generateSwitch = true;
			timePassed = 0f;
			distancePassed = 0f;
		}
		if (generateSwitch)
		{
			System.Random random = new System.Random();
			int num2 = 50;
			int num3 = random.Next(num2 * 2) - num2;
			int num4 = random.Next(num2 * 2) - num2;
			num3 = ((num3 < 0) ? (num3 - 50 + 1) : (num3 + 50));
			num4 = ((num4 < 0) ? (num4 - 50 + 1) : (num4 + 50));
			IntVector2 genPos = new IntVector2((int)playerTrans.position.x + num3, (int)playerTrans.position.z + num4);
			if (IsTerrainAvailable(genPos, out var pos) && IsAreaAvalable(new Vector2(pos.x, pos.z)))
			{
				InstantiateEntrance(pos - new Vector3(0f, 0.5f, 0f));
				generateSwitch = false;
			}
		}
		counter = 0;
	}

	private bool CheckGenerate(float passedTime, float passedDistance)
	{
		bool result = false;
		if (passedTime < 360f)
		{
			return result;
		}
		if (passedDistance < 1000f)
		{
			return result;
		}
		float num = 1f;
		if (PeGameMgr.IsMulti)
		{
			num = multiFactor;
		}
		if (passedTime * passedDistance / 360f / 1000f > 1.5f * num)
		{
			result = true;
		}
		return result;
	}

	private bool IsAreaAvalable(Vector2 pos)
	{
		if (VArtifactTownManager.Instance.TileContainsTown(new IntVector2(Mathf.RoundToInt(pos.x) >> 5, Mathf.RoundToInt(pos.y) >> 5)))
		{
			return false;
		}
		IntVector2 intVector = new IntVector2(Mathf.RoundToInt(pos.x) >> 8, Mathf.RoundToInt(pos.y) >> 8);
		for (int i = intVector.x - 1; i < intVector.x + 2; i++)
		{
			for (int j = intVector.y - 1; j < intVector.y + 2; j++)
			{
				if (entranceArea.ContainsKey(new IntVector2(i, j)))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool IsTerrainAvailable(IntVector2 GenPos, out Vector3 pos)
	{
		if (!VFDataRTGen.IsDungeonEntranceAvailable(GenPos))
		{
			pos = Vector3.zero;
			return false;
		}
		return RandomDunGenUtil.GetAreaLowestPos(GenPos, 10, out pos);
	}

	private void InstantiateEntrance(Vector3 genPos, int level = -1)
	{
		if (level == -1)
		{
			level = RandomDunGenUtil.GetEntranceLevel(genPos);
		}
		DungeonBaseData dataFromLevel = RandomDungeonDataBase.GetDataFromLevel(level);
		if (dataFromLevel != null)
		{
			if (PeGameMgr.IsSingle)
			{
				GenDunEntrance(genPos, dataFromLevel);
			}
			else
			{
				PlayerNetwork.mainPlayer.RequestGenDunEntrance(genPos, dataFromLevel.id);
			}
		}
	}

	public UnityEngine.Object GetEntrancePrefabPath(DungeonType type)
	{
		return type switch
		{
			DungeonType.Iron => entrancePrefab0, 
			DungeonType.Cave => entrancePrefab1, 
			_ => entrancePrefab0, 
		};
	}

	public void GenDunEntrance(Vector3 genPos, DungeonBaseData basedata)
	{
		if (!allEntrance.ContainsKey(genPos))
		{
			UnityEngine.Object entrancePrefabPath = GetEntrancePrefabPath(basedata.Type);
			DunEntranceObj dunEntranceObj = new DunEntranceObj(entrancePrefabPath, genPos);
			dunEntranceObj.Level = basedata.level;
			dunEntranceObj.DungeonId = basedata.id;
			allEntrance[genPos] = dunEntranceObj;
			if (basedata.level < 100)
			{
				IntVector2 key = new IntVector2(Mathf.RoundToInt(genPos.x) >> 8, Mathf.RoundToInt(genPos.z) >> 8);
				entranceArea[key] = dunEntranceObj;
			}
			SceneMan.AddSceneObj(dunEntranceObj);
		}
	}

	public void DestroyEntrance(Vector3 entrancePos)
	{
		if (allEntrance.ContainsKey(entrancePos))
		{
			allEntrance[entrancePos].DestroySelf();
			allEntrance.Remove(entrancePos);
			IntVector2 key = new IntVector2(Mathf.RoundToInt(entrancePos.x) >> 8, Mathf.RoundToInt(entrancePos.z) >> 8);
			entranceArea.Remove(key);
		}
	}

	public void GenTestEntrance(int level = -1)
	{
		int num = Mathf.RoundToInt(PeSingleton<PeCreature>.Instance.mainPlayer.position.x);
		int num2 = Mathf.RoundToInt(PeSingleton<PeCreature>.Instance.mainPlayer.position.z + 15f);
		int posHeight = VFDataRTGen.GetPosHeight(num, num2);
		Vector3 genPos = new Vector3(num, posHeight, num2);
		InstantiateEntrance(genPos, level);
	}

	public void GenTaskEntrance(IntVector2 genXZ, int level = -1)
	{
		Vector3 posOnGround = RandomDunGenUtil.GetPosOnGround(genXZ);
		InstantiateEntrance(posOnGround, level);
	}
}
