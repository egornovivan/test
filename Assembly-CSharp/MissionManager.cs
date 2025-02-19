using System;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtPlayerPackage;
using Pathea.PeEntityExtTrans;
using PETools;
using Railway;
using uLink;
using UnityEngine;

public class MissionManager : UnityEngine.MonoBehaviour
{
	public struct PathInfo
	{
		public Vector3 pos;

		public bool isFinish;
	}

	public enum TakeMissionType
	{
		TakeMissionType_Unkown,
		TakeMissionType_Get,
		TakeMissionType_In,
		TakeMissionType_Complete
	}

	private class TimerTodo
	{
		public PETimer timer;

		public Action act;

		public int fixedID;
	}

	public const int m_SpecialMissionID5 = 71;

	public const int m_SpecialMissionID9 = 888;

	public const int m_SpecialMissionID10 = 242;

	public const int m_SpecialMissionID13 = 997;

	public const int m_SpecialMissionID14 = 998;

	public const int m_SpecialMissionID15 = 999;

	public const int m_SpecialMissionID16 = 191;

	public const int m_SpecialMissionID22 = 66;

	public const int m_SpecialMissionID24 = 67;

	public const int m_SpecialMissionID31 = 204;

	public const int m_SpecialMissionID42 = 212;

	public const int m_SpecialMissionID43 = 158;

	public const int m_SpecialMissionID45 = 8;

	public const int m_SpecialMissionID47 = 889;

	public const int m_SpecialMissionID51 = 254;

	public const int m_SpecialMissionID52 = 139;

	public const int m_SpecialMissionID53 = 61;

	public const int m_SpecialMissionID55 = 251;

	public const int m_SpecialMissionID58 = 444;

	public const int m_SpecialMissionID59 = 480;

	public const int m_SpecialMissionID60 = 481;

	public const int m_SpecialMissionID61 = 505;

	public const int m_SpecialMissionID62 = 506;

	public const int m_SpecialMissionID63 = 507;

	public const int m_SpecialMissionID64 = 497;

	public const int m_SpecialMissionID65 = 550;

	public const int m_SpecialMissionID66 = 553;

	public const int m_SpecialMissionID67 = 554;

	public const int m_SpecialMissionID68 = 500;

	public const int m_SpecialMissionID69 = 562;

	public const int m_SpecialMissionID80 = 629;

	public const int m_SpecialMissionID81 = 628;

	public const int m_SpecialMissionID82 = 710;

	public const int m_SpecialMissionID83 = 678;

	public const int m_SpecialMissionID84 = 700;

	public const int m_SpecialMissionID85 = 703;

	public const int m_SpecialMissionID86 = 704;

	public const int m_SpecialMissionID87 = 697;

	public const int m_SpecialMissionID88 = 714;

	public const int m_SpecialMissionID89 = 822;

	public const int m_SpecialMissionID90 = 825;

	public const int m_SpecialMissionID91 = 826;

	public const int m_SpecialMissionID92 = 846;

	public const int m_SpecialMissionID93 = 953;

	public const int m_TalkInfoPlayer = -9999;

	public List<int> m_OnCarMissionList;

	public static int m_CurSpecialMissionID = -1;

	private string mNpcName = string.Empty;

	private string mItemID = string.Empty;

	private string mItemNum = string.Empty;

	private string mMisID = string.Empty;

	private string mMovePos = string.Empty;

	private string mNPCFollower = string.Empty;

	private string mAbilityID = string.Empty;

	private string mSickNum = string.Empty;

	private string mNpcNum = string.Empty;

	private string mAItypeId = string.Empty;

	private string mPlotID = string.Empty;

	private string mStyleID = string.Empty;

	private string mStrEntityId = string.Empty;

	private string mStrEntityPos = string.Empty;

	private string level = string.Empty;

	public static string mTowerCurWave = string.Empty;

	public static string mTowerTotalWave = string.Empty;

	public static bool mShowTools;

	public static int ToolsPage = 1;

	public Dictionary<int, string> npcName = new Dictionary<int, string>();

	public PlayerMission m_PlayerMission = new PlayerMission();

	public bool m_bHadInitMission;

	private List<int> iHadTalkedMap = new List<int>();

	private static MissionManager mInstance;

	private PeEntity _entityToSet;

	private Vector3 _posToSet;

	private bool _bUseSetDirty;

	public Vector3 transPoint;

	public string yirdName;

	private float gameTime;

	private Vector3 recordTownPos;

	private List<TimerTodo> timers = new List<TimerTodo>();

	private List<TimerTodo> recordRemove = new List<TimerTodo>();

	private Dictionary<int, float> targetId_time = new Dictionary<int, float>();

	private Dictionary<int, int[]> followTarget_num = new Dictionary<int, int[]>();

	private Dictionary<int, List<object>> npcWaitTime = new Dictionary<int, List<object>>();

	private float timer;

	public static MissionManager Instance => mInstance;

	private void Awake()
	{
		mInstance = this;
		m_OnCarMissionList = new List<int>();
		m_OnCarMissionList.Add(553);
		m_OnCarMissionList.Add(699);
		m_OnCarMissionList.Add(700);
		m_OnCarMissionList.Add(701);
		m_OnCarMissionList.Add(702);
		m_OnCarMissionList.Add(703);
		m_OnCarMissionList.Add(704);
	}

	private void Start()
	{
		StartCoroutine(WaitingPlayer());
	}

	private IEnumerator WaitingPlayer()
	{
		while (PeSingleton<PeCreature>.Instance.mainPlayer == null || UIMissionMgr.Instance == null)
		{
			yield return new WaitForSeconds(0.1f);
		}
		RecordRemove();
		PeSingleton<PeCreature>.Instance.mainPlayer.ExtSetFaceIconBig("npc_big_UnKnown");
		PeSingleton<EntityMgr>.Instance.eventor.Subscribe(EntityCreateMgr.Instance.NpcMouseEventHandler);
		PeSingleton<EntityMgr>.Instance.npcTalkEventor.Subscribe(EntityCreateMgr.Instance.NpcTalkRequest);
		if (PeGameMgr.IsAdventure)
		{
			VArtifactTownManager.Instance.RegistTownDestryedEvent(OnTownDestroy);
			PeSingleton<ReputationSystem>.Instance.onReputationChange += OnReputationChange;
		}
		MonsterHandbookData.GetAllMonsterEvent = (Action)Delegate.Combine(MonsterHandbookData.GetAllMonsterEvent, new Action(AllMonsterBook));
		if (CSMain.s_MgCreator != null)
		{
			CSMain.s_MgCreator.RegisterListener(MoveVploas);
		}
		StroyManager.Instance.InitPlayerEvent();
		if (!PeGameMgr.IsMulti)
		{
			InitPlayerMission();
		}
		if (PeGameMgr.IsSingleStory)
		{
			StroyManager.Instance.InitMission();
		}
		UIMissionMgr.Instance.e_DeleteMission += AbortMission;
		yield return 0;
	}

	private void OnTownDestroy(int n)
	{
		AdventureWin();
	}

	private void OnReputationChange(int playerId, int targetId)
	{
		AdventureWin();
	}

	private void AdventureWin()
	{
		if (m_PlayerMission.HadCompleteMission(9139) && m_PlayerMission.HadCompleteMission(9140))
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		int playerID = Mathf.RoundToInt(PeSingleton<PeCreature>.Instance.mainPlayer.GetAttribute(AttribType.DefaultPlayerID));
		for (int i = 1; i < RandomMapConfig.allyCount; i++)
		{
			if (VATownGenerator.Instance.GetAllyTownExistCount(i) > 0)
			{
				num++;
				int playerId = VATownGenerator.Instance.GetPlayerId(i);
				if (PeSingleton<ReputationSystem>.Instance.GetReputationLevel(playerID, playerId) > ReputationSystem.ReputationLevel.Neutral)
				{
					num2++;
				}
			}
		}
		int num3 = -1;
		if (num == 0)
		{
			num3 = 9139;
		}
		else if (num == num2)
		{
			num3 = 9140;
		}
		if (num3 == -1 || m_PlayerMission.HadCompleteMission(num3))
		{
			return;
		}
		if (MissionRepository.HaveTalkOP(num3))
		{
			GameUI.Instance.mNPCTalk.NormalOrSP(0);
			if (!GameUI.Instance.mNPCTalk.isPlayingTalk)
			{
				GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(num3, 1);
				GameUI.Instance.mNPCTalk.PreShow();
			}
			else
			{
				GameUI.Instance.mNPCTalk.AddNpcTalkInfo(num3, 1);
			}
		}
		else if (IsGetTakeMission(num3))
		{
			SetGetTakeMission(num3, null, TakeMissionType.TakeMissionType_Get);
		}
	}

	private void AllMonsterBook()
	{
		if (PeGameMgr.IsMulti)
		{
			Instance.RequestCompleteMission(10035);
		}
		else
		{
			CompleteMission(10035);
		}
	}

	private void MoveVploas(int event_type, CSEntity entity)
	{
		if (event_type == 1001 && entity != null && entity is CSAssembly)
		{
			ReflashCSUseItemMission();
		}
		if (StroyManager.Instance.moveVploas && event_type == 1001 && entity is CSAssembly)
		{
			PeEntity npc = PeSingleton<EntityMgr>.Instance.Get(9056);
			if (CSMain.GetAssemblyLogic() != null)
			{
				StroyManager.Instance.Translate(npc, CSMain.GetAssemblyLogic().m_NPCTrans[19].position);
			}
		}
	}

	private void RecordRemove()
	{
		SpecialHatred.ClearRecord();
		StroyManager.ClearRecord();
		foreach (PeEntity item in PeSingleton<EntityMgr>.Instance.All)
		{
			if (item.proto == EEntityProto.Npc && item.GetUserData() is NpcMissionData npcMissionData)
			{
				npcMissionData.mInFollowMission = false;
			}
		}
	}

	private void OnGUI()
	{
	}

	private void LateUpdate()
	{
		if (Input.GetKey(KeyCode.F11))
		{
			_bUseSetDirty = false;
		}
		if (Input.GetKey(KeyCode.F12))
		{
			_bUseSetDirty = true;
		}
		if (_entityToSet != null)
		{
			_entityToSet.peTrans.position = _posToSet;
			if (_bUseSetDirty)
			{
				SceneMan.SetDirty(_entityToSet.lodCmpt);
			}
			_entityToSet = null;
		}
	}

	private void ChangeDoodadShowVar(int n, bool tmp)
	{
		PeEntity[] doodadEntities = PeSingleton<EntityMgr>.Instance.GetDoodadEntities(n);
		if (doodadEntities.Length > 0)
		{
			SceneDoodadLodCmpt component = doodadEntities[0].GetComponent<SceneDoodadLodCmpt>();
			if (component != null)
			{
				component.SetShowVar(tmp);
			}
		}
	}

	public void TransPlayerAndMissionFollower(Vector3 pos)
	{
		PeTrans peTrans = PeSingleton<PeCreature>.Instance.mainPlayer.peTrans;
		if (peTrans == null)
		{
			return;
		}
		if (!PeGameMgr.IsSingle)
		{
			peTrans.position = pos;
		}
		else
		{
			peTrans.fastTravelPos = pos;
		}
		for (int i = 0; i < m_PlayerMission.followers.Count; i++)
		{
			if (!(m_PlayerMission.followers[i] == null))
			{
				m_PlayerMission.followers[i].GetComponent<PeEntity>().ExtSetPos(pos);
			}
		}
	}

	public void TransMissionFollower(Vector3 pos)
	{
		for (int i = 0; i < m_PlayerMission.followers.Count; i++)
		{
			if (!(m_PlayerMission.followers[i] == null))
			{
				m_PlayerMission.followers[i].GetComponent<PeEntity>().ExtSetPos(pos);
			}
		}
	}

