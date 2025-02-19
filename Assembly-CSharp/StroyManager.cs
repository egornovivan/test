using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using AiAsset;
using CustomData;
using ItemAsset;
using Pathea;
using Pathea.Effect;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtMotion_Move;
using Pathea.PeEntityExtNpcPackage;
using Pathea.PeEntityExtPlayerPackage;
using Pathea.PeEntityExtTrans;
using PeMap;
using PETools;
using PeUIMap;
using Railway;
using SkillSystem;
using UnityEngine;
using WhiteCat;
using WhiteCat.UnityExtension;

public class StroyManager : MonoBehaviour
{
	public struct PathInfo
	{
		public Vector3 pos;

		public bool isFinish;
	}

	public class RandScenarios
	{
		public int id;

		public float curTime;

		public List<int> scenarioIds;

		public float cd;
	}

	public class CurPathInfo
	{
		public int idx;

		public Vector3 curPos;

		public CurPathInfo()
		{
			curPos = Vector3.zero;
		}
	}

	public const int SpeMissionID = -10000;

	private Dictionary<Vector2, List<Vector3>> m_CreatedNpcList;

	public static Dictionary<int, PassengerInfo> m_Passengers = new Dictionary<int, PassengerInfo>();

	public Dictionary<Vector3, TentScript> m_TentList = new Dictionary<Vector3, TentScript>();

	public Dictionary<int, float> m_MisDelay = new Dictionary<int, float>();

	public Dictionary<int, bool> m_StoryList;

	public List<int> m_AdStoryList;

	public Dictionary<int, int> m_AdStoryListNpc = new Dictionary<int, int>();

	public List<string> m_DriveNpc = new List<string>();

	public List<int> m_iDriveNpc = new List<int>();

	private int m_CurMissionID;

	private Dictionary<int, CurPathInfo> m_iCurPathMap;

	private int m_CurPathIdx;

	public bool npcLoaded;

	public int maxCount;

	public static int m_SpeAdNpcId = -1;

	private Transform mFollowCameraTarget;

	public CamMode mCamMode;

	private bool m_bCamModing;

	private bool m_bCamMoveModing;

	private bool m_bCamRotModing;

	private int m_CamIdx;

	private List<int> delayPlotID = new List<int>();

	public List<int> m_NeedCampTalk = new List<int>();

	private PeTrans m_PlayerTrans;

	private List<Bounds> colliderBoundList;

	private static StroyManager mInstance;

	public Dictionary<int, stShopInfo> m_BuyInfo = new Dictionary<int, stShopInfo>();

	public Dictionary<int, List<ItemObject>> m_SellInfo = new Dictionary<int, List<ItemObject>>();

	public Dictionary<int, KillMons> m_RecordKillMons = new Dictionary<int, KillMons>();

	public List<MoveMons> m_RecordMoveMons = new List<MoveMons>();

	public Dictionary<int, Dictionary<PeEntity, bool>> m_RecordIsReachPoitn = new Dictionary<int, Dictionary<PeEntity, bool>>();

	public static int PRICE_ID = 229;

	private int mMoney;

	public static bool isPausing = false;

	private static Dictionary<int, RandScenarios> randScenarios = new Dictionary<int, RandScenarios>();

	private Material hidingMat;

	private Material[] record;

	private List<AdEnterArea> checkEnterArea = new List<AdEnterArea>();

	private Queue<SkinnedMeshRenderer> changingAdd = new Queue<SkinnedMeshRenderer>();

	private Queue<Material> changingMinus = new Queue<Material>();

	private bool recordPatrol;

	private List<ChangePartrolmode> patrolModeRecord = new List<ChangePartrolmode>();

	private static UnityEngine.Object samplePosObj;

	private static UnityEngine.Object sampleObj;

	private GameObject mcTalk;

	public static List<string> deadNpcsName = new List<string>();

	public static List<ItemDrop> languages = new List<ItemDrop>();

	public bool enableBook;

	public bool moveVploas;

	private bool returnZjComplete;

	public PeTrans PlayerTrans => m_PlayerTrans;

	public List<Bounds> ColliderBoundList => colliderBoundList;

	public static StroyManager Instance => mInstance;

	public int m_Money
	{
		get
		{
			if (PeGameMgr.IsMultiAdventure)
			{
				return mMoney;
			}
			PackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PackageCmpt>();
			return cmpt.money.current;
		}
	}

	private Material HidingMat
	{
		get
		{
			if (hidingMat == null)
			{
				hidingMat = Resources.Load("Shaders/HidingWaveMat", typeof(Material)) as Material;
			}
			return hidingMat;
		}
	}

	private static UnityEngine.Object SamplePosObj
	{
		get
		{
			if (samplePosObj == null)
			{
				samplePosObj = Resources.Load("Prefab/Item/Other/TextSamplePos");
			}
			return samplePosObj;
		}
	}

	private static UnityEngine.Object SampleObj
	{
		get
		{
			if (sampleObj == null)
			{
				sampleObj = Resources.Load("Prefab/Item/Other/language_sample_canUse");
			}
			return sampleObj;
		}
	}

	private GameObject Mctalk
	{
		get
		{
			if (mcTalk == null)
			{
				mcTalk = GameObject.Find("McTalk");
			}
			return mcTalk;
		}
	}

	private void Start()
	{
		StartCoroutine(WaitNpcRail(m_Passengers));
	}

	private IEnumerator WaitNpcRail(Dictionary<int, PassengerInfo> tmp)
	{
		while (Instance == null || PeSingleton<Manager>.Instance == null)
		{
			yield return new WaitForSeconds(0.1f);
		}
		foreach (PassengerInfo item in tmp.Values)
		{
			if (item == null)
			{
				continue;
			}
			Route route = PeSingleton<Manager>.Instance.GetRoute(item.startRouteID);
			if (route == null)
			{
				continue;
			}
			Point start = route.GetPointByIndex(item.startIndexID);
			if (start == null)
			{
				continue;
			}
			route = PeSingleton<Manager>.Instance.GetRoute(item.endRouteID);
			if (route == null)
			{
				continue;
			}
			Point end = route.GetPointByIndex(item.endIndexID);
			if (end != null)
			{
				if (item.type == PassengerInfo.Course.before)
				{
					Instance.StartCoroutine(WaitingNpcRailStart(start, PeSingleton<EntityMgr>.Instance.Get(item.npcID), end, item.dest));
				}
				else if (item.type == PassengerInfo.Course.on)
				{
					Instance.StartCoroutine(WaitingNpcRailEnd(end, PeSingleton<EntityMgr>.Instance.Get(item.npcID), item.dest));
				}
			}
		}
	}

	private void Awake()
	{
		mInstance = this;
		m_CreatedNpcList = new Dictionary<Vector2, List<Vector3>>();
		m_StoryList = new Dictionary<int, bool>();
		m_AdStoryList = new List<int>();
		m_iCurPathMap = new Dictionary<int, CurPathInfo>();
		colliderBoundList = new List<Bounds>();
		GameObject gameObject = new GameObject("mission camera follow");
		gameObject.transform.parent = base.transform;
		mFollowCameraTarget = gameObject.transform;
		if (GameConfig.IsMultiMode)
		{
			mInstance = this;
		}
	}

	public void InitBuyInfo(StoreData npc, int npcid)
	{
		m_BuyInfo[npcid] = new stShopInfo();
		List<int> itemList = npc.itemList;
		for (int i = 0; i < itemList.Count; i++)
		{
			ShopData shopData = ShopRespository.GetShopData(itemList[i]);
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.CreateItem(shopData.m_ItemID);
			itemObject.stackCount = shopData.m_LimitNum;
			stShopData value = new stShopData(itemObject.instanceId, GameTime.Timer.Second);
			m_BuyInfo[npcid].ShopList[shopData.m_ID] = value;
		}
	}

	public bool BuyItem(ItemObject itemObj, int num, int shopID, int npcId, bool bReduce)
	{
		ShopData shopData = null;
		int num2;
		if (bReduce)
		{
			shopData = ShopRespository.GetShopData(shopID);
			if (shopData == null)
			{
				PeTipMsg.Register(PELocalization.GetString(8000496), PeTipMsg.EMsgLevel.Warning);
				return false;
			}
			num2 = shopData.m_Price;
		}
		else
		{
			num2 = itemObj.GetSellPrice();
		}
		if (m_Money < num2 * num)
		{
			PeTipMsg.Register(PELocalization.GetString((!Money.Digital) ? 8000073 : 8000092), PeTipMsg.EMsgLevel.Warning);
			return false;
		}
		if (itemObj != null)
		{
			ItemObject itemObject = null;
			if (itemObj.protoData.maxStackNum == 1)
			{
				int num3 = num;
				for (int i = 0; i < num3; i++)
				{
					num = 1;
					if (num < itemObj.GetCount())
					{
						itemObject = PeSingleton<ItemMgr>.Instance.CreateItem(itemObj.protoId);
						itemObject.IncreaseStackCount(num - 1);
					}
					else
					{
						itemObject = itemObj;
						if (!m_BuyInfo.ContainsKey(npcId))
						{
							PeTipMsg.Register(PELocalization.GetString(8000496), PeTipMsg.EMsgLevel.Warning);
							return false;
						}
						if (m_BuyInfo[npcId].ShopList.ContainsKey(shopID))
						{
							m_BuyInfo[npcId].ShopList[shopID].ItemObjID = 0;
							m_BuyInfo[npcId].ShopList[shopID].CreateTime = GameTime.Timer.Second;
						}
					}
					PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
					if (null == cmpt)
					{
						PeTipMsg.Register(PELocalization.GetString(8000496), PeTipMsg.EMsgLevel.Warning);
						return false;
					}
					if (ItemPackage.InvalidIndex != cmpt.package.AddItem(itemObject))
					{
						ReduceMoney(num2 * num);
						GameUI.Instance.mItemPackageCtrl.ResetItem();
						continue;
					}
					PeTipMsg.Register(PELocalization.GetString(9500312), PeTipMsg.EMsgLevel.Warning);
					return false;
				}
			}
			else
			{
				if (num < itemObj.GetCount())
				{
					itemObject = PeSingleton<ItemMgr>.Instance.CreateItem(itemObj.protoId);
					itemObject.IncreaseStackCount(num - 1);
				}
				else
				{
					itemObject = itemObj;
					if (!m_BuyInfo.ContainsKey(npcId))
					{
						PeTipMsg.Register(PELocalization.GetString(8000496), PeTipMsg.EMsgLevel.Warning);
						return false;
					}
					if (m_BuyInfo[npcId].ShopList.ContainsKey(shopID))
					{
						m_BuyInfo[npcId].ShopList[shopID].ItemObjID = 0;
						m_BuyInfo[npcId].ShopList[shopID].CreateTime = GameTime.Timer.Second;
					}
				}
				PlayerPackageCmpt cmpt2 = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
				if (null == cmpt2)
				{
					PeTipMsg.Register(PELocalization.GetString(8000496), PeTipMsg.EMsgLevel.Warning);
					return false;
				}
				if (!cmpt2.package.CanAdd(itemObject.protoId, itemObject.GetCount()))
				{
					PeTipMsg.Register(PELocalization.GetString(9500312), PeTipMsg.EMsgLevel.Warning);
					return false;
				}
				cmpt2.package.Add(itemObject.protoId, itemObject.GetCount());
				ReduceMoney(num2 * num);
				GameUI.Instance.mItemPackageCtrl.ResetItem();
			}
		}
		return true;
	}

	public bool AddMoney(int num)
	{
		if (PeGameMgr.IsMultiAdventure)
		{
			mMoney += num;
			return true;
		}
		return PeSingleton<PeCreature>.Instance.mainPlayer.AddToPkg(PRICE_ID, num);
	}

	public void ReduceMoney(int num)
	{
		if (PeGameMgr.IsMultiAdventure)
		{
			mMoney -= num;
			return;
		}
		PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
		if (cmpt != null)
		{
			cmpt.money.current -= num;
		}
	}

	public void InitMission(int npcId = -1)
	{
		foreach (KeyValuePair<int, MissionInit> item4 in MisInitRepository.m_MisInitMap)
		{
			MissionInit value = item4.Value;
			if (value == null || (PeGameMgr.IsMulti && value.m_NpcAct.Count > 0 && npcId == -1))
			{
				continue;
			}
			bool flag = false;
			for (int i = 0; i < value.m_NpcAct.Count; i++)
			{
				if (npcId == value.m_NpcAct[i].npcid && npcId != -1)
				{
					flag = true;
					break;
				}
			}
			if (!flag && npcId != -1)
			{
				continue;
			}
			bool flag2 = true;
			for (int j = 0; j < value.m_ComMisID.Count; j++)
			{
				if (value.m_ComMisID[j] > 999 && value.m_ComMisID[j] < 10000)
				{
					if (!MissionManager.Instance.HadCompleteTarget(value.m_ComMisID[j]))
					{
						flag2 = false;
						break;
					}
				}
				else if (value.m_ComMisID[j] > 0 && !MissionManager.Instance.HadCompleteMission(value.m_ComMisID[j]))
				{
					flag2 = false;
					break;
				}
			}
			if (!flag2)
			{
				continue;
			}
			if (value.m_NComMisID > 999 && value.m_NComMisID < 10000)
			{
				if (MissionManager.Instance.HadCompleteTarget(value.m_NComMisID))
				{
					continue;
				}
			}
			else if (value.m_NComMisID > 0 && MissionManager.Instance.HadCompleteMission(value.m_NComMisID))
			{
				continue;
			}
			PeEntity npc;
			for (int k = 0; k < value.m_NpcAct.Count; k++)
			{
				NpcAct npcAct = value.m_NpcAct[k];
				npc = PeSingleton<EntityMgr>.Instance.Get(npcAct.npcid);
				if (npc == null)
				{
					continue;
				}
				if (npcAct.animation == "SpiderWeb")
				{
					npc.TrapInSpiderWeb(npcAct.btrue, 1f);
				}
				else if (npcAct.animation == "AddItem")
				{
					AddNpcItem(npc.Id, 90002421);
				}
				else if (npcAct.animation == "CarryUp")
				{
					CarryUp(npc, 9008, bCarryUp: true);
				}
				else if (npcAct.animation == "PutDown")
				{
					PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(9008);
					MotionMgrCmpt cmpt = peEntity.GetCmpt<MotionMgrCmpt>();
					if (cmpt == null)
					{
						return;
					}
					cmpt.FreezePhyState(GetType(), v: true);
					SetIdle(peEntity, "InjuredRest");
				}
				else if (npcAct.animation == "Cure")
				{
					RemoveReq(npc, EReqType.Idle);
					MotionMgrCmpt cmpt2 = npc.GetCmpt<MotionMgrCmpt>();
					if (cmpt2 == null)
					{
						return;
					}
					cmpt2.FreezePhyState(GetType(), v: false);
				}
				else if (npcAct.animation == "InjuredSit")
				{
					if (npc.Id != 9008 || npc.NpcCmpt.Req_GetRequest(EReqType.Idle) == null || !((npc.NpcCmpt.Req_GetRequest(EReqType.Idle) as RQIdle).state == "BeCarry"))
					{
						SetIdle(npc, npcAct.animation);
					}
				}
				else if (npcAct.animation == "Lie")
				{
					SetIdle(npc, npcAct.animation);
				}
				else if (npcAct.animation == "npcidle")
				{
					SetIdle(npc, "Idle");
				}
				else if (npcAct.animation == "npcdidle")
				{
					RemoveReq(npc, EReqType.Idle);
				}
				else if (npcAct.animation != "InjuredLevel")
				{
					npc.CmdPlayAnimation(npcAct.animation, npcAct.btrue);
				}
			}
			if (npcId != -1)
			{
				break;
			}
			if (value.m_triggerPlot.Count > 0)
			{
				Instance.PushStoryList(value.m_triggerPlot);
			}
			for (int l = 0; l < value.m_NpcFace.Count; l++)
			{
				NpcFace npcFace = value.m_NpcFace[l];
				npc = PeSingleton<EntityMgr>.Instance.Get(npcFace.npcid);
				if (npc == null)
				{
					continue;
				}
				if (npcFace.angle == -2)
				{
					npc.CmdFaceToPoint(PeSingleton<PeCreature>.Instance.mainPlayer.ExtGetPos());
				}
				else if (npcFace.angle != -1)
				{
					SetRotation(npc, Quaternion.AngleAxis(npcFace.angle, Vector3.up));
				}
				else
				{
					PeEntity peEntity2 = PeSingleton<EntityMgr>.Instance.Get(npcFace.otherid);
					if (peEntity2 != null)
					{
						npc.CmdFaceToPoint(peEntity2.ExtGetPos());
					}
				}
				if (!npcFace.bmove)
				{
					npc.CmdStartIdle();
				}
			}
			for (int m = 0; m < value.m_NpcCamp.Count; m++)
			{
				NpcCamp npcCamp = value.m_NpcCamp[m];
				npc = PeSingleton<EntityMgr>.Instance.Get(npcCamp.npcid);
				if (!(npc == null))
				{
					npc.SetCamp(npcCamp.camp);
				}
			}
			for (int num = 0; num < value.m_DeleteMonster.Count; num++)
			{
				PeSingleton<SceneEntityCreatorArchiver>.Instance.SetFixedSpawnPointActive(value.m_DeleteMonster[num], active: false);
			}
			for (int num2 = 0; num2 < value.m_MonsterCampList.Count; num2++)
			{
				AiDataBlock.SetCamp(value.m_MonsterCampList[num2].id, value.m_MonsterCampList[num2].value);
			}
			for (int num3 = 0; num3 < value.m_MonsterHarmList.Count; num3++)
			{
				AiDataBlock.SetHarm(value.m_MonsterHarmList[num3].id, value.m_MonsterHarmList[num3].value);
			}
			for (int num4 = 0; num4 < value.m_iDeleteNpc.Count; num4++)
			{
				npc = PeSingleton<EntityMgr>.Instance.Get(value.m_iDeleteNpc[num4]);
				if (npc == null)
				{
				}
			}
			for (int num5 = 0; num5 < value.m_TransNpc.Count; num5++)
			{
				NpcStyle npcStyle = value.m_TransNpc[num5];
				if (npcStyle == null)
				{
					continue;
				}
				npc = PeSingleton<EntityMgr>.Instance.Get(npcStyle.npcid);
				if (!(npc == null))
				{
					Instance.Translate(npc, npcStyle.pos);
					NpcCmpt npcCmpt = npc.NpcCmpt;
					if (null != npcCmpt)
					{
						npcCmpt.FixedPointPos = npcStyle.pos;
					}
					else
					{
						Debug.LogError("Failed to set fixed point.");
					}
				}
			}
			for (int num6 = 0; num6 < value.m_FollowPlayerList.Count; num6++)
			{
				NpcOpen npcOpen = value.m_FollowPlayerList[num6];
				npc = PeSingleton<EntityMgr>.Instance.Get(npcOpen.npcid);
				if (npc == null)
				{
					continue;
				}
				NpcCmpt component = npc.GetComponent<NpcCmpt>();
				if (component == null)
				{
					continue;
				}
				if (npcOpen.bopen)
				{
					if (PeGameMgr.IsSingle)
					{
						ServantLeaderCmpt cmpt3 = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>();
						cmpt3.AddForcedServant(component, isMove: true);
					}
				}
				else
				{
					ServantLeaderCmpt cmpt4 = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>();
					cmpt4.RemoveForcedServant(component);
				}
			}
			for (int num7 = 0; num7 < value.m_iColonyNoOrderNpcList.Count; num7++)
			{
				npc = PeSingleton<EntityMgr>.Instance.Get(value.m_iColonyNoOrderNpcList[num7]);
				if (!(npc == null))
				{
					npc.NpcCmpt.FixedPointPos = npc.position;
					npc.NpcCmpt.BaseNpcOutMission = true;
				}
			}
			ProcessSpecial(value.m_Special);
			if (value.m_plotMissionTrigger.Count > 0)
			{
				StartCoroutine(WaitPlotMissionTrigger(value.m_plotMissionTrigger));
			}
			int num8 = value.m_monsterHatredList.Count / 5;
			List<int> list = new List<int>();
			for (int num9 = 0; num9 < num8; num9++)
			{
				list = value.m_monsterHatredList.GetRange(5 * num9, 5);
				SpecialHatred.MonsterHatredAdd(list);
			}
			num8 = value.m_npcHatredList.Count / 4;
			for (int num10 = 0; num10 < num8; num10++)
			{
				list = value.m_npcHatredList.GetRange(4 * num10, 4);
				SpecialHatred.NpcHatredAdd(list);
			}
			num8 = value.m_harmList.Count / 3;
			for (int num11 = 0; num11 < num8; num11++)
			{
				list = value.m_harmList.GetRange(3 * num11, 3);
				SpecialHatred.HarmAdd(list);
			}
			num8 = value.m_doodadHarmList.Count / 3;
			for (int num12 = 0; num12 < num8; num12++)
			{
				list = value.m_doodadHarmList.GetRange(3 * num12, 3);
				PeEntity[] array = ((list[1] == 0) ? PeSingleton<EntityMgr>.Instance.GetDoodadEntitiesByProtoId(list[2]) : PeSingleton<EntityMgr>.Instance.GetDoodadEntities(list[1]));
				for (int num13 = 0; num13 < array.Length; num13++)
				{
					if (!(array[num13].GetCmpt<SceneDoodadLodCmpt>() == null))
					{
						if (list[0] == 0)
						{
							array[num13].GetCmpt<SceneDoodadLodCmpt>().IsDamagable = true;
						}
						else
						{
							array[num13].GetCmpt<SceneDoodadLodCmpt>().IsDamagable = false;
						}
					}
				}
			}
			SetDoodadEffect item;
			foreach (SetDoodadEffect doodadEffect in value.m_doodadEffectList)
			{
				item = doodadEffect;
				PeEntity[] doodadEntities = PeSingleton<EntityMgr>.Instance.GetDoodadEntities(item.id);
				if (doodadEntities == null || doodadEntities.Length == 0)
				{
					break;
				}
				doodadEntities[0].transform.TraverseHierarchy(delegate(Transform trans, int d)
				{
					if (item.names.Contains(trans.gameObject.name))
					{
						trans.gameObject.SetActive(item.openOrClose);
					}
				});
			}
			for (int num14 = 0; num14 < value.cantReviveNpc.Count; num14++)
			{
				npc = PeSingleton<EntityMgr>.Instance.Get(value.cantReviveNpc[num14]);
				if (npc != null && npc.NpcCmpt != null)
				{
					npc.NpcCmpt.ReviveTime = -1;
				}
			}
			foreach (ENpcBattleInfo item5 in value.m_npcsBattle)
			{
				ENpcBattle battle = (ENpcBattle)(item5.type - 1);
				foreach (int item6 in item5.npcId)
				{
					npc = PeSingleton<EntityMgr>.Instance.Get(item6);
					if (!(npc == null))
					{
						npc.GetComponent<NpcCmpt>().Battle = battle;
					}
				}
			}
			List<PeEntity> list2 = new List<PeEntity>(PeSingleton<EntityMgr>.Instance.All);
			KillMons item2;
			foreach (KillMons killMon in value.m_killMons)
			{
				item2 = killMon;
				if (m_RecordKillMons.Count == 0)
				{
					MonsterEntityCreator.commonCreateEvent += KillMonster;
				}
				if (!m_RecordKillMons.ContainsKey(item2.id))
				{
					m_RecordKillMons.Add(item2.id, item2);
				}
				if (item2.type == KillMons.Type.protoTypeId)
				{
					list2 = list2.FindAll(delegate(PeEntity mon)
					{
						if (mon == null)
						{
							return false;
						}
						return (mon.proto == EEntityProto.Monster && Vector3.Distance(mon.position, item2.center) <= item2.radius && (item2.monId == -999 || mon.entityProto.protoId == item2.monId)) ? true : false;
					});
				}
				else if (item2.type == KillMons.Type.fixedId)
				{
					PeEntity entityByFixedSpId = PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(item2.monId);
					if (entityByFixedSpId != null)
					{
						list2.Add(entityByFixedSpId);
					}
				}
				foreach (PeEntity item7 in list2)
				{
					if (item7 != null && item7.GetComponent<PESkEntity>() != null)
					{
						item7.GetComponent<PESkEntity>().SetAttribute(AttribType.Hp, 0f, eventOff: false);
					}
				}
			}
			if (value.m_npcType.Count > 0)
			{
				foreach (NpcType item3 in value.m_npcType)
				{
					item3.npcs.ForEach(delegate(int n)
					{
						npc = PeSingleton<EntityMgr>.Instance.Get(n);
						NpcCmpt nc;
						if (npc != null && (bool)(nc = npc.GetComponent<NpcCmpt>()))
						{
							nc.NpcControlCmdId = item3.type;
						}
					});
				}
			}
			num8 = value.m_killNpcList.Count / 3;
			list = new List<int>();
			List<PeEntity> list3 = new List<PeEntity>();
			for (int num15 = 0; num15 < num8; num15++)
			{
				list = value.m_killNpcList.GetRange(3 * num15, 3);
				list3 = KillNPC.NPCBeKilled(list[0]);
			}
		}
	}

