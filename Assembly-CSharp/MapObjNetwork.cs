using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ItemAsset;
using Pathea;
using uLink;
using UnityEngine;

public class MapObjNetwork : SkNetworkInterface
{
	protected DoodadType objType;

	protected int _assetId;

	protected int _protoTypeId;

	protected int _campId;

	protected int _damageId;

	protected int _dPlayerId;

	protected uLink.NetworkView playerView;

	private List<int> itemList = new List<int>();

	private ItemBox itemBox;

	public WareHouseObject wareHouseObj;

	private ItemDrop itemDrop;

	private AiTowerSyncData _syncData;

	private PeEntity entity;

	private string _sceneItemName;

	public static List<MapObjNetwork> mapObjNetworkMgr = new List<MapObjNetwork>();

	private static Transform ParentTrans;

	private int townId;

	internal DoodadType ObjType => objType;

	public int AssetId => _assetId;

	public int CampId => _campId;

	public int DamageId => _damageId;

	public int DPlayerId => _dPlayerId;

	public int aimId { get; protected set; }

	internal uLink.NetworkView PlayerView => playerView;

	public bool isTower { get; private set; }

	public static MapObjNetwork GetNet(int objId, int type)
	{
		if (objId != -1)
		{
			foreach (MapObjNetwork item in mapObjNetworkMgr)
			{
				if (item._assetId == objId && item.objType == (DoodadType)type)
				{
					return item;
				}
			}
		}
		return null;
	}

	public static MapObjNetwork GetNet(int entityId)
	{
		foreach (MapObjNetwork item in mapObjNetworkMgr)
		{
			if (item.Id == entityId)
			{
				return item;
			}
		}
		return null;
	}

	public static MapObjNetwork GetNet(string objName)
	{
		if (objName.Length == 0)
		{
			return null;
		}
		foreach (MapObjNetwork item in mapObjNetworkMgr)
		{
			if (item._sceneItemName == objName)
			{
				return item;
			}
		}
		return null;
	}

	public static bool HadCreate(int boxid, int type)
	{
		if (boxid != -1)
		{
			foreach (MapObjNetwork item in mapObjNetworkMgr)
			{
				if (item._assetId == boxid && item.objType == (DoodadType)type)
				{
					return true;
				}
			}
		}
		return false;
	}

	protected override void OnPEStart()
	{
		BindSkAction();
		BindAction(EPacketType.PT_MO_RequestItemList, RPC_S2C_RequestItemList);
		BindAction(EPacketType.PT_MO_ModifyItemList, RPC_S2C_ModifyItemList);
		BindAction(EPacketType.PT_MO_RemoveItem, RPC_S2C_RemoveItem);
		BindAction(EPacketType.PT_CL_SyncCreationHP, RPC_S2C_SyncCreationHP);
		BindAction(EPacketType.PT_MO_StartRepair, RPC_S2C_Repair);
		BindAction(EPacketType.PT_MO_StopRepair, RPC_S2C_StopRepair);
		BindAction(EPacketType.PT_MO_SyncRepairTime, RPC_S2C_SyncRepairTime);
		BindAction(EPacketType.PT_CL_SyncCreationFuel, RPC_S2C_SyncCreationFuel);
		BindAction(EPacketType.PT_Common_ScenarioId, RPC_S2C_ScenarioId);
		BindAction(EPacketType.PT_InGame_SetController, RPC_S2C_SetController);
		BindAction(EPacketType.PT_InGame_LostController, RPC_S2C_LostController);
		BindAction(EPacketType.PT_Tower_Target, RPC_Tower_Target);
		BindAction(EPacketType.PT_Tower_Fire, RPC_S2C_Fire);
		BindAction(EPacketType.PT_Tower_AimPosition, RPC_S2C_AimPosition);
		BindAction(EPacketType.PT_Tower_LostEnemy, RPC_S2C_LostEnemy);
		BindAction(EPacketType.PT_InGame_InitData, RPC_S2C_InitData);
	}