	public void SceneTranslate()
	{
		TransPlayerAndMissionFollower(transPoint);
		if (PeGameMgr.IsSingle && yirdName != AdventureScene.Dungen.ToString())
		{
			SinglePlayerTypeLoader singleScenario = PeSingleton<SinglePlayerTypeArchiveMgr>.Instance.singleScenario;
			singleScenario.SetYirdName(yirdName);
		}
		if (PeGameMgr.IsSingle)
		{
			if (yirdName == "main")
			{
				ChangeDoodadShowVar(242, tmp: true);
				ChangeDoodadShowVar(240, tmp: true);
				ChangeDoodadShowVar(324, tmp: false);
				ChangeDoodadShowVar(326, tmp: true);
				ChangeDoodadShowVar(327, tmp: true);
				for (int i = 461; i < 464; i++)
				{
					ChangeDoodadShowVar(i, tmp: false);
				}
				for (int j = 456; j < 461; j++)
				{
					ChangeDoodadShowVar(j, tmp: true);
				}
			}
			else if (!(yirdName == AdventureScene.MainAdventure.ToString()) && !(yirdName == AdventureScene.Dungen.ToString()))
			{
				ChangeDoodadShowVar(242, tmp: false);
				ChangeDoodadShowVar(240, tmp: false);
				ChangeDoodadShowVar(324, tmp: true);
				ChangeDoodadShowVar(326, tmp: false);
				ChangeDoodadShowVar(327, tmp: false);
				for (int k = 461; k < 464; k++)
				{
					ChangeDoodadShowVar(k, tmp: true);
				}
				for (int l = 456; l < 461; l++)
				{
					ChangeDoodadShowVar(l, tmp: false);
				}
			}
		}
		if (PeGameMgr.IsSingle)
		{
			PeGameMgr.targetYird = yirdName;
			PeGameMgr.loadArchive = ArchiveMgr.ESave.Min;
			PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.GameScene);
			return;
		}
		int sceneId = 0;
		if (yirdName == "main")
		{
			sceneId = 0;
		}
		else if (yirdName == "L1Ship")
		{
			sceneId = 1;
		}
		else if (yirdName == "DienShip0")
		{
			sceneId = 2;
		}
		else if (yirdName == "DienShip1")
		{
			sceneId = 6;
		}
		else if (yirdName == "DienShip2")
		{
			sceneId = 7;
		}
		else if (yirdName == "DienShip3")
		{
			sceneId = 8;
		}
		else if (yirdName == "DienShip4")
		{
			sceneId = 9;
		}
		else if (yirdName == "DienShip5")
		{
			sceneId = 10;
		}
		else if (yirdName == "PajaShip")
		{
			sceneId = 4;
		}
		else if (yirdName == "LaunchCenter")
		{
			sceneId = 5;
		}
		PlayerNetwork.mainPlayer.RequestChangeScene(sceneId);
	}

	public static bool IsTalkMission(int MissionID)
	{
		MissionCommonData missionCommonData = GetMissionCommonData(MissionID);
		if (missionCommonData == null)
		{
			return false;
		}
		if (missionCommonData.IsTalkMission())
		{
			return true;
		}
		return false;
	}

	public static List<PeEntity> GetInteractiveEntity()
	{
		List<PeEntity> list = new List<PeEntity>();
		foreach (NpcCmpt follower in Instance.m_PlayerMission.followers)
		{
			list.Add(follower.Entity);
		}
		foreach (NpcCmpt pathFollower in Instance.m_PlayerMission.pathFollowers)
		{
			if (!list.Contains(pathFollower.Entity))
			{
				list.Add(pathFollower.Entity);
			}
		}
		KeyValuePair<int, Dictionary<string, string>> item;
		foreach (KeyValuePair<int, Dictionary<string, string>> item2 in Instance.m_PlayerMission.m_MissionInfo)
		{
			item = item2;
			if (GetMissionCommonData(item.Key).m_iReplyNpc != 0 && list.Find((PeEntity e) => e.Id == GetMissionCommonData(item.Key).m_iReplyNpc) == null)
			{
				list.Add(PeSingleton<EntityMgr>.Instance.Get(GetMissionCommonData(item.Key).m_iReplyNpc));
			}
		}
		return list;
	}

	public static MissionCommonData GetMissionCommonData(int MissionID)
	{
		int num = MissionID / 1000;
		if (num == 9)
		{
			if (PeGameMgr.IsStory)
			{
				return RMRepository.GetRandomMission(MissionID);
			}
			return AdRMRepository.GetAdRandomMission(MissionID);
		}
		return MissionRepository.GetMissionCommonData(MissionID);
	}

	public static bool IsMainMission(int missionID)
	{
		MissionCommonData missionCommonData = GetMissionCommonData(missionID);
		if (missionCommonData == null)
		{
			return false;
		}
		if (missionCommonData.m_Type != MissionType.MissionType_Main)
		{
			return false;
		}
		return true;
	}

	public static bool CanDragAssembly(Vector3 pos, out int num)
	{
		num = -1;
		int num2 = 1600;
		int num3 = 800;
		int num4 = 400;
		if (PeGameMgr.IsStory)
		{
			MapMaskData mapMaskData = MapMaskData.s_tblMaskData.Find((MapMaskData ret) => ret.mId == 29);
			if (mapMaskData == null)
			{
				return true;
			}
			float num5 = Vector3.Distance(pos, mapMaskData.mPosition);
			if (num5 < (float)num2)
			{
				if (num5 > (float)num3)
				{
					num = 0;
				}
				else if (num5 > (float)num4)
				{
					num = 1;
				}
				else
				{
					num = 2;
				}
				return false;
			}
			return true;
		}
		return true;
	}

	public static bool HasRandomMission(int MissionID)
	{
		if (PeGameMgr.IsStory)
		{
			return RMRepository.HasRandomMission(MissionID);
		}
		if (MissionID == 9135 || MissionID == 9136 || MissionID == 9137 || MissionID == 9138 || MissionID == 9139 || MissionID == 9140)
		{
			return false;
		}
		return AdRMRepository.HasAdRandomMission(MissionID);
	}

	public static TypeMonsterData GetTypeMonsterData(int TargetID)
	{
		if (PeGameMgr.IsStory)
		{
			return MissionRepository.GetTypeMonsterData(TargetID);
		}
		return AdRMRepository.GetAdTypeMonsterData(TargetID);
	}

	public static TypeCollectData GetTypeCollectData(int TargetID)
	{
		if (PeGameMgr.IsStory)
		{
			return MissionRepository.GetTypeCollectData(TargetID);
		}
		return AdRMRepository.GetAdTypeCollectData(TargetID);
	}

	public static TypeFollowData GetTypeFollowData(int TargetID)
	{
		if (PeGameMgr.IsStory)
		{
			return MissionRepository.GetTypeFollowData(TargetID);
		}
		return AdRMRepository.GetAdTypeFollowData(TargetID);
	}

	public static TypeSearchData GetTypeSearchData(int TargetID)
	{
		if (PeGameMgr.IsStory || PeGameMgr.IsTutorial)
		{
			return MissionRepository.GetTypeSearchData(TargetID);
		}
		return AdRMRepository.GetAdTypeSearchData(TargetID);
	}

	public static TypeUseItemData GetTypeUseItemData(int TargetID)
	{
		if (PeGameMgr.IsStory)
		{
			return MissionRepository.GetTypeUseItemData(TargetID);
		}
		return AdRMRepository.GetAdTypeUseItemData(TargetID);
	}

	public static TypeMessengerData GetTypeMessengerData(int TargetID)
	{
		if (PeGameMgr.IsStory)
		{
			return MissionRepository.GetTypeMessengerData(TargetID);
		}
		return AdRMRepository.GetAdTypeMessengerData(TargetID);
	}

	public static TypeTowerDefendsData GetTypeTowerDefendsData(int TargetID)
	{
		if (PeGameMgr.IsStory)
		{
			return MissionRepository.GetTypeTowerDefendsData(TargetID);
		}
		return AdRMRepository.GetAdTypeTowerDefendsData(TargetID);
	}

	public void RemoveFollowTowerMission()
	{
		List<int> list = new List<int>();
		foreach (int key in m_PlayerMission.m_MissionInfo.Keys)
		{
			bool flag = false;
			MissionCommonData missionCommonData = GetMissionCommonData(key);
			for (int i = 0; i < missionCommonData.m_TargetIDList.Count; i++)
			{
				TargetType targetType = MissionRepository.GetTargetType(missionCommonData.m_TargetIDList[i]);
				if (targetType == TargetType.TargetType_Follow || targetType == TargetType.TargetType_TowerDif)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				list.Add(key);
			}
		}
		foreach (int item in list)
		{
			m_PlayerMission.FailureMission(item);
			if (item == 9113)
			{
				m_PlayerMission.SetQuestVariable(9112, PlayerMission.MissionFlagStep, "0", pushStory: true, isRecord: true);
			}
		}
	}

	public void InitPlayerMission()
	{
		m_bHadInitMission = false;
		if (PeGameMgr.IsSingleAdventure)
		{
			m_PlayerMission.adId_entityId[1] = 20008;
		}
		GetSpecialItem.ClearLootSpecialItemRecord();
		foreach (KeyValuePair<int, Dictionary<string, string>> item in m_PlayerMission.m_RecordMisInfo)
		{
			MissionCommonData data = GetMissionCommonData(item.Key);
			if (data == null)
			{
				continue;
			}
			foreach (KeyValuePair<string, string> item2 in item.Value)
			{
				if (item2.Key == PlayerMission.MissionFlagStep)
				{
					SetQuestVariable(item.Key, item2.Key, item2.Value, pushStory: true, isRecord: true);
					Debug.Log("InitPlayerMission\ufffd\ufffdID\ufffd\ufffd" + item.Key);
					for (int i = 0; i < data.m_TargetIDList.Count; i++)
					{
						TargetType targetType = MissionRepository.GetTargetType(data.m_TargetIDList[i]);
						if (targetType == TargetType.TargetType_Unkown || targetType != TargetType.TargetType_KillMonster)
						{
							continue;
						}
						TypeMonsterData monData = GetTypeMonsterData(data.m_TargetIDList[i]);
						if (monData == null)
						{
							continue;
						}
						int idx = i * 10;
						if (!monData.m_destroyTown)
						{
							continue;
						}
						VArtifactTownManager.Instance.RegistTownDestryedEvent(delegate(int n)
						{
							if (n == monData.m_campID[0])
							{
								int allyTownDestroyedCount = VATownGenerator.Instance.GetAllyTownDestroyedCount(monData.m_campID[0]);
								string text = $"-1_{allyTownDestroyedCount}";
								ModifyQuestVariable(data.m_ID, PlayerMission.MissionFlagMonster + idx, text);
								if (PeGameMgr.IsMulti && PlayerNetwork.mainPlayer != null)
								{
									PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ModifyMissionFlag, data.m_ID, PlayerMission.MissionFlagMonster + idx, text);
								}
								CompleteTarget(monData.m_TargetID, data.m_ID);
							}
						});
					}
					continue;
				}
				if (PeGameMgr.IsMultiStory)
				{
					Dictionary<string, string> missionFlagType = m_PlayerMission.GetMissionFlagType(item.Key);
					if (missionFlagType == null)
					{
						SetQuestVariable(item.Key, item2.Key, item2.Value, pushStory: true, isRecord: true);
					}
				}
				ModifyQuestVariable(item.Key, item2.Key, item2.Value);
			}
			if (data.m_TargetIDList.Find((int ite) => MissionRepository.GetTargetType(ite) == TargetType.TargetType_Collect) != 0)
			{
				UpdateMissionTrack(data.m_ID);
			}
		}
		if (PeGameMgr.IsAdventure)
		{
			foreach (int key in m_PlayerMission.m_MissionInfo.Keys)
			{
				MissionCommonData missionCommonData = GetMissionCommonData(key);
				if (!missionCommonData.creDungeon.effect || !m_PlayerMission.adId_entityId.ContainsKey(missionCommonData.creDungeon.npcID))
				{
					continue;
				}
				PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(m_PlayerMission.adId_entityId[missionCommonData.creDungeon.npcID]);
				if (!(peEntity == null))
				{
					Vector3 position = peEntity.position;
					Vector2 vector = UnityEngine.Random.insideUnitCircle.normalized * missionCommonData.creDungeon.radius;
					Vector2 vector2 = new Vector2(position.x + vector.x, position.z + vector.y);
					IntVector2 vec = new IntVector2((int)vector2.x, (int)vector2.y);
					vec = m_PlayerMission.AvoidTownPos(vec);
					if (RandomDungenMgr.Instance == null)
					{
						RandomDungenMgrData.AddInitTaskEntrance(vec, missionCommonData.creDungeon.dungeonLevel);
					}
					else
					{
						RandomDungenMgr.Instance.GenTaskEntrance(vec, missionCommonData.creDungeon.dungeonLevel);
					}
				}
			}
		}
		m_bHadInitMission = true;
		CheckAllGetableMission();
		PlotLensAnimation.IsPlaying = false;
		if (PeGameMgr.IsMultiStory && GameUI.Instance != null && GameUI.Instance.mNpcWnd != null)
		{
			GameUI.Instance.mNpcWnd.UpdateMission();
		}
	}

	public bool HasMission(int MissionID)
	{
		return m_PlayerMission.HasMission(MissionID);
	}

	public bool HadCompleteTarget(int TargetID)
	{
		return m_PlayerMission.HadCompleteTarget(TargetID);
	}

	public bool HadCompleteMission(int MissionID)
	{
		return m_PlayerMission.HadCompleteMission(MissionID);
	}

	public bool HadCompleteMissionAnyNum(int MissionID)
	{
		return m_PlayerMission.HadCompleteMissionAnyNum(MissionID);
	}

	public bool IsGetTakeMission(int MissionID)
	{
		return m_PlayerMission.IsGetTakeMission(MissionID);
	}

	public bool IsReplyTarget(int MissionID, int TargetID)
	{
		return m_PlayerMission.IsReplyTarget(MissionID, TargetID);
	}

	public bool IsReplyMission(int MissionID)
	{
		return m_PlayerMission.IsReplyMission(MissionID);
	}

	public string GetQuestVariable(int MissionID, string MissionFlag)
	{
		return m_PlayerMission.GetQuestVariable(MissionID, MissionFlag);
	}

	public int GetTowerDefineKillNum(int MissionID)
	{
		return m_PlayerMission.GetTowerDefineKillNum(MissionID);
	}

	public int SetQuestVariable(int MissionID, string MissionFlag, string MissionValue, bool pushStory = true, bool isRecord = false)
	{
		return m_PlayerMission.SetQuestVariable(MissionID, MissionFlag, MissionValue, pushStory, isRecord);
	}

	public bool ModifyQuestVariable(int MissionID, string MissionFlag, string MissionValue)
	{
		return m_PlayerMission.ModifyQuestVariable(MissionID, MissionFlag, MissionValue);
	}

	public void CompleteTarget(int TargetID, int MissionID, bool forceComplete = false, bool bFromNet = false, bool isOwner = true)
	{
		m_PlayerMission.CompleteTarget(TargetID, MissionID, forceComplete, bFromNet, isOwner);
	}

	public void CompleteMission(int MissionID, int TargetID = -1, bool bCheck = true, bool pushStory = true)
	{
		m_PlayerMission.CompleteMission(MissionID, TargetID, bCheck, pushStory);
	}

	public void AbortMission(UIMissionMgr.MissionView misView)
	{
		if (misView != null)
		{
			MissionCommonData missionCommonData = GetMissionCommonData(misView.mMissionID);
			if (missionCommonData != null && missionCommonData.m_bGiveUp)
			{
				m_PlayerMission.AbortMission(misView.mMissionID);
			}
		}
	}

	public void FailureMission(int MissionID)
	{
		m_PlayerMission.FailureMission(MissionID);
	}

	public int HasFollowMissionNet()
	{
		foreach (KeyValuePair<int, Dictionary<string, string>> item in m_PlayerMission.m_MissionInfo)
		{
			int key = item.Key;
			MissionCommonData missionCommonData = GetMissionCommonData(key);
			if (missionCommonData == null)
			{
				continue;
			}
			for (int i = 0; i < missionCommonData.m_TargetIDList.Count; i++)
			{
				TargetType targetType = MissionRepository.GetTargetType(missionCommonData.m_TargetIDList[i]);
				if (targetType != TargetType.TargetType_Follow)
				{
					continue;
				}
				TypeFollowData typeFollowData = GetTypeFollowData(missionCommonData.m_TargetIDList[i]);
				if (typeFollowData == null || typeFollowData.m_EMode == 1)
				{
					continue;
				}
				if (PeGameMgr.IsMultiStory)
				{
					for (int j = 0; j < typeFollowData.m_iNpcList.Count; j++)
					{
						NetworkInterface networkInterface = NetworkInterface.Get(typeFollowData.m_iNpcList[j]);
						if (networkInterface != null && networkInterface as AiAdNpcNetwork != null)
						{
							NpcCmpt npcCmpt = (networkInterface as AiAdNpcNetwork).npcCmpt;
							if (npcCmpt != null && npcCmpt.GetFollowTargetId() == PlayerNetwork.mainPlayerId && npcCmpt.Net.hasOwnerAuth)
							{
								return key;
							}
						}
					}
					continue;
				}
				return key;
			}
		}
		return -1;
	}

	public int HasFollowMission()
	{
		foreach (KeyValuePair<int, Dictionary<string, string>> item in m_PlayerMission.m_MissionInfo)
		{
			int key = item.Key;
			MissionCommonData missionCommonData = GetMissionCommonData(key);
			if (missionCommonData == null)
			{
				continue;
			}
			for (int i = 0; i < missionCommonData.m_TargetIDList.Count; i++)
			{
				TargetType targetType = MissionRepository.GetTargetType(missionCommonData.m_TargetIDList[i]);
				if (targetType == TargetType.TargetType_Follow)
				{
					TypeFollowData typeFollowData = GetTypeFollowData(missionCommonData.m_TargetIDList[i]);
					if (typeFollowData != null && typeFollowData.m_EMode != 1)
					{
						return key;
					}
				}
			}
		}
		return -1;
	}

	public bool HasTowerDifMission()
	{
		foreach (KeyValuePair<int, Dictionary<string, string>> item in m_PlayerMission.m_MissionInfo)
		{
			int key = item.Key;
			MissionCommonData missionCommonData = GetMissionCommonData(key);
			if (missionCommonData == null || !MissionRepository.HasTargetType(key, TargetType.TargetType_TowerDif))
			{
				continue;
			}
			if (PeGameMgr.IsSingle)
			{
				return true;
			}
			List<AiTowerDefense> list = NetworkInterface.Get<AiTowerDefense>();
			if (list == null)
			{
				continue;
			}
			foreach (AiTowerDefense item2 in list)
			{
				if (item2 != null && item2.hasOwnerAuth)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void RequestCompleteMission(int MissionID, int TargetID = -1, bool bChcek = true)
	{
		m_PlayerMission.RequestCompleteMission(MissionID, TargetID, bChcek);
	}

	public void RequestDeleteMission(int MissionID)
	{
		m_PlayerMission.ReplyDeleteMission(MissionID);
	}

	public void ProcessMonsterDead(int proid, int autoid)
	{
		m_PlayerMission.ProcessMonsterDead(proid, autoid);
	}

	public bool CheckCSCreatorMis(int MissionID)
	{
		return m_PlayerMission.CheckCSCreatorMis(MissionID);
	}

	public bool CheckHeroMis()
	{
		return m_PlayerMission.CheckHeroMis();
	}

	public void ProcessCollectMissionByID(int ItemID)
	{
		m_PlayerMission.ProcessCollectMissionByID(ItemID);
	}

	public void ProcessUseItemMissionByID(int ItemID, Vector3 pos, int addOrSubtract = 1, ItemObject itemobj = null)
	{
		m_PlayerMission.ProcessUseItemByID(ItemID, pos, addOrSubtract, itemobj);
	}

	private Vector3 GetSpawnPos(Vector3 center, int minAngle, int maxAngle, float dist, float radius)
	{
		System.Random random = new System.Random();
		float f = (float)(random.Next(minAngle, maxAngle) / 180) * (float)Math.PI;
		Vector3 vector = center + new Vector3(Mathf.Cos(f) * dist, 0f, Mathf.Sin(f) * dist);
		Vector2 insideUnitCircle = UnityEngine.Random.insideUnitCircle;
		return vector + new Vector3(insideUnitCircle.x * radius, 0f, insideUnitCircle.y * radius);
	}

	private Vector3 GetSpawnPos(Vector3 sor, Vector3 dst, float percent, float radius)
	{
		Vector3 vector = sor + (dst - sor) * percent;
		Vector2 insideUnitCircle = UnityEngine.Random.insideUnitCircle;
		return vector + new Vector3(insideUnitCircle.x * radius, 0f, insideUnitCircle.y * radius);
	}

	public bool IsTempLimit(int missionID)
	{
		MissionCommonData missionCommonData = GetMissionCommonData(missionID);
		foreach (int item in missionCommonData.m_tempLimit)
		{
			if (HasMission(item))
			{
				return true;
			}
		}
		return false;
	}

	public void SetGetTakeMission(int MissionID, PeEntity npc, TakeMissionType type, bool bCheck = true)
	{
		if (type == TakeMissionType.TakeMissionType_Unkown)
		{
			return;
		}
		if (!GameConfig.IsMultiMode || type == TakeMissionType.TakeMissionType_Complete)
		{
			ProcessSingleMode(MissionID, npc, type, bCheck);
			SteamAchievementsSystem.Instance.OnMissionChange(MissionID, 1);
		}
		else
		{
			MissionCommonData missionCommonData = GetMissionCommonData(MissionID);
			if (missionCommonData == null || (PeGameMgr.IsMulti && bCheck && !IsGetTakeMission(MissionID) && !missionCommonData.IsTalkMission()))
			{
				return;
			}
			m_PlayerMission.m_FollowPlayerName = PeSingleton<PeCreature>.Instance.mainPlayer.ExtGetName();
			int num = -1;
			if (npc != null)
			{
				num = ((npc.proto != EEntityProto.Npc && npc.proto != EEntityProto.RandomNpc) ? (-1) : npc.Id);
			}
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_AccessMission, MissionID, num, (byte)type, bCheck);
		}
		SteamAchievementsSystem.Instance.OnMissionChange(MissionID, 1);
	}

	public void ReflashCSUseItemMission()
	{
		int missionID = 794;
		if (m_PlayerMission.ConTainsMission(missionID))
		{
			MissionCommonData missionCommonData = GetMissionCommonData(missionID);
			if (missionCommonData != null)
			{
				PeEntity npc = PeSingleton<EntityMgr>.Instance.Get(missionCommonData.m_iNpc);
				SetGetTakeMission(missionID, npc, TakeMissionType.TakeMissionType_Get, bCheck: false);
			}
		}
		missionID = 847;
		if (m_PlayerMission.ConTainsMission(missionID))
		{
			MissionCommonData missionCommonData = GetMissionCommonData(missionID);
			if (missionCommonData != null)
			{
				PeEntity npc2 = PeSingleton<EntityMgr>.Instance.Get(missionCommonData.m_iNpc);
				SetGetTakeMission(missionID, npc2, TakeMissionType.TakeMissionType_Get, bCheck: false);
			}
		}
	}

	public void ProcessSingleMode(int MissionID, PeEntity npc, TakeMissionType type, bool bCheck, AiAdNpcNetwork adNpc = null, bool pushStory = true)
	{
		if (MissionID < 0 || MissionID > 20000)
		{
			return;
		}
		MissionCommonData data = GetMissionCommonData(MissionID);
		if (data == null)
		{
			return;
		}
		NpcMissionData npcMissionData = npc.GetUserData() as NpcMissionData;
		if (MissionID == 191)
		{
			CSCreator creator = CSMain.GetCreator(0);
			if (creator == null)
			{
				return;
			}
			creator.AddNpc(npc, bSetPos: true);
		}
		else if (MissionID == 888)
		{
			if (!PeGameMgr.IsMulti)
			{
				ServantLeaderCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>();
				if (cmpt.GetServantNum() < ServantLeaderCmpt.mMaxFollower)
				{
					if (!EntityCreateMgr.Instance.IsRandomNpc(npc) || npcMissionData == null)
					{
						return;
					}
					npc.SetBirthPos(npc.position);
					npc.CmdStopTalk();
					StroyManager.Instance.RemoveReq(npc, EReqType.Dialogue);
					npc.Recruit();
					NpcCmpt npcCmpt = npc.NpcCmpt;
					if (npcCmpt != null)
					{
						if (cmpt != null)
						{
							CSMain.SetNpcFollower(npc);
						}
						npcCmpt.SetServantLeader(cmpt);
					}
					npc.SetShopIcon(null);
				}
			}
			else
			{
				PlayerNetwork.RequestNpcRecruit(npc.Id, findPlayer: true);
			}
		}
		bool flag = false;
		if (type == TakeMissionType.TakeMissionType_Get)
		{
			if (PeGameMgr.IsMultiStory)
			{
				if (bCheck && !IsGetTakeMission(MissionID) && !data.IsTalkMission())
				{
					return;
				}
			}
			else if (bCheck && !IsGetTakeMission(MissionID))
			{
				return;
			}
			if (HasRandomMission(MissionID))
			{
				if (npc == null || npc == PeSingleton<PeCreature>.Instance.mainPlayer)
				{
					npc = GameUI.Instance.mNpcWnd.m_CurSelNpc;
				}
				if (!ProcessRandomMission(ref data, npc, adNpc))
				{
					return;
				}
				if (!PeGameMgr.IsMulti)
				{
					if (PeGameMgr.IsAdventure)
					{
						AdRMRepository.CreateRandomMission(MissionID);
					}
					else
					{
						RMRepository.CreateRandomMission(MissionID, ((int?)npcMissionData?.mCurComMisNum) ?? (-1));
					}
				}
			}
			SetQuestVariable(MissionID, PlayerMission.MissionFlagStep, "0", pushStory);
			if (!data.IsTalkMission())
			{
				for (int i = 0; i < data.m_TargetIDList.Count; i++)
				{
					switch (MissionRepository.GetTargetType(data.m_TargetIDList[i]))
					{
					case TargetType.TargetType_KillMonster:
					{
						TypeMonsterData monData = GetTypeMonsterData(data.m_TargetIDList[i]);
						if (monData == null)
						{
							break;
						}
						if (monData.m_destroyTown)
						{
							int idx = i * 10;
							int allyTownDestroyedCount = VATownGenerator.Instance.GetAllyTownDestroyedCount(monData.m_campID[0]);
							string missionValue2 = $"-1_{allyTownDestroyedCount}";
							ModifyQuestVariable(MissionID, PlayerMission.MissionFlagMonster + idx, missionValue2);
							if (allyTownDestroyedCount >= monData.m_townNum[0])
							{
								if (PeGameMgr.IsMulti && PlayerNetwork.mainPlayer != null)
								{
									int allyTownDestroyedCount2 = VATownGenerator.Instance.GetAllyTownDestroyedCount(monData.m_campID[0]);
									string text = $"-1_{allyTownDestroyedCount2}";
									PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ModifyMissionFlag, data.m_ID, PlayerMission.MissionFlagMonster + idx, text);
								}
								CompleteTarget(monData.m_TargetID, data.m_ID);
								break;
							}
							VArtifactTownManager.Instance.RegistTownDestryedEvent(delegate(int n)
							{
								bool flag2 = false;
								foreach (int item in monData.m_campID)
								{
									if (n == item)
									{
										flag2 = true;
									}
								}
								if (flag2)
								{
									int allyTownDestroyedCount3 = VATownGenerator.Instance.GetAllyTownDestroyedCount(n);
									string text2 = $"-1_{allyTownDestroyedCount3}";
									ModifyQuestVariable(MissionID, PlayerMission.MissionFlagMonster + idx, text2);
									if (PeGameMgr.IsMulti && PlayerNetwork.mainPlayer != null)
									{
										PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ModifyMissionFlag, data.m_ID, PlayerMission.MissionFlagMonster + idx, text2);
									}
									CompleteTarget(monData.m_TargetID, data.m_ID);
								}
							});
						}
						else
						{
							for (int j = 0; j < monData.m_MonsterList.Count; j++)
							{
								int num = i * 10 + j;
								string missionValue2 = monData.m_MonsterList[j].npcs[UnityEngine.Random.Range(0, monData.m_MonsterList[j].npcs.Count)] + "_0";
								ModifyQuestVariable(MissionID, PlayerMission.MissionFlagMonster + num, missionValue2);
							}
						}
						break;
					}
					case TargetType.TargetType_UseItem:
					{
						TypeUseItemData typeUseItemData = GetTypeUseItemData(data.m_TargetIDList[i]);
						if (typeUseItemData == null)
						{
							break;
						}
						if (typeUseItemData.m_allowOld && CSMain.HasBuilding(typeUseItemData.m_ItemID, CSMain.s_MgCreator, out var pos) && CSMain.GetAssemblyPos(out var pos2))
						{
							ModifyQuestVariable(MissionValue: (typeUseItemData.m_Type == 0) ? (typeUseItemData.m_ItemID + "_1") : ((!(Vector3.Distance(pos, pos2) < (float)typeUseItemData.m_Radius)) ? (typeUseItemData.m_ItemID + "_0") : (typeUseItemData.m_ItemID + "_1")), MissionID: MissionID, MissionFlag: PlayerMission.MissionFlagItem + i);
							if (IsReplyTarget(data.m_ID, data.m_TargetIDList[i]))
							{
								if (typeUseItemData.m_comMission)
								{
									flag = true;
								}
								else
								{
									CompleteTarget(data.m_TargetIDList[i], data.m_ID);
								}
							}
						}
						else
						{
							string missionValue2 = typeUseItemData.m_ItemID + "_0";
							ModifyQuestVariable(MissionID, PlayerMission.MissionFlagItem + i, missionValue2);
						}
						break;
					}
					}
				}
			}
		}
		switch (type)
		{
		case TakeMissionType.TakeMissionType_Get:
		{
			if (data.m_OPID == null)
			{
				break;
			}
			for (int l = 0; l < data.m_OPID.Count; l++)
			{
				MissionCommonData data3 = GetMissionCommonData(data.m_OPID[l]);
				if (data3 != null)
				{
					if (PeGameMgr.IsAdventure && HasRandomMission(data.m_OPID[l]))
					{
						ProcessRandomMission(ref data3, npc, adNpc);
					}
					if (MissionRepository.HaveTalkOP(data.m_OPID[l]))
					{
						GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(data.m_OPID[l], 1);
						GameUI.Instance.mNPCTalk.PreShow();
					}
					else if (IsGetTakeMission(data.m_OPID[l]))
					{
						SetGetTakeMission(data.m_OPID[l], npc, TakeMissionType.TakeMissionType_Get);
					}
				}
			}
			break;
		}
		case TakeMissionType.TakeMissionType_In:
		{
			if (data.m_INID == null)
			{
				break;
			}
			for (int m = 0; m < data.m_INID.Count; m++)
			{
				if (IsGetTakeMission(data.m_INID[m]))
				{
					SetGetTakeMission(data.m_INID[m], npc, TakeMissionType.TakeMissionType_Get);
				}
			}
			break;
		}
		case TakeMissionType.TakeMissionType_Complete:
		{
			if (!HadCompleteMission(MissionID))
			{
				CompleteMission(MissionID);
			}
			if (data.m_EDID == null)
			{
				break;
			}
			for (int k = 0; k < data.m_EDID.Count; k++)
			{
				MissionCommonData data2 = GetMissionCommonData(data.m_EDID[k]);
				if (data2 != null)
				{
					if (PeGameMgr.IsAdventure && HasRandomMission(data.m_EDID[k]))
					{
						ProcessRandomMission(ref data2, npc, adNpc);
					}
					if (MissionRepository.HaveTalkOP(data.m_EDID[k]))
					{
						GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(data.m_EDID[k], 1);
						GameUI.Instance.mNPCTalk.PreShow();
					}
					else if (IsGetTakeMission(data.m_EDID[k]))
					{
						SetGetTakeMission(data.m_EDID[k], npc, TakeMissionType.TakeMissionType_Get);
					}
				}
			}
			break;
		}
		}
		if (flag)
		{
			CompleteMission(MissionID);
		}
	}

	private bool ProcessRandomMission(ref MissionCommonData data, PeEntity npc, AiAdNpcNetwork adNpc = null)
	{
		if (npc != null && npc.proto != EEntityProto.RandomNpc)
		{
			return false;
		}
		if (null == GameUI.Instance.mNPCTalk)
		{
			return false;
		}
		int num = 0;
		Vector3 failResetPos = Vector3.zero;
		NpcMissionData npcMissionData = null;
		if (npc != null)
		{
			num = npc.Id;
			npcMissionData = npc.GetUserData() as NpcMissionData;
			failResetPos = npc.ExtGetPos();
		}
		else if (adNpc != null)
		{
			npcMissionData = adNpc.useData;
			failResetPos = adNpc.transform.position;
		}
		data.m_iNpc = num;
		data.m_iReplyNpc = num;
		if (data.m_ID / 1000 != 9 && npcMissionData == null)
		{
			return false;
		}
		for (int i = 0; i < data.m_TargetIDList.Count; i++)
		{
			switch (MissionRepository.GetTargetType(data.m_TargetIDList[i]))
			{
			case TargetType.TargetType_Follow:
			{
				TypeFollowData typeFollowData = GetTypeFollowData(data.m_TargetIDList[i]);
				if (typeFollowData != null)
				{
					typeFollowData.m_iNpcList.Clear();
					typeFollowData.m_FailResetPos = failResetPos;
					typeFollowData.m_iNpcList.Add(num);
				}
				break;
			}
			case TargetType.TargetType_TowerDif:
			{
				TypeTowerDefendsData typeTowerDefendsData = GetTypeTowerDefendsData(data.m_TargetIDList[i]);
				if (typeTowerDefendsData != null)
				{
					typeTowerDefendsData.m_iNpcList.Clear();
					typeTowerDefendsData.m_iNpcList.Add(num);
				}
				break;
			}
			default:
				data.m_iReplyNpc = num;
				if (!data.isAutoReply && npcMissionData != null && !npcMissionData.m_MissionListReply.Contains(data.m_ID))
				{
					npcMissionData.m_MissionListReply.Add(data.m_ID);
				}
				break;
			case TargetType.TargetType_Discovery:
				break;
			}
		}
		return true;
	}

	public void UpdateMissionMainGUI(int MissionID, bool bComplete = true)
	{
		if (MissionID == 71 || MissionID <= 0 || MissionID > 20000)
		{
			return;
		}
		if (bComplete)
		{
			UIMissionMgr.Instance.DeleteMission(MissionID);
			return;
		}
		Dictionary<string, string> missionFlagType = m_PlayerMission.GetMissionFlagType(MissionID);
		if (missionFlagType == null)
		{
			return;
		}
		MissionCommonData missionCommonData = GetMissionCommonData(MissionID);
		if (missionCommonData != null && !missionCommonData.IsTalkMission())
		{
			UIMissionMgr.MissionView missionView = new UIMissionMgr.MissionView();
			missionView.mMissionID = missionCommonData.m_ID;
			missionView.mMissionType = missionCommonData.m_Type;
			missionView.mMissionTitle = missionCommonData.m_MissionName;
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(missionCommonData.m_iNpc);
			if (peEntity != null)
			{
				missionView.mMissionStartNpc.mName = peEntity.ExtGetName();
				missionView.mMissionStartNpc.mNpcIcoStr = peEntity.ExtGetFaceIconBig();
			}
			peEntity = PeSingleton<EntityMgr>.Instance.Get(missionCommonData.m_iReplyNpc);
			if (peEntity != null)
			{
				missionView.mMissionEndNpc.mName = peEntity.ExtGetName();
				missionView.mMissionEndNpc.mNpcIcoStr = peEntity.ExtGetFaceIconBig();
				missionView.mEndMisPos = peEntity.ExtGetPos();
				missionView.mAttachOnId = peEntity.Id;
				missionView.NeedArrow = true;
			}
			peEntity = PeSingleton<EntityMgr>.Instance.Get(missionCommonData.m_replyIconId);
			if (peEntity != null)
			{
				missionView.mMissionReplyNpc.mName = peEntity.ExtGetName();
				missionView.mMissionReplyNpc.mNpcIcoStr = peEntity.ExtGetFaceIconBig();
			}
			ParseMissionFlag(missionCommonData, missionFlagType, missionView);
			UIMissionMgr.Instance.AddMission(missionView);
			UIMissionMgr.Instance.RefalshMissionWnd();
		}
	}

	private void CheckViewComplete(UIMissionMgr.MissionView view)
	{
		bool mComplete = true;
		foreach (UIMissionMgr.TargetShow mTarget in view.mTargetList)
		{
			if (!mTarget.mComplete)
			{
				mComplete = false;
				break;
			}
		}
		view.mComplete = mComplete;
	}

	public void UpdateUseMissionTrack(int MissionID, int targetID, int count = 0)
	{
		if (MissionID < 0 || MissionID > 20000)
		{
			return;
		}
		UIMissionMgr.MissionView missionView = UIMissionMgr.Instance.GetMissionView(MissionID);
		if (missionView == null)
		{
			return;
		}
		MissionCommonData data = GetMissionCommonData(MissionID);
		if (data != null && targetID < data.m_TargetIDList.Count)
		{
			UIMissionMgr.TargetShow targetShow = missionView.mTargetList.Find((UIMissionMgr.TargetShow ite) => UIMissionMgr.MissionView.MatchID(ite, data.m_TargetIDList[targetID]));
			targetShow.mCount = count;
			if (targetShow.mMaxCount <= targetShow.mCount)
			{
				targetShow.mComplete = true;
			}
			else
			{
				targetShow.mComplete = false;
			}
			CheckViewComplete(missionView);
		}
	}

	public void UpdateMissionTrack(int MissionID, int count = 0, int TargetID = -1)
	{
		if (MissionID < 0 || MissionID > 20000)
		{
			return;
		}
		UIMissionMgr.MissionView missionView = UIMissionMgr.Instance.GetMissionView(MissionID);
		if (missionView == null)
		{
			return;
		}
		if (TargetID != -1)
		{
			UIMissionMgr.TargetShow targetShow = missionView.mTargetList.Find((UIMissionMgr.TargetShow ite) => UIMissionMgr.MissionView.MatchID(ite, TargetID));
			TargetType targetType = MissionRepository.GetTargetType(TargetID);
			switch (targetType)
			{
			case TargetType.TargetType_Follow:
			case TargetType.TargetType_Discovery:
				switch (targetType)
				{
				case TargetType.TargetType_Follow:
				{
					TypeFollowData typeFollowData = GetTypeFollowData(TargetID);
					if (typeFollowData == null)
					{
						return;
					}
					break;
				}
				case TargetType.TargetType_Discovery:
				{
					TypeSearchData typeSearchData = GetTypeSearchData(TargetID);
					if (typeSearchData == null)
					{
						return;
					}
					break;
				}
				}
				if (targetShow != null)
				{
					targetShow.mPosition = Vector3.zero;
					targetShow.mComplete = true;
				}
				return;
			case TargetType.TargetType_KillMonster:
				targetShow.mCount = count;
				if (targetShow.mMaxCount <= targetShow.mCount)
				{
					targetShow.mComplete = true;
				}
				else
				{
					targetShow.mComplete = false;
				}
				return;
			}
		}
		MissionCommonData data = GetMissionCommonData(MissionID);
		if (data == null)
		{
			return;
		}
		for (int i = 0; i < data.m_TargetIDList.Count; i++)
		{
			UIMissionMgr.TargetShow targetShow2 = missionView.mTargetList.Find((UIMissionMgr.TargetShow ite) => UIMissionMgr.MissionView.MatchID(ite, data.m_TargetIDList[i]));
			if (targetShow2 == null)
			{
				continue;
			}
			if (PeSingleton<PeCreature>.Instance.mainPlayer == null)
			{
				return;
			}
			switch (MissionRepository.GetTargetType(data.m_TargetIDList[i]))
			{
			case TargetType.TargetType_KillMonster:
			{
				TypeMonsterData typeMonsterData = GetTypeMonsterData(data.m_TargetIDList[i]);
				if (typeMonsterData != null)
				{
					targetShow2.mCount = count;
				}
				break;
			}
			case TargetType.TargetType_Collect:
			{
				TypeCollectData typeCollectData = GetTypeCollectData(data.m_TargetIDList[i]);
				if (typeCollectData != null)
				{
					ECreation eCreation = m_PlayerMission.IsSpecialID(typeCollectData.ItemID);
					if (eCreation != 0)
					{
						targetShow2.mCount = PeSingleton<PeCreature>.Instance.mainPlayer.GetCreationItemCount(eCreation);
					}
					else
					{
						targetShow2.mCount = PeSingleton<PeCreature>.Instance.mainPlayer.GetPkgItemCount(typeCollectData.ItemID);
					}
				}
				break;
			}
			case TargetType.TargetType_UseItem:
			{
				TypeUseItemData typeUseItemData = GetTypeUseItemData(data.m_TargetIDList[i]);
				if (typeUseItemData != null)
				{
					Dictionary<string, string> missionFlagType = Instance.m_PlayerMission.GetMissionFlagType(MissionID);
					if (missionFlagType.ContainsKey(typeUseItemData.m_ItemID.ToString()))
					{
						targetShow2.mCount = Convert.ToInt32(missionFlagType[typeUseItemData.m_ItemID.ToString()]) + count;
					}
				}
				break;
			}
			case TargetType.TargetType_TowerDif:
			{
				TypeTowerDefendsData typeTowerDefendsData = GetTypeTowerDefendsData(data.m_TargetIDList[i]);
				if (typeTowerDefendsData != null)
				{
					count = GetTowerDefineKillNum(MissionID);
					targetShow2.mCount = count;
				}
				break;
			}
			default:
				continue;
			}
			if (targetShow2.mMaxCount <= targetShow2.mCount)
			{
				targetShow2.mComplete = true;
			}
			else
			{
				targetShow2.mComplete = false;
			}
		}
		CheckViewComplete(missionView);
	}

	public void CheckAllGetableMission()
	{
		foreach (KeyValuePair<int, MissionCommonData> item in MissionRepository.m_MissionCommonMap)
		{
			if (MissionRepository.IsMainMission(item.Key) && IsGetTakeMission(item.Key))
			{
				MissionCommonData value = item.Value;
				PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(value.m_iNpc);
				if (!(peEntity == null) && !m_PlayerMission.followers.Contains(peEntity.NpcCmpt) && !ServantLeaderCmpt.Instance.mForcedFollowers.Contains(peEntity.NpcCmpt) && peEntity.GetUserData() is NpcMissionData npcMissionData && npcMissionData.m_MissionList.Contains(item.Key))
				{
					UIMissionMgr.GetableMisView getableMisView = new UIMissionMgr.GetableMisView(item.Key, value.m_MissionName, peEntity.ExtGetPos(), peEntity.Id);
					getableMisView.TargetNpcInfo.mName = peEntity.ExtGetName();
					getableMisView.TargetNpcInfo.mNpcIcoStr = peEntity.ExtGetFaceIconBig();
					UIMissionMgr.Instance.AddGetableMission(getableMisView);
				}
			}
		}
		if (!PeGameMgr.IsStory && !PeGameMgr.IsAdventure)
		{
			return;
		}
		foreach (KeyValuePair<int, NpcMissionData> dicMissionDatum in NpcMissionDataRepository.dicMissionData)
		{
			if (dicMissionDatum.Value.m_RandomMission == 0 || !IsGetTakeMission(dicMissionDatum.Value.m_RandomMission))
			{
				continue;
			}
			PeEntity npc = PeSingleton<EntityMgr>.Instance.Get(dicMissionDatum.Key);
			if (!(npc == null) && !m_PlayerMission.followers.Contains(npc.NpcCmpt) && !Array.Find(ServantLeaderCmpt.Instance.mFollowers, (NpcCmpt nc) => (nc == npc.NpcCmpt) ? true : false))
			{
				NpcMissionData value2 = dicMissionDatum.Value;
				if (value2 != null)
				{
					UIMissionMgr.GetableMisView getableMisView2 = new UIMissionMgr.GetableMisView(dicMissionDatum.Value.m_RandomMission, "RandomMission", npc.ExtGetPos(), npc.Id);
					getableMisView2.TargetNpcInfo.mName = npc.ExtGetName();
					getableMisView2.TargetNpcInfo.mNpcIcoStr = npc.ExtGetFaceIconBig();
					UIMissionMgr.Instance.AddGetableMission(getableMisView2);
				}
			}
		}
	}

	private void CheckAdAllGetableMission()
	{
		foreach (KeyValuePair<int, MissionCommonData> item in AdRMRepository.m_AdRandMisMap)
		{
			if (!IsGetTakeMission(item.Key))
			{
				continue;
			}
			MissionCommonData value = item.Value;
			int num = 0;
			foreach (KeyValuePair<int, PeEntity> item2 in PeSingleton<EntityMgr>.Instance.mDicEntity)
			{
				if (!(item2.Value.GetUserData() is NpcMissionData npcMissionData))
				{
					continue;
				}
				foreach (KeyValuePair<int, List<GroupInfo>> group in AdRMRepository.m_AdRandomGroup[npcMissionData.m_QCID].m_GroupList)
				{
					for (int i = 0; i < group.Value.Count; i++)
					{
						if (group.Value[i].id == value.m_ID)
						{
							num = item2.Key;
							break;
						}
					}
				}
			}
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(num);
			if (!(peEntity == null))
			{
				AdRMRepository.m_AdRandMisMap[item.Key].m_iNpc = num;
				UIMissionMgr.GetableMisView getableMisView = new UIMissionMgr.GetableMisView(item.Key, value.m_MissionName, peEntity.ExtGetPos(), peEntity.Id);
				getableMisView.TargetNpcInfo.mName = peEntity.ExtGetName();
				getableMisView.TargetNpcInfo.mNpcIcoStr = peEntity.ExtGetFaceIconBig();
				UIMissionMgr.Instance.AddGetableMission(getableMisView);
			}
		}
	}

	public void ParseMissionFlag(MissionCommonData data, Dictionary<string, string> MissionFlagType, UIMissionMgr.MissionView stMV)
	{
		string description = data.m_Description;
		if (description == null)
		{
			return;
		}
		data.m_MissionName = GameUI.Instance.mNPCTalk.ParseStrDefine(data.m_MissionName, data);
		description = GameUI.Instance.mNPCTalk.ParseStrDefine(description, data);
		if (data.m_Type == MissionType.MissionType_Mul)
		{
			UIMissionMgr.TargetShow targetShow = new UIMissionMgr.TargetShow();
			targetShow.mContent = data.m_MulDesc;
			stMV.mTargetList.Add(targetShow);
			stMV.mMissionDesc = description;
			return;
		}
		string text = "\"monsterid%\"";
		string text2 = "\"monsternum%\"";
		string text3 = "\"position%\"";
		string text4 = "\"npcid1%\"";
		string text5 = "\"npcid2%\"";
		string text6 = "\"npcid3%\"";
		string text7 = "\"itemid%\"";
		string text8 = "\"itemnum%\"";
		string text9 = "\"targetitemid%\"";
		string text10 = "\"givenpcid%\"";
		string text11 = "\"receivenpcid%\"";
		string value = "\"AdvNPC%\"";
		string value2 = "\"Town%\"";
		string value3 = "\"AI%\"";
		for (int i = 0; i < data.m_TargetIDList.Count; i++)
		{
			TypeMonsterData typeMonsterData = GetTypeMonsterData(data.m_TargetIDList[i]);
			TypeCollectData typeCollectData = GetTypeCollectData(data.m_TargetIDList[i]);
			TypeFollowData typeFollowData = GetTypeFollowData(data.m_TargetIDList[i]);
			TypeSearchData typeSearchData = GetTypeSearchData(data.m_TargetIDList[i]);
			TypeUseItemData typeUseItemData = GetTypeUseItemData(data.m_TargetIDList[i]);
			TypeMessengerData typeMessengerData = GetTypeMessengerData(data.m_TargetIDList[i]);
			TypeTowerDefendsData typeTowerDefendsData = GetTypeTowerDefendsData(data.m_TargetIDList[i]);
			UIMissionMgr.TargetShow targetShow2 = new UIMissionMgr.TargetShow(data.m_TargetIDList[i]);
			if (typeMonsterData != null)
			{
				targetShow2.mContent = ((!(typeMonsterData.m_Desc != "0")) ? string.Empty : typeMonsterData.m_Desc);
				for (int j = 0; j < typeMonsterData.m_MonsterList.Count; j++)
				{
					List<MonsterProtoDb.Item> list = new List<MonsterProtoDb.Item>();
					for (int k = 0; k < typeMonsterData.m_MonsterList[j].npcs.Count; k++)
					{
						MonsterProtoDb.Item item = MonsterProtoDb.Get(typeMonsterData.m_MonsterList[j].npcs[k]);
						if (item != null)
						{
							list.Add(item);
						}
					}
					foreach (MonsterProtoDb.Item item2 in list)
					{
						targetShow2.mIconName.Add(item2.icon);
					}
					targetShow2.mMaxCount = typeMonsterData.m_MonsterList[j].type;
					if ((targetShow2.mCount = m_PlayerMission.GetQuestVariable(data.m_ID, typeMonsterData.m_MonsterList[j].npcs[UnityEngine.Random.Range(0, typeMonsterData.m_MonsterList[j].npcs.Count)])) >= typeMonsterData.m_MonsterList[j].type)
					{
						targetShow2.mComplete = true;
					}
					string text12 = string.Empty;
					for (int l = 0; l < list.Count; l++)
					{
						text12 += list[l].name;
						if (l != list.Count - 1)
						{
							text12 += "s or ";
						}
					}
					if (targetShow2.mContent.Contains(text))
					{
						targetShow2.mContent = targetShow2.mContent.Replace(text, text12);
					}
					if (targetShow2.mContent.Contains(text2))
					{
						targetShow2.mContent = targetShow2.mContent.Replace(text2, typeMonsterData.m_MonsterList[j].type.ToString());
					}
				}
			}
			else if (typeCollectData != null)
			{
				targetShow2.mContent = ((!(typeCollectData.m_Desc != "0")) ? string.Empty : typeCollectData.m_Desc);
				string[] array = ItemProto.GetIconName(typeCollectData.ItemID);
				if (array == null)
				{
					switch (m_PlayerMission.IsSpecialID(typeCollectData.ItemID))
					{
					case ECreation.Sword:
						array = new string[3] { "task_created_001", "0", "0" };
						break;
					case ECreation.HandGun:
						array = new string[3] { "task_created_002", "0", "0" };
						break;
					case ECreation.Rifle:
						array = new string[3] { "task_created_003", "0", "0" };
						break;
					case ECreation.Vehicle:
						array = new string[3] { "task_created_007", "0", "0" };
						break;
					case ECreation.Aircraft:
						array = new string[3] { "task_created_009", "0", "0" };
						break;
					}
				}
				if (array == null || array.Length != 3)
				{
					continue;
				}
				if (typeCollectData.m_TargetPos != Vector3.zero)
				{
					targetShow2.mPosition = typeCollectData.m_TargetPos;
					targetShow2.Radius = 10f;
					stMV.NeedArrow = true;
				}
				targetShow2.mIconName.Add(array[0]);
				targetShow2.mMaxCount = typeCollectData.ItemNum;
				ECreation eCreation = m_PlayerMission.IsSpecialID(typeCollectData.ItemID);
				if ((targetShow2.mCount = ((eCreation == ECreation.Null) ? PeSingleton<PeCreature>.Instance.mainPlayer.GetPkgItemCount(typeCollectData.ItemID) : PeSingleton<PeCreature>.Instance.mainPlayer.GetCreationItemCount(eCreation))) >= typeCollectData.ItemNum)
				{
					targetShow2.mComplete = true;
				}
				if (targetShow2.mContent.Contains(text7))
				{
					targetShow2.mContent = targetShow2.mContent.Replace(text7, ItemProto.GetName(typeCollectData.ItemID));
				}
				if (targetShow2.mContent.Contains(text8))
				{
					targetShow2.mContent = targetShow2.mContent.Replace(text8, typeCollectData.ItemNum.ToString());
				}
				if (targetShow2.mContent.Contains(text9))
				{
					targetShow2.mContent = targetShow2.mContent.Replace(text9, typeCollectData.m_TargetItemID.ToString());
				}
				if (targetShow2.mContent.Contains(text4))
				{
					PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(data.m_iNpc);
					if (peEntity != null)
					{
						targetShow2.mContent = targetShow2.mContent.Replace(text4, peEntity.ExtGetName());
					}
				}
			}
			else if (typeFollowData != null)
			{
				targetShow2.mContent = ((!(typeFollowData.m_Desc != "0")) ? string.Empty : typeFollowData.m_Desc);
				for (int m = 0; m < typeFollowData.m_iNpcList.Count; m++)
				{
					PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(typeFollowData.m_iNpcList[m]);
					if (peEntity != null)
					{
						if (peEntity.ExtGetName() == "AllenCarryingGerdy")
						{
							targetShow2.mIconName.Add("npc_AllenCarter");
						}
						else
						{
							targetShow2.mIconName.Add(peEntity.ExtGetFaceIconBig());
						}
						if (m == 0 && targetShow2.mContent.Contains(text4))
						{
							targetShow2.mContent = targetShow2.mContent.Replace(text4, peEntity.ExtGetName());
						}
						else if (m == 1 && targetShow2.mContent.Contains(text5))
						{
							targetShow2.mContent = targetShow2.mContent.Replace(text5, peEntity.ExtGetName());
						}
						else if (m == 2 && targetShow2.mContent.Contains(text6))
						{
							targetShow2.mContent = targetShow2.mContent.Replace(text6, peEntity.ExtGetName());
						}
					}
				}
				if (typeFollowData.m_BuildID != 0)
				{
					PlayerMission.GetBuildingPos((data.m_ID != 9032) ? data.m_ID : 0, out targetShow2.mPosition);
					targetShow2.Radius = typeFollowData.m_TrackRadius;
					stMV.NeedArrow = true;
				}
				else if (typeFollowData.m_DistPos != Vector3.zero)
				{
					targetShow2.mPosition = typeFollowData.m_DistPos;
					targetShow2.Radius = typeFollowData.m_TrackRadius;
					stMV.NeedArrow = true;
				}
				else if (typeFollowData.m_LookNameID != 0)
				{
					PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(typeFollowData.m_LookNameID);
					if (peEntity != null)
					{
						targetShow2.mPosition = new Vector3(1f, 1f, 1f);
						targetShow2.Radius = typeFollowData.m_TrackRadius;
						stMV.NeedArrow = true;
						targetShow2.mAttachOnID = typeFollowData.m_LookNameID;
					}
				}
				if (PeGameMgr.IsMulti && !PeGameMgr.IsMultiStory && typeFollowData.m_DistPos == Vector3.zero)
				{
					typeFollowData.m_DistPos = new Vector3(1f, 1f, 1f);
					targetShow2.mPosition = typeFollowData.m_DistPos;
					targetShow2.Radius = typeFollowData.m_TrackRadius;
					stMV.NeedArrow = true;
				}
				if (typeFollowData.m_SceneType != 0)
				{
					if (PeGameMgr.IsMultiStory)
					{
						if (PlayerNetwork.mainPlayer != null)
						{
							if (typeFollowData.m_SceneType == 1)
							{
								if (typeFollowData.m_SceneType != PlayerNetwork.mainPlayer._curSceneId)
								{
									targetShow2.mPosition = new Vector3(9687f, 370f, 12799f);
								}
							}
							else if (typeFollowData.m_SceneType == 2 && typeFollowData.m_SceneType != PlayerNetwork.mainPlayer._curSceneId)
							{
								targetShow2.mPosition = new Vector3(14820f, 107f, 8353f);
							}
						}
					}
					else if (typeFollowData.m_SceneType == 1)
					{
						if (typeFollowData.m_SceneType != (int)SingleGameStory.curType)
						{
							targetShow2.mPosition = new Vector3(9687f, 370f, 12799f);
						}
					}
					else if (typeFollowData.m_SceneType == 2 && typeFollowData.m_SceneType != (int)SingleGameStory.curType)
					{
						targetShow2.mPosition = new Vector3(14820f, 107f, 8353f);
					}
				}
				if (targetShow2.mContent.Contains(text3))
				{
					targetShow2.mContent = targetShow2.mContent.Replace(text3, typeFollowData.m_DistPos.ToString());
				}
				if (HadCompleteTarget(typeFollowData.m_TargetID))
				{
					targetShow2.mComplete = true;
					targetShow2.mPosition = Vector3.zero;
				}
			}
			else if (typeSearchData != null)
			{
				targetShow2.mContent = ((!(typeSearchData.m_Desc != "0")) ? string.Empty : typeSearchData.m_Desc);
				if (typeSearchData.m_TrackRadius != 0)
				{
					if (typeSearchData.m_DistPos != Vector3.zero)
					{
						targetShow2.mPosition = typeSearchData.m_DistPos;
						targetShow2.Radius = typeSearchData.m_TrackRadius;
						stMV.NeedArrow = true;
					}
					if (!PeGameMgr.IsMultiStory && PeGameMgr.IsMulti && typeSearchData.m_DistPos == Vector3.zero)
					{
						typeSearchData.m_DistPos = new Vector3(1f, 1f, 1f);
						targetShow2.mPosition = typeSearchData.m_DistPos;
						targetShow2.Radius = typeSearchData.m_TrackRadius;
						stMV.NeedArrow = true;
					}
					if (typeSearchData.m_NpcID != 0)
					{
						PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(typeSearchData.m_NpcID);
						if (peEntity != null)
						{
							targetShow2.mPosition = new Vector3(1f, 1f, 1f);
							targetShow2.Radius = typeSearchData.m_TrackRadius;
							targetShow2.mAttachOnID = typeSearchData.m_NpcID;
							stMV.NeedArrow = true;
						}
					}
				}
				if (typeSearchData.m_SceneType != 0)
				{
					if (typeSearchData.m_SceneType == 1)
					{
						if (typeSearchData.m_SceneType != (int)SingleGameStory.curType)
						{
							targetShow2.mPosition = new Vector3(9687f, 370f, 12799f);
						}
					}
					else if (typeSearchData.m_SceneType == 2 && typeSearchData.m_SceneType != (int)SingleGameStory.curType)
					{
						targetShow2.mPosition = new Vector3(14820f, 107f, 8353f);
					}
				}
				if (targetShow2.mContent.Contains(text3))
				{
					targetShow2.mContent = targetShow2.mContent.Replace(text3, typeSearchData.m_DistPos.ToString());
				}
				if (targetShow2.mContent.Contains(text4))
				{
					PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(typeSearchData.m_NpcID);
					if (peEntity != null)
					{
						targetShow2.mContent = targetShow2.mContent.Replace(text4, peEntity.ExtGetName());
					}
				}
				if (HadCompleteTarget(typeSearchData.m_TargetID))
				{
					targetShow2.mComplete = true;
					targetShow2.mPosition = Vector3.zero;
				}
			}
			else if (typeUseItemData != null)
			{
				targetShow2.mContent = ((!(typeUseItemData.m_Desc != "0")) ? string.Empty : typeUseItemData.m_Desc);
				string[] array2 = ItemProto.GetIconName(typeUseItemData.m_ItemID);
				if (array2 == null)
				{
					switch (m_PlayerMission.IsSpecialID(typeUseItemData.m_ItemID))
					{
					case ECreation.Sword:
						array2 = new string[3] { "task_created_001", "0", "0" };
						break;
					case ECreation.HandGun:
						array2 = new string[3] { "task_created_002", "0", "0" };
						break;
					case ECreation.Rifle:
						array2 = new string[3] { "task_created_003", "0", "0" };
						break;
					case ECreation.Vehicle:
						array2 = new string[3] { "task_created_007", "0", "0" };
						break;
					case ECreation.Aircraft:
						array2 = new string[3] { "task_created_009", "0", "0" };
						break;
					}
				}
				if (array2 == null || array2.Length != 3)
				{
					continue;
				}
				targetShow2.mIconName.Add(array2[0]);
				targetShow2.mMaxCount = typeUseItemData.m_UseNum;
				targetShow2.mCount = m_PlayerMission.GetQuestVariable(data.m_ID, typeUseItemData.m_ItemID);
				if (typeUseItemData.m_Pos != Vector3.zero)
				{
					if (typeUseItemData.m_Pos == new Vector3(-255f, -255f, -255f))
					{
						if (CSMain.GetAssemblyPos(out var pos))
						{
							targetShow2.mPosition = pos;
						}
					}
					else
					{
						targetShow2.mPosition = typeUseItemData.m_Pos;
					}
					targetShow2.Radius = typeUseItemData.m_Radius;
					stMV.NeedArrow = true;
				}
				if (PeGameMgr.IsMulti && !PeGameMgr.IsMultiStory && typeUseItemData.m_Pos == Vector3.zero)
				{
					typeUseItemData.m_Pos = new Vector3(1f, 1f, 1f);
					if (typeUseItemData.m_Pos == new Vector3(-255f, -255f, -255f))
					{
						if (CSMain.GetAssemblyPos(out var pos2))
						{
							targetShow2.mPosition = pos2;
						}
					}
					else
					{
						targetShow2.mPosition = typeUseItemData.m_Pos;
					}
					targetShow2.Radius = typeUseItemData.m_Radius;
					stMV.NeedArrow = true;
				}
				if (targetShow2.mContent.Contains(text3))
				{
					targetShow2.mContent = targetShow2.mContent.Replace(text3, typeUseItemData.m_Pos.ToString());
				}
				if (targetShow2.mContent.Contains(text7))
				{
					targetShow2.mContent = targetShow2.mContent.Replace(text7, ItemProto.GetName(typeUseItemData.m_ItemID));
				}
				if (targetShow2.mContent.Contains(text8))
				{
					targetShow2.mContent = targetShow2.mContent.Replace(text8, typeUseItemData.m_UseNum.ToString());
				}
				if (targetShow2.mCount >= targetShow2.mMaxCount)
				{
					targetShow2.mPosition = Vector3.zero;
					targetShow2.mComplete = true;
				}
			}
			else if (typeMessengerData != null)
			{
				targetShow2.mContent = ((!(typeMessengerData.m_Desc != "0")) ? string.Empty : typeMessengerData.m_Desc);
				PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(typeMessengerData.m_iReplyNpc);
				if (peEntity != null)
				{
					targetShow2.mIconName.Add(peEntity.ExtGetFaceIconBig());
					if (targetShow2.mContent.Contains(text11))
					{
						targetShow2.mContent = targetShow2.mContent.Replace(text11, peEntity.ExtGetName());
					}
				}
				peEntity = PeSingleton<EntityMgr>.Instance.Get(typeMessengerData.m_iNpc);
				if (peEntity != null && targetShow2.mContent.Contains(text10))
				{
					targetShow2.mContent = targetShow2.mContent.Replace(text10, peEntity.ExtGetName());
				}
				if (targetShow2.mContent.Contains(text7))
				{
					targetShow2.mContent = targetShow2.mContent.Replace(text7, ItemProto.GetName(typeMessengerData.m_ItemID));
				}
				if (targetShow2.mContent.Contains(text8))
				{
					targetShow2.mContent = targetShow2.mContent.Replace(text8, typeMessengerData.m_ItemNum.ToString());
				}
				targetShow2.mMaxCount = typeMessengerData.m_ItemNum;
			}
			else if (typeTowerDefendsData != null)
			{
				targetShow2.mContent = ((!(typeTowerDefendsData.m_Desc != "0")) ? string.Empty : typeTowerDefendsData.m_Desc);
				targetShow2.mMaxCount = typeTowerDefendsData.m_Count;
			}
			if (PeGameMgr.IsAdventure)
			{
				if (targetShow2.mContent.Contains(value))
				{
					int num = targetShow2.mContent.IndexOf(value);
					if (targetShow2.mContent.Length >= num + 9 + 3)
					{
						string text13 = targetShow2.mContent.Substring(num + 9, 3);
						if (PEMath.IsNumeral(text13))
						{
							int key = Convert.ToInt32(text13);
							if (Instance.m_PlayerMission.adId_entityId.ContainsKey(key))
							{
								PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(Instance.m_PlayerMission.adId_entityId[key]);
								if (peEntity != null)
								{
									string oldValue = targetShow2.mContent.Substring(num, 12);
									string newValue = peEntity.name.Substring(0, peEntity.name.Length - 1 - Convert.ToString(peEntity.Id).Length);
									targetShow2.mContent = targetShow2.mContent.Replace(oldValue, newValue);
								}
							}
						}
					}
				}
				if (targetShow2.mContent.Contains(value2))
				{
					int num2 = targetShow2.mContent.IndexOf(value2);
					if (targetShow2.mContent.Length >= num2 + 7 + 3)
					{
						string text13 = targetShow2.mContent.Substring(num2 + 7, 3);
						if (PEMath.IsNumeral(text13))
						{
							int townId = Convert.ToInt32(text13);
							VArtifactUtil.GetTownName(townId, out var newValue2);
							targetShow2.mContent = targetShow2.mContent.Replace(targetShow2.mContent.Substring(num2, 10), newValue2);
						}
					}
				}
				if (targetShow2.mContent.Contains(value3))
				{
					int num3 = targetShow2.mContent.IndexOf(value3);
					if (targetShow2.mContent.Length >= num3 + 5 + 3)
					{
						targetShow2.mContent = targetShow2.mContent.Replace(targetShow2.mContent.Substring(num3, 8), "Puja");
					}
				}
			}
			stMV.mTargetList.Add(targetShow2);
		}
		if (PeGameMgr.IsAdventure)
		{
			if (description.Contains(value))
			{
				int num4 = description.IndexOf(value);
				if (description.Length >= num4 + 9 + 3)
				{
					string text14 = description.Substring(num4 + 9, 3);
					if (PEMath.IsNumeral(text14))
					{
						int key2 = Convert.ToInt32(text14);
						if (Instance.m_PlayerMission.adId_entityId.ContainsKey(key2))
						{
							PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(Instance.m_PlayerMission.adId_entityId[key2]);
							if (peEntity != null)
							{
								string oldValue2 = description.Substring(num4, 12);
								string newValue3 = peEntity.name.Substring(0, peEntity.name.Length - 1 - Convert.ToString(peEntity.Id).Length);
								description = description.Replace(oldValue2, newValue3);
							}
						}
					}
				}
			}
			if (description.Contains(value2))
			{
				int num5 = description.IndexOf(value2);
				if (description.Length >= num5 + 7 + 3)
				{
					string text14 = description.Substring(num5 + 7, 3);
					if (PEMath.IsNumeral(text14))
					{
						int townId2 = Convert.ToInt32(text14);
						VArtifactUtil.GetTownName(townId2, out var newValue4);
						description = description.Replace(description.Substring(num5, 10), newValue4);
					}
				}
			}
			if (description.Contains(value3))
			{
				int num6 = description.IndexOf(value3);
				if (description.Length >= num6 + 5 + 3)
				{
					description = description.Replace(description.Substring(num6, 8), "Puja");
				}
			}
		}
		stMV.mMissionDesc = description;
		for (int n = 0; n < data.m_Com_RewardItem.Count; n++)
		{
			ItemSample itemSample = new ItemSample(data.m_Com_RewardItem[n].id, data.m_Com_RewardItem[n].num);
			if (itemSample.protoData == null)
			{
				Debug.LogError("xxxxxxxxxxxxxxxxxx========================================" + data.m_Com_RewardItem[n].id);
			}
			else if (itemSample != null && itemSample.protoData.equipSex == PeSex.Undefined && data.m_Com_RewardItem[n].id > 0)
			{
				stMV.mRewardsList.Add(itemSample);
			}
		}
		CheckViewComplete(stMV);
	}

	private void Update()
	{
		if (PeSingleton<PeCreature>.Instance.mainPlayer != null)
		{
			UpdateMission();
			UpdateMissionNeedTime();
			UpdateTimer();
			MoveAdventureLeader();
		}
	}

	private void MoveAdventureLeader()
	{
		if (!PeGameMgr.IsAdventure || !(Time.time - gameTime > 3f))
		{
			return;
		}
		gameTime = Time.time;
		int allyId;
		Vector3 townPos;
		float nearestAllyDistance = VATownGenerator.Instance.GetNearestAllyDistance(PeSingleton<PeCreature>.Instance.mainPlayer.position, out allyId, out townPos);
		if (allyId != 0 && nearestAllyDistance < 150f && Vector3.Distance(townPos, recordTownPos) > 1f)
		{
			recordTownPos = townPos;
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(20000 - allyId);
			if (peEntity != null)
			{
				Vector3 vector = townPos + (PeSingleton<PeCreature>.Instance.mainPlayer.position - townPos).normalized * 105f;
				IntVector2 intVector = new IntVector2((int)vector.x, (int)vector.z);
				StroyManager.Instance.MoveTo(peEntity, new Vector3(intVector.x, VFDataRTGen.GetPosTop(intVector), intVector.y), 1f);
			}
		}
	}

	private void UpdateTimer()
	{
		if (timers.Count == 0)
		{
			return;
		}
		foreach (TimerTodo timer in timers)
		{
			timer.timer.ElapseSpeed = 0f - GameTime.Timer.ElapseSpeed;
			timer.timer.Update(Time.deltaTime);
			if (timer.timer.Second <= 0.0)
			{
				timer.act();
				recordRemove.Add(timer);
			}
		}
		foreach (TimerTodo item in recordRemove)
		{
			timers.Remove(item);
		}
	}

	public void PeTimeToDo(Action _func, double _second, int fixedID)
	{
		PETimer pETimer = new PETimer();
		pETimer.Second = _second;
		TimerTodo timerTodo = new TimerTodo();
		timerTodo.timer = pETimer;
		timerTodo.act = _func;
		timerTodo.fixedID = fixedID;
		timers.Add(timerTodo);
	}

	public void RemoveTimerByID(int fixedID)
	{
		TimerTodo timerTodo = timers.Find((TimerTodo e) => (e.fixedID == fixedID) ? true : false);
		if (timerTodo != null)
		{
			timers.Remove(timerTodo);
		}
	}

	private void UpdateMissionNeedTime()
	{
		if (m_PlayerMission.m_MissionTime.Count == 0)
		{
			return;
		}
		MissionCommonData missionCommonData = null;
		foreach (KeyValuePair<int, double> item in m_PlayerMission.m_MissionTime)
		{
			MissionCommonData missionCommonData2 = GetMissionCommonData(item.Key);
			if (missionCommonData2 == null || !(GameTime.Timer.Second - item.Value > (double)missionCommonData2.m_NeedTime))
			{
				continue;
			}
			missionCommonData = missionCommonData2;
			break;
		}
		if (missionCommonData != null)
		{
			m_PlayerMission.UpdateNpcMissionTex(PeSingleton<EntityMgr>.Instance.Get(missionCommonData.m_iReplyNpc));
			m_PlayerMission.m_MissionTime.Remove(missionCommonData.m_ID);
			if (missionCommonData.m_timeOverToPlot != 0)
			{
				StroyManager.Instance.PushStoryList(new List<int> { missionCommonData.m_timeOverToPlot });
			}
		}
	}

	public void UpdateMission()
	{
		Dictionary<int, Dictionary<string, string>> missionInfo = m_PlayerMission.m_MissionInfo;
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, Dictionary<string, string>> item in missionInfo)
		{
			list.Add(item.Key);
		}
		foreach (int item2 in list)
		{
			int num = item2;
			if (num == 8 || num == 678)
			{
				continue;
			}
			if (num == 497)
			{
				CSCreator creator = CSMain.GetCreator(0);
				if (creator == null || creator.Assembly == null)
				{
					continue;
				}
				Vector3 position = creator.Assembly.Position;
				MapMaskData mapMaskData = MapMaskData.s_tblMaskData.Find((MapMaskData ret) => ret.mId == 29);
				if (mapMaskData == null)
				{
					continue;
				}
				Vector3 mPosition = mapMaskData.mPosition;
				if (PERailwayCtrl.HasRoute(position, mPosition))
				{
					if (PeGameMgr.IsMulti)
					{
						Instance.RequestCompleteMission(497);
					}
					else
					{
						CompleteMission(497);
					}
					break;
				}
			}
			MissionCommonData missionCommonData = GetMissionCommonData(num);
			if (missionCommonData == null)
			{
				continue;
			}
			for (int i = 0; i < missionCommonData.m_TargetIDList.Count; i++)
			{
				TargetType targetType = MissionRepository.GetTargetType(missionCommonData.m_TargetIDList[i]);
				switch (targetType)
				{
				case TargetType.TargetType_TowerDif:
					if (UpdateTowerMission(num, missionCommonData.m_TargetIDList[i]))
					{
						return;
					}
					break;
				case TargetType.TargetType_Follow:
					if (UpdateFollowMission(num, missionCommonData.m_TargetIDList[i]))
					{
						return;
					}
					break;
				}
				if ((targetType == TargetType.TargetType_Discovery || missionCommonData.m_ID == 212) && UpdateSearchMission(num, missionCommonData.m_TargetIDList[i]))
				{
					return;
				}
			}
		}
	}

	public void SetTowerMissionStartTime(int targetID)
	{
		targetId_time.Add(targetID, Time.time);
	}

	public bool UpdateTowerMission(int MissionID, int TargetID)
	{
		TypeTowerDefendsData typeTowerDefendsData = GetTypeTowerDefendsData(TargetID);
		if (typeTowerDefendsData == null)
		{
			return false;
		}
		foreach (KeyValuePair<int, float> item in targetId_time)
		{
			if (TargetID == item.Key && Time.time - item.Value > (float)typeTowerDefendsData.m_tolTime)
			{
				targetId_time.Remove(TargetID);
				CompleteTarget(TargetID, MissionID, forceComplete: true);
			}
		}
		if (!PeGameMgr.IsMulti)
		{
			float num = Vector3.Distance(typeTowerDefendsData.finallyPos, StroyManager.Instance.GetPlayerPos());
			if (typeTowerDefendsData.m_range != 0 && num > (float)typeTowerDefendsData.m_range)
			{
				if (targetId_time.ContainsKey(TargetID))
				{
					targetId_time.Remove(TargetID);
				}
				FailureMission(MissionID);
				return true;
			}
		}
		for (int i = 0; i < typeTowerDefendsData.m_iNpcList.Count; i++)
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(typeTowerDefendsData.m_iNpcList[i]);
			if (!(peEntity == null) && peEntity.IsDead())
			{
				if (targetId_time.ContainsKey(TargetID))
				{
					targetId_time.Remove(TargetID);
				}
				if (PeGameMgr.IsMulti)
				{
					PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_MissionFailed, MissionID);
				}
				else
				{
					FailureMission(MissionID);
				}
				return true;
			}
		}
		foreach (int @object in typeTowerDefendsData.m_ObjectList)
		{
			PeEntity[] doodadEntities = PeSingleton<EntityMgr>.Instance.GetDoodadEntities(@object);
			if (doodadEntities == null || doodadEntities.Length == 0)
			{
				continue;
			}
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.GetDoodadEntities(@object)[0];
			if (peEntity == null || !peEntity.IsDead())
			{
				continue;
			}
			if (targetId_time.ContainsKey(TargetID))
			{
				targetId_time.Remove(TargetID);
			}
			if (PeGameMgr.IsMulti)
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_MissionFailed, MissionID);
			}
			else
			{
				FailureMission(MissionID);
			}
			return true;
		}
		if (SPAutomatic.IsSpawning())
		{
			return false;
		}
		MissionCommonData data = GetMissionCommonData(MissionID);
		for (int j = 0; j < data.m_TargetIDList.Count; j++)
		{
			UIMissionMgr.MissionView missionView = UIMissionMgr.Instance.GetMissionView(MissionID);
			UIMissionMgr.TargetShow targetShow = missionView.mTargetList.Find((UIMissionMgr.TargetShow ite) => UIMissionMgr.MissionView.MatchID(ite, data.m_TargetIDList[j]));
			if (targetShow != null && targetShow.mMaxCount <= targetShow.mCount)
			{
				if (PeGameMgr.IsMulti)
				{
					Instance.RequestCompleteMission(MissionID, -1, bChcek: false);
				}
				else
				{
					Instance.CompleteMission(MissionID, -1, bCheck: false);
				}
				if (UITowerInfo.Instance != null)
				{
					UITowerInfo.Instance.Hide();
				}
			}
		}
		SPTowerDefence towerDefence = SPAutomatic.GetTowerDefence(MissionID);
		if (towerDefence == null)
		{
			return false;
		}
		if (towerDefence.KilledCount < typeTowerDefendsData.m_Count)
		{
			return false;
		}
		CompleteTarget(typeTowerDefendsData.m_TargetID, MissionID);
		return true;
	}

	public bool UpdateFollowMission(int MissionID, int TargetID)
	{
		TypeFollowData typeFollowData = GetTypeFollowData(TargetID);
		if (typeFollowData == null)
		{
			return false;
		}
		if ((MissionID == 697 || MissionID == 714) && (bool)PeSingleton<MainPlayer>.Instance.entity.GetCmpt<Motion_Equip>() && PeSingleton<MainPlayer>.Instance.entity.GetCmpt<Motion_Equip>().IsWeaponActive() && Vector3.Distance(PeSingleton<MainPlayer>.Instance.entity.position, new Vector3(10973f, 226.9163f, 9259f)) <= 100f)
		{
			if (PeGameMgr.IsMulti)
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_MissionFailed, MissionID);
			}
			else
			{
				FailureMission(MissionID);
			}
			return true;
		}
		if (!followTarget_num.ContainsKey(TargetID))
		{
			followTarget_num.Add(TargetID, new int[typeFollowData.m_iNpcList.Count]);
		}
		UpdateNpcTalk(typeFollowData.m_TalkInfo);
		Vector3 pos = typeFollowData.m_DistPos;
		if (typeFollowData.m_LookNameID != 0)
		{
			pos = StroyManager.Instance.GetNpcPos(typeFollowData.m_LookNameID);
		}
		else if (typeFollowData.m_BuildID > 0)
		{
			PlayerMission.GetBuildingPos((MissionID != 9032) ? MissionID : 0, out pos);
			typeFollowData.m_DistRadius = 1;
		}
		for (int i = 0; i < typeFollowData.m_iFailNpc.Count; i++)
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(typeFollowData.m_iFailNpc[i]);
			if (!(peEntity == null) && peEntity.IsDeath())
			{
				if (PeGameMgr.IsMulti)
				{
					PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_MissionFailed, MissionID);
				}
				else
				{
					FailureMission(MissionID);
				}
				return true;
			}
		}
		if (timer < float.Epsilon)
		{
			timer = Time.time;
		}
		for (int j = 0; j < typeFollowData.m_iNpcList.Count; j++)
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(typeFollowData.m_iNpcList[j]);
			if (peEntity == null)
			{
				continue;
			}
			if (typeFollowData.m_EMode != 1 && typeFollowData.m_LookNameID != 0)
			{
				NpcCmpt npcCmpt = peEntity.NpcCmpt;
				if (npcCmpt != null && !npcCmpt.Req_Contains(EReqType.FollowPath) && Time.time - timer > 3f && Vector3.Distance(peEntity.position, pos) > 2f && (TargetID != 3081 || peEntity.Id != 9033))
				{
					Vector3 randomPosition = AiUtil.GetRandomPosition(pos, 0.5f, 1f);
					StroyManager.Instance.MoveTo(peEntity, randomPosition, typeFollowData.m_DistRadius, bForce: true, SpeedState.Run);
					npcCmpt.FixedPointPos = randomPosition;
					if (j == typeFollowData.m_iNpcList.Count - 1)
					{
						timer = Time.time;
					}
				}
			}
			if ((MissionID == 553 || MissionID == 700 || MissionID == 703 || MissionID == 704) && peEntity.IsOnCarrier())
			{
				if (PeGameMgr.IsMultiStory)
				{
					PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_MissionFailed, MissionID);
				}
				else
				{
					FailureMission(MissionID);
				}
				GameUI.Instance.mNPCTalk.NormalOrSP(0);
				List<int> list = new List<int>();
				list.Add(3011);
				if (!GameUI.Instance.mNPCTalk.isPlayingTalk)
				{
					GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(list);
					GameUI.Instance.mNPCTalk.PreShow();
				}
				else
				{
					GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(list, null, IsClearTalkList: false);
				}
				return false;
			}
			if (peEntity.IsDead())
			{
				peEntity.SetInvincible(value: true);
			}
			float num = Vector3.Distance(peEntity.ExtGetPos(), pos);
			if (typeFollowData.m_EMode != 0 && typeFollowData.m_EMode != 1)
			{
				float num2 = Vector3.Distance(peEntity.ExtGetPos(), StroyManager.Instance.GetPlayerPos());
				float num3 = Vector3.Distance(StroyManager.Instance.GetPlayerPos(), pos);
				if (typeFollowData.m_EMode == 2)
				{
					if (num2 > (float)typeFollowData.m_WaitDist[0])
					{
						if (!npcWaitTime.ContainsKey(peEntity.Id))
						{
							List<object> list2 = new List<object>();
							if (num3 < num)
							{
								list2.Add(false);
							}
							else
							{
								list2.Add(true);
							}
							list2.Add(Time.time);
							npcWaitTime.Add(peEntity.Id, list2);
						}
						peEntity.NpcCmpt.MisstionAskStop = true;
					}
				}
				else if (typeFollowData.m_EMode == 3 && num3 < num && num2 > (float)typeFollowData.m_WaitDist[0])
				{
					if (!npcWaitTime.ContainsKey(peEntity.Id))
					{
						List<object> list3 = new List<object>();
						list3.Add(false);
						list3.Add(Time.time);
						npcWaitTime.Add(peEntity.Id, list3);
					}
					peEntity.NpcCmpt.MisstionAskStop = true;
				}
				else if (typeFollowData.m_EMode == 4 && num3 > num && num2 > (float)typeFollowData.m_WaitDist[0])
				{
					if (!npcWaitTime.ContainsKey(peEntity.Id))
					{
						List<object> list4 = new List<object>();
						list4.Add(true);
						list4.Add(Time.time);
						npcWaitTime.Add(peEntity.Id, list4);
					}
					peEntity.NpcCmpt.MisstionAskStop = true;
				}
				if (num2 < (float)typeFollowData.m_WaitDist[1])
				{
					if (npcWaitTime.ContainsKey(peEntity.Id))
					{
						npcWaitTime.Remove(peEntity.Id);
					}
					peEntity.NpcCmpt.MisstionAskStop = false;
				}
			}
			if (npcWaitTime.ContainsKey(peEntity.Id) && Time.time - (float)npcWaitTime[peEntity.Id][1] > 7f)
			{
				npcWaitTime[peEntity.Id][1] = Time.time;
				if (!typeFollowData.npcid_behindTalk_forwardTalk.ContainsKey(peEntity.Id))
				{
					continue;
				}
				if (!(bool)npcWaitTime[peEntity.Id][0])
				{
					GameUI.Instance.mNPCTalk.SpTalkSymbol(spOrHalf: false);
					UINPCTalk.NpcTalkInfo talkinfo = default(UINPCTalk.NpcTalkInfo);
					GameUI.Instance.mNPCTalk.GetTalkInfo(typeFollowData.npcid_behindTalk_forwardTalk[peEntity.Id][0], ref talkinfo, null);
					GameUI.Instance.mNPCTalk.ParseName(null, ref talkinfo);
					GameUI.Instance.mNPCTalk.m_NpcTalkList.Add(talkinfo);
					if (!GameUI.Instance.mNPCTalk.isPlayingTalk)
					{
						GameUI.Instance.mNPCTalk.SPTalkClose();
					}
				}
				else
				{
					GameUI.Instance.mNPCTalk.SpTalkSymbol(spOrHalf: false);
					UINPCTalk.NpcTalkInfo talkinfo2 = default(UINPCTalk.NpcTalkInfo);
					GameUI.Instance.mNPCTalk.GetTalkInfo(typeFollowData.npcid_behindTalk_forwardTalk[peEntity.Id][1], ref talkinfo2, null);
					GameUI.Instance.mNPCTalk.ParseName(null, ref talkinfo2);
					GameUI.Instance.mNPCTalk.m_NpcTalkList.Add(talkinfo2);
					if (!GameUI.Instance.mNPCTalk.isPlayingTalk)
					{
						GameUI.Instance.mNPCTalk.SPTalkClose();
					}
				}
			}
			if (HasRandomMission(MissionID))
			{
				Vector2 vector = new Vector2(peEntity.ExtGetPos().x, peEntity.ExtGetPos().z);
				num = Vector3.Distance(b: new Vector2(pos.x, pos.z), a: vector);
			}
			if (!(num <= (float)typeFollowData.m_DistRadius))
			{
				continue;
			}
			if (PeGameMgr.IsMultiStory)
			{
				if (PlayerNetwork.mainPlayer._curSceneId == typeFollowData.m_SceneType && j < followTarget_num[TargetID].Length && j >= 0)
				{
					peEntity.NpcCmpt.MisstionAskStop = false;
					followTarget_num[TargetID][j] = 1;
				}
			}
			else if (typeFollowData.m_SceneType == (int)SingleGameStory.curType && j < followTarget_num[TargetID].Length && j >= 0)
			{
				peEntity.NpcCmpt.MisstionAskStop = false;
				followTarget_num[TargetID][j] = 1;
			}
			if (npcWaitTime.ContainsKey(peEntity.Id))
			{
				npcWaitTime.Remove(peEntity.Id);
			}
		}
		if (!Array.Exists(followTarget_num[TargetID], (int ite) => ite != 1))
		{
			if (typeFollowData.m_isNeedPlayer && Vector3.Distance(StroyManager.Instance.GetPlayerPos(), pos) > (float)typeFollowData.m_DistRadius)
			{
				return false;
			}
			CompleteTarget(TargetID, MissionID);
			if (followTarget_num.ContainsKey(TargetID))
			{
				followTarget_num.Remove(TargetID);
			}
			return true;
		}
		return false;
	}

	public bool UpdateSearchMission(int MissionID, int TargetID)
	{
		TypeSearchData typeSearchData = GetTypeSearchData(TargetID);
		if (typeSearchData == null)
		{
			return false;
		}
		if (PeGameMgr.IsAdventure && typeSearchData.m_notForDungeon && PeSingleton<PeCreature>.Instance.mainPlayer.position.y < 0f)
		{
			return false;
		}
		if (MissionID == 497)
		{
			if (RailWayMissionIsCompleted(typeSearchData) && !HadCompleteTarget(TargetID))
			{
				if (PeGameMgr.IsMultiStory)
				{
					if (typeSearchData.m_SceneType == PlayerNetwork.mainPlayer._curSceneId)
					{
						CompleteTarget(TargetID, MissionID);
						m_PlayerMission.UpdateAllNpcMisTex();
						return true;
					}
				}
				else if (typeSearchData.m_SceneType == (int)SingleGameStory.curType)
				{
					CompleteTarget(TargetID, MissionID);
					m_PlayerMission.UpdateAllNpcMisTex();
					return true;
				}
			}
			return false;
		}
		PeEntity mainPlayer = PeSingleton<PeCreature>.Instance.mainPlayer;
		if ((MissionID == 825 || MissionID == 826) && mainPlayer.IsOnCarrier())
		{
			FailureMission(MissionID);
			return false;
		}
		float num = 0f;
		if (typeSearchData.m_NpcID != 0)
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(typeSearchData.m_NpcID);
			if (peEntity == null)
			{
				return false;
			}
			num = Vector3.Distance(peEntity.position, StroyManager.Instance.GetPlayerPos());
			if (HasRandomMission(MissionID))
			{
				Vector2 a = new Vector2(peEntity.ExtGetPos().x, peEntity.ExtGetPos().z);
				Vector2 b = new Vector2(StroyManager.Instance.GetPlayerPos().x, StroyManager.Instance.GetPlayerPos().z);
				num = Vector2.Distance(a, b);
			}
		}
		else
		{
			num = Vector3.Distance(StroyManager.Instance.GetPlayerPos(), typeSearchData.m_DistPos);
			if (HasRandomMission(MissionID))
			{
				Vector2 a2 = new Vector2(typeSearchData.m_DistPos.x, typeSearchData.m_DistPos.z);
				Vector2 b2 = new Vector2(StroyManager.Instance.GetPlayerPos().x, StroyManager.Instance.GetPlayerPos().z);
				num = Vector2.Distance(a2, b2);
			}
		}
		if (num < (float)typeSearchData.m_DistRadius && !HadCompleteTarget(TargetID))
		{
			if (PeGameMgr.IsMultiStory)
			{
				if (typeSearchData.m_SceneType == PlayerNetwork.mainPlayer._curSceneId)
				{
					CompleteTarget(TargetID, MissionID);
					m_PlayerMission.UpdateAllNpcMisTex();
					return true;
				}
			}
			else if (typeSearchData.m_SceneType == (int)SingleGameStory.curType)
			{
				CompleteTarget(TargetID, MissionID);
				m_PlayerMission.UpdateAllNpcMisTex();
				return true;
			}
		}
		return false;
	}

	private bool RailWayMissionIsCompleted(TypeSearchData searchData)
	{
		if (!CSMain.HasCSAssembly())
		{
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		foreach (Route route in PeSingleton<Manager>.Instance.GetRoutes())
		{
			for (int i = 0; i < route.pointCount; i++)
			{
				Point pointByIndex = route.GetPointByIndex(i);
				if (pointByIndex.pointType != 0)
				{
					CSMain.GetAssemblyPos(out var pos);
					if (Vector3.Distance(pointByIndex.position, pos) < (float)searchData.m_DistRadius)
					{
						flag = true;
					}
					if (Vector3.Distance(pointByIndex.position, searchData.m_DistPos) < (float)searchData.m_DistRadius)
					{
						flag2 = true;
					}
				}
			}
			if (flag && flag2 && route.train != null)
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateNpcTalk(List<TalkInfo> talkInfos)
	{
		foreach (TalkInfo talkInfo in talkInfos)
		{
			if (!iHadTalkedMap.Contains(talkInfo.talkid[0]) && Vector3.Distance(PeSingleton<PeCreature>.Instance.mainPlayer.position, talkInfo.pos) <= (float)talkInfo.radius)
			{
				GameUI.Instance.mNPCTalk.SpTalkSymbol(spOrHalf: true);
				GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(talkInfo.talkid, null, IsClearTalkList: false);
				if (!GameUI.Instance.mNPCTalk.isPlayingTalk)
				{
					GameUI.Instance.mNPCTalk.SPTalkClose();
				}
				if (!iHadTalkedMap.Contains(talkInfo.talkid[0]))
				{
					iHadTalkedMap.Add(talkInfo.talkid[0]);
				}
			}
		}
	}

	public static void RPC_S2C_SetMonsterLeft(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool flag = false;
		int missionID = stream.Read<int>(new object[0]);
		stream.Read<int>(new object[0]);
		MissionCommonData missionCommonData = GetMissionCommonData(missionID);
		if (missionCommonData != null && !flag)
		{
		}
	}
}