	public static void ClearRecord()
	{
		isPausing = false;
		randScenarios.Clear();
	}

	private void AddChangingMaterial(SkinnedMeshRenderer tmp)
	{
		if (!(tmp == null))
		{
			changingAdd.Enqueue(tmp);
		}
	}

	private void AddChangingMaterial(Material[] tmp)
	{
		foreach (Material material in tmp)
		{
			if (!(material == null))
			{
				changingMinus.Enqueue(material);
			}
		}
	}

	private void UpdateChangingMaterial()
	{
		if (changingAdd != null && changingAdd.Count > 0)
		{
			foreach (SkinnedMeshRenderer item in changingAdd)
			{
				if (item == null || item.materials == null)
				{
					continue;
				}
				for (int i = 0; i < item.materials.Length; i++)
				{
					if (!(item.materials[i] == null))
					{
						float @float = item.materials[i].GetFloat("_shaderChange");
						item.materials[i].SetFloat("_shaderChange", Mathf.Clamp(@float + 0.005f, 0f, 1f));
					}
				}
			}
			SkinnedMeshRenderer skinnedMeshRenderer = changingAdd.Peek();
			if (skinnedMeshRenderer != null && skinnedMeshRenderer.materials != null && record != null && skinnedMeshRenderer.materials[0].GetFloat("_shaderChange") >= 1f)
			{
				SkinnedMeshRenderer skinnedMeshRenderer2 = changingAdd.Dequeue();
				if (skinnedMeshRenderer2 != null)
				{
					skinnedMeshRenderer2.materials = record;
				}
			}
		}
		if (changingMinus == null || changingMinus.Count <= 0)
		{
			return;
		}
		foreach (Material changingMinu in changingMinus)
		{
			float float2 = changingMinu.GetFloat("_shaderChange");
			changingMinu.SetFloat("_shaderChange", Mathf.Clamp(float2 - 0.005f, 0f, 1f));
		}
		Material material = changingMinus.Peek();
		if (material != null && material.GetFloat("_shaderChange") < float.Epsilon)
		{
			changingMinus.Dequeue();
		}
	}