	protected override void OnPEDestroy()
	{
		base.OnPEDestroy();
		if (null != itemBox && ItemBoxMgr.Instance != null)
		{
			ItemBoxMgr.Instance.RemoveItemMultiPlay(itemBox.mID);
		}
		mapObjNetworkMgr.Remove(this);
		if (_sceneItemName == "1_larve_Q425")
		{
			GameObject gameObject = GameObject.Find("larve_Q425(Clone)");
			if (!(gameObject == null))
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}
		else if (_sceneItemName == "backpack")
		{
			GameObject gameObject2 = GameObject.Find("backpack");
			if (!(gameObject2 == null))
			{
				UnityEngine.Object.Destroy(gameObject2);
			}
		}
		else if (_sceneItemName == "Fruit_pack_1")
		{
			GameObject gameObject3 = GameObject.Find("fruitpack");
			if (!(gameObject3 == null))
			{
				UnityEngine.Object.Destroy(gameObject3);
			}
		}
		else if (_sceneItemName != null && _sceneItemName.Contains("language_sample_canUse(Clone):"))
		{
			GameObject gameObject4 = GameObject.Find(_sceneItemName);
			if (!(gameObject4 == null))
			{
				UnityEngine.Object.Destroy(gameObject4);
			}
		}
	}

	protected override void ResetContorller()
	{
		base.ResetContorller();
		if (base.hasOwnerAuth && null != entity && null != entity.Tower && isTower)
		{
			StartCoroutine(SyncMove());
		}
	}

	public override void InitForceData()
	{
		if (null != entity && isTower)
		{
			entity.SetAttribute(AttribType.CampID, CampId);
			entity.SetAttribute(AttribType.DamageID, DamageId);
		}
	}

	protected IEnumerator SyncMove()
	{
		base._pos = base.transform.position;
		base.rot = base.transform.rotation;
		while (base.hasOwnerAuth)
		{
			if (null != entity && null != entity.Tower && (entity.Tower.ChassisY != _syncData.ChassisY || !entity.Tower.PitchEuler.Equals(_syncData.PitchEuler)))
			{
				_syncData.ChassisY = entity.Tower.ChassisY;
				_syncData.PitchEuler = entity.Tower.PitchEuler;
			}
			yield return new WaitForSeconds(1f / uLink.Network.sendRate);
		}
	}

	private IEnumerator WaitForTarget()
	{
		yield return new WaitForSeconds(3f);
		int oldId = -1;
		while (true)
		{
			if (oldId != aimId)
			{
				PeEntity tarEntity = PeSingleton<EntityMgr>.Instance.Get(aimId);
				if (null != tarEntity && tarEntity.hasView && null != entity && null != entity.Tower)
				{
					entity.Tower.Target = tarEntity.centerBone;
					oldId = aimId;
				}
			}
			yield return new WaitForSeconds(1f);
		}
	}