	private void UpdateAdvPlot()
	{
		if (m_AdStoryList.Count == 0)
		{
			return;
		}
		AdStoryData adStroyData = StoryRepository.GetAdStroyData(m_AdStoryList[0]);
		if (adStroyData == null)
		{
			return;
		}
		for (int i = 0; i < adStroyData.m_creNPC.Count; i++)
		{
			Vector3 pos;
			switch (adStroyData.m_creNPC[i].referToType)
			{
			case ReferToType.Player:
				pos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
				break;
			case ReferToType.Town:
				VArtifactUtil.GetTownPos(adStroyData.m_creNPC[i].m_referToID, out pos);
				break;
			case ReferToType.Npc:
				pos = PeSingleton<EntityMgr>.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[adStroyData.m_creNPC[i].m_referToID]).position;
				break;
			default:
				pos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
				break;
			}
			if (pos == Vector3.zero)
			{
				continue;
			}
			Vector2 vector = UnityEngine.Random.insideUnitCircle.normalized * adStroyData.m_creNPC[i].m_radius;
			Vector2 vector2 = new Vector2(pos.x + vector.x, pos.z + vector.y);
			PeEntity peEntity = NpcEntityCreator.CreateNpc(pos: new Vector3(vector2.x, VFDataRTGen.GetPosTop(new IntVector2((int)vector2.x, (int)vector2.y)), vector2.y), protoId: adStroyData.m_creNPC[i].m_NPCID);
			if (peEntity != null)
			{
				if (VFDataRTGen.IsTownAvailable((int)vector2.x, (int)vector2.y))
				{
					peEntity.NpcCmpt.FixedPointPos = new Vector3(vector2.x, VFDataRTGen.GetPosHeightWithTown(new IntVector2((int)vector2.x, (int)vector2.y)), vector2.y);
				}
				else
				{
					peEntity.NpcCmpt.FixedPointPos = new Vector3(vector2.x, VFDataRTGen.GetPosHeight(new IntVector2((int)vector2.x, (int)vector2.y)), vector2.y);
				}
			}
		}
		checkEnterArea.AddRange(adStroyData.m_enterArea);
		for (int j = 0; j < adStroyData.m_npcMove.Count; j++)
		{
			Vector3 pos3;
			switch (adStroyData.m_npcMove[j].referToType)
			{
			case ReferToType.Player:
				pos3 = PeSingleton<PeCreature>.Instance.mainPlayer.position;
				break;
			case ReferToType.Town:
				VArtifactUtil.GetTownPos(adStroyData.m_npcMove[j].m_referToID, out pos3);
				break;
			case ReferToType.Npc:
				pos3 = PeSingleton<EntityMgr>.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[adStroyData.m_npcMove[j].m_referToID]).position;
				break;
			default:
				pos3 = PeSingleton<PeCreature>.Instance.mainPlayer.position;
				break;
			}
			if (!(pos3 == Vector3.zero) && MissionManager.Instance.m_PlayerMission.adId_entityId.ContainsKey(adStroyData.m_npcMove[j].npcID))
			{
				PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[adStroyData.m_npcMove[j].npcID]);
				if (!(peEntity == null))
				{
					MoveTo(peEntity, pos3, adStroyData.m_npcMove[j].m_radius, bForce: true, SpeedState.Run);
					peEntity.NpcCmpt.FixedPointPos = pos3;
				}
			}
		}
		for (int k = 0; k < adStroyData.m_getMissionID.Count; k++)
		{
			int missionID = adStroyData.m_getMissionID[k];
			MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(missionID);
			if (missionCommonData == null)
			{
				continue;
			}
			if (MissionRepository.HaveTalkOP(missionID))
			{
				GameUI.Instance.mNPCTalk.NormalOrSP(0);
				if (!GameUI.Instance.mNPCTalk.isPlayingTalk)
				{
					GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(missionID, 1);
					GameUI.Instance.mNPCTalk.PreShow();
				}
				else
				{
					GameUI.Instance.mNPCTalk.AddNpcTalkInfo(missionID, 1);
				}
			}
			else if (MissionManager.Instance.IsGetTakeMission(missionID))
			{
				PeEntity peEntity2 = PeSingleton<EntityMgr>.Instance.Get(missionCommonData.m_iNpc);
				if (peEntity2 == null || peEntity2.NpcCmpt == null)
				{
					peEntity2 = PeSingleton<EntityMgr>.Instance.Get(m_AdStoryListNpc[m_AdStoryList[0]]);
				}
				MissionManager.Instance.SetGetTakeMission(missionID, peEntity2, MissionManager.TakeMissionType.TakeMissionType_Get);
			}
		}
		for (int l = 0; l < adStroyData.m_comMissionID.Count; l++)
		{
			MissionCommonData missionCommonData2 = MissionManager.GetMissionCommonData(adStroyData.m_comMissionID[l]);
			if (missionCommonData2 != null)
			{
				PeEntity peEntity3 = PeSingleton<EntityMgr>.Instance.Get(missionCommonData2.m_iNpc);
				if (PeGameMgr.IsSingle)
				{
					MissionManager.Instance.CompleteMission(adStroyData.m_comMissionID[l]);
				}
				else
				{
					MissionManager.Instance.RequestCompleteMission(adStroyData.m_comMissionID[l]);
				}
			}
		}
		if (adStroyData.m_showTip != 0)
		{
			new PeTipMsg(PELocalization.GetString(adStroyData.m_showTip), PeTipMsg.EMsgLevel.Warning, PeTipMsg.EMsgType.Stroy);
		}
		m_AdStoryListNpc.Remove(m_AdStoryList[0]);
		m_AdStoryList.RemoveAt(0);
	}

	private void OnDestroy()
	{
		MonsterEntityCreator.commonCreateEvent -= MonsterCreatePatrolSet;
	}

	private void Update()
	{
		UpdateUIWindow();
		UpdateChangingMaterial();
		if (KillNPC.isHaveAsh_inScene())
		{
			KillNPC.UpdateAshBox();
		}
		if (!GameConfig.IsMultiMode)
		{
			UpdateCamp();
		}
		if (StroyManager.randScenarios.Count > 0)
		{
			foreach (int key in StroyManager.randScenarios.Keys)
			{
				if (Time.time - StroyManager.randScenarios[key].curTime > StroyManager.randScenarios[key].cd)
				{
					StroyManager.randScenarios[key].curTime = Time.time;
					int index = UnityEngine.Random.Range(0, StroyManager.randScenarios[key].scenarioIds.Count);
					GameUI.Instance.mNPCTalk.SpTalkSymbol(spOrHalf: true);
					UINPCTalk.NpcTalkInfo talkinfo = default(UINPCTalk.NpcTalkInfo);
					GameUI.Instance.mNPCTalk.GetTalkInfo(StroyManager.randScenarios[key].scenarioIds[index], ref talkinfo, null);
					GameUI.Instance.mNPCTalk.ParseName(null, ref talkinfo);
					GameUI.Instance.mNPCTalk.m_NpcTalkList.Add(talkinfo);
					if (!GameUI.Instance.mNPCTalk.isPlayingTalk)
					{
						GameUI.Instance.mNPCTalk.SPTalkClose();
					}
				}
			}
		}
		if (PeGameMgr.IsMulti && !PlayerNetwork._missionInited && PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			return;
		}
		if (PeGameMgr.IsMulti && MissionManager.Instance.HadCompleteMission(18) && !MissionManager.Instance.HadCompleteMission(27))
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(9008);
			if (peEntity != null && peEntity.peTrans.rotation.eulerAngles.y != 270f)
			{
				peEntity.peTrans.rotation = Quaternion.Euler(0f, 270f, 0f);
			}
		}
		int count = checkEnterArea.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			bool flag = false;
			Vector3 pos;
			switch (checkEnterArea[num].referToType)
			{
			case 1:
				pos = PeSingleton<EntityMgr>.Instance.Get(checkEnterArea[num].m_referToID).position;
				break;
			case 2:
				VArtifactUtil.GetTownPos(checkEnterArea[num].m_referToID, out pos);
				break;
			case 3:
				if (!CSMain.GetAssemblyPos(out pos))
				{
					flag = true;
				}
				break;
			default:
				pos = PeSingleton<PeCreature>.Instance.mainPlayer.position;
				break;
			}
			if (!flag)
			{
				if (pos == Vector3.zero)
				{
					checkEnterArea.RemoveAt(num);
				}
				else if (PeSingleton<PeCreature>.Instance != null && null != PeSingleton<PeCreature>.Instance.mainPlayer && Vector3.Distance(PeSingleton<PeCreature>.Instance.mainPlayer.position, pos) < checkEnterArea[num].m_radius)
				{
					PushAdStoryList(checkEnterArea[num].m_plotID);
					checkEnterArea.RemoveAt(num);
				}
			}
		}
		UpdateAdvPlot();
		if (m_StoryList.Count == 0)
		{
			return;
		}
		int num2 = 0;
		bool flag2 = true;
		KeyValuePair<int, bool> ite2;
		foreach (KeyValuePair<int, bool> story in m_StoryList)
		{
			ite2 = story;
			if (delayPlotID.Find((int tmp) => tmp == ite2.Key) != 0)
			{
				continue;
			}
			num2 = ite2.Key;
			flag2 = ite2.Value;
			break;
		}
		StoryData stroyData = StoryRepository.GetStroyData(num2);
		if (stroyData == null)
		{
			m_StoryList.Remove(num2);
			return;
		}
		if (m_MisDelay.ContainsKey(num2) && Time.time - m_MisDelay[num2] < stroyData.m_Delay)
		{
			if (!delayPlotID.Contains(num2))
			{
				delayPlotID.Add(num2);
			}
			if (delayPlotID.Count == m_StoryList.Count)
			{
				delayPlotID.Clear();
			}
			return;
		}
		if (stroyData.m_triggerPlot.Count > 0)
		{
			Instance.PushStoryList(stroyData.m_triggerPlot);
		}
		if (stroyData.m_TalkList.Count > 0)
		{
			GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(stroyData.m_TalkList);
			GameUI.Instance.mNPCTalk.PreShow();
		}
		if (stroyData.m_ServantTalkList.Count > 0)
		{
			GameUI.Instance.mNPCTalk.SpTalkSymbol(spOrHalf: true);
			GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(stroyData.m_ServantTalkList, null, IsClearTalkList: false);
			if (!GameUI.Instance.mNPCTalk.isPlayingTalk)
			{
				GameUI.Instance.mNPCTalk.SPTalkClose();
			}
		}
		if (stroyData.m_randScenarios.id != 0)
		{
			if (stroyData.m_randScenarios.startOrClose)
			{
				RandScenarios randScenarios = new RandScenarios();
				randScenarios.id = stroyData.m_randScenarios.id;
				randScenarios.scenarioIds = stroyData.m_randScenarios.scenarioIds;
				randScenarios.cd = stroyData.m_randScenarios.cd;
				randScenarios.curTime = Time.time;
				if (!StroyManager.randScenarios.ContainsKey(randScenarios.id))
				{
					StroyManager.randScenarios.Add(randScenarios.id, randScenarios);
				}
			}
			else if (StroyManager.randScenarios.ContainsKey(stroyData.m_randScenarios.id))
			{
				StroyManager.randScenarios.Remove(stroyData.m_randScenarios.id);
			}
			if (PeGameMgr.IsMulti)
			{
				if (MissionManager.Instance.HadCompleteMission(69) && StroyManager.randScenarios.ContainsKey(1))
				{
					StroyManager.randScenarios.Remove(1);
				}
				if (MissionManager.Instance.HadCompleteMission(127) && StroyManager.randScenarios.ContainsKey(2))
				{
					StroyManager.randScenarios.Remove(2);
				}
			}
		}
		for (int i = 0; i < stroyData.m_CreateMonster.Count; i++)
		{
			MissionIDNum missionIDNum = stroyData.m_CreateMonster[i];
			PeSingleton<SceneEntityCreatorArchiver>.Instance.SetFixedSpawnPointActive(missionIDNum.id, active: true);
			MissionManager.Instance.RemoveTimerByID(missionIDNum.id);
		}
		for (int j = 0; j < stroyData.m_DeleteMonster.Count; j++)
		{
			AISpawnPoint.Activate(stroyData.m_DeleteMonster[j], isActive: false);
			PeSingleton<SceneEntityCreatorArchiver>.Instance.SetFixedSpawnPointActive(stroyData.m_DeleteMonster[j], active: false);
		}
		for (int k = 0; k < stroyData.m_MonsterCampList.Count; k++)
		{
			AiDataBlock.SetCamp(stroyData.m_MonsterCampList[k].id, stroyData.m_MonsterCampList[k].value);
		}
		for (int l = 0; l < stroyData.m_MonsterHarmList.Count; l++)
		{
			AiDataBlock.SetHarm(stroyData.m_MonsterHarmList[l].id, stroyData.m_MonsterHarmList[l].value);
		}
		for (int m = 0; m < stroyData.m_MonAct.Count; m++)
		{
			MonAct monAct = stroyData.m_MonAct[m];
			for (int num3 = 0; num3 < monAct.mons.Count; num3++)
			{
				PeEntity entityByFixedSpId = PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(monAct.mons[num3]);
				if (entityByFixedSpId == null)
				{
					continue;
				}
				RequestCmpt component = entityByFixedSpId.GetComponent<RequestCmpt>();
				if (!(component == null))
				{
					if (monAct.btrue)
					{
						component.Register(EReqType.Animation, monAct.animation, monAct.time);
					}
					else
					{
						component.RemoveRequest(EReqType.Animation);
					}
				}
			}
		}
		PeEntity npc;
		for (int num4 = 0; num4 < stroyData.m_NpcAct.Count; num4++)
		{
			NpcAct npcAct = stroyData.m_NpcAct[num4];
			npc = PeSingleton<EntityMgr>.Instance.Get(npcAct.npcid);
			if (npc == null)
			{
				continue;
			}
			switch (npcAct.animation)
			{
			case "AddItem":
				AddNpcItem(npc.Id, 90002421);
				break;
			case "CarryUp":
				CarryUp(npc, 9008, bCarryUp: true);
				break;
			case "PutDown":
			{
				PeEntity peEntity2 = PeSingleton<EntityMgr>.Instance.Get(9008);
				NpcCmpt npcCmpt = peEntity2.NpcCmpt;
				npcCmpt.Req_Remove(EReqType.Salvation);
				if (PeGameMgr.IsMultiStory)
				{
					GameObject gameObject = GameObject.Find("scene_healing_tube(Clone)");
					if (gameObject != null)
					{
						gameObject.GetComponent<Collider>().enabled = false;
					}
					if (npc != null && !npc.NpcCmpt.Net.hasOwnerAuth)
					{
						break;
					}
				}
				CarryUp(npc, 9008, bCarryUp: false);
				BiologyViewCmpt biologyViewCmpt2 = peEntity2.biologyViewCmpt;
				if (null != biologyViewCmpt2)
				{
					biologyViewCmpt2.ActivateInjured(value: false);
				}
				MotionMgrCmpt motionMgr2 = peEntity2.motionMgr;
				if (!(motionMgr2 == null))
				{
					motionMgr2.FreezePhyState(GetType(), v: true);
					SetIdle(peEntity2, "InjuredRest");
				}
				break;
			}
			case "Cure":
			{
				RemoveReq(npc, EReqType.Idle);
				MotionMgrCmpt motionMgr = npc.motionMgr;
				if (!(motionMgr == null))
				{
					BiologyViewCmpt biologyViewCmpt = npc.biologyViewCmpt;
					if (null != biologyViewCmpt)
					{
						biologyViewCmpt.ActivateInjured(value: true);
					}
					motionMgr.FreezePhyState(GetType(), v: false);
				}
				break;
			}
			case "InjuredSit":
			case "InjuredSitEX":
			case "Lie":
				if (npcAct.btrue)
				{
					SetIdle(npc, npcAct.animation);
				}
				else
				{
					RemoveReq(npc, EReqType.Idle);
				}
				break;
			case "npcidle":
				SetIdle(npc, "Idle");
				break;
			case "npcdidle":
				RemoveReq(npc, EReqType.Idle);
				break;
			}
		}
		for (int num5 = 0; num5 < stroyData.m_NpcReq.Count; num5++)
		{
			npc = PeSingleton<EntityMgr>.Instance.Get(stroyData.m_NpcReq[num5].type);
			PEActionType valve = (PEActionType)stroyData.m_NpcReq[num5].valve;
			if (stroyData.m_NpcReq[num5].isEffect)
			{
				npc.motionMgr.DoAction(valve);
			}
			else
			{
				npc.motionMgr.EndAction(valve);
			}
		}
		foreach (NpcAct item5 in stroyData.m_NpcAnimator)
		{
			npc = PeSingleton<EntityMgr>.Instance.Get(item5.npcid);
			if (!(npc == null))
			{
				npc.NpcCmpt.Req_PlayAnimation(item5.animation, 0f, item5.btrue);
			}
		}
		if (stroyData.m_PlayerAni.Count == 2)
		{
			AnimatorCmpt cmpt = PeSingleton<MainPlayer>.Instance.entity.GetCmpt<AnimatorCmpt>();
			if (cmpt != null)
			{
				cmpt.SetBool(stroyData.m_PlayerAni[0], stroyData.m_PlayerAni[1].Equals("1") ? true : false);
			}
		}
		foreach (MotionStyle item6 in stroyData.m_MotionStyle)
		{
			npc = PeSingleton<EntityMgr>.Instance.Get(item6.id);
			if (!(npc == null))
			{
				NpcCmpt cmpt2 = npc.GetCmpt<NpcCmpt>();
				if (!(cmpt2 == null))
				{
					cmpt2.MotionStyle = item6.type;
				}
			}
		}
		for (int num6 = 0; num6 < stroyData.m_TransNpc.Count; num6++)
		{
			NpcStyle npcStyle = stroyData.m_TransNpc[num6];
			if (npcStyle == null)
			{
				continue;
			}
			npc = PeSingleton<EntityMgr>.Instance.Get(npcStyle.npcid);
			if (!(npc == null))
			{
				Instance.Translate(npc, npcStyle.pos);
				NpcCmpt npcCmpt2 = npc.NpcCmpt;
				if (null != npcCmpt2)
				{
					npcCmpt2.FixedPointPos = npcStyle.pos;
				}
				else
				{
					Debug.LogError("Failed to set fixed point.");
				}
			}
		}
		for (int num7 = 0; num7 < stroyData.m_FollowPlayerList.Count; num7++)
		{
			NpcOpen npcOpen = stroyData.m_FollowPlayerList[num7];
			npc = PeSingleton<EntityMgr>.Instance.Get(npcOpen.npcid);
			if (npc == null)
			{
				continue;
			}
			NpcCmpt npcCmpt3 = npc.NpcCmpt;
			if (npcCmpt3 == null)
			{
				continue;
			}
			if (npcOpen.bopen)
			{
				ServantLeaderCmpt cmpt3 = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>();
				if (PeGameMgr.IsMultiStory)
				{
					if (npcCmpt3.Net.hasOwnerAuth)
					{
						npcCmpt3.Net.RPCServer(EPacketType.PT_NPC_ForcedServant, true);
					}
				}
				else
				{
					cmpt3.AddForcedServant(npcCmpt3);
				}
				continue;
			}
			ServantLeaderCmpt cmpt4 = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>();
			if (PeGameMgr.IsMultiStory)
			{
				if (npcCmpt3.Net.hasOwnerAuth)
				{
					npcCmpt3.Net.RPCServer(EPacketType.PT_NPC_ForcedServant, false);
				}
			}
			else
			{
				cmpt4.RemoveForcedServant(npcCmpt3);
			}
			npcCmpt3.FixedPointPos = npc.position;
		}
		for (int num8 = 0; num8 < stroyData.m_MoveNpc.Count; num8++)
		{
			bool flag3 = false;
			MoveNpcData moveNpcData = stroyData.m_MoveNpc[num8];
			if (moveNpcData == null)
			{
				continue;
			}
			SpeedState ss = ((stroyData.m_MoveType == 0) ? SpeedState.Walk : SpeedState.Run);
			Vector3 pos2;
			if (moveNpcData.pos != Vector3.zero)
			{
				pos2 = moveNpcData.pos;
			}
			else if (moveNpcData.targetNpc > 0)
			{
				if (moveNpcData.targetNpc == 20000)
				{
					if (PeGameMgr.IsMultiStory)
					{
						PlayerNetwork nearestPlayer = PlayerNetwork.GetNearestPlayer(PlayerNetwork.mainPlayer.PlayerPos);
						pos2 = nearestPlayer.PlayerPos;
					}
					else
					{
						pos2 = PeSingleton<MainPlayer>.Instance.entity.position;
					}
				}
				else
				{
					if (PeSingleton<EntityMgr>.Instance.Get(moveNpcData.targetNpc) == null)
					{
						continue;
					}
					pos2 = GetNpcPos(moveNpcData.targetNpc);
				}
			}
			else
			{
				flag3 = true;
				if (!CSMain.GetAssemblyPos(out pos2))
				{
					if (moveNpcData.targetNpc == -99)
					{
						continue;
					}
					if (moveNpcData.targetNpc == -98)
					{
						pos2 = PeSingleton<PeCreature>.Instance.mainPlayer.position;
					}
				}
			}
			List<Vector3> meetingPosition = GetMeetingPosition(pos2, moveNpcData.npcsId.Count, 2f);
			Dictionary<PeEntity, bool> dictionary = new Dictionary<PeEntity, bool>();
			for (int num9 = 0; num9 < moveNpcData.npcsId.Count; num9++)
			{
				npc = PeSingleton<EntityMgr>.Instance.Get(moveNpcData.npcsId[num9]);
				if (npc == null)
				{
					continue;
				}
				if (pos2 == Vector3.zero)
				{
					break;
				}
				Vector3 pos3;
				if (!flag3)
				{
					pos3 = meetingPosition[num9];
				}
				else if (moveNpcData.targetNpc == -99)
				{
					CSBuildingLogic assemblyLogic = CSMain.GetAssemblyLogic();
					if (assemblyLogic == null)
					{
						break;
					}
					int index2 = ((num9 + 2 < assemblyLogic.m_NPCTrans.Length) ? (num9 + 1) : 0);
					if (!assemblyLogic.GetNpcPos(index2, out pos3))
					{
						break;
					}
				}
				else
				{
					if (moveNpcData.targetNpc != -98)
					{
						break;
					}
					pos3 = AiUtil.GetRandomPositionInLand(pos2, 30f, 50f, 10f, LayerMask.GetMask("Default", "VFVoxelTerrain", "SceneStatic"), 30);
				}
				MoveTo(npc, pos3, 1f, bForce: true, ss);
				NpcCmpt npcCmpt4 = npc.NpcCmpt;
				if (null != npcCmpt4)
				{
					npcCmpt4.FixedPointPos = pos3;
				}
				else
				{
					Debug.LogError("Failed to set fixed point.");
				}
				if (moveNpcData.missionOrPlot_id != 0)
				{
					dictionary.Add(npc, value: false);
				}
			}
			if (moveNpcData.missionOrPlot_id != 0 && !m_RecordIsReachPoitn.ContainsKey(moveNpcData.missionOrPlot_id))
			{
				m_RecordIsReachPoitn.Add(moveNpcData.missionOrPlot_id, dictionary);
			}
		}
		if (stroyData.m_moveNpc_missionOrPlot_id != 0)
		{
			Dictionary<PeEntity, bool> dictionary2 = new Dictionary<PeEntity, bool>();
			foreach (MoveNpcData item7 in stroyData.m_MoveNpc)
			{
				foreach (int item8 in item7.npcsId)
				{
					npc = PeSingleton<EntityMgr>.Instance.Get(item8);
					if (!(npc == null))
					{
						dictionary2.Add(npc, value: false);
					}
				}
			}
			if (!m_RecordIsReachPoitn.ContainsKey(stroyData.m_moveNpc_missionOrPlot_id))
			{
				m_RecordIsReachPoitn.Add(stroyData.m_moveNpc_missionOrPlot_id, dictionary2);
			}
		}
		if (stroyData.m_NpcRail.inpclist.Count > 0 && PeSingleton<Manager>.Instance.GetRoutes().Count > 0)
		{
			Vector3 vector = Vector3.zero;
			if (stroyData.m_NpcRail.bplayer)
			{
				vector = PeSingleton<PeCreature>.Instance.mainPlayer.position;
			}
			else if (stroyData.m_NpcRail.othernpcid > 0)
			{
				npc = PeSingleton<EntityMgr>.Instance.Get(stroyData.m_NpcRail.othernpcid);
				if (npc != null)
				{
					vector = npc.position;
				}
			}
			else
			{
				vector = stroyData.m_NpcRail.pos;
			}
			if (vector != Vector3.zero)
			{
				for (int num10 = 0; num10 < stroyData.m_NpcRail.inpclist.Count; num10++)
				{
					npc = PeSingleton<EntityMgr>.Instance.Get(stroyData.m_NpcRail.inpclist[num10]);
					PeSingleton<Manager>.Instance.GetTwoPointClosest(npc.ExtGetPos(), vector, out var start, out var end, out var startIndex, out var endIndex);
					if (npc != null)
					{
						PassengerInfo passengerInfo = new PassengerInfo();
						passengerInfo.npcID = npc.Id;
						passengerInfo.startRouteID = start.routeId;
						passengerInfo.startIndexID = startIndex;
						passengerInfo.endRouteID = end.routeId;
						passengerInfo.endIndexID = endIndex;
						passengerInfo.dest = vector;
						passengerInfo.type = PassengerInfo.Course.before;
						if (!m_Passengers.ContainsKey(passengerInfo.npcID))
						{
							m_Passengers.Add(passengerInfo.npcID, passengerInfo);
						}
						StartCoroutine(WaitingNpcRailStart(start, npc, end, vector));
					}
				}
			}
		}
		for (int num11 = 0; num11 < stroyData.m_NpcFace.Count; num11++)
		{
			NpcFace npcFace = stroyData.m_NpcFace[num11];
			npc = PeSingleton<EntityMgr>.Instance.Get(npcFace.npcid);
			if (npc == null)
			{
				continue;
			}
			if (npcFace.angle == -2)
			{
				npc.CmdFaceToPoint(PeSingleton<PeCreature>.Instance.mainPlayer.ExtGetPos());
				continue;
			}
			if (npcFace.angle != -1)
			{
				SetRotation(npc, Quaternion.AngleAxis(npcFace.angle, Vector3.up));
				continue;
			}
			PeEntity peEntity3 = PeSingleton<EntityMgr>.Instance.Get(npcFace.otherid);
			if (peEntity3 != null)
			{
				npc.CmdFaceToPoint(peEntity3.ExtGetPos());
			}
		}
		for (int num12 = 0; num12 < stroyData.m_NpcCamp.Count; num12++)
		{
			NpcCamp npcCamp = stroyData.m_NpcCamp[num12];
			npc = PeSingleton<EntityMgr>.Instance.Get(npcCamp.npcid);
			if (!(npc == null))
			{
				npc.SetCamp(npcCamp.camp);
			}
		}
		for (int num13 = 0; num13 < stroyData.m_SenceFace.Count; num13++)
		{
			SenceFace senceFace = stroyData.m_SenceFace[num13];
			GameObject gameObject2 = GameObject.Find(senceFace.name);
			if (!(gameObject2 == null))
			{
				gameObject2.transform.rotation = Quaternion.AngleAxis(senceFace.angle, Vector3.up);
			}
		}
		for (int num14 = 0; num14 < stroyData.m_NpcAI.Count; num14++)
		{
			NpcOpen npcOpen2 = stroyData.m_NpcAI[num14];
			npc = PeSingleton<EntityMgr>.Instance.Get(npcOpen2.npcid);
			if (!(npc == null))
			{
				npc.SetAiActive(npcOpen2.bopen);
			}
		}
		for (int num15 = 0; num15 < stroyData.m_NpcInvincible.Count; num15++)
		{
			NpcOpen npcOpen3 = stroyData.m_NpcInvincible[num15];
			npc = PeSingleton<EntityMgr>.Instance.Get(npcOpen3.npcid);
			if (!(npc == null))
			{
				npc.SetInvincible(npcOpen3.bopen);
			}
		}
		for (int num16 = 0; num16 < stroyData.m_cantReviveNpc.Count; num16++)
		{
			npc = PeSingleton<EntityMgr>.Instance.Get(stroyData.m_cantReviveNpc[num16]);
			if (npc != null && npc.NpcCmpt != null)
			{
				npc.NpcCmpt.ReviveTime = -1;
			}
		}
		if (stroyData.m_attractMons.Count >= 3)
		{
			List<int> list = new List<int>();
			int num17 = stroyData.m_attractMons.FindIndex((int ite) => ite == -9999);
			bool missionOrPlot = true;
			for (int num18 = 0; num18 < stroyData.m_attractMons.Count; num18++)
			{
				if (num18 != num17)
				{
					if (num18 == num17 + 1)
					{
						missionOrPlot = stroyData.m_attractMons[num18] == 1;
					}
					else if (num18 > num17 + 1)
					{
						list.Add(stroyData.m_attractMons[num18]);
					}
				}
			}
			StartCoroutine(CheckAttractMons(stroyData.m_attractMons.GetRange(0, num17), missionOrPlot, list));
		}
		int num19 = stroyData.m_killNpcList.Count / 3;
		List<int> list2 = new List<int>();
		List<PeEntity> list3 = new List<PeEntity>();
		for (int num20 = 0; num20 < num19; num20++)
		{
			list2 = stroyData.m_killNpcList.GetRange(3 * num20, 3);
			list3 = KillNPC.NPCBeKilled(list2[0]);
			KillNPC.NPCaddItem(list3, list2[1], list2[2]);
		}
		num19 = stroyData.m_monsterHatredList.Count / 5;
		for (int num21 = 0; num21 < num19; num21++)
		{
			list2 = stroyData.m_monsterHatredList.GetRange(5 * num21, 5);
			SpecialHatred.MonsterHatredAdd(list2);
		}
		num19 = stroyData.m_npcHatredList.Count / 4;
		for (int num22 = 0; num22 < num19; num22++)
		{
			list2 = stroyData.m_npcHatredList.GetRange(4 * num22, 4);
			SpecialHatred.NpcHatredAdd(list2);
		}
		num19 = stroyData.m_harmList.Count / 3;
		for (int num23 = 0; num23 < num19; num23++)
		{
			list2 = stroyData.m_harmList.GetRange(3 * num23, 3);
			SpecialHatred.HarmAdd(list2);
		}
		num19 = stroyData.m_doodadHarmList.Count / 3;
		for (int num24 = 0; num24 < num19; num24++)
		{
			list2 = stroyData.m_doodadHarmList.GetRange(3 * num24, 3);
			PeEntity[] array = ((list2[1] == 0) ? PeSingleton<EntityMgr>.Instance.GetDoodadEntitiesByProtoId(list2[2]) : PeSingleton<EntityMgr>.Instance.GetDoodadEntities(list2[1]));
			for (int num25 = 0; num25 < array.Length; num25++)
			{
				if (!(array[num25].GetCmpt<SceneDoodadLodCmpt>() == null))
				{
					if (list2[0] == 0)
					{
						array[num25].GetCmpt<SceneDoodadLodCmpt>().IsDamagable = true;
					}
					else
					{
						array[num25].GetCmpt<SceneDoodadLodCmpt>().IsDamagable = false;
					}
				}
			}
		}
		SetDoodadEffect item;
		foreach (SetDoodadEffect doodadEffect in stroyData.m_doodadEffectList)
		{
			item = doodadEffect;
			PeEntity[] doodadEntities = PeSingleton<EntityMgr>.Instance.GetDoodadEntities(item.id);
			if (doodadEntities.Length == 0 || doodadEntities[0] == null)
			{
				continue;
			}
			doodadEntities[0].transform.TraverseHierarchy(delegate(Transform trans, int d)
			{
				if (item.names.Contains(trans.gameObject.name))
				{
					trans.gameObject.SetActive(item.openOrClose);
				}
			});
		}
		if (stroyData.m_npcType.Count > 0)
		{
			foreach (NpcType item2 in stroyData.m_npcType)
			{
				item2.npcs.ForEach(delegate(int n)
				{
					npc = PeSingleton<EntityMgr>.Instance.Get(n);
					if (npc != null && npc.NpcCmpt != null)
					{
						npc.NpcCmpt.NpcControlCmdId = item2.type;
					}
				});
			}
		}
		if (stroyData.m_checkMons.Count > 0)
		{
			StartCoroutine(CheckMonsExist(stroyData.m_checkMons));
		}
		KillMons item3;
		foreach (KillMons killMon in stroyData.m_killMons)
		{
			item3 = killMon;
			if (m_RecordKillMons.Count == 0)
			{
				MonsterEntityCreator.commonCreateEvent += KillMonster;
			}
			if (!m_RecordKillMons.ContainsKey(item3.id))
			{
				m_RecordKillMons.Add(item3.id, item3);
			}
			if (item3.type == KillMons.Type.protoTypeId)
			{
				List<PeEntity> list4 = new List<PeEntity>(PeSingleton<EntityMgr>.Instance.All);
				list4 = list4.FindAll(delegate(PeEntity mon)
				{
					if (mon == null)
					{
						return false;
					}
					return (mon.proto == EEntityProto.Monster && Vector3.Distance(mon.position, item3.center) <= item3.radius && (item3.monId == -999 || mon.entityProto.protoId == item3.monId)) ? true : false;
				});
				foreach (PeEntity item9 in list4)
				{
					if (!(item9 == null))
					{
						item9.GetComponent<PESkEntity>().SetAttribute(AttribType.Hp, 0f, eventOff: false);
					}
				}
			}
			else if (item3.type == KillMons.Type.fixedId)
			{
				PeEntity entityByFixedSpId2 = PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(item3.monId);
				if (entityByFixedSpId2 != null && entityByFixedSpId2.GetComponent<PESkEntity>() != null)
				{
					entityByFixedSpId2.GetComponent<PESkEntity>().SetAttribute(AttribType.Hp, 0f, eventOff: false);
				}
			}
		}
		foreach (int item10 in stroyData.m_stopKillMonsID)
		{
			m_RecordKillMons.Remove(item10);
			if (m_RecordKillMons.Count <= 0)
			{
				MonsterEntityCreator.commonCreateEvent -= KillMonster;
			}
		}
		ChangePartrolmode item4 = default(ChangePartrolmode);
		foreach (ChangePartrolmode item11 in stroyData.m_monPatrolMode)
		{
			bool flag4 = false;
			item4.monsId = new List<int>();
			item4.type = 0;
			item4.radius = 0;
			foreach (int item12 in item11.monsId)
			{
				npc = PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(item12);
				if (null != npc)
				{
					if (npc is EntityGrp)
					{
						EntityGrp entityGrp = npc as EntityGrp;
						foreach (ISceneObjAgent memberAgent in entityGrp.memberAgents)
						{
							if (memberAgent is SceneEntityPosAgent sceneEntityPosAgent && !(sceneEntityPosAgent.entity == null))
							{
								sceneEntityPosAgent.entity.BehaveCmpt.PatrolMode = (BHPatrolMode)item11.type;
								sceneEntityPosAgent.entity.BehaveCmpt.MinPatrolRadius = item11.radius;
								sceneEntityPosAgent.entity.BehaveCmpt.MaxPatrolRadius = item11.radius;
							}
						}
					}
					else
					{
						npc.BehaveCmpt.PatrolMode = (BHPatrolMode)item11.type;
						npc.BehaveCmpt.MinPatrolRadius = item11.radius;
						npc.BehaveCmpt.MaxPatrolRadius = item11.radius;
					}
				}
				else
				{
					if (!flag4)
					{
						flag4 = true;
						item4.type = item11.type;
						item4.radius = item11.radius;
					}
					item4.monsId.Add(item12);
				}
			}
			if (flag4)
			{
				patrolModeRecord.Add(item4);
				if (!recordPatrol)
				{
					recordPatrol = true;
					MonsterEntityCreator.commonCreateEvent += MonsterCreatePatrolSet;
				}
			}
		}
		foreach (MoveMons moveMon in stroyData.m_moveMons)
		{
			PeEntity entityByFixedSpId3 = PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(moveMon.fixedId);
			if (entityByFixedSpId3 != null)
			{
				MonsterCmpt component2 = entityByFixedSpId3.GetComponent<MonsterCmpt>();
				if (component2 != null)
				{
					component2.Req_MoveToPosition(moveMon.dist, 1f, isForce: true, (SpeedState)moveMon.stepOrRun);
				}
			}
			else
			{
				if (m_RecordMoveMons.Count == 0)
				{
					MonsterEntityCreator.commonCreateEvent += MoveMonster;
				}
				if (!m_RecordMoveMons.Contains(moveMon))
				{
					m_RecordMoveMons.Add(moveMon);
				}
			}
			if (moveMon.missionOrPlot_id != 0 && entityByFixedSpId3 != null)
			{
				Dictionary<PeEntity, bool> dictionary3 = new Dictionary<PeEntity, bool>();
				dictionary3.Add(entityByFixedSpId3, value: false);
				if (!m_RecordIsReachPoitn.ContainsKey(moveMon.missionOrPlot_id))
				{
					m_RecordIsReachPoitn.Add(moveMon.missionOrPlot_id, dictionary3);
				}
			}
		}
		if (stroyData.m_moveMons_missionOrPlot_id != 0)
		{
			Dictionary<PeEntity, bool> dictionary4 = new Dictionary<PeEntity, bool>();
			foreach (MoveMons moveMon2 in stroyData.m_moveMons)
			{
				npc = PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(moveMon2.fixedId);
				if (!(npc == null))
				{
					dictionary4.Add(npc, value: false);
				}
			}
			m_RecordIsReachPoitn.Add(stroyData.m_moveNpc_missionOrPlot_id, dictionary4);
		}
		foreach (ENpcBattleInfo item13 in stroyData.m_npcsBattle)
		{
			ENpcBattle battle = (ENpcBattle)(item13.type - 1);
			foreach (int item14 in item13.npcId)
			{
				npc = PeSingleton<EntityMgr>.Instance.Get(item14);
				if (!(npc == null))
				{
					npc.GetComponent<NpcCmpt>().Battle = battle;
				}
			}
		}
		for (int num26 = 0; num26 < stroyData.m_abnormalInfo.Count; num26++)
		{
			AbnormalInfo abnormalInfo = stroyData.m_abnormalInfo[num26];
			for (int num27 = 0; num27 < abnormalInfo.npcs.Count; num27++)
			{
				AbnormalConditionCmpt cmpt5;
				if (abnormalInfo.npcs[num27] == 30000)
				{
					foreach (PeEntity item15 in PeSingleton<EntityMgr>.Instance.All)
					{
						if (item15 == null || (item15.proto != EEntityProto.Npc && item15.proto != EEntityProto.RandomNpc))
						{
							continue;
						}
						cmpt5 = item15.GetCmpt<AbnormalConditionCmpt>();
						if (!(cmpt5 == null))
						{
							if (abnormalInfo.setOrRevive)
							{
								cmpt5.StartAbnormalCondition((PEAbnormalType)abnormalInfo.virusNum);
							}
							else
							{
								cmpt5.EndAbnormalCondition((PEAbnormalType)abnormalInfo.virusNum);
							}
						}
					}
					break;
				}
				if (abnormalInfo.npcs[num27] == 20000)
				{
					npc = PeSingleton<MainPlayer>.Instance.entity;
				}
				else if (abnormalInfo.npcs[num27] == 0)
				{
					if (!CSMain.HasCSAssembly() || CSMain.GetCSRandomNpc().Count <= 0)
					{
						continue;
					}
					npc = CSMain.GetCSRandomNpc()[UnityEngine.Random.Range(0, CSMain.GetCSRandomNpc().Count)];
				}
				else
				{
					npc = PeSingleton<EntityMgr>.Instance.Get(abnormalInfo.npcs[num27]);
				}
				if (npc == null)
				{
					continue;
				}
				cmpt5 = npc.GetCmpt<AbnormalConditionCmpt>();
				if (!(cmpt5 == null))
				{
					if (abnormalInfo.setOrRevive)
					{
						cmpt5.StartAbnormalCondition((PEAbnormalType)abnormalInfo.virusNum);
					}
					else
					{
						cmpt5.EndAbnormalCondition((PEAbnormalType)abnormalInfo.virusNum);
					}
				}
			}
		}
		if (null != PeSingleton<PeCreature>.Instance.mainPlayer)
		{
			int playerID = Mathf.RoundToInt(PeSingleton<PeCreature>.Instance.mainPlayer.GetAttribute(AttribType.DefaultPlayerID));
			for (int num28 = 0; num28 < stroyData.m_reputationChange.Length; num28++)
			{
				if (stroyData.m_reputationChange[num28].isEffect)
				{
					if (stroyData.m_reputationChange[num28].type == 0)
					{
						PeSingleton<ReputationSystem>.Instance.SetReputationValue(playerID, num28 + 5, stroyData.m_reputationChange[num28].valve);
					}
					else
					{
						PeSingleton<ReputationSystem>.Instance.ChangeReputationValue(playerID, num28 + 5, stroyData.m_reputationChange[num28].type * stroyData.m_reputationChange[num28].valve);
					}
				}
			}
			if (stroyData.m_nativeAttitude.isEffect)
			{
				if (stroyData.m_nativeAttitude.valve == -1)
				{
					PeSingleton<ReputationSystem>.Instance.CancelEXValue(playerID, stroyData.m_nativeAttitude.type + 5);
				}
				else
				{
					PeSingleton<ReputationSystem>.Instance.SetEXValue(playerID, stroyData.m_nativeAttitude.type + 5, stroyData.m_nativeAttitude.valve);
				}
			}
		}
		if (stroyData.oldDoodad.Count > 0 || stroyData.newDoodad.Count > 0)
		{
			ReplaceBuilding(stroyData.oldDoodad, stroyData.newDoodad);
		}
		if (stroyData.m_CameraList.Count > 0)
		{
			if (!PeGameMgr.IsMultiStory)
			{
				PlotLensAnimation.PlotPlay(stroyData.m_CameraList);
			}
			else if (!PlotLensAnimation.TooFar(new List<int>(Array.ConvertAll(stroyData.m_CameraList.ToArray(), (CameraInfo tmp) => tmp.cameraId))))
			{
				PlotLensAnimation.PlotPlay(stroyData.m_CameraList);
			}
		}
		ProcessSpecial(stroyData.m_Special);
		if (stroyData.m_PausePlayer != 0)
		{
			isPausing = stroyData.m_PausePlayer == 1;
		}
		foreach (PeEntity item16 in PeSingleton<EntityMgr>.Instance.All)
		{
			if (!(item16 == null))
			{
				item16.SetAiActive(stroyData.m_PauseNPC);
				if (!stroyData.m_PauseNPC)
				{
					Translate(item16, Vector3.zero);
					item16.PatrolMoveTo(Vector3.zero);
				}
			}
		}
		if (stroyData.m_EffectPosList.Count > 0 && stroyData.m_EffectID > 0)
		{
			for (int num29 = 0; num29 < stroyData.m_EffectPosList.Count; num29++)
			{
				Singleton<EffectBuilder>.Instance.Register(stroyData.m_EffectID, null, stroyData.m_EffectPosList[num29], Quaternion.identity);
			}
		}
		if (stroyData.m_SoundPosList.Count > 0 && stroyData.m_SoundID > 0)
		{
			for (int num30 = 0; num30 < stroyData.m_SoundPosList.Count; num30++)
			{
				Vector3 position = ((stroyData.m_SoundPosList[num30].type != 1) ? stroyData.m_SoundPosList[num30].pos : PeSingleton<EntityMgr>.Instance.Get(stroyData.m_SoundPosList[num30].npcID).position);
				AudioManager.instance.Create(position, stroyData.m_SoundID);
			}
		}
		for (int num31 = 0; num31 < stroyData.m_iColonyNoOrderNpcList.Count; num31++)
		{
			npc = PeSingleton<EntityMgr>.Instance.Get(stroyData.m_iColonyNoOrderNpcList[num31]);
			if (!(npc == null) && !(npc.NpcCmpt == null))
			{
				npc.NpcCmpt.BaseNpcOutMission = true;
			}
		}
		for (int num32 = 0; num32 < stroyData.m_iColonyOrderNpcList.Count; num32++)
		{
			npc = PeSingleton<EntityMgr>.Instance.Get(stroyData.m_iColonyOrderNpcList[num32]);
			if (!(npc == null) && !(npc.NpcCmpt == null))
			{
				npc.NpcCmpt.BaseNpcOutMission = false;
			}
		}
		if (stroyData.m_pauseSiege != 0)
		{
			MonsterSiege_Base.MonsterSiegeBasePause = stroyData.m_pauseSiege == 1;
		}
		foreach (CampState item17 in stroyData.m_campAlert)
		{
			Camp camp = Camp.GetCamp(item17.id);
			camp.SetCampNpcAlert(item17.isActive);
		}
		foreach (CampState item18 in stroyData.m_campActive)
		{
			Camp.SetCampActive(item18.id, item18.isActive);
		}
		foreach (int item19 in stroyData.m_comMission)
		{
			if (PeGameMgr.IsMulti)
			{
				MissionManager.Instance.RequestCompleteMission(item19);
			}
			else
			{
				MissionManager.Instance.CompleteMission(item19);
			}
		}
		for (int num33 = 0; num33 < stroyData.m_whackedList.Count; num33++)
		{
			ENpcBattleInfo eNpcBattleInfo = stroyData.m_whackedList[num33];
			for (int num34 = 0; num34 < eNpcBattleInfo.npcId.Count; num34++)
			{
				npc = PeSingleton<EntityMgr>.Instance.Get(eNpcBattleInfo.npcId[num34]);
				if (!(npc == null))
				{
					if (eNpcBattleInfo.type == 1)
					{
						SkEntity.MountBuff(npc.skEntity, 30200176, new List<int>(), new List<float>());
					}
					else if (eNpcBattleInfo.type == 0)
					{
						npc.skEntity.CancelBuffById(30200176);
					}
				}
			}
		}
		for (int num35 = 0; num35 < stroyData.m_getMission.Count; num35++)
		{
			int missionID = stroyData.m_getMission[num35];
			if (!MissionManager.Instance.m_PlayerMission.IsGetTakeMission(missionID))
			{
				continue;
			}
			if (MissionRepository.HaveTalkOP(missionID))
			{
				GameUI.Instance.mNPCTalk.NormalOrSP(0);
				if (!GameUI.Instance.mNPCTalk.isPlayingTalk)
				{
					GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(missionID, 1);
					GameUI.Instance.mNPCTalk.PreShow();
				}
				else
				{
					GameUI.Instance.mNPCTalk.AddNpcTalkInfo(missionID, 1);
				}
				continue;
			}
			MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(missionID);
			if (stroyData == null)
			{
				return;
			}
			npc = PeSingleton<EntityMgr>.Instance.Get(missionCommonData.m_iNpc);
			MissionManager.Instance.SetGetTakeMission(missionID, npc, MissionManager.TakeMissionType.TakeMissionType_Get);
		}
		if (stroyData.m_showTip != 0)
		{
			new PeTipMsg(PELocalization.GetString(stroyData.m_showTip), PeTipMsg.EMsgLevel.Warning, PeTipMsg.EMsgType.Stroy);
		}
		MissionManager.Instance.m_PlayerMission.LanguegeSkill += stroyData.m_increaseLangSkill;
		if (stroyData.m_plotMissionTrigger.Count > 0)
		{
			StartCoroutine(WaitPlotMissionTrigger(stroyData.m_plotMissionTrigger));
		}
		m_StoryList.Remove(num2);
		if (delayPlotID.Count == m_StoryList.Count)
		{
			delayPlotID.Clear();
		}
		if (m_MisDelay.ContainsKey(num2))
		{
			m_MisDelay.Remove(num2);
		}
		MissionManager.Instance.m_PlayerMission.UpdateAllNpcMisTex();
	}

	private void MonsterCreatePatrolSet(PeEntity mon)
	{
		if (patrolModeRecord.Count == 0)
		{
			return;
		}
		foreach (ChangePartrolmode item in patrolModeRecord)
		{
			foreach (int item2 in item.monsId)
			{
				PeEntity entityByFixedSpId = PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(item2);
				if (entityByFixedSpId is EntityGrp)
				{
					EntityGrp entityGrp = entityByFixedSpId as EntityGrp;
					if (entityGrp.memberAgents == null)
					{
						continue;
					}
					foreach (ISceneObjAgent memberAgent in entityGrp.memberAgents)
					{
						if (memberAgent is SceneEntityPosAgent sceneEntityPosAgent && !(sceneEntityPosAgent.entity == null) && sceneEntityPosAgent.entity == mon)
						{
							mon.BehaveCmpt.PatrolMode = (BHPatrolMode)item.type;
							mon.BehaveCmpt.MinPatrolRadius = item.radius;
							mon.BehaveCmpt.MaxPatrolRadius = item.radius;
						}
					}
				}
				else if (entityByFixedSpId != null && entityByFixedSpId == mon)
				{
					entityByFixedSpId.BehaveCmpt.PatrolMode = (BHPatrolMode)item.type;
					entityByFixedSpId.BehaveCmpt.MinPatrolRadius = item.radius;
					entityByFixedSpId.BehaveCmpt.MaxPatrolRadius = item.radius;
				}
			}
		}
	}

	private void MoveMonster(PeEntity mon)
	{
		List<MoveMons> list = new List<MoveMons>();
		foreach (MoveMons recordMoveMon in m_RecordMoveMons)
		{
			if (PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(recordMoveMon.fixedId) == mon && (bool)mon.GetCmpt<MonsterCmpt>())
			{
				mon.GetCmpt<MonsterCmpt>().Req_MoveToPosition(recordMoveMon.dist, 0f, isForce: true, (SpeedState)recordMoveMon.stepOrRun);
				list.Add(recordMoveMon);
			}
		}
		foreach (MoveMons item in list)
		{
			if (m_RecordMoveMons.Contains(item))
			{
				m_RecordMoveMons.Remove(item);
			}
		}
		if (m_RecordMoveMons.Count == 0)
		{
			MonsterEntityCreator.commonCreateEvent -= MoveMonster;
		}
	}

	public void EntityReach(PeEntity entity, bool trigger, bool fromNet = false)
	{
		List<int> list = new List<int>();
		List<int> list2 = new List<int>(m_RecordIsReachPoitn.Keys);
		foreach (int item in list2)
		{
			List<PeEntity> list3 = new List<PeEntity>(m_RecordIsReachPoitn[item].Keys);
			foreach (PeEntity item2 in list3)
			{
				if (!(item2 == entity))
				{
					continue;
				}
				m_RecordIsReachPoitn[item][entity] = true;
				if (new List<bool>(m_RecordIsReachPoitn[item].Values).FindAll((bool ite) => ite).Count != m_RecordIsReachPoitn[item].Count)
				{
					continue;
				}
				if (trigger)
				{
					if (PeGameMgr.IsMulti && (PlayerNetwork.mainPlayer != null || !fromNet))
					{
						PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_EntityReach, item, PlayerNetwork.mainPlayerId);
					}
					GetMissionOrPlotById(item);
				}
				list.Add(item);
			}
		}
		foreach (int item3 in list)
		{
			if (m_RecordIsReachPoitn.ContainsKey(item3))
			{
				m_RecordIsReachPoitn.Remove(item3);
			}
		}
	}

	public void GetMissionOrPlotById(int id)
	{
		switch (id / 10000)
		{
		case 1:
		{
			int item = id % 10000;
			if (!MissionManager.Instance.m_PlayerMission.IsGetTakeMission(item))
			{
				break;
			}
			if (MissionRepository.HaveTalkOP(item))
			{
				GameUI.Instance.mNPCTalk.NormalOrSP(0);
				if (!GameUI.Instance.mNPCTalk.isPlayingTalk)
				{
					GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(item, 1);
					GameUI.Instance.mNPCTalk.PreShow();
				}
				else
				{
					GameUI.Instance.mNPCTalk.AddNpcTalkInfo(item, 1);
				}
			}
			else
			{
				MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(item);
				if (missionCommonData != null)
				{
					PeEntity npc = PeSingleton<EntityMgr>.Instance.Get(missionCommonData.m_iNpc);
					MissionManager.Instance.SetGetTakeMission(item, npc, MissionManager.TakeMissionType.TakeMissionType_Get);
				}
			}
			break;
		}
		case 2:
		{
			int item = id % 10000;
			List<int> list = new List<int>();
			list.Add(item);
			Instance.PushStoryList(list);
			break;
		}
		}
	}

	private void KillMonster(PeEntity mon)
	{
		foreach (KillMons value in m_RecordKillMons.Values)
		{
			if (value.type == KillMons.Type.fixedId && PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(value.monId) == mon)
			{
				if (mon.GetComponent<PESkEntity>() != null)
				{
					mon.GetComponent<PESkEntity>().SetAttribute(AttribType.Hp, 0f, eventOff: false);
				}
				break;
			}
			if (value.type == KillMons.Type.protoTypeId)
			{
				if ((value.monId == -999 || mon.entityProto.protoId == value.monId) && Vector3.Distance(mon.position, value.center) <= value.radius && mon.GetComponent<PESkEntity>() != null)
				{
					mon.GetComponent<PESkEntity>().SetAttribute(AttribType.Hp, 0f, eventOff: false);
				}
				break;
			}
		}
	}

	private void ReplaceBuilding(List<int> oldBuilding, List<int> newBuilding)
	{
		try
		{
			foreach (int item in oldBuilding)
			{
				SceneDoodadLodCmpt component = PeSingleton<EntityMgr>.Instance.GetDoodadEntities(item)[0].GetComponent<SceneDoodadLodCmpt>();
				if (component == null)
				{
					return;
				}
				component.IsShown = false;
			}
			foreach (int item2 in newBuilding)
			{
				SceneDoodadLodCmpt component = PeSingleton<EntityMgr>.Instance.GetDoodadEntities(item2)[0].GetComponent<SceneDoodadLodCmpt>();
				if (component == null)
				{
					break;
				}
				component.IsShown = true;
			}
		}
		catch
		{
			Debug.LogError("Replacing buildingDoodad failed.");
		}
	}

	private void DestroyRailWay()
	{
		MapMaskData mapMaskData = MapMaskData.s_tblMaskData.Find((MapMaskData ret) => ret.mId == 29);
		if (mapMaskData == null)
		{
			return;
		}
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		foreach (Route route in PeSingleton<Manager>.Instance.GetRoutes())
		{
			for (int i = 0; i < route.pointCount; i++)
			{
				Point pointByIndex = route.GetPointByIndex(i);
				if (!(Vector3.Distance(pointByIndex.position, mapMaskData.mPosition) < 200f))
				{
					continue;
				}
				if (!list.Contains(pointByIndex.routeId))
				{
					list.Add(pointByIndex.routeId);
					if (null != route.train)
					{
						route.train.ClearPassenger();
					}
					foreach (KeyValuePair<int, PassengerInfo> passenger in m_Passengers)
					{
						if (passenger.Value.startRouteID == route.id)
						{
							list2.Add(passenger.Key);
						}
					}
					foreach (int item in list2)
					{
						Translate(PeSingleton<EntityMgr>.Instance.Get(item), m_Passengers[item].dest);
						m_Passengers.Remove(item);
					}
				}
				pointByIndex.Destroy();
			}
		}
		foreach (int item2 in list)
		{
			PeSingleton<RailwayOperate>.Instance.RequestDeleteRoute(item2);
		}
	}

	private void TakeMissionOrPlot(bool missionOrPlot, List<int> trigerId)
	{
		if (missionOrPlot)
		{
			for (int i = 0; i < trigerId.Count; i++)
			{
				if (!MissionManager.Instance.IsGetTakeMission(trigerId[i]))
				{
					continue;
				}
				MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(trigerId[i]);
				if (missionCommonData == null)
				{
					continue;
				}
				if (MissionRepository.HaveTalkOP(trigerId[i]))
				{
					GameUI.Instance.mNPCTalk.NormalOrSP(0);
					if (!GameUI.Instance.mNPCTalk.isPlayingTalk)
					{
						GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(trigerId[i], 1);
						GameUI.Instance.mNPCTalk.PreShow();
						continue;
					}
					int num = 0;
					List<int> list = GameUI.Instance.mNPCTalk.m_NpcTalkList.ConvertAll((UINPCTalk.NpcTalkInfo ite) => ite.talkid);
					int num2 = ((list.Count >= missionCommonData.m_TalkOP.Count) ? missionCommonData.m_TalkOP.Count : list.Count);
					for (int j = 0; j < num2; j++)
					{
						if (list[j] == missionCommonData.m_TalkOP[j])
						{
							num++;
						}
					}
					if (num != num2)
					{
						GameUI.Instance.mNPCTalk.AddNpcTalkInfo(trigerId[i], 1);
					}
				}
				else
				{
					PeEntity npc = PeSingleton<EntityMgr>.Instance.Get(missionCommonData.m_iNpc);
					MissionManager.Instance.SetGetTakeMission(trigerId[i], npc, MissionManager.TakeMissionType.TakeMissionType_Get, bCheck: false);
				}
			}
		}
		else
		{
			Instance.PushStoryList(trigerId);
		}
	}

	private IEnumerator CheckAttractMons(List<int> monsId, bool missionOrPlot, List<int> ids)
	{
		List<PeEntity> mons = new List<PeEntity>();
		PeEntity player = PeSingleton<PeCreature>.Instance.mainPlayer;
		bool canPass = false;
		while (true)
		{
			if (mons.Count < monsId.Count)
			{
				foreach (int item in monsId)
				{
					PeEntity mon = PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(item);
					if (mon == null)
					{
						break;
					}
					mons.Add(mon);
				}
				if (mons.Count < monsId.Count)
				{
					mons.Clear();
					yield return new WaitForSeconds(2f);
					continue;
				}
			}
			foreach (PeEntity item2 in mons)
			{
				if (item2.attackEnemy == null || !(item2.attackEnemy.entityTarget == player))
				{
					continue;
				}
				canPass = true;
				break;
			}
			if (canPass)
			{
				break;
			}
			yield return new WaitForSeconds(2f);
		}
		TakeMissionOrPlot(missionOrPlot, ids);
	}

	private IEnumerator CheckMonsExist(List<CheckMons> data)
	{
		bool[] n = new bool[data.Count - 1];
		CheckMons cm;
		while (true)
		{
			for (int i = 0; i < data.Count - 1; i++)
			{
				cm = data[i];
				Vector3 center = ((cm.npcid == 0) ? cm.center : ((cm.npcid != 20000) ? PeSingleton<EntityMgr>.Instance.Get(cm.npcid).position : PeSingleton<PeCreature>.Instance.mainPlayer.position));
				int num = PeSingleton<EntityMgr>.Instance.GetHatredEntities(center, cm.radius, cm.protoTypeid).Length;
				if ((cm.existOrNot && num > 0) || (!cm.existOrNot && num <= 0))
				{
					n[i] = true;
				}
			}
			if (!Array.Exists(n, (bool ite) => !ite))
			{
				break;
			}
			yield return new WaitForSeconds(2f);
		}
		cm = data[data.Count - 1];
		TakeMissionOrPlot(cm.missionOrPlot, cm.trigerId);
	}

	private IEnumerator WaitDayuLowHP()
	{
		PeEntity dayu = PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(94);
		if (!(null == dayu))
		{
			while (dayu.GetAttribute(AttribType.Hp) > 800f)
			{
				yield return new WaitForSeconds(0.5f);
			}
			List<int> tmp = new List<int> { 282 };
			Instance.PushStoryList(tmp);
		}
	}

	private IEnumerator WaitPlotMissionTrigger(List<string> triggerData)
	{
		int enterOrExit = Convert.ToInt32(triggerData[0]);
		List<string> tmp = new List<string>(triggerData[2].Split(','));
		Vector3 position;
		if (tmp.Count == 3)
		{
			position = new Vector3(Convert.ToSingle(tmp[0]), Convert.ToSingle(tmp[1]), Convert.ToSingle(tmp[2]));
		}
		else
		{
			if (!(PeSingleton<EntityMgr>.Instance.Get(Convert.ToInt32(tmp[0])) != null))
			{
				yield break;
			}
			position = PeSingleton<EntityMgr>.Instance.Get(Convert.ToInt32(tmp[0])).position;
		}
		if (position == new Vector3(-255f, -255f, -255f))
		{
			if (!CSMain.HasCSAssembly())
			{
				yield break;
			}
			CSMain.GetAssemblyPos(out position);
		}
		int radius = Convert.ToInt32(triggerData[3]);
		int plotOrMission = Convert.ToInt32(triggerData[4]);
		List<int> triggerID = new List<int>();
		tmp = new List<string>(triggerData[5].Split(','));
		for (int i = 0; i < tmp.Count; i++)
		{
			triggerID.Add(Convert.ToInt32(tmp[i]));
		}
		List<PeEntity> entities = new List<PeEntity>();
		tmp = new List<string>(triggerData[1].Split(','));
		for (int j = 0; j < tmp.Count; j++)
		{
			if (tmp[j] == "20000")
			{
				entities.Add(PeSingleton<PeCreature>.Instance.mainPlayer);
			}
			else if (PeSingleton<EntityMgr>.Instance.Get(Convert.ToInt32(tmp[j])) != null)
			{
				entities.Add(PeSingleton<EntityMgr>.Instance.Get(Convert.ToInt32(tmp[j])));
			}
		}
		while ((enterOrExit != 1 || entities.FindAll((PeEntity ite) => Vector3.Distance(position, ite.position) <= (float)radius).Count != entities.Count) && (enterOrExit != 2 || entities.FindAll((PeEntity ite) => Vector3.Distance(position, ite.position) >= (float)radius).Count != entities.Count))
		{
			yield return new WaitForSeconds(1f);
		}
		TakeMissionOrPlot(plotOrMission != 1, triggerID);
	}

	public IEnumerator WaitingNpcRailStart(Point start, PeEntity npc, Point dest, Vector3 pos)
	{
		yield return 0;
		if (!(npc.NpcCmpt != null) || !npc.NpcCmpt.Req_Contains(EReqType.FollowTarget))
		{
			MoveTo(npc, start.position, 6f, bForce: true, SpeedState.Run);
			while (!(start.GetArriveTime() < 0.3f) || !(Vector3.Distance(npc.position, start.position) < 10f))
			{
				yield return new WaitForSeconds(0.2f);
			}
			if (m_Passengers.ContainsKey(npc.Id))
			{
				m_Passengers[npc.Id].type = PassengerInfo.Course.on;
			}
			npc.GetOnTrain(start.routeId, checkState: false);
			StartCoroutine(WaitingNpcRailEnd(dest, npc, pos));
		}
	}

	public IEnumerator WaitingNpcRailEnd(Point dest, PeEntity npc, Vector3 pos)
	{
		yield return 0;
		while (true)
		{
			if (dest.GetArriveTime() < 0.3f)
			{
				if (!(npc == null))
				{
					break;
				}
			}
			else
			{
				yield return new WaitForSeconds(0.2f);
			}
		}
		if (m_Passengers.ContainsKey(npc.Id))
		{
			m_Passengers[npc.Id].type = PassengerInfo.Course.latter;
		}
		Vector3 rpos = AiUtil.GetRandomPosition(dest.position, 4f, 6f, 100f, AiUtil.groundedLayer, 10);
		npc.GetOffTrain(rpos);
		MoveTo(npc, pos, 1f, bForce: true, SpeedState.Run);
		m_Passengers.Remove(npc.Id);
	}

	private void ProcessCamera(CameraPlot cpData)
	{
		if (cpData == null)
		{
			return;
		}
		m_bCamModing = true;
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = Vector3.zero;
		if (cpData.m_CamMove.npcid > 0)
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(cpData.m_CamMove.npcid);
			if (peEntity == null)
			{
				return;
			}
			vector = peEntity.position;
			if (cpData.m_CamMove.dirType == 1)
			{
				vector = vector + peEntity.GetForward() * 5f + Vector3.up * 5f;
			}
			else if (cpData.m_CamMove.dirType == 2)
			{
				vector = vector + peEntity.GetForward() * -5f + Vector3.up * 5f;
			}
		}
		else if (cpData.m_CamMove.pos != Vector3.zero)
		{
			vector = cpData.m_CamMove.pos;
		}
		if (cpData.m_CamMove.type == 1)
		{
			if (m_CamIdx == 0)
			{
				mFollowCameraTarget.position = PEUtil.MainCamTransform.position;
				mFollowCameraTarget.rotation = PEUtil.MainCamTransform.rotation;
			}
			StartCoroutine(CamFollowMove(vector, cpData.m_Delay));
			if (mCamMode == null || PECameraMan.Instance.m_Controller.currentMode != mCamMode)
			{
				mCamMode = PECameraMan.Instance.EnterFollow(mFollowCameraTarget);
			}
		}
		else if (cpData.m_CamMove.type == 2)
		{
			mFollowCameraTarget.position = vector;
			if (m_CamIdx == 0)
			{
				mFollowCameraTarget.rotation = PEUtil.MainCamTransform.rotation;
			}
			if (mCamMode == null || PECameraMan.Instance.m_Controller.currentMode != mCamMode)
			{
				mCamMode = PECameraMan.Instance.EnterFollow(mFollowCameraTarget);
			}
		}
		if (cpData.m_CamRot.angleY != 0f || cpData.m_CamRot.angleX != 0f)
		{
			float num = 0f;
			float num2 = 0f;
			if (cpData.m_CamRot.type1 == 1)
			{
				vector2 = Vector3.up;
				num = ((cpData.m_CamRot.dirType != 1) ? (cpData.m_CamRot.angleY - 360f) : cpData.m_CamRot.angleY);
			}
			else if (cpData.m_CamRot.type1 == 2)
			{
				vector2 = Vector3.right;
				num = ((cpData.m_CamRot.dirType != 1) ? (cpData.m_CamRot.angleY - 360f) : cpData.m_CamRot.angleY);
			}
			else if (cpData.m_CamRot.type1 == 3)
			{
				vector2 = Vector3.forward;
				if (cpData.m_CamRot.dirType == 1)
				{
					num = cpData.m_CamRot.angleY;
					num2 = cpData.m_CamRot.angleX;
				}
				else
				{
					num = cpData.m_CamRot.angleY - 360f;
					num2 = cpData.m_CamRot.angleX - 360f;
				}
			}
			num2 = ((!(num2 < -360f)) ? num2 : (num2 + 360f));
			num = ((!(num < -360f)) ? num : (num + 360f));
			if (m_CamIdx == 0)
			{
				mFollowCameraTarget.position = PEUtil.MainCamTransform.position;
			}
			if (cpData.m_CamRot.type == 1)
			{
				StartCoroutine(CamFollow(0, 0, 2, num, vector2, Vector3.zero, cpData.m_Delay, num2));
				if (mCamMode == null || PECameraMan.Instance.m_Controller.currentMode != mCamMode)
				{
					mCamMode = PECameraMan.Instance.EnterFollow(mFollowCameraTarget);
				}
			}
			else if (cpData.m_CamRot.type == 2)
			{
				Quaternion rotation = default(Quaternion);
				if (vector2 == Vector3.up)
				{
					rotation = Quaternion.Euler(new Vector3(0f, num, 0f));
				}
				else if (vector2 == Vector3.right)
				{
					rotation = Quaternion.Euler(new Vector3(0f - num, 0f, 0f));
				}
				else if (vector2 == Vector3.forward)
				{
					rotation = Quaternion.Euler(new Vector3(0f - num2, num, 0f));
				}
				mFollowCameraTarget.rotation = rotation;
			}
			if (mCamMode == null || PECameraMan.Instance.m_Controller.currentMode != mCamMode)
			{
				mCamMode = PECameraMan.Instance.EnterFollow(mFollowCameraTarget);
			}
		}
		if (cpData.m_CamTrack.npcid > 0)
		{
			if (m_CamIdx == 0)
			{
				mFollowCameraTarget.position = PEUtil.MainCamTransform.position;
				mFollowCameraTarget.rotation = PEUtil.MainCamTransform.rotation;
			}
			if (mCamMode == null || PECameraMan.Instance.m_Controller.currentMode != mCamMode)
			{
				mCamMode = PECameraMan.Instance.EnterFollow(mFollowCameraTarget);
			}
			StartCoroutine(CamFollow(cpData.m_CamTrack.npcid, cpData.m_CamTrack.type, 3, 0f, Vector3.zero, Vector3.zero, cpData.m_Delay, 0f));
		}
		if (cpData.m_CamToPlayer)
		{
		}
		m_bCamModing = false;
	}

	private IEnumerator CamFollowMove(Vector3 distpos, int delay)
	{
		bool bfinish = false;
		m_bCamMoveModing = true;
		while (!bfinish)
		{
			mFollowCameraTarget.position = Vector3.Lerp(mFollowCameraTarget.position, distpos, 0.03f);
			float dist = Vector3.Distance(mFollowCameraTarget.position, distpos);
			if (dist < 1f)
			{
				bfinish = true;
			}
			yield return new WaitForSeconds(0.01f);
		}
		yield return new WaitForSeconds(delay);
		m_bCamMoveModing = false;
	}

	private IEnumerator CamFollow(int npcid, int type, int CameraType, float angle, Vector3 dir, Vector3 distpos, int delay, float angle1)
	{
		PeEntity npc = null;
		Vector3 velocity = Vector3.zero;
		Quaternion endQua = default(Quaternion);
		Quaternion endQua2 = default(Quaternion);
		bool bfinish = false;
		bool bdouble = false;
		float trueangle = angle;
		float trueangle2 = angle1;
		float rotateSpeed = 30f;
		m_bCamRotModing = true;
		switch (CameraType)
		{
		case 2:
			if (angle > 180f || angle < -180f)
			{
				bdouble = true;
				trueangle = angle / 2f;
			}
			if (angle1 > 180f || angle1 < -180f)
			{
				bdouble = true;
				trueangle2 = angle1 / 2f;
			}
			if (dir == Vector3.up)
			{
				endQua = Quaternion.Euler(new Vector3(0f, trueangle, 0f));
				endQua2 = Quaternion.Euler(new Vector3(0f, angle, 0f));
			}
			else if (dir == Vector3.right)
			{
				endQua = Quaternion.Euler(new Vector3(0f - trueangle, 0f, 0f));
				endQua2 = Quaternion.Euler(new Vector3(0f - angle, 0f, 0f));
			}
			else if (dir == Vector3.forward)
			{
				endQua = Quaternion.Euler(new Vector3(0f - trueangle2, trueangle, 0f));
				endQua2 = Quaternion.Euler(new Vector3(0f - angle1, angle, 0f));
			}
			break;
		case 3:
			npc = PeSingleton<EntityMgr>.Instance.Get(npcid);
			if (npc == null)
			{
				bfinish = true;
			}
			break;
		}
		while (!bfinish)
		{
			switch (CameraType)
			{
			case 2:
			{
				float t = rotateSpeed / Quaternion.Angle(mFollowCameraTarget.rotation, endQua) * Time.deltaTime;
				mFollowCameraTarget.rotation = Quaternion.Slerp(mFollowCameraTarget.rotation, endQua, t);
				float dist = Quaternion.Angle(mFollowCameraTarget.rotation, endQua);
				if (dist < 1f)
				{
					if (bdouble)
					{
						bdouble = false;
						endQua = endQua2;
					}
					else
					{
						bfinish = true;
					}
				}
				break;
			}
			case 3:
				switch (type)
				{
				case 1:
				{
					Vector3 endpos2 = npc.position + Vector3.up * 5f + npc.GetForward() * -16f;
					mFollowCameraTarget.position = Vector3.SmoothDamp(mFollowCameraTarget.position, endpos2, ref velocity, 0.3f);
					break;
				}
				case 2:
					mFollowCameraTarget.LookAt(npc.GetGameObject().transform);
					break;
				case 3:
				{
					Vector3 endpos = npc.position + Vector3.up * 5f + npc.GetForward() * -16f;
					mFollowCameraTarget.position = Vector3.SmoothDamp(mFollowCameraTarget.position, endpos, ref velocity, 0.1f);
					Vector3 relativePos = endpos - mFollowCameraTarget.position;
					Quaternion rotation2 = Quaternion.LookRotation(relativePos);
					mFollowCameraTarget.rotation = rotation2;
					break;
				}
				}
				break;
			default:
			{
				float dist = Vector3.Distance(distpos, mFollowCameraTarget.position);
				dist /= 30f;
				mFollowCameraTarget.position = Vector3.SmoothDamp(mFollowCameraTarget.position, distpos, ref velocity, dist);
				dist = Vector3.Distance(mFollowCameraTarget.position, distpos);
				if (dist < 1f)
				{
					bfinish = true;
					Quaternion rotation = Quaternion.LookRotation(PeSingleton<PeCreature>.Instance.mainPlayer.GetGameObject().transform.forward);
					mFollowCameraTarget.rotation = rotation;
					PECameraMan.Instance.RemoveCamMode(mCamMode);
				}
				break;
			}
			}
			yield return new WaitForSeconds(0.01f);
		}
		yield return new WaitForSeconds(delay);
		m_bCamRotModing = false;
	}

	public void PushStoryList(List<int> idlist)
	{
		for (int i = 0; i < idlist.Count; i++)
		{
			int num = idlist[i];
			if (num == -1)
			{
				continue;
			}
			StoryData stroyData = StoryRepository.GetStroyData(num);
			if (stroyData == null)
			{
				continue;
			}
			if (stroyData.m_Delay > float.Epsilon && !m_MisDelay.ContainsKey(num))
			{
				m_MisDelay.Add(num, Time.time);
			}
			if (!m_StoryList.ContainsKey(num))
			{
				if (PeGameMgr.IsMulti)
				{
					PlayerNetwork._storyPlot.Add(num);
				}
				m_StoryList.Add(num, MissionManager.Instance.m_bHadInitMission);
			}
		}
	}

	public void PushAdStoryList(List<int> idlist, int npcid = -1)
	{
		for (int i = 0; i < idlist.Count; i++)
		{
			int num = idlist[i];
			AdStoryData adStroyData = StoryRepository.GetAdStroyData(num);
			if (adStroyData != null && !m_AdStoryList.Contains(num))
			{
				m_AdStoryList.Add(num);
				m_AdStoryListNpc.Add(num, npcid);
			}
		}
	}

	private Vector3 Str2V3(string param)
	{
		string[] array = param.Split(',');
		if (array.Length < 3)
		{
			return Vector3.zero;
		}
		float x = Convert.ToSingle(array[0]);
		float y = Convert.ToSingle(array[1]);
		float z = Convert.ToSingle(array[2]);
		return new Vector3(x, y, z);
	}

	public static void CreateAndHeraNest(int index)
	{
		if (PeGameMgr.IsSingle)
		{
			switch (index)
			{
			case 0:
				CreateAndHeraNest_index(index, "Prefab/Item/Other/coelodonta_rhino_bone", new Vector3(5164.348f, 480.4204f, 12205.1f), new Vector3(340f, 68f, 3f), 1569);
				break;
			case 1:
				CreateAndHeraNest_index(index, "Prefab/Item/Other/coelodonta_rhino_bone", new Vector3(5159.757f, 478.46f, 12206.4f), default(Vector3), 1569);
				break;
			case 2:
				CreateAndHeraNest_index(index, "Prefab/Item/Other/lepus_hare_bone", new Vector3(5159.303f, 478.9189f, 12192.89f), default(Vector3), 1568);
				break;
			case 3:
				CreateAndHeraNest_index(index, "Prefab/Item/Other/andhera_queen_egg", new Vector3(5159.537f, 479.3898f, 12186.93f), default(Vector3), 1570);
				break;
			case 4:
				CreateAndHeraNest_index(index, "Prefab/Item/Other/andhera_queen_egg", new Vector3(5158.81f, 478.58f, 12185.42f), new Vector3(9f, 2f, 20f), 1570);
				break;
			case 5:
				CreateAndHeraNest_index(index, "Prefab/Item/Other/andhera_queen_egg", new Vector3(5161.023f, 480.3488f, 12184.96f), new Vector3(0f, 0f, 30f), 1570);
				break;
			case 6:
				CreateAndHeraNest_index(index, "Prefab/Item/Other/andhera_queen_egg", new Vector3(5159.489f, 478.992f, 12183f), new Vector3(15f, 2f, 5f), 1570);
				break;
			case 7:
				CreateAndHeraNest_index(index, "Prefab/Item/Other/andhera_queen_egg", new Vector3(5157.84f, 478.05f, 12184.52f), new Vector3(350f, 358f, 7f), 1570);
				break;
			}
		}
		else
		{
			switch (index)
			{
			case 0:
				PlayerNetwork.mainPlayer.CreateSceneItem("coelodonta_rhino_bone0", new Vector3(5164.348f, 480.4204f, 12205.1f), "1569,1", -1, precise: true);
				break;
			case 1:
				PlayerNetwork.mainPlayer.CreateSceneItem("coelodonta_rhino_bone1", new Vector3(5159.757f, 478.46f, 12206.4f), "1569,1", -1, precise: true);
				break;
			case 2:
				PlayerNetwork.mainPlayer.CreateSceneItem("lepus_hare_bone2", new Vector3(5159.303f, 478.9189f, 12192.89f), "1568,1", -1, precise: true);
				break;
			case 3:
				PlayerNetwork.mainPlayer.CreateSceneItem("andhera_queen_egg3", new Vector3(5159.537f, 479.3898f, 12186.93f), "1570,1", -1, precise: true);
				break;
			case 4:
				PlayerNetwork.mainPlayer.CreateSceneItem("andhera_queen_egg4", new Vector3(5158.81f, 478.58f, 12185.42f), "1570,1", -1, precise: true);
				break;
			case 5:
				PlayerNetwork.mainPlayer.CreateSceneItem("andhera_queen_egg5", new Vector3(5161.023f, 480.3488f, 12184.96f), "1570,1", -1, precise: true);
				break;
			case 6:
				PlayerNetwork.mainPlayer.CreateSceneItem("andhera_queen_egg6", new Vector3(5159.489f, 478.992f, 12183f), "1570,1", -1, precise: true);
				break;
			case 7:
				PlayerNetwork.mainPlayer.CreateSceneItem("andhera_queen_egg7", new Vector3(5157.84f, 478.05f, 12184.52f), "1570,1", -1, precise: true);
				break;
			}
		}
	}

	private static void CreateAndHeraNest_index(int index, string path, Vector3 pos, Vector3 euler, int protoID)
	{
		UnityEngine.Object original = Resources.Load(path);
		GameObject go = UnityEngine.Object.Instantiate(original) as GameObject;
		go.transform.position = pos;
		go.transform.eulerAngles = euler;
		ItemDrop component = go.GetComponent<ItemDrop>();
		if (component != null)
		{
			component.AddItem(protoID, 1);
			MissionManager.Instance.m_PlayerMission.recordAndHer.Add(index);
			component.fetchItem = (Action)Delegate.Combine(component.fetchItem, (Action)delegate
			{
				UnityEngine.Object.Destroy(go);
				MissionManager.Instance.m_PlayerMission.recordAndHer.Remove(index);
			});
		}
	}

	public static ItemDrop CreateAndHeraNest_indexNet(string objName, Vector3 objPos, List<int> itemObjId = null, MapObjNetwork net = null)
	{
		int index = Convert.ToInt32(objName.Substring(objName.Length - 1, 1));
		string path = string.Empty;
		Vector3 eulerAngles = default(Vector3);
		switch (index)
		{
		case 0:
			path = "Prefab/Item/Other/coelodonta_rhino_bone";
			eulerAngles = new Vector3(340f, 68f, 3f);
			break;
		case 1:
			path = "Prefab/Item/Other/coelodonta_rhino_bone";
			eulerAngles = default(Vector3);
			break;
		case 2:
			path = "Prefab/Item/Other/lepus_hare_bone";
			eulerAngles = default(Vector3);
			break;
		case 3:
			path = "Prefab/Item/Other/andhera_queen_egg";
			eulerAngles = default(Vector3);
			break;
		case 4:
			path = "Prefab/Item/Other/andhera_queen_egg";
			eulerAngles = new Vector3(9f, 2f, 20f);
			break;
		case 5:
			path = "Prefab/Item/Other/andhera_queen_egg";
			eulerAngles = new Vector3(0f, 0f, 30f);
			break;
		case 6:
			path = "Prefab/Item/Other/andhera_queen_egg";
			eulerAngles = new Vector3(15f, 2f, 5f);
			break;
		case 7:
			path = "Prefab/Item/Other/andhera_queen_egg";
			eulerAngles = new Vector3(350f, 358f, 7f);
			break;
		}
		UnityEngine.Object original = Resources.Load(path);
		GameObject go = UnityEngine.Object.Instantiate(original) as GameObject;
		go.transform.position = objPos;
		go.transform.eulerAngles = eulerAngles;
		ItemDrop component = go.GetComponent<ItemDrop>();
		if (component != null)
		{
			component.SetNet(net);
			foreach (int item in itemObjId)
			{
				if (PeSingleton<ItemMgr>.Instance.Get(item) != null)
				{
					component.AddItem(PeSingleton<ItemMgr>.Instance.Get(item));
				}
				else
				{
					Debug.LogError(objName + " item is null");
				}
			}
			MissionManager.Instance.m_PlayerMission.recordAndHer.Add(index);
			component.fetchItem = (Action)Delegate.Combine(component.fetchItem, (Action)delegate
			{
				UnityEngine.Object.Destroy(go);
				MissionManager.Instance.m_PlayerMission.recordAndHer.Remove(index);
			});
		}
		return component;
	}

	public static void CreateLanguageSample_byIndex(int index)
	{
		if (index >= 0 && index <= 19)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(SamplePosObj) as GameObject;
			Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>();
			GameObject gameObject2 = UnityEngine.Object.Instantiate(SampleObj) as GameObject;
			StringBuilder stringBuilder = new StringBuilder(gameObject2.name);
			stringBuilder.AppendFormat(":{0}", index);
			gameObject2.name = stringBuilder.ToString();
			gameObject2.transform.position = componentsInChildren[index + 1].position;
			gameObject2.transform.eulerAngles = componentsInChildren[index + 1].eulerAngles;
			ItemDrop component = gameObject2.GetComponent<ItemDrop>();
			if (component != null)
			{
				component.AddItem(1541, 1);
			}
			UnityEngine.Object.Destroy(gameObject);
		}
	}

	public static void CreateLanguageSample()
	{
		if (MissionManager.Instance.m_PlayerMission.textSamples.Count > 0)
		{
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(SamplePosObj) as GameObject;
		Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>();
		for (int i = 1; i < componentsInChildren.Length; i++)
		{
			if (PeGameMgr.IsSingle)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(SampleObj) as GameObject;
				StringBuilder stringBuilder = new StringBuilder(gameObject2.name);
				stringBuilder.AppendFormat(":{0}", i - 1);
				gameObject2.name = stringBuilder.ToString();
				gameObject2.transform.position = componentsInChildren[i].position;
				gameObject2.transform.eulerAngles = componentsInChildren[i].eulerAngles;
				ItemDrop component = gameObject2.GetComponent<ItemDrop>();
				if (component != null)
				{
					component.AddItem(1541, 1);
				}
				MissionManager.Instance.m_PlayerMission.textSamples.Add(i - 1, componentsInChildren[i].position);
			}
			else
			{
				string sceneItemName = "language_sample_canUse(Clone):" + (i - 1);
				PlayerNetwork.mainPlayer.CreateSceneItem(sceneItemName, componentsInChildren[i].position, "1541,1");
			}
		}
		UnityEngine.Object.Destroy(gameObject);
	}

	public static ItemDrop CreateLanguageSampleNet(string objName, Vector3 objPos, List<int> itemObjId = null, MapObjNetwork net = null)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(SampleObj) as GameObject;
		StringBuilder stringBuilder = new StringBuilder(gameObject.name);
		gameObject.name = objName;
		gameObject.transform.position = objPos;
		ItemDrop component = gameObject.GetComponent<ItemDrop>();
		if (component != null)
		{
			component.SetNet(net);
			foreach (int item in itemObjId)
			{
				if (PeSingleton<ItemMgr>.Instance.Get(item) != null)
				{
					component.AddItem(PeSingleton<ItemMgr>.Instance.Get(item));
				}
				else
				{
					Debug.LogError(objName + " item is null");
				}
			}
		}
		string value = objName.Replace("language_sample_canUse(Clone):", string.Empty);
		int key = Convert.ToInt32(value);
		if (!MissionManager.Instance.m_PlayerMission.textSamples.ContainsKey(key))
		{
			MissionManager.Instance.m_PlayerMission.textSamples.Add(key, objPos);
		}
		return component;
	}

	public static ItemDrop CreateBackpack(Vector3 objPos, List<int> itemObjId = null, MapObjNetwork net = null)
	{
		UnityEngine.Object @object = Resources.Load("Prefab/Item/Other/backpack");
		if (null != @object)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(@object) as GameObject;
			gameObject.transform.position = objPos;
			gameObject.name = "backpack";
			ItemDrop component = gameObject.GetComponent<ItemDrop>();
			if (component != null)
			{
				if (PeGameMgr.IsSingleStory)
				{
					component.AddItem(1332, 1);
				}
				else
				{
					component.SetNet(net);
					foreach (int item in itemObjId)
					{
						if (PeSingleton<ItemMgr>.Instance.Get(item) != null)
						{
							component.AddItem(PeSingleton<ItemMgr>.Instance.Get(item));
						}
						else
						{
							Debug.LogError(gameObject.name + " item is null");
						}
					}
				}
				return component;
			}
		}
		return null;
	}

	public static ItemDrop CreatePajaLanguage(Vector3 objPos, List<int> itemObjId = null, MapObjNetwork net = null)
	{
		UnityEngine.Object @object = Resources.Load("Prefab/Item/Other/Paja_language_sample");
		if (null != @object)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(@object) as GameObject;
			gameObject.transform.position = objPos;
			gameObject.name = "pajaLanguage";
			ItemDrop component = gameObject.GetComponent<ItemDrop>();
			if (component == null)
			{
				return null;
			}
			if (PeGameMgr.IsSingle)
			{
				component.AddItem(1508, 1);
			}
			else
			{
				component.SetNet(net);
				foreach (int item in itemObjId)
				{
					if (PeSingleton<ItemMgr>.Instance.Get(item) != null)
					{
						component.AddItem(PeSingleton<ItemMgr>.Instance.Get(item));
					}
					else
					{
						Debug.LogError("Paja_language item is null");
					}
				}
			}
			languages.Add(component);
			return component;
		}
		return null;
	}

	public static ItemDrop CreateProbe(Vector3 objPos, List<int> itemObjId = null, MapObjNetwork net = null)
	{
		UnityEngine.Object @object = Resources.Load("Prefab/Item/Other/probe_michelson02");
		if (null != @object)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(@object) as GameObject;
			gameObject.transform.position = objPos;
			gameObject.name = "probe";
			ItemDrop componentInChildren = gameObject.GetComponentInChildren<ItemDrop>();
			if (componentInChildren != null)
			{
				if (PeGameMgr.IsSingleStory)
				{
					componentInChildren.AddItem(1340, 1);
				}
				else
				{
					componentInChildren.SetNet(net);
					foreach (int item in itemObjId)
					{
						if (PeSingleton<ItemMgr>.Instance.Get(item) != null)
						{
							componentInChildren.AddItem(PeSingleton<ItemMgr>.Instance.Get(item));
						}
						else
						{
							Debug.LogError("probe item is null");
						}
					}
				}
				return componentInChildren;
			}
		}
		return null;
	}

	public static ItemDrop CreateHugefish_bone(Vector3 objPos, List<int> itemObjId = null, MapObjNetwork net = null)
	{
		UnityEngine.Object @object = Resources.Load("Prefab/Item/Other/hugefish_bone");
		if (null == @object)
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(@object) as GameObject;
		gameObject.transform.position = objPos;
		gameObject.name = "hugefish_bone";
		ItemDrop componentInChildren = gameObject.GetComponentInChildren<ItemDrop>();
		if (componentInChildren != null)
		{
			if (PeGameMgr.IsSingleStory)
			{
				componentInChildren.AddItem(1342, 1);
			}
			else
			{
				componentInChildren.SetNet(net);
				foreach (int item in itemObjId)
				{
					if (PeSingleton<ItemMgr>.Instance.Get(item) != null)
					{
						componentInChildren.AddItem(PeSingleton<ItemMgr>.Instance.Get(item));
					}
					else
					{
						Debug.LogError("Hugefish_bone item is null");
					}
				}
			}
			return componentInChildren;
		}
		return null;
	}

	public static ItemDrop Createlarve_Q425(Vector3 objPos)
	{
		UnityEngine.Object @object = Resources.Load("Prefab/Item/Other/larve_Q425");
		if (null == @object)
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(@object) as GameObject;
		gameObject.transform.position = objPos;
		return null;
	}

	public static ItemDrop CreateFruitpack(Vector3 objPos)
	{
		UnityEngine.Object @object = Resources.Load("Prefab/Item/Other/iron_bed");
		if (@object != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(@object) as GameObject;
			gameObject.name = "fruitpack";
			gameObject.transform.position = objPos;
		}
		return null;
	}

	public static ItemDrop CreateAsh_box(Vector3 objPos, int entityId, List<int> itemObjId = null, MapObjNetwork net = null)
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(entityId);
		UnityEngine.Object @object = Resources.Load("Prefab/Item/Other/DropItem");
		if (null != @object)
		{
			List<int> list = new List<int>();
			foreach (int key in MissionManager.Instance.m_PlayerMission.m_MissionInfo.Keys)
			{
				MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(key);
				if (missionCommonData.m_iReplyNpc == peEntity.Id)
				{
					list.Add(key);
				}
			}
			foreach (int item in list)
			{
				MissionManager.Instance.FailureMission(item);
			}
			ItemDrop itemDrop;
			if (!PeGameMgr.IsSingleStory)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(@object, objPos, Quaternion.identity) as GameObject;
				gameObject.name = "ashBox_Sphere";
				itemDrop = gameObject.GetComponent<ItemDrop>();
				if (itemDrop == null)
				{
					itemDrop = gameObject.AddComponent<ItemDrop>();
				}
				if (itemDrop != null)
				{
					itemDrop.SetNet(net);
					foreach (int item2 in itemObjId)
					{
						if (PeSingleton<ItemMgr>.Instance.Get(item2) != null)
						{
							itemDrop.AddItem(PeSingleton<ItemMgr>.Instance.Get(item2));
						}
						else
						{
							Debug.LogError("Ash_box item is null");
						}
					}
					if (peEntity != null && peEntity.skEntity != null && peEntity.skEntity._net != null && peEntity.skEntity.IsController())
					{
						peEntity.skEntity._net.RPCServer(EPacketType.PT_NPC_Destroy);
					}
				}
			}
			else
			{
				if (peEntity == null)
				{
					return null;
				}
				ItemBox itemBox = ItemBoxMgr.Instance.AddItemSinglePlay(objPos);
				itemBox.AddItem(PeSingleton<ItemMgr>.Instance.CreateItem(1339));
				for (int i = 0; i < peEntity.GetComponent<EquipmentCmpt>()._ItemList.Count; i++)
				{
					itemBox.AddItem(peEntity.GetComponent<EquipmentCmpt>()._ItemList[i]);
				}
				itemDrop = null;
			}
			if (peEntity != null)
			{
				Singleton<PeLogicGlobal>.Instance.DestroyEntity(peEntity.skEntity);
			}
			return itemDrop;
		}
		return null;
	}

	public static ItemDrop CreateAsh_ball(Vector3 objPos, List<int> itemObjId = null, MapObjNetwork net = null)
	{
		UnityEngine.Object @object = Resources.Load("Prefab/Other/ItemBox");
		if (null == @object)
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(@object) as GameObject;
		gameObject.transform.position = objPos;
		ItemDrop itemDrop = gameObject.GetComponentInChildren<ItemDrop>();
		if (itemDrop == null)
		{
			itemDrop = gameObject.gameObject.AddComponent<ItemDrop>();
		}
		if (itemDrop != null)
		{
			if (!PeGameMgr.IsSingleStory)
			{
				itemDrop.SetNet(net);
				foreach (int item in itemObjId)
				{
					if (PeSingleton<ItemMgr>.Instance.Get(item) != null)
					{
						itemDrop.AddItem(PeSingleton<ItemMgr>.Instance.Get(item));
					}
					else
					{
						Debug.LogError("Ash_ball item is null");
					}
				}
			}
			return itemDrop;
		}
		return null;
	}

	private IEnumerator GameEnd()
	{
		if (!(Mctalk != null))
		{
			yield break;
		}
		CutsceneClip clip = Mctalk.GetComponentInChildren<CutsceneClip>();
		if (clip != null)
		{
			clip.testClip = true;
		}
		clip.transform.SetParent(null, worldPositionStays: true);
		Transform[] trans = Mctalk.GetComponentsInChildren<Transform>(includeInactive: true);
		trans[1].SetParent(null, worldPositionStays: true);
		float dis = 0f;
		if (PeGameMgr.IsSingle)
		{
			PeSingleton<PeCreature>.Instance.mainPlayer.biologyViewCmpt.Fadeout();
		}
		bool beginFoward = false;
		while (dis < 30f)
		{
			if (!beginFoward)
			{
				mcTalk.transform.position += Vector3.up * Time.deltaTime * 4f;
				clip.transform.position += Vector3.up * Time.deltaTime * 4f;
				if (dis > 20f)
				{
					beginFoward = true;
				}
			}
			else
			{
				mcTalk.transform.position += -mcTalk.transform.forward * Time.deltaTime * dis;
			}
			dis += Time.deltaTime;
			yield return 0;
		}
		if (PeGameMgr.IsMulti)
		{
			MessageBox_N.ShowYNBox(PELocalization.GetString(8000621), null, PeSceneCtrl.Instance.GotoLobbyScene);
		}
		else
		{
			Application.LoadLevel("GameED");
		}
	}

	private void ProcessSpecial(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return;
		}
		string[] array = str.Split(';');
		if (array.Length == 0)
		{
			return;
		}
		string text = array[0];
		switch (text)
		{
		case "Vploas_1":
		{
			PeEntity npc = PeSingleton<EntityMgr>.Instance.Get(9056);
			if (CSMain.HasCSAssembly())
			{
				if (CSMain.GetAssemblyLogic() != null)
				{
					Translate(npc, CSMain.GetAssemblyLogic().m_NPCTrans[19].position);
				}
				moveVploas = true;
			}
			return;
		}
		case "RecordMission":
			MissionManager.Instance.m_PlayerMission.isRecordCreation = true;
			return;
		case "EnableBook":
			enableBook = true;
			return;
		case "Mons":
		{
			string[] array2 = array[1].Split(',');
			string[] array3 = array2;
			foreach (string text2 in array3)
			{
				string[] array4 = text2.Split('_');
				if (array4.Length == 2)
				{
					int protoId = ((Convert.ToInt32(array4[0]) != 0) ? (Convert.ToInt32(array4[1]) | 0x40000000) : Convert.ToInt32(array4[1]));
					Vector3 pos2 = Vector3.zero;
					if (CSMain.HasCSAssembly())
					{
						CSMain.GetAssemblyPos(out pos2);
					}
					else if (PeSingleton<PeCreature>.Instance.mainPlayer != null)
					{
						pos2 = PeSingleton<PeCreature>.Instance.mainPlayer.peTrans.position;
					}
					pos2 = AiUtil.GetRandomPositionInLand(pos2, 30f, 50f, 10f, LayerMask.GetMask("Default", "VFVoxelTerrain", "SceneStatic"), 30);
					MonsterEntityCreator.CreateMonster(protoId, pos2);
				}
			}
			return;
		}
		case "gameend":
			StartCoroutine(GameEnd());
			return;
		case "McTalk":
		{
			string[] array5 = array[1].Split(',');
			if (Mctalk == null)
			{
				return;
			}
			Transform[] componentsInChildren = Mctalk.GetComponentsInChildren<Transform>(includeInactive: true);
			string[] array6 = array5;
			foreach (string value in array6)
			{
				int num = Convert.ToInt32(value);
				int[] array7 = new int[0];
				switch (num)
				{
				case 1:
					array7 = new int[2] { 3, 4 };
					break;
				case 2:
					array7 = new int[2] { 5, 6 };
					break;
				case 3:
					array7 = new int[4] { 7, 8, 9, 10 };
					break;
				case 4:
					array7 = new int[2] { 11, 12 };
					break;
				}
				int[] array8 = array7;
				foreach (int num2 in array8)
				{
					if (num2 < componentsInChildren.Length)
					{
						componentsInChildren[num2].gameObject.SetActive(value: true);
					}
				}
			}
			return;
		}
		case "backpack":
		{
			PlayerPackageCmpt cmpt3 = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
			if (cmpt3.package.GetCount(1332) == 0 && array.Length > 1)
			{
				Vector3 vector4 = Str2V3(array[1]);
				if (vector4 == Vector3.zero)
				{
					Debug.LogError("backpack pos invalid.");
				}
				if (PeGameMgr.IsMultiStory)
				{
					PlayerNetwork.mainPlayer.CreateSceneItem("backpack", vector4, "1332,1");
				}
				else
				{
					CreateBackpack(vector4);
				}
			}
			return;
		}
		case "TextSample":
			CreateLanguageSample();
			return;
		case "PujaTrade":
			PeSingleton<DetectedTownMgr>.Instance.AddStoryCampByMission(31);
			return;
		case "EnableGerdy":
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(9008);
			peEntity.biologyViewCmpt.ActivateCollider(value: true);
			peEntity.motionMgr.FreezePhyState(GetType(), v: false);
			return;
		}
		case "pajaLanguage":
		{
			PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
			if (array.Length <= 1)
			{
				return;
			}
			Vector3 vector2 = Str2V3(array[1]);
			if (vector2 == Vector3.zero)
			{
				Debug.LogError("pajaLanguage pos invalid.");
				return;
			}
			PlayerMission playerMission = MissionManager.Instance.m_PlayerMission;
			if (playerMission.pajaLanguageBePickup == 0 || (SingleGameStory.curType == SingleGameStory.StoryScene.PajaShip && vector2.z < 9000f && playerMission.pajaLanguageBePickup != 1 && playerMission.pajaLanguageBePickup != 3) || (SingleGameStory.curType == SingleGameStory.StoryScene.LaunchCenter && vector2.z > 9000f && playerMission.pajaLanguageBePickup != 2 && playerMission.pajaLanguageBePickup != 3))
			{
				if (PeGameMgr.IsMultiStory)
				{
					Vector3 pos = vector2;
					pos.y -= 500f;
					PlayerNetwork.mainPlayer.CreateSceneItem("pajaLanguage", pos, "1508,1");
				}
				else
				{
					languages.Add(CreatePajaLanguage(vector2));
				}
			}
			return;
		}
		case "probe":
		{
			PlayerPackageCmpt cmpt2 = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
			if (cmpt2.package.GetCount(1340) == 0 && array.Length > 1)
			{
				Vector3 vector3 = Str2V3(array[1]);
				if (vector3 == Vector3.zero)
				{
					Debug.LogError("probe pos invalid.");
				}
				if (PeGameMgr.IsMultiStory)
				{
					PlayerNetwork.mainPlayer.CreateSceneItem("probe", vector3, "1340,1");
				}
				else
				{
					CreateProbe(vector3);
				}
			}
			return;
		}
		case "hugefish_bone":
			if (array.Length > 1)
			{
				Vector3 vector = Str2V3(array[1]);
				if (vector == Vector3.zero)
				{
					Debug.LogError("hugefish_bone pos invalid.");
				}
				if (PeGameMgr.IsMultiStory)
				{
					PlayerNetwork.mainPlayer.CreateSceneItem("hugefish_bone", vector, "1342,1");
				}
				else
				{
					CreateHugefish_bone(vector);
				}
				return;
			}
			break;
		}
		if (text == "1_larve_Q425" && array.Length > 1)
		{
			Vector3 vector5 = Str2V3(array[1]);
			if (vector5 == Vector3.zero)
			{
				Debug.LogError("1_larve_Q425 pos invalid.");
			}
			if (PeGameMgr.IsMultiStory)
			{
				PlayerNetwork.mainPlayer.CreateSceneItem("1_larve_Q425", vector5, string.Empty);
			}
			else
			{
				Createlarve_Q425(vector5);
			}
			return;
		}
		switch (text)
		{
		case "0_larve_Q425":
		{
			GameObject gameObject2 = GameObject.Find("larve_Q425(Clone)");
			if (!(gameObject2 == null))
			{
				if (PeGameMgr.IsSingleStory)
				{
					UnityEngine.Object.Destroy(gameObject2);
				}
				else
				{
					PlayerNetwork.mainPlayer.DestroySceneItem(gameObject2.transform.position);
				}
			}
			return;
		}
		case "earthCamp":
		{
			CSCreator creator = CSMain.GetCreator(0);
			if (creator == null)
			{
				return;
			}
			MapMaskData mapMaskData = MapMaskData.s_tblMaskData.Find((MapMaskData ret) => ret.mId == 22);
			if (mapMaskData == null)
			{
				return;
			}
			{
				foreach (PeEntity item2 in PeSingleton<EntityMgr>.Instance.All)
				{
					if (!(item2 == null) && !item2.IsRecruited() && !(Vector3.Distance(mapMaskData.mPosition, item2.position) > mapMaskData.mRadius) && (item2.proto == EEntityProto.Npc || item2.proto == EEntityProto.RandomNpc))
					{
						if (PeGameMgr.IsMulti)
						{
							MissionManager.Instance.SetGetTakeMission(191, item2, MissionManager.TakeMissionType.TakeMissionType_Get);
						}
						else
						{
							creator.AddNpc(item2, bSetPos: true);
						}
					}
				}
				return;
			}
		}
		case "DrawCir_1":
		{
			GameObject gameObject = GameObject.Find("MissionCylinder");
			if (!(gameObject == null))
			{
				UnityEngine.Object.Destroy(gameObject);
			}
			return;
		}
		case "npcdrive":
			NpcDrive();
			return;
		case "playerlie":
		{
			MotionMgrCmpt motionMgr = PeSingleton<PeCreature>.Instance.mainPlayer.motionMgr;
			if (!(motionMgr == null))
			{
				motionMgr.EndAction(PEActionType.Lie);
			}
			return;
		}
		case "npctobed":
			if (!CSMain.HasCSAssembly())
			{
			}
			return;
		case "Fruit_pack":
			if (array.Length > 1)
			{
				Vector3 vector6 = Str2V3(array[1]);
				if (vector6 == Vector3.zero)
				{
					Debug.LogError("fruitpack pos invalid.");
				}
				if (PeGameMgr.IsMultiStory)
				{
					PlayerNetwork.mainPlayer.CreateSceneItem("Fruit_pack", vector6, string.Empty);
				}
				else
				{
					CreateFruitpack(vector6);
				}
				return;
			}
			break;
		}
		switch (text)
		{
		case "Fruit_pack_1":
		{
			GameObject gameObject3 = GameObject.Find("fruitpack");
			if (!(gameObject3 == null))
			{
				if (PeGameMgr.IsSingleStory)
				{
					UnityEngine.Object.Destroy(gameObject3);
				}
				else
				{
					PlayerNetwork.mainPlayer.DestroySceneItem(gameObject3.transform.position);
				}
			}
			return;
		}
		case "closeviyus_22,23":
		{
			for (int m = 0; m < 4; m++)
			{
				DienManager.DoorClose(DienManager.doors[m]);
			}
			DienManager.doorsCanTrigger = false;
			return;
		}
		case "openviyus_22,23":
			DienManager.DoorOpen(DienManager.doors[4]);
			DienManager.doorsCanTrigger = true;
			return;
		case "openL1":
		{
			GameObject gameObject5 = GameObject.Find("Epiphany_L1Outside195");
			if (gameObject5 != null)
			{
				gameObject5.SetActive(value: false);
			}
			return;
		}
		case "DestroyRail":
			DestroyRailWay();
			return;
		case "GetVirus":
			return;
		case "GetThruster":
			return;
		case "dayuLowHP":
			StartCoroutine(WaitDayuLowHP());
			return;
		case "dayuRunaway":
		{
			PeEntity entityByFixedSpId = PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(94);
			if (null != entityByFixedSpId)
			{
				AnimatorCmpt component = entityByFixedSpId.GetComponent<AnimatorCmpt>();
				component.SetBool("jump", value: true);
				Invoke("DestroyDayu", 3f);
			}
			return;
		}
		case "EnableReputation":
			PeSingleton<ReputationSystem>.Instance.ActiveReputation((int)PeSingleton<PeCreature>.Instance.mainPlayer.GetAttribute(AttribType.DefaultPlayerID));
			return;
		case "Cartercycle":
			VCEditor.MakeCreation("Isos/Mission/Cartercycle");
			return;
		case "copyisozj":
		{
			for (int num3 = 0; num3 < 3; num3++)
			{
				VCEditor.CopyCretion(ECreation.Aircraft);
			}
			return;
		}
		case "movezj":
			if (PeGameMgr.IsSingle)
			{
				for (int l = 0; l < MissionManager.Instance.m_PlayerMission.recordCreationName.Count; l++)
				{
					GameObject gameObject4 = GameObject.Find(MissionManager.Instance.m_PlayerMission.recordCreationName[l]);
					if (!(gameObject4 == null))
					{
						MissionManager.Instance.m_PlayerMission.recordCretionPos.Add(gameObject4.transform.position);
						UnityEngine.Object @object = Resources.Load("Cutscene Clips/PathClip" + (l + 1));
						if (!(@object == null))
						{
							GameObject pathObj = UnityEngine.Object.Instantiate(@object) as GameObject;
							MoveByPath moveByPath = gameObject4.AddComponent<MoveByPath>();
							moveByPath.SetDurationDelay(15f, 0f);
							moveByPath.StartMove(pathObj, RotationMode.ConstantUp);
							gameObject4.GetComponent<Rigidbody>().useGravity = false;
						}
					}
				}
			}
			else
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_MissionMoveAircraft, true);
			}
			return;
		case "returnzj":
			if (PeGameMgr.IsSingle)
			{
				for (int num4 = 0; num4 < MissionManager.Instance.m_PlayerMission.recordCreationName.Count; num4++)
				{
					GameObject zj = GameObject.Find(MissionManager.Instance.m_PlayerMission.recordCreationName[num4]);
					if (zj == null)
					{
						continue;
					}
					UnityEngine.Object object2 = Resources.Load("Cutscene Clips/PathClip" + (num4 + 5));
					if (object2 == null)
					{
						continue;
					}
					GameObject pathObj2 = UnityEngine.Object.Instantiate(object2) as GameObject;
					MoveByPath moveByPath2 = zj.AddComponent<MoveByPath>();
					moveByPath2.SetDurationDelay(15f, 0f);
					if (num4 >= MissionManager.Instance.m_PlayerMission.recordCretionPos.Count)
					{
						continue;
					}
					Vector3 recordPos = MissionManager.Instance.m_PlayerMission.recordCretionPos[num4] + Vector3.up * 20f;
					moveByPath2.AddEndListener(delegate
					{
						returnZjComplete = true;
						if (zj != null)
						{
							zj.transform.position = recordPos;
							Rigidbody component2 = zj.GetComponent<Rigidbody>();
							if (component2 != null)
							{
								component2.useGravity = true;
							}
						}
					});
					UIMap curMap = GameUI.Instance.mUIWorldMap.CurMap;
					curMap.onTravel = (Action)Delegate.Combine(curMap.onTravel, (Action)delegate
					{
						if (zj != null && !returnZjComplete)
						{
							zj.transform.position = recordPos;
							Rigidbody component3 = zj.GetComponent<Rigidbody>();
							if (component3 != null)
							{
								component3.useGravity = true;
							}
						}
					});
					moveByPath2.StartMove(pathObj2, RotationMode.ConstantUp);
				}
			}
			else
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_MissionMoveAircraft, false);
			}
			return;
		case "MeatToMoney":
			if (PeGameMgr.IsMulti)
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ChangeCurrency, EMoneyType.Digital);
				return;
			}
			foreach (PeEntity item3 in PeSingleton<EntityMgr>.Instance.All)
			{
				if (!(item3 == null) && !(item3.gameObject == null))
				{
					NpcPackageCmpt cmpt4 = item3.GetCmpt<NpcPackageCmpt>();
					if (!(cmpt4 == null))
					{
						cmpt4.money.SetCur(cmpt4.money.current * 4);
					}
				}
			}
			Money.Digital = true;
			GameUI.Instance.mShopWnd.mMeatSprite.gameObject.SetActive(value: false);
			GameUI.Instance.mShopWnd.mMoneySprite.gameObject.SetActive(value: true);
			GameUI.Instance.mItemPackageCtrl.nMoneyRoot.SetActive(value: true);
			return;
		}
		if (text.Length < 6)
		{
			return;
		}
		if (text.Substring(0, 6) == "gotoCS")
		{
			if (CSMain.GetAssemblyPos(out var pos3))
			{
				string text3 = text.Substring(7, text.Length - 7);
				List<int> list = new List<int>(Array.ConvertAll(text3.Split(','), (string s) => int.Parse(s)));
				for (int num5 = 0; num5 < list.Count; num5++)
				{
					CSCreator creator2 = CSMain.GetCreator(0);
					if (creator2 == null)
					{
						return;
					}
					PeEntity peEntity2 = PeSingleton<EntityMgr>.Instance.Get(list[num5]);
					if (peEntity2 != null)
					{
						peEntity2.NpcCmpt.FixedPointPos = PEUtil.GetRandomPosition(pos3, 10f, 30f, is3D: true);
						if (PeGameMgr.IsMulti)
						{
							MissionManager.Instance.SetGetTakeMission(191, peEntity2, MissionManager.TakeMissionType.TakeMissionType_Get);
						}
						else
						{
							creator2.AddNpc(peEntity2, bSetPos: true);
						}
					}
				}
			}
		}
		else if (text.Substring(0, 7) == "moncall")
		{
			if (text.Length > 7)
			{
				string text4 = text.Substring(8, text.Length - 8);
				List<string> list2 = new List<string>(text4.Split('_', ';'));
				int num6 = list2.Count / 3;
				for (int num7 = 0; num7 < num6; num7++)
				{
					list2.GetRange(num7 * 3, 3).ConvertAll((string n) => Convert.ToInt32(n));
				}
			}
		}
		else if (text.Substring(0, 7) == "npcdead")
		{
			deadNpcsName.Clear();
			string text5 = text.Substring(8, text.Length - 8);
			if (text5.Length <= 2)
			{
				int num8 = Convert.ToInt32(text5);
				List<int> list3 = new List<int>();
				List<PeEntity> cSRandomNpc = CSMain.GetCSRandomNpc();
				if (cSRandomNpc.Count != 0)
				{
					ServantLeaderCmpt component4 = PeSingleton<PeCreature>.Instance.mainPlayer.GetComponent<ServantLeaderCmpt>();
					NpcCmpt[] servants = component4.GetServants();
					for (int num9 = 0; num9 < servants.Length; num9++)
					{
						for (int num10 = 0; num10 < cSRandomNpc.Count; num10++)
						{
							if (servants[num9] == cSRandomNpc[num10].NpcCmpt)
							{
								cSRandomNpc.RemoveAt(num10);
								break;
							}
						}
					}
					if (cSRandomNpc.Count <= num8)
					{
						for (int num11 = 0; num11 < cSRandomNpc.Count; num11++)
						{
							bool flag = true;
							for (int num12 = 0; num12 < servants.Length; num12++)
							{
								if (servants[num12] == cSRandomNpc[num11].NpcCmpt)
								{
									flag = false;
									break;
								}
							}
							if (flag)
							{
								list3.Add(num11);
							}
						}
					}
					else
					{
						while (list3.Count < num8)
						{
							int item = UnityEngine.Random.Range(0, cSRandomNpc.Count);
							if (!list3.Contains(item))
							{
								list3.Add(item);
							}
						}
					}
					for (int num13 = 0; num13 < list3.Count; num13++)
					{
						if (!deadNpcsName.Contains(cSRandomNpc[list3[num13]].name))
						{
							deadNpcsName.Add(cSRandomNpc[list3[num13]].name.Substring(0, cSRandomNpc[list3[num13]].name.Length - 5));
						}
						if (cSRandomNpc[list3[num13]] != null)
						{
							CSMain.RemoveNpc(cSRandomNpc[list3[num13]]);
						}
						if (PeGameMgr.IsMultiStory)
						{
							PlayerNetwork.mainPlayer.CreateSceneItem("ash_box", cSRandomNpc[list3[num13]].position, "1339,1", cSRandomNpc[list3[num13]].Id);
						}
						else
						{
							CreateAsh_box(cSRandomNpc[list3[num13]].position, cSRandomNpc[list3[num13]].Id);
						}
					}
				}
			}
			else
			{
				List<int> list4 = new List<int>(Array.ConvertAll(text5.Split(','), (string s) => int.Parse(s)));
				for (int num14 = 0; num14 < list4.Count; num14++)
				{
					PeEntity peEntity3 = PeSingleton<EntityMgr>.Instance.Get(list4[num14]);
					if (!(peEntity3 == null))
					{
						if (PeGameMgr.IsMultiStory)
						{
							PlayerNetwork.mainPlayer.CreateSceneItem("ash_box", peEntity3.position, "1339,1", peEntity3.Id);
						}
						else
						{
							CreateAsh_box(peEntity3.position, peEntity3.Id);
						}
						if (!deadNpcsName.Contains(peEntity3.name))
						{
							deadNpcsName.Add(peEntity3.name.Substring(0, peEntity3.name.Length - 5));
						}
						CSMain.RemoveNpc(peEntity3);
					}
				}
			}
		}
		if (text.Length < 10)
		{
			return;
		}
		if (text.Substring(0, 8) == "special_")
		{
			string text6 = text.Substring(8, text.Length - 8);
			string[] array9 = text6.Split('_');
			if (array9.Length != 2)
			{
				return;
			}
			int[][] array10 = new int[3][]
			{
				new int[3] { 603, 604, 605 },
				new int[3] { 926, 927, 928 },
				new int[3] { 937, 938, 939 }
			};
			int num15 = Convert.ToInt32(array9[0]);
			int num16 = Convert.ToInt32(array9[1]);
			int num17 = (CSMain.HasCSAssembly() ? ((num16 > CSMain.GetEmptyBedRoom()) ? 1 : 2) : 0);
			int missionID = array10[num15 - 1][num17];
			MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(missionID);
			if (missionCommonData != null)
			{
				PeEntity npc2 = PeSingleton<EntityMgr>.Instance.Get(missionCommonData.m_iNpc);
				if (MissionRepository.HaveTalkOP(missionID))
				{
					GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(missionID, 1);
					GameUI.Instance.mNPCTalk.NormalOrSP(0);
					GameUI.Instance.mNPCTalk.PreShow();
				}
				else
				{
					MissionManager.Instance.SetGetTakeMission(missionID, npc2, MissionManager.TakeMissionType.TakeMissionType_Get);
				}
			}
			return;
		}
		if (text.Substring(0, 10) == "SpiderWeb_")
		{
			string text7 = text.Substring(10, text.Length - 10);
			List<string> list5 = new List<string>(text7.Split('_'));
			List<int> list6 = new List<int>(Array.ConvertAll(list5[0].Split(','), (string s) => Convert.ToInt32(s)));
			if (list5.Count == 2 && Convert.ToInt32(list5[1]) == 1)
			{
				UnityEngine.Object object3 = Resources.Load("Prefab/Item/Other/SpiderWeb");
				if (null != object3)
				{
					for (int num18 = 0; num18 < list6.Count; num18++)
					{
						if (list6[num18] == 20000)
						{
							if (PeSingleton<PeCreature>.Instance.mainPlayer.transform.FindChild("DummyTransform/spiderWeb") == null)
							{
								GameObject gameObject6 = UnityEngine.Object.Instantiate(object3) as GameObject;
								gameObject6.transform.parent = PEUtil.GetChild(PeSingleton<PeCreature>.Instance.mainPlayer.transform, "DummyTransform");
								gameObject6.transform.localPosition = Vector3.zero;
								gameObject6.name = "spiderWeb";
							}
						}
						else if (PeSingleton<EntityMgr>.Instance.Get(list6[num18]).gameObject.transform.FindChild("DummyTransform/spiderWeb") == null)
						{
							GameObject gameObject7 = UnityEngine.Object.Instantiate(object3) as GameObject;
							gameObject7.transform.parent = PEUtil.GetChild(PeSingleton<EntityMgr>.Instance.Get(list6[num18]).transform, "DummyTransform");
							gameObject7.transform.localPosition = Vector3.zero;
							gameObject7.name = "spiderWeb";
						}
					}
				}
			}
			if (list5.Count != 2 || Convert.ToInt32(list5[1]) != 0)
			{
				return;
			}
			for (int num19 = 0; num19 < list6.Count; num19++)
			{
				if (list6[num19] == 20000)
				{
					Transform transform = PeSingleton<PeCreature>.Instance.mainPlayer.transform.FindChild("DummyTransform/spiderWeb");
					if (transform != null)
					{
						GameObject gameObject8 = transform.gameObject;
						if (gameObject8 != null)
						{
							UnityEngine.Object.Destroy(gameObject8);
						}
					}
					continue;
				}
				Transform transform2 = PeSingleton<EntityMgr>.Instance.Get(list6[num19]).gameObject.transform.FindChild("DummyTransform/spiderWeb");
				if (transform2 != null)
				{
					GameObject gameObject9 = transform2.gameObject;
					if (gameObject9 != null)
					{
						UnityEngine.Object.Destroy(gameObject9);
					}
				}
			}
			return;
		}
		if (text.Substring(0, 11) == "AndheraNest")
		{
			for (int num20 = 0; num20 < 8; num20++)
			{
				CreateAndHeraNest(num20);
			}
			return;
		}
		if (!(text.Substring(0, 12) == "changeshader"))
		{
			return;
		}
		string text8 = text.Substring(12, text.Length - 12);
		string[] array11 = text8.Split('_');
		if (array11.Length != 2)
		{
			return;
		}
		bool flag2 = Convert.ToInt32(array11[0]) == 1;
		PeEntity peEntity4 = PeSingleton<EntityMgr>.Instance.Get(Convert.ToInt32(array11[1]));
		if (!(null != peEntity4))
		{
			return;
		}
		if (flag2)
		{
			SkinnedMeshRenderer[] componentsInChildren2 = PeSingleton<EntityMgr>.Instance.Get(9029).GetComponentsInChildren<SkinnedMeshRenderer>();
			if (componentsInChildren2.Length >= 2 && componentsInChildren2[1].materials[0].name != "HidingWaveMat(Clone) (Instance)")
			{
				record = componentsInChildren2[1].materials;
				Material[] array12 = new Material[componentsInChildren2[1].materials.Length];
				for (int num21 = 0; num21 < componentsInChildren2[1].materials.Length; num21++)
				{
					Texture texture = componentsInChildren2[1].materials[num21].GetTexture(0);
					array12[num21] = UnityEngine.Object.Instantiate(HidingMat);
					array12[num21].SetTexture("_SrcMap", texture);
				}
				componentsInChildren2[1].materials = array12;
				AddChangingMaterial(componentsInChildren2[1].materials);
			}
		}
		else
		{
			SkinnedMeshRenderer[] componentsInChildren3 = PeSingleton<EntityMgr>.Instance.Get(9029).GetComponentsInChildren<SkinnedMeshRenderer>();
			if (componentsInChildren3.Length >= 2)
			{
				AddChangingMaterial(componentsInChildren3[1]);
			}
		}
	}

	private void DestroyDayu()
	{
		PeEntity entityByFixedSpId = PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(94);
		Singleton<PeLogicGlobal>.Instance.DestroyEntity(entityByFixedSpId.skEntity, 0f, 1f);
	}

	private void MonCall(List<int> monCallData)
	{
		if (monCallData.Count == 3)
		{
		}
	}

	public void TestMeetingPos(int num)
	{
		foreach (Vector3 item in GetMeetingPosition(PeSingleton<PeCreature>.Instance.mainPlayer.position, num, 3f))
		{
			GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			gameObject.transform.position = item;
			UnityEngine.Object.Destroy(gameObject.GetComponent<SphereCollider>());
			gameObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			gameObject.name = "MeetingPos";
		}
	}

	public List<Vector3> GetMeetingPosition(Vector3 center, int num, float radius)
	{
		List<Vector3> list = new List<Vector3>();
		if (num == 1)
		{
			list.Add(center);
			return list;
		}
		float num2 = Mathf.Sin((float)Math.PI / (float)num) * radius * 2f;
		int num3 = 0;
		while (list.Count < num)
		{
			num3++;
			Vector3 horizontalDir = GetHorizontalDir();
			bool flag = false;
			foreach (Vector3 item in list)
			{
				if (Vector3.Distance(item, center + Vector3.up + horizontalDir * radius) < num2 * 0.5f)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (!Physics.Raycast(center + Vector3.up, horizontalDir + Vector3.up * 0.5f, radius) && Physics.Raycast(center + Vector3.up * radius / 2f + horizontalDir * radius, Vector3.down, 3f))
				{
					list.Add(center + Vector3.up + horizontalDir * radius);
				}
				if (num3 >= 100)
				{
					break;
				}
			}
		}
		while (list.Count < num)
		{
			num3++;
			for (int i = 0; i < list.Count; i++)
			{
				list.Add((center + Vector3.up + list[i]) / 2f);
				if (list.Count >= num)
				{
					break;
				}
			}
			if (num3 >= 105)
			{
				break;
			}
		}
		while (list.Count < num)
		{
			Vector3 horizontalDir2 = GetHorizontalDir();
			bool flag2 = false;
			foreach (Vector3 item2 in list)
			{
				if (Vector3.Distance(item2, center + Vector3.up + horizontalDir2 * radius) < num2 * 0.5f)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				list.Add(center + Vector3.up + horizontalDir2 * radius);
			}
		}
		return list;
	}

	private Vector3 GetHorizontalDir()
	{
		float f = UnityEngine.Random.Range(0f, (float)Math.PI * 2f);
		return new Vector3(Mathf.Sin(f), 0f, Mathf.Cos(f));
	}

	public void ResetPathIdx()
	{
		m_CurPathIdx = 0;
		m_iCurPathMap.Clear();
	}

	public int GetPathIdx(List<Vector3> pathList)
	{
		if (pathList.Count == 0)
		{
			return 0;
		}
		Vector2 a = new Vector2(PeSingleton<PeCreature>.Instance.mainPlayer.position.x, PeSingleton<PeCreature>.Instance.mainPlayer.position.z);
		Vector2 vector = new Vector2(pathList[0].x, pathList[0].z);
		float num = Vector2.Distance(a, vector);
		float num2 = num;
		int num3 = 0;
		for (int i = 1; i < pathList.Count; i++)
		{
			vector = new Vector2(pathList[i].x, pathList[i].z);
			num = Vector2.Distance(a, vector);
			if (num < num2)
			{
				num2 = num;
				num3 = i;
			}
		}
		if (num3 >= pathList.Count - 1)
		{
			return num3;
		}
		vector = new Vector2(pathList[num3].x, pathList[num3].z);
		Vector2 b = new Vector2(pathList[pathList.Count - 1].x, pathList[pathList.Count - 1].z);
		num = Vector2.Distance(a, b);
		num2 = Vector2.Distance(vector, b);
		if (num <= num2)
		{
			return num3 + 1;
		}
		return num3;
	}

	public PathInfo GetPath(int id, bool bChangePos, PeEntity npc)
	{
		PathInfo result = default(PathInfo);
		result.pos = Vector3.zero;
		result.isFinish = true;
		if (id == m_CurMissionID && !bChangePos && m_iCurPathMap.ContainsKey(npc.Id))
		{
			result.isFinish = false;
			result.pos = m_iCurPathMap[npc.Id].curPos;
			return result;
		}
		int num = 0;
		if (npc != null && m_iCurPathMap.ContainsKey(npc.Id))
		{
			num = m_iCurPathMap[npc.Id].idx;
		}
		else
		{
			num = m_CurPathIdx;
		}
		return result;
	}

	public int CreateMissionRandomNpc(Vector3 pos, int num)
	{
		return 0;
	}

	public void NpcTakeMission(int Rnpcid, int Qcid, Vector3 pos, List<int> csrecruit = null)
	{
	}

	private void SetRandomNpcInitState()
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(3);
		if (peEntity != null)
		{
			peEntity.SetInjuredLevel(0.9f);
		}
		peEntity = PeSingleton<EntityMgr>.Instance.Get(13);
		if (peEntity != null)
		{
			peEntity.SetInjuredLevel(1f);
		}
		peEntity = PeSingleton<EntityMgr>.Instance.Get(14);
		if (peEntity != null)
		{
			peEntity.SetAiActive(value: false);
		}
		peEntity = PeSingleton<EntityMgr>.Instance.Get(15);
		if (peEntity != null)
		{
			peEntity.SetInvincible(value: false);
			peEntity.ApplyDamage(1000000f);
		}
		peEntity = PeSingleton<EntityMgr>.Instance.Get(16);
		if (peEntity != null)
		{
			peEntity.SetInvincible(value: false);
			peEntity.ApplyDamage(1000000f);
		}
	}

	private void AddNpcItem(int npcid, int itemid)
	{
		if (GameConfig.IsMultiMode)
		{
			return;
		}
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(npcid);
		if (peEntity == null || !EntityCreateMgr.Instance.IsRandomNpc(peEntity))
		{
			return;
		}
		int bagItemCount = peEntity.GetBagItemCount();
		if (bagItemCount > 0)
		{
			for (int i = 0; i < bagItemCount; i++)
			{
				ItemObject bagItem = peEntity.GetBagItem(i);
				if (bagItem != null)
				{
					if (bagItem.protoId != itemid)
					{
						bagItem = PeSingleton<ItemMgr>.Instance.CreateItem(itemid);
						peEntity.AddToBag(bagItem);
					}
					break;
				}
			}
		}
		else
		{
			ItemObject bagItem = PeSingleton<ItemMgr>.Instance.CreateItem(itemid);
			peEntity.AddToBag(bagItem);
		}
	}

	public Vector3 GetNpcPos(int npcid)
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(npcid);
		if (peEntity == null)
		{
			return Vector3.zero;
		}
		return peEntity.position;
	}

	public Vector3 GetPlayerPos()
	{
		if (m_PlayerTrans == null)
		{
			m_PlayerTrans = PeSingleton<PeCreature>.Instance.mainPlayer.peTrans;
		}
		if (m_PlayerTrans == null)
		{
			return Vector3.zero;
		}
		return m_PlayerTrans.position;
	}

	public PeEntity GetNpc(int npcid)
	{
		return PeSingleton<EntityMgr>.Instance.Get(npcid);
	}

	public bool IsZeroPoint(Vector3 npcPos)
	{
		Vector2 a = new Vector2(npcPos.x, npcPos.z);
		Vector2 b = new Vector2(0f, 0f);
		if (Vector2.Distance(a, b) < 5f)
		{
			return true;
		}
		return false;
	}

	public void PauseAll(PeEntity npc)
	{
		NpcCmpt npcCmpt = npc.NpcCmpt;
		if (!(npcCmpt == null))
		{
			npcCmpt.Req_PauseAll();
		}
	}

	public void MoveTo(PeEntity npc, Vector3 pos, float radius = 1f, bool bForce = true, SpeedState ss = SpeedState.Walk)
	{
		if (!(npc == null))
		{
			NpcCmpt npcCmpt = npc.NpcCmpt;
			if (!(npcCmpt == null))
			{
				npcCmpt.Req_MoveToPosition(pos, radius, bForce, ss);
			}
		}
	}

	public void TalkMoveTo(PeEntity npc, Vector3 pos, float radius = 1f, bool bForce = true, SpeedState ss = SpeedState.Walk)
	{
		if (!(npc == null))
		{
			NpcCmpt npcCmpt = npc.NpcCmpt;
			if (!(npcCmpt == null))
			{
				npcCmpt.Req_TalkMoveToPosition(pos, radius, bForce, ss);
			}
		}
	}

	public void MoveToByPath(PeEntity npc, Vector3[] posList, SpeedState state = SpeedState.Run)
	{
		NpcCmpt npcCmpt = npc.NpcCmpt;
		if (!(npcCmpt == null))
		{
			if (npc.target != null && npc.target.GetAttackEnemy() != null)
			{
				npc.target.ClearEnemy();
			}
			npcCmpt.Req_FollowPath(posList, isLoop: false, state);
		}
	}

	public void Translate(PeEntity npc, Vector3 position)
	{
		NpcCmpt npcCmpt = npc.NpcCmpt;
		if (!(npcCmpt == null))
		{
			npcCmpt.Req_Translate(position, adjust: false);
		}
	}

	public void FollowTarget(PeEntity npc, Transform trans)
	{
		NpcCmpt npcCmpt = npc.NpcCmpt;
		if (!(npcCmpt == null))
		{
			if (npc.target != null && npc.target.GetAttackEnemy() != null)
			{
				npc.target.ClearEnemy();
			}
			npcCmpt.Req_FollowTarget(npc.Id, Vector3.zero, 0, 0f);
		}
	}

	public void FollowTarget(PeEntity npc, int targetId, Vector3 pos, int dirTargetid, float radius, bool send = true)
	{
		NpcCmpt npcCmpt = npc.NpcCmpt;
		if (!(npcCmpt == null))
		{
			if (npc.target != null && npc.target.GetAttackEnemy() != null)
			{
				npc.target.ClearEnemy();
			}
			npcCmpt.Req_FollowTarget(targetId, pos, dirTargetid, radius, bNet: false, send);
		}
	}

	public void FollowTarget(List<int> npcs, int targetId)
	{
		foreach (int npc in npcs)
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(npc);
			NpcCmpt npcCmpt = peEntity.NpcCmpt;
			if (npcCmpt == null)
			{
				return;
			}
			if (peEntity.target != null && peEntity.target.GetAttackEnemy() != null)
			{
				peEntity.target.ClearEnemy();
			}
		}
		if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_NPC_RequestAiOp, 4, targetId, npcs.ToArray());
		}
	}

	public void CarryUp(PeEntity npc, int carryedNpc, bool bCarryUp)
	{
		NpcCmpt npcCmpt = npc.NpcCmpt;
		if (!(npcCmpt == null))
		{
			npcCmpt.Req_Salvation(carryedNpc, bCarryUp);
		}
	}

	public void RemoveReq(PeEntity npc, EReqType type)
	{
		NpcCmpt npcCmpt = npc.NpcCmpt;
		if (!(npcCmpt == null))
		{
			npcCmpt.Req_Remove(type);
		}
	}

	public void SetIdle(PeEntity npc, string anim)
	{
		NpcCmpt npcCmpt = npc.NpcCmpt;
		if (!(npcCmpt == null))
		{
			npcCmpt.Req_SetIdle(anim);
		}
	}

	public void SetRotation(PeEntity npc, Quaternion qua)
	{
		NpcCmpt npcCmpt = npc.NpcCmpt;
		if (!(npcCmpt == null))
		{
			Motion_Move motionMove = npc.motionMove;
			if (!(motionMove == null))
			{
				motionMove.Stop();
				npcCmpt.Req_Rotation(qua);
			}
		}
	}

	public void SetTalking(PeEntity npc, string RqAction = "", object npcidOrVecter3 = null)
	{
		NpcCmpt npcCmpt = npc.NpcCmpt;
		if (npcCmpt == null)
		{
			return;
		}
		if (npcidOrVecter3 != null)
		{
			if (npcidOrVecter3 is int)
			{
				if ((int)npcidOrVecter3 == 0 || PeSingleton<EntityMgr>.Instance.Get((int)npcidOrVecter3) == null)
				{
					npcCmpt.Req_Dialogue(RqAction, null);
					return;
				}
				PeTrans peTrans = PeSingleton<EntityMgr>.Instance.Get((int)npcidOrVecter3).peTrans;
				if (peTrans != null)
				{
					npcCmpt.Req_Dialogue(RqAction, peTrans);
				}
			}
			else if (npcidOrVecter3 is Vector3)
			{
				npcCmpt.Req_Dialogue(RqAction, (Vector3)npcidOrVecter3);
			}
		}
		else
		{
			npcCmpt.Req_Dialogue(RqAction);
		}
	}

	public AudioController PlaySound(PeEntity npc, int audioid)
	{
		if (npc == null)
		{
			return null;
		}
		return AudioManager.instance.Create(npc.ExtGetPos(), audioid);
	}

	private void NpcDrive()
	{
		if (!CSMain.GetAssemblyPos(out var pos))
		{
			return;
		}
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(9001);
		if (peEntity == null)
		{
			return;
		}
		PeEntity peEntity2 = PeSingleton<EntityMgr>.Instance.Get(9021);
		if (peEntity2 == null)
		{
			return;
		}
		PeEntity peEntity3 = PeSingleton<EntityMgr>.Instance.Get(9033);
		if (peEntity3 == null)
		{
			return;
		}
		StaticPoint staticPoint = PeSingleton<StaticPoint.Mgr>.Instance.Find((StaticPoint e) => e.campId == 7);
		if (staticPoint != null)
		{
			Vector3[] array = new Vector3[4];
			for (int i = 0; i < 3; i++)
			{
				ref Vector3 reference = ref array[i];
				reference = AiUtil.GetRandomPosition(pos, 20f, 50f, 100f, AiUtil.groundedLayer, 10);
			}
			ref Vector3 reference2 = ref array[3];
			reference2 = staticPoint.position;
			MoveToByPath(peEntity, array, SpeedState.Walk);
			Vector3[] array2 = new Vector3[4];
			Vector3[] array3 = new Vector3[4];
			for (int j = 0; j < array.Length - 1; j++)
			{
				Vector3 normalized = Vector3.Cross(array[j + 1] - array[j], Vector3.up).normalized;
				ref Vector3 reference3 = ref array2[j];
				reference3 = array[j] + normalized;
				ref Vector3 reference4 = ref array3[j];
				reference4 = array[j] - normalized;
			}
			ref Vector3 reference5 = ref array2[3];
			reference5 = staticPoint.position;
			ref Vector3 reference6 = ref array3[3];
			reference6 = staticPoint.position;
			MoveToByPath(peEntity2, array2, SpeedState.Walk);
			MoveToByPath(peEntity3, array3, SpeedState.Walk);
		}
	}

	public int GetMgCampNpcCount()
	{
		int num = 0;
		CSCreator creator = CSMain.GetCreator(0);
		if (creator == null)
		{
			return 0;
		}
		MapMaskData mapMaskData = MapMaskData.s_tblMaskData.Find((MapMaskData ret) => ret.mId == 22);
		if (mapMaskData == null)
		{
			return 0;
		}
		foreach (PeEntity item in PeSingleton<EntityMgr>.Instance.All)
		{
			if (!(item == null) && !item.IsRecruited() && (item.proto == EEntityProto.Npc || item.proto == EEntityProto.RandomNpc) && !(Vector3.Distance(mapMaskData.mPosition, item.position) > mapMaskData.mRadius))
			{
				num++;
			}
		}
		return num;
	}

	public int ParseIDByColony(int id)
	{
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(id);
		if (missionCommonData == null)
		{
			return 0;
		}
		if (missionCommonData.m_ColonyMis[0] == 0)
		{
			return id;
		}
		if (missionCommonData.m_iColonyNpcList.Count == 0)
		{
			return id;
		}
		for (int i = 0; i < missionCommonData.m_iColonyNpcList.Count; i++)
		{
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(missionCommonData.m_iColonyNpcList[i]);
			if (!(peEntity == null) && !CSMain.IsColonyNpc(peEntity.Id))
			{
				return missionCommonData.m_ColonyMis[1];
			}
		}
		return missionCommonData.m_ColonyMis[0];
	}

	public void PlayerGetOnTrain(int index)
	{
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(500);
		if (missionCommonData == null)
		{
			return;
		}
		for (int i = 0; i < missionCommonData.m_TargetIDList.Count; i++)
		{
			TypeFollowData typeFollowData = MissionManager.GetTypeFollowData(missionCommonData.m_TargetIDList[i]);
			if (typeFollowData != null)
			{
				for (int j = 0; j < typeFollowData.m_iNpcList.Count; j++)
				{
					PeEntity entity = PeSingleton<EntityMgr>.Instance.Get(typeFollowData.m_iNpcList[j]);
					entity.GetOnTrain(index, checkState: false);
				}
			}
		}
	}

	public void PlayerGetOffTrain(Vector3 pos)
	{
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(500);
		if (missionCommonData == null)
		{
			return;
		}
		for (int i = 0; i < missionCommonData.m_TargetIDList.Count; i++)
		{
			TypeFollowData typeFollowData = MissionManager.GetTypeFollowData(missionCommonData.m_TargetIDList[i]);
			if (typeFollowData != null)
			{
				for (int j = 0; j < typeFollowData.m_iNpcList.Count; j++)
				{
					PeEntity entity = PeSingleton<EntityMgr>.Instance.Get(typeFollowData.m_iNpcList[j]);
					entity.GetOffTrain(Vector3.zero);
				}
			}
		}
		if (MissionManager.Instance.HadCompleteMission(500))
		{
			GlobalEvent.OnPlayerGetOffTrain -= PlayerGetOffTrain;
		}
	}

	public void InitPlayerEvent()
	{
		PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
		if (!(cmpt == null))
		{
			cmpt.package._playerPak.changeEventor.Subscribe(PlayerItemEventHandler);
			PlayerPackage._missionPak.changeEventor.Subscribe(PlayerItemEventHandler);
			PeSingleton<DraggingMgr>.Instance.eventor.Subscribe(PlayerDragItemEventHandler);
			SkEntitySubTerrain.Instance.AddListener(TreeCutDownListen);
			UseItemCmpt cmpt2 = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<UseItemCmpt>();
			if (!(cmpt2 == null))
			{
				cmpt2.eventor.Subscribe(PlayerUseItemEventHandler);
				PEBuildingMan.Self.onVoxelMotify += PlayerBuildingEventHandler;
				MainPlayerCmpt.gMainPlayer.onEquipmentAttack += PlayerUseEquipmentHandler;
			}
		}
	}

	private void TreeCutDownListen(SkEntity skEntity, GlobalTreeInfo tree)
	{
		PeEntity component = skEntity.GetComponent<PeEntity>();
		if (!(component == null))
		{
			if (PeGameMgr.IsStory)
			{
				TreeCutDown(component.position, tree._treeInfo, tree.WorldPos);
			}
			else if (PeGameMgr.IsAdventure)
			{
				TreeCutDown(component.position, tree._treeInfo, tree._treeInfo.m_pos);
			}
		}
	}

	public void TreeCutDown(Vector3 casterPosition, TreeInfo tree, Vector3 worldPos)
	{
		if (!tree.IsTree())
		{
			return;
		}
		GameObject gameObject = null;
		if (PeGameMgr.IsStory)
		{
			gameObject = LSubTerrainMgr.Instance.GlobalPrototypePrefabList[tree.m_protoTypeIdx];
		}
		else if (PeGameMgr.IsAdventure)
		{
			gameObject = RSubTerrainMgr.Instance.GlobalPrototypePrefabList[tree.m_protoTypeIdx];
		}
		if (null == gameObject)
		{
			return;
		}
		gameObject = UnityEngine.Object.Instantiate(gameObject);
		gameObject.name = "CutDownTree";
		gameObject.transform.position = worldPos;
		gameObject.transform.rotation = Quaternion.identity;
		gameObject.transform.localScale = new Vector3(tree.m_widthScale, tree.m_heightScale, tree.m_widthScale);
		Collider[] components = gameObject.GetComponents<Collider>();
		if (components != null)
		{
			Collider[] array = components;
			foreach (Collider obj in array)
			{
				UnityEngine.Object.Destroy(obj);
			}
		}
		Bounds bounds = default(Bounds);
		if (PeGameMgr.IsStory)
		{
			bounds = LSubTerrainMgr.Instance.GlobalPrototypeBounds[tree.m_protoTypeIdx];
		}
		else if (PeGameMgr.IsAdventure)
		{
			bounds = RSubTerrainMgr.Instance.GlobalPrototypeBounds[tree.m_protoTypeIdx];
		}
		if (tree.IsDoubleFoot(out var footsPos, worldPos, gameObject.transform.localScale))
		{
			gameObject.AddComponent<TreeCutDown>().SetDirection(casterPosition, bounds.center.y + bounds.extents.y, bounds.extents.x, footsPos);
		}
		else
		{
			gameObject.AddComponent<TreeCutDown>().SetDirection(casterPosition, bounds.center.y + bounds.extents.y, bounds.extents.x);
		}
	}

	public int ItemClassIdtoProtoId(int itemClassId)
	{
		int result = 0;
		switch (CreationHelper.GetCreationItemClass(itemClassId))
		{
		case CreationItemClass.Sword:
			result = 1322;
			break;
		case CreationItemClass.HandGun:
			result = 1323;
			break;
		case CreationItemClass.Rifle:
			result = 1324;
			break;
		case CreationItemClass.Vehicle:
			result = 1328;
			break;
		case CreationItemClass.Aircraft:
			result = 1330;
			break;
		case CreationItemClass.SimpleObject:
			result = 1542;
			break;
		}
		return result;
	}

	public void PlayerItemEventHandler(object sender, ItemPackage.EventArg e)
	{
		ItemPackage.EventArg.Op op = e.op;
		PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
		if (cmpt == null)
		{
			return;
		}
		ItemObject itemObj = e.itemObj;
		if (itemObj == null)
		{
			return;
		}
		if (e.itemObj.protoId == 1508)
		{
			PlayerMission playerMission = MissionManager.Instance.m_PlayerMission;
			if (SingleGameStory.curType == SingleGameStory.StoryScene.PajaShip)
			{
				playerMission.pajaLanguageBePickup = ((playerMission.pajaLanguageBePickup == 0) ? 1 : ((playerMission.pajaLanguageBePickup != 2) ? playerMission.pajaLanguageBePickup : 3));
			}
			else if (SingleGameStory.curType == SingleGameStory.StoryScene.LaunchCenter)
			{
				playerMission.pajaLanguageBePickup = ((playerMission.pajaLanguageBePickup == 0) ? 2 : ((playerMission.pajaLanguageBePickup != 1) ? playerMission.pajaLanguageBePickup : 3));
			}
		}
		if (itemObj.protoId == 1541)
		{
			int num = -1;
			foreach (KeyValuePair<int, Vector3> textSample in MissionManager.Instance.m_PlayerMission.textSamples)
			{
				if (Vector3.Distance(PeSingleton<PeCreature>.Instance.mainPlayer.position, textSample.Value) < 50f)
				{
					num = textSample.Key;
					break;
				}
			}
			if (num != -1)
			{
				GameObject gameObject = GameObject.Find("language_sample_canUse(Clone):" + num);
				if (gameObject != null)
				{
					if (PeGameMgr.IsSingle)
					{
						MissionManager.Instance.m_PlayerMission.textSamples.Remove(num);
						UnityEngine.Object.Destroy(gameObject);
					}
					else
					{
						PlayerNetwork.mainPlayer.DestroySceneItem(gameObject.transform.position);
					}
				}
			}
		}
		if (itemObj.protoId == 1332)
		{
			GameObject gameObject2 = GameObject.Find("backpack");
			if (gameObject2 != null)
			{
				if (PeGameMgr.IsSingle)
				{
					UnityEngine.Object.Destroy(gameObject2);
					if (GameUI.Instance.mItemGet != null)
					{
						GameUI.Instance.mItemGet.Hide();
					}
				}
				else
				{
					PlayerNetwork.mainPlayer.DestroySceneItem(gameObject2.transform.position);
				}
			}
		}
		if (itemObj.protoId == 1508 && PeGameMgr.IsSingle)
		{
			ItemDrop itemDrop = languages.Find(delegate(ItemDrop item)
			{
				if (item == null)
				{
					return false;
				}
				float num2 = Vector3.Distance(item.gameObject.transform.position, PeSingleton<PeCreature>.Instance.mainPlayer.position);
				return num2 < 10f;
			});
			if (null != itemDrop && null != itemDrop.gameObject)
			{
				UnityEngine.Object.Destroy(itemDrop.gameObject);
				if (GameUI.Instance.mItemGet != null)
				{
					GameUI.Instance.mItemGet.Hide();
				}
			}
		}
		if (itemObj.protoId == 1340)
		{
			GameObject gameObject3 = GameObject.Find("probe");
			if (gameObject3 != null)
			{
				UnityEngine.Object.Destroy(gameObject3);
				if (GameUI.Instance.mItemGet != null)
				{
					GameUI.Instance.mItemGet.Hide();
				}
			}
		}
		if (itemObj.protoId == 1342)
		{
			GameObject gameObject4 = GameObject.Find("hugefish_bone");
			if (gameObject4 != null)
			{
				UnityEngine.Object.Destroy(gameObject4);
				if (GameUI.Instance.mItemGet != null)
				{
					GameUI.Instance.mItemGet.Hide();
				}
			}
		}
		if (op != 0)
		{
			return;
		}
		if (itemObj.protoData.itemClassId > 0)
		{
			int num3 = ItemClassIdtoProtoId(itemObj.protoData.itemClassId);
			if (num3 == 0)
			{
				MissionManager.Instance.ProcessCollectMissionByID(itemObj.protoId);
			}
			else
			{
				MissionManager.Instance.ProcessCollectMissionByID(num3);
			}
		}
		else
		{
			MissionManager.Instance.ProcessCollectMissionByID(itemObj.protoId);
		}
		if (itemObj.protoId == 1339 && KillNPC.ashBox_inScene > 0)
		{
			KillNPC.ashBox_inScene--;
		}
	}

	public void PlayerBuildingEventHandler(IntVector3[] indexes, BSVoxel[] voxels, BSVoxel[] oldvoxels, EBSBrushMode mode, IBSDataSource d)
	{
		if (d != BuildingMan.Blocks)
		{
			return;
		}
		if (mode == EBSBrushMode.Add)
		{
			for (int i = 0; i < indexes.Length; i++)
			{
				Vector3 pos = new Vector3((float)indexes[i].x * d.Scale, (float)indexes[i].y * d.Scale, (float)indexes[i].z * d.Scale);
				MissionManager.Instance.ProcessUseItemMissionByID(PEBuildingMan.GetBlockItemProtoID(voxels[i].materialType), pos);
			}
		}
		else
		{
			for (int j = 0; j < indexes.Length; j++)
			{
				Vector3 pos2 = new Vector3((float)indexes[j].x * d.Scale, (float)indexes[j].y * d.Scale, (float)indexes[j].z * d.Scale);
				MissionManager.Instance.ProcessUseItemMissionByID(PEBuildingMan.GetBlockItemProtoID(oldvoxels[j].materialType), pos2, -1);
			}
		}
	}

	public void PlayerDragItemEventHandler(object sender, DraggingMgr.EventArg e)
	{
		if (e.dragable is ItemObjDragging itemObjDragging)
		{
			PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
			if (!(cmpt == null) && !PeGameMgr.IsMulti)
			{
				MissionManager.Instance.ProcessUseItemMissionByID(itemObjDragging.GetItemProtoId(), itemObjDragging.GetPos(), 1, itemObjDragging.GetItemDrag().itemObj);
			}
		}
	}

	public void PlayerUseItemEventHandler(object sender, UseItemCmpt.EventArg e)
	{
		ItemObject itemObj = e.itemObj;
		if (itemObj != null)
		{
			MissionManager.Instance.ProcessUseItemMissionByID(itemObj.protoId, GetPlayerPos());
			MissionManager.Instance.ProcessCollectMissionByID(itemObj.protoId);
		}
	}

	public void PlayerUseEquipmentHandler(int protoId)
	{
		MissionManager.Instance.ProcessUseItemMissionByID(protoId, GetPlayerPos());
		MissionManager.Instance.ProcessCollectMissionByID(protoId);
	}

	public int GetPosEntityCount(Vector3 pos, int Radius)
	{
		int result = 0;
		int num = ((!(pos.x > 0f)) ? (Radius * -1) : Radius);
		int num2 = (int)(pos.x + (float)num) / (Radius * 2);
		num = ((!(pos.z > 0f)) ? (Radius * -1) : Radius);
		int num3 = (int)(pos.z + (float)num) / (Radius * 2);
		Vector2 key = new Vector2(num2, num3);
		if (!m_CreatedNpcList.ContainsKey(key))
		{
			return 0;
		}
		return result;
	}

	public Vector3 GetPatrolPoint(Vector3 center, int iMin, int iMax, bool bCheck = true)
	{
		Vector2 vector = UnityEngine.Random.insideUnitCircle;
		Vector3 result;
		do
		{
			vector = vector.normalized * UnityEngine.Random.Range(iMin, iMax);
			result = center + new Vector3(vector.x, 0f, vector.y);
			IntVector2 worldPosXZ = new IntVector2((int)result.x, (int)result.z);
			bool canGenNpc = false;
			result.y = VFDataRTGen.GetPosTop(worldPosXZ, out canGenNpc);
		}
		while (result.y > -1.01f && result.y < -0.99f);
		return result;
	}

	private void UpdateCamp()
	{
		bool flag = true;
		for (int i = 0; i < m_NeedCampTalk.Count; i++)
		{
			CampPatrolData campData = CampPatrolData.GetCampData(m_NeedCampTalk[i]);
			if (campData == null)
			{
				continue;
			}
			float num = Vector3.Distance(GetPlayerPos(), campData.mPosition);
			if (num > campData.mRadius)
			{
				continue;
			}
			for (int j = 0; j < campData.m_PreLimit.Count; j++)
			{
				if (campData.m_PreLimit[j] == 0)
				{
					continue;
				}
				if (campData.m_PreLimit[j] < 1000)
				{
					if (!MissionManager.Instance.HadCompleteMission(campData.m_PreLimit[j]))
					{
						flag = false;
						break;
					}
				}
				else if (!MissionManager.Instance.HadCompleteTarget(campData.m_PreLimit[j]))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				for (int k = 0; k < campData.m_TalkList.Count; k++)
				{
					GameUI.Instance.mServantTalk.AddTalk(campData.m_TalkList[k]);
				}
				m_NeedCampTalk.Remove(m_NeedCampTalk[i]);
			}
		}
	}

	private void UpdateUIWindow()
	{
		if (PeSingleton<PeCreature>.Instance.mainPlayer == null || GameUI.Instance == null)
		{
			return;
		}
		if (GameUI.Instance.mNpcWnd.isShow && GameUI.Instance.mNpcWnd.m_CurSelNpc != null)
		{
			float num = Vector3.Distance(EntityCreateMgr.Instance.GetPlayerPos(), GameUI.Instance.mNpcWnd.m_CurSelNpc.ExtGetPos());
			if (!(num < 8f))
			{
				GameUI.Instance.mNpcWnd.Hide();
			}
		}
		else if (GameUI.Instance.mShopWnd.isShow && GameUI.Instance.mNpcWnd.m_CurSelNpc != null)
		{
			float num2 = Vector3.Distance(EntityCreateMgr.Instance.GetPlayerPos(), GameUI.Instance.mNpcWnd.m_CurSelNpc.ExtGetPos());
			if (!(num2 < 8f))
			{
				GameUI.Instance.mShopWnd.Hide();
			}
		}
	}
}