	public void AddToItemlist(int objID)
	{
		if (objID > 0)
		{
			itemList.Add(objID);
		}
	}

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		objType = (DoodadType)info.networkView.initialData.Read<int>(new object[0]);
		int opID = info.networkView.initialData.Read<int>(new object[0]);
		_teamId = info.networkView.initialData.Read<int>(new object[0]);
		_assetId = info.networkView.initialData.Read<int>(new object[0]);
		_id = info.networkView.initialData.Read<int>(new object[0]);
		_protoTypeId = info.networkView.initialData.Read<int>(new object[0]);
		Vector3 scl = info.networkView.initialData.Read<Vector3>(new object[0]);
		string text = info.networkView.initialData.Read<string>(new object[0]);
		SceneDoodadDesc sceneDoodadDesc = null;
		base._pos = base.transform.position;
		base.rot = base.transform.rotation;
		if (null == ParentTrans)
		{
			ParentTrans = new GameObject("DoodadNetworkMgr").transform;
		}
		base.transform.parent = ParentTrans;
		if (PeGameMgr.IsMultiStory)
		{
			sceneDoodadDesc = StoryDoodadMap.Get(_assetId);
		}
		if (objType == DoodadType.DoodadType_Drop || objType == DoodadType.DoodadType_Dead)
		{
			itemBox = ItemBoxMgr.Instance.AddItemMultiPlay(base.OwnerView.viewID.id, opID, base._pos, this);
		}
		else if (objType == DoodadType.DoodadType_SceneBox)
		{
			if (sceneDoodadDesc != null)
			{
				entity = DoodadEntityCreator.CreateStoryDoodadNet(_assetId, base.Id);
			}
			else
			{
				entity = DoodadEntityCreator.CreateDoodadNet(_protoTypeId, base._pos, scl, base.rot, base.Id);
			}
			if (entity != null)
			{
				WareHouseObject component = entity.gameObject.GetComponent<WareHouseObject>();
				if (component != null)
				{
					component._id = _assetId;
				}
			}
		}
		else if (objType == DoodadType.DoodadType_SceneItem)
		{
			string[] array = text.Split('|');
			if (array.Length != 2)
			{
				return;
			}
			_sceneItemName = array[1];
			if (_sceneItemName == "ash_box")
			{
				itemBox = ItemBoxMgr.Instance.AddItemMultiPlay(base.OwnerView.viewID.id, _assetId, base._pos, this);
			}
			else if (_sceneItemName == "ash_ball")
			{
				itemBox = ItemBoxMgr.Instance.AddItemMultiPlay(base.OwnerView.viewID.id, _assetId, base._pos, this);
			}
			else
			{
				RequestItemList();
			}
		}
		else if (objType == DoodadType.DoodadType_Repair)
		{
			if (sceneDoodadDesc != null)
			{
				entity = DoodadEntityCreator.CreateStoryDoodadNet(_assetId, base.Id);
			}
			else
			{
				entity = DoodadEntityCreator.CreateDoodadNet(_protoTypeId, base._pos, scl, base.rot, base.Id);
			}
		}
		else if (objType == DoodadType.DoodadType_Power)
		{
			if (sceneDoodadDesc != null)
			{
				entity = DoodadEntityCreator.CreateStoryDoodadNet(_assetId, base.Id);
			}
			else
			{
				entity = DoodadEntityCreator.CreateDoodadNet(_protoTypeId, base._pos, scl, base.rot, base.Id);
			}
		}
		else if (objType == DoodadType.DoodadType_RandomBuilding || objType == DoodadType.DoodadType_RandomBuilding_Repair || objType == DoodadType.DoodadType_RandomBuilding_Power)
		{
			ExtractParam(text, out townId, out _campId, out _damageId, out _dPlayerId);
			if (sceneDoodadDesc != null)
			{
				entity = DoodadEntityCreator.CreateStoryDoodadNet(_assetId, base.Id);
			}
			else
			{
				entity = DoodadEntityCreator.CreateNetRandTerDoodad(base.Id, _protoTypeId, base._pos, scl, base.rot, townId, _campId, _damageId, _dPlayerId);
			}
		}
		else if (sceneDoodadDesc != null)
		{
			entity = DoodadEntityCreator.CreateStoryDoodadNet(_assetId, base.Id);
		}
		else
		{
			entity = DoodadEntityCreator.CreateDoodadNet(_protoTypeId, base._pos, scl, base.rot, base.Id);
		}
		if (entity != null)
		{
			OnSpawned(entity.gameObject);
		}
		mapObjNetworkMgr.Add(this);
		base.gameObject.name = $"Mapobj assetId:{_assetId}, protoTypeId:{_protoTypeId}, objType:{objType}, entityId:{_id}";
	}

	public static string PackParam(int townId, int campId, int damageId, int dPlayerId)
	{
		return townId + "," + campId + "," + damageId + "," + dPlayerId;
	}

	public static void ExtractParam(string param, out int townId, out int campId, out int damageId, out int dPlayerId)
	{
		List<int> list = new List<int>();
		string[] array = param.Split(',');
		if (array.Length != 4)
		{
			Debug.LogError("doodadRandomBuilding param error: " + param);
		}
		townId = Convert.ToInt32(array[0]);
		campId = Convert.ToInt32(array[1]);
		damageId = Convert.ToInt32(array[2]);
		dPlayerId = Convert.ToInt32(array[3]);
	}

	public override void OnSpawned(GameObject obj)
	{
		base.OnSpawned(obj);
		StartCoroutine(AuthorityCheckCoroutine());
		if (null == entity.netCmpt)
		{
			entity.netCmpt = entity.Add<NetCmpt>();
		}
		entity.netCmpt.network = this;
		RPCServer(EPacketType.PT_InGame_InitData);
	}

	public void GetAllItem()
	{
		RPCServer(EPacketType.PT_MO_GetAllItem);
	}

	public void GetItem(int itemID)
	{
		if (itemID > 0)
		{
			RPCServer(EPacketType.PT_MO_GetItem, itemID);
		}
	}

	public void RequestItemList()
	{
		RPCServer(EPacketType.PT_MO_RequestItemList);
	}

	public void InsertItemList(int[] itemlist)
	{
		if (itemlist.Count() != 0)
		{
			RPCServer(EPacketType.PT_MO_InsertItemList, itemlist.ToArray());
		}
	}

	public void InsertItemList(int itemId, int index)
	{
		RPCServer(EPacketType.PT_MO_InsertItemList, itemId, index);
	}

	public void RequestRepair(int itemObj)
	{
		RPCServer(EPacketType.PT_MO_StartRepair, itemObj);
	}

	public void RequestStopRepair(int itemObj)
	{
		RPCServer(EPacketType.PT_MO_StopRepair, itemObj);
	}

	public void RequestRepairTime()
	{
		RPCServer(EPacketType.PT_MO_SyncRepairTime);
	}

	public void RequestInitData()
	{
		RPCServer(EPacketType.PT_Common_ScenarioId);
	}

	public void RequestAimTarget(int skEntityId)
	{
		RPCServer(EPacketType.PT_Tower_AimPosition, skEntityId);
	}

	public void RequestFire(int skEntityId)
	{
		RPCServer(EPacketType.PT_Tower_Fire, skEntityId);
	}

	public void RequestEnemyLost()
	{
		RPCServer(EPacketType.PT_Tower_LostEnemy);
	}

	public void RPC_S2C_RequestItemList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] array = stream.Read<int[]>(new object[0]);
		itemList.Clear();
		int[] array2 = array;
		foreach (int item in array2)
		{
			itemList.Add(item);
		}
		if (objType == DoodadType.DoodadType_Dead || objType == DoodadType.DoodadType_Drop)
		{
			itemBox.OnRequestItemList(itemList);
		}
		else if (objType == DoodadType.DoodadType_SceneBox)
		{
			if (wareHouseObj != null)
			{
				wareHouseObj.ResetItemByIdList(itemList);
			}
		}
		else if (objType == DoodadType.DoodadType_SceneItem)
		{
			if (_sceneItemName == "backpack")
			{
				if (itemList.Count > 0)
				{
					itemDrop = StroyManager.CreateBackpack(base.transform.position, itemList, this);
				}
			}
			else if (_sceneItemName == "pajaLanguage")
			{
				if (itemList.Count > 0)
				{
					itemDrop = StroyManager.CreatePajaLanguage(base.transform.position, itemList, this);
				}
			}
			else if (_sceneItemName == "probe")
			{
				if (itemList.Count > 0)
				{
					itemDrop = StroyManager.CreateProbe(base.transform.position, itemList, this);
				}
			}
			else if (_sceneItemName == "hugefish_bone")
			{
				if (itemList.Count > 0)
				{
					itemDrop = StroyManager.CreateHugefish_bone(base.transform.position, itemList, this);
				}
			}
			else if (_sceneItemName == "1_larve_Q425")
			{
				itemDrop = StroyManager.Createlarve_Q425(base.transform.position);
			}
			else if (_sceneItemName == "ash_box")
			{
				itemBox.OnRequestItemList(itemList);
			}
			else if (_sceneItemName == "ash_ball")
			{
				itemBox.OnRequestItemList(itemList);
			}
			else if (_sceneItemName.Contains("language_sample_canUse(Clone):"))
			{
				if (itemList.Count > 0)
				{
					itemDrop = StroyManager.CreateLanguageSampleNet(_sceneItemName, base.transform.position, itemList, this);
				}
			}
			else if (_sceneItemName.Contains("coelodonta_rhino_bone"))
			{
				if (itemList.Count > 0)
				{
					itemDrop = StroyManager.CreateAndHeraNest_indexNet(_sceneItemName, base.transform.position, itemList, this);
				}
			}
			else if (_sceneItemName.Contains("lepus_hare_bone"))
			{
				if (itemList.Count > 0)
				{
					itemDrop = StroyManager.CreateAndHeraNest_indexNet(_sceneItemName, base.transform.position, itemList, this);
				}
			}
			else if (_sceneItemName.Contains("andhera_queen_egg") && itemList.Count > 0)
			{
				itemDrop = StroyManager.CreateAndHeraNest_indexNet(_sceneItemName, base.transform.position, itemList, this);
			}
		}
		else if (objType == DoodadType.DoodadType_Repair || objType == DoodadType.DoodadType_RandomBuilding_Repair)
		{
			if (itemList.Count > 0)
			{
				GameUI.Instance.mRepair.SetItemByNet(this, itemList[0]);
			}
			else
			{
				GameUI.Instance.mRepair.SetItemByNet(this, -1);
			}
		}
		else if (objType == DoodadType.DoodadType_Power || objType == DoodadType.DoodadType_RandomBuilding_Power)
		{
			GameUI.Instance.mPowerPlantSolar.OnMultiOpenDropCallBack(this, itemList.ToArray());
		}
	}

	public void RPC_S2C_ModifyItemList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] array = stream.Read<int[]>(new object[0]);
		itemList.Clear();
		int[] array2 = array;
		foreach (int item in array2)
		{
			itemList.Add(item);
		}
		if (objType == DoodadType.DoodadType_Dead || objType == DoodadType.DoodadType_Drop)
		{
			itemBox.ResetItem(itemList);
		}
		else if (objType == DoodadType.DoodadType_SceneBox)
		{
			wareHouseObj.ResetItemByIdList(itemList);
		}
		else if (objType == DoodadType.DoodadType_SceneItem)
		{
			if (!(itemDrop != null))
			{
				return;
			}
			itemDrop.RemoveDroppableItemAll();
			int[] array3 = array;
			foreach (int id in array3)
			{
				ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
				if (itemObject != null)
				{
					itemDrop.AddItem(itemObject);
				}
			}
		}
		else if (objType == DoodadType.DoodadType_Repair || objType == DoodadType.DoodadType_RandomBuilding_Repair)
		{
			if (itemList.Count > 0)
			{
				GameUI.Instance.mRepair.DropItemByNet(this, itemList[0]);
			}
		}
		else if (objType == DoodadType.DoodadType_Power || objType == DoodadType.DoodadType_RandomBuilding_Power)
		{
			GameUI.Instance.mPowerPlantSolar.OnMultiOpenDropCallBack(this, itemList.ToArray());
		}
	}

	public void RPC_S2C_RemoveItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int itemID = stream.Read<int>(new object[0]);
		int num = itemList.FindIndex((int iter) => iter == itemID);
		if (num >= 0 && objType != DoodadType.DoodadType_Power && objType != DoodadType.DoodadType_RandomBuilding_Power)
		{
			itemList.RemoveAt(num);
		}
		if (objType == DoodadType.DoodadType_Dead || objType == DoodadType.DoodadType_Drop)
		{
			itemBox.RemoveItem(itemID);
		}
		else if (objType == DoodadType.DoodadType_SceneBox)
		{
			wareHouseObj.RemoveItemById(itemID);
		}
		else if (objType == DoodadType.DoodadType_SceneItem)
		{
			if (itemDrop != null)
			{
				ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(itemID);
				if (itemObject != null)
				{
					itemDrop.RemoveDroppableItem(itemObject);
				}
			}
		}
		else if (objType == DoodadType.DoodadType_Repair || objType == DoodadType.DoodadType_RandomBuilding_Repair)
		{
			GameUI.Instance.mRepair.DropItemByNet(this, -1);
		}
		else if ((objType == DoodadType.DoodadType_Power || objType == DoodadType.DoodadType_RandomBuilding_Power) && itemList.Count > num && num >= 0)
		{
			itemList[num] = -1;
			GameUI.Instance.mPowerPlantSolar.OnMultiRemoveCallBack(this, num, itemID);
		}
	}

	private void RPC_S2C_Repair(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (objType == DoodadType.DoodadType_Repair || objType == DoodadType.DoodadType_RandomBuilding_Repair)
		{
			GameUI.Instance.mRepair.UpdateItemForNet(this);
		}
	}

	private void RPC_S2C_StopRepair(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int instanceId = stream.Read<int>(new object[0]);
		if (objType == DoodadType.DoodadType_Repair || objType == DoodadType.DoodadType_RandomBuilding_Repair)
		{
			GameUI.Instance.mRepair.ResetItemByNet(this, instanceId);
		}
	}

	private void RPC_S2C_SyncRepairTime(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		float curTime = stream.Read<float>(new object[0]);
		float finalTime = stream.Read<float>(new object[0]);
		if (objType == DoodadType.DoodadType_Repair || objType == DoodadType.DoodadType_RandomBuilding_Repair)
		{
			GameUI.Instance.mRepair.SetCounterByNet(this, curTime, finalTime);
		}
	}

	private void RPC_S2C_SyncCreationHP(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		float current = stream.Read<float>(new object[0]);
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
		LifeLimit cmpt = itemObject.GetCmpt<LifeLimit>();
		cmpt.floatValue.current = current;
	}

	private void RPC_S2C_SyncCreationFuel(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int id = stream.Read<int>(new object[0]);
		float current = stream.Read<float>(new object[0]);
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
		Energy cmpt = itemObject.GetCmpt<Energy>();
		cmpt.floatValue.current = current;
	}

	private void RPC_S2C_ScenarioId(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int scenarioId = stream.Read<int>(new object[0]);
		if (null != entity)
		{
			entity.scenarioId = scenarioId;
		}
	}

	protected override void RPC_S2C_SetController(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		base.authId = stream.Read<int>(new object[0]);
		ResetContorller();
	}

	protected override void RPC_S2C_LostController(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		base.authId = -1;
		ResetContorller();
		if (base.canGetAuth)
		{
			RPCServer(EPacketType.PT_InGame_SetController);
		}
	}

	protected void RPC_Tower_Target(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth && !(null == entity) && !(null == entity.Tower) && stream.TryRead<float>(out var value) && stream.TryRead<Vector3>(out var value2))
		{
			entity.Tower.ApplyChassis(value);
			entity.Tower.ApplyPitchEuler(value2);
		}
	}

	private void RPC_S2C_AimPosition(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth)
		{
			aimId = stream.Read<int>(new object[0]);
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(aimId);
			if (!(null == peEntity) && !(peEntity.skEntity == null) && null != entity && null != entity.Tower)
			{
				entity.Tower.Target = peEntity.centerBone;
			}
		}
	}

	private void RPC_S2C_LostEnemy(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (null != entity && null != entity.Tower)
		{
			entity.Tower.Target = null;
		}
	}

	private void RPC_S2C_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		aimId = stream.Read<int>(new object[0]);
		StartCoroutine(WaitForTarget());
	}

	private void RPC_S2C_Fire(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!base.hasOwnerAuth)
		{
			int entityId = stream.Read<int>(new object[0]);
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(entityId);
			if (!(null == peEntity) && !(peEntity.skEntity == null) && null != entity && null != entity.Tower)
			{
				entity.Tower.Fire(peEntity.skEntity);
			}
		}
	}
}
