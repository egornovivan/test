using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ItemAsset;
using Pathea.Effect;
using Pathea.PeEntityExt;
using PeEvent;
using SkillSystem;
using SoundAsset;
using UnityEngine;

namespace Pathea;

public class EquipmentCmpt : PeCmpt, IPeMsg
{
	public class EventArg : PeEvent.EventArg
	{
		public bool isAdd;

		public ItemObject itemObj;
	}

	public interface Receiver
	{
		bool CanAddItemList(List<ItemObject> items);

		void AddItemList(List<ItemObject> items);
	}

	private const int VersionID = 1;

	private Event<EventArg> mEventor = new Event<EventArg>();

	public Action<List<SuitSetData.MatchData>> onSuitSetChange;

	private SkEntity mSkEntity;

	private BiologyViewCmpt mViewCmpt;

	private NpcCmpt mNPC;

	private Motion_Equip mMotionEquip;

	[HideInInspector]
	public bool mShowModel;

	public Receiver mItemReciver;

	private List<ItemObject> mItemList;

	private List<PEEquipment> mEquipments;

	private Dictionary<int, PEEquipmentLogic> mLogics;

	private List<ItemObject> m_InitItems = new List<ItemObject>();

	private List<IWeapon> m_RetList = new List<IWeapon>();

	public EquipType mEquipType;

	private Transform m_EquipmentLogicParent;

	private bool m_HideEquipmentByFirstPerson;

	private bool m_HideEquipmentByVehicle;

	private bool m_HideEquipmentByRagdoll;

	private List<ItemObject> m_TakeOffEquip;

	private bool m_EquipmentDirty;

	private List<int> m_EXBuffs = new List<int>();

	private List<SuitSetData.MatchData> m_SuitSetMatchDatas = new List<SuitSetData.MatchData>();

	public Event<EventArg> changeEventor => mEventor;

	public List<ItemObject> _ItemList => mItemList;

	private List<PEEquipment> _equipmentList => mEquipments;

	public List<IWeapon> _Weapons
	{
		get
		{
			m_RetList.Clear();
			for (int i = 0; i < _equipmentList.Count; i++)
			{
				if (_equipmentList[i] is IWeapon weapon && !weapon.Equals(null))
				{
					m_RetList.Add(weapon);
				}
			}
			return m_RetList;
		}
	}

	public bool handEmpty
	{
		get
		{
			for (int i = 0; i < _equipmentList.Count; i++)
			{
				PEEquipment pEEquipment = _equipmentList[i];
				if (pEEquipment is IWeapon)
				{
					return false;
				}
				if (pEEquipment is PEHoldAbleEquipment)
				{
					return false;
				}
				if (pEEquipment is PEWaterPitcher)
				{
					return false;
				}
			}
			return true;
		}
	}

	public ItemObject mainHandEquipment { get; set; }

	public List<SuitSetData.MatchData> matchDatas => m_SuitSetMatchDatas;

	public int DroppableItemCount => mItemList.Count;

	public event Action OnEquipmentChange;

	public void SetSkillBook(ISkillTree skillTree)
	{
	}

	public override void Awake()
	{
		base.Awake();
		mItemList = new List<ItemObject>();
		mEquipments = new List<PEEquipment>();
		mLogics = new Dictionary<int, PEEquipmentLogic>();
		m_TakeOffEquip = new List<ItemObject>();
		m_EquipmentLogicParent = new GameObject("EquipmentLogic").transform;
		m_EquipmentLogicParent.parent = base.transform;
	}

	public override void Start()
	{
		base.Start();
		mSkEntity = base.Entity.skEntity;
		mViewCmpt = base.Entity.biologyViewCmpt;
		mNPC = base.Entity.NpcCmpt;
		mMotionEquip = base.Entity.motionEquipment;
		if (m_InitItems != null && m_InitItems.Count > 0)
		{
			for (int i = 0; i < m_InitItems.Count; i++)
			{
				PutOnEquipment(m_InitItems[i]);
			}
			m_InitItems.Clear();
			LodCmpt lodCmpt = base.Entity.lodCmpt;
			lodCmpt.onConstruct = (Action<PeEntity>)Delegate.Combine(lodCmpt.onConstruct, (Action<PeEntity>)delegate(PeEntity e)
			{
				e.StartCoroutine(PreLoad());
			});
		}
	}

	public override void OnUpdate()
	{
		CheckEXBuffs();
	}

	public void AddInitEquipment(ItemObject itemObj)
	{
		m_InitItems.Add(itemObj);
	}

	public bool HasEquip(EquipType equipType)
	{
		return null != GetEquip(equipType);
	}

	public PEEquipment GetEquip(EquipType equipType)
	{
		return mEquipments.Find((PEEquipment itr) => itr.equipType == equipType);
	}

	public bool IsEquipNow(int itemInstanceID)
	{
		for (int i = 0; i < _ItemList.Count; i++)
		{
			if (_ItemList[i].instanceId == itemInstanceID)
			{
				return true;
			}
		}
		return false;
	}

	private void ReduceWeaponDurability(ItemObject itemObj)
	{
		if (itemObj != null)
		{
			if (GameConfig.IsMultiMode)
			{
				PlayerNetwork.mainPlayer.RequestWeaponDurability(base.Entity.Id, itemObj.instanceId);
				return;
			}
			Equip cmpt = itemObj.GetCmpt<Equip>();
			cmpt.ExpendAttackDurability(mSkEntity);
		}
	}

	private void ReduceArmorDurability(float damage, SkEntity caster)
	{
		if (GameConfig.IsMultiMode)
		{
			int[] equipIds = mItemList.Select((ItemObject iter) => iter?.instanceId ?? (-1)).ToArray();
			PlayerNetwork.mainPlayer.RequestArmorDurability(base.Entity.Id, equipIds, damage, caster);
			return;
		}
		for (int i = 0; i < mItemList.Count; i++)
		{
			ItemObject itemObject = mItemList[i];
			if (itemObject != null)
			{
				Equip cmpt = itemObject.GetCmpt<Equip>();
				cmpt.ExpendDefenceDurability(mSkEntity, damage);
			}
		}
	}

	public bool NetTryPutOnEquipment(ItemObject itemObj, bool addToReceiver = true, Receiver receiver = null)
	{
		if (null != mViewCmpt && mViewCmpt.IsRagdoll)
		{
			return false;
		}
		Receiver receiver3;
		if (receiver == null)
		{
			Receiver receiver2 = mItemReciver;
			receiver3 = receiver2;
		}
		else
		{
			receiver3 = receiver;
		}
		Receiver receiver4 = receiver3;
		if (itemObj == null)
		{
			return false;
		}
		if (mItemList.Contains(itemObj))
		{
			return false;
		}
		Equip cmpt = itemObj.GetCmpt<Equip>();
		if (cmpt == null)
		{
			return false;
		}
		if (!PeGender.IsMatch(cmpt.sex, base.Entity.ExtGetSex()))
		{
			return false;
		}
		SkillTreeUnitMgr cmpt2 = base.Entity.GetCmpt<SkillTreeUnitMgr>();
		if (cmpt2 != null && RandomMapConfig.useSkillTree && !cmpt2.CheckEquipEnable(cmpt.protoData.itemClassId, cmpt.itemObj.level))
		{
			return false;
		}
		m_TakeOffEquip.Clear();
		for (int i = 0; i < mItemList.Count; i++)
		{
			ItemObject itemObject = mItemList[i];
			if (itemObject == itemObj)
			{
				return false;
			}
			Equip cmpt3 = itemObject.GetCmpt<Equip>();
			if (cmpt3 != null && Convert.ToBoolean(cmpt.equipPos & cmpt3.equipPos))
			{
				m_TakeOffEquip.Add(itemObject);
			}
		}
		for (int j = 0; j < mEquipments.Count; j++)
		{
			if (m_TakeOffEquip.Contains(mEquipments[j].m_ItemObj) && !mEquipments[j].CanTakeOff())
			{
				return false;
			}
		}
		if (receiver4 != null && !receiver4.CanAddItemList(m_TakeOffEquip))
		{
			return false;
		}
		return true;
	}

	public bool PutOnEquipment(ItemObject itemObj, bool addToReceiver = true, Receiver receiver = null, bool netRequest = false)
	{
		if (!netRequest && null != mViewCmpt && mViewCmpt.IsRagdoll)
		{
			return false;
		}
		Receiver receiver3;
		if (receiver == null)
		{
			Receiver receiver2 = mItemReciver;
			receiver3 = receiver2;
		}
		else
		{
			receiver3 = receiver;
		}
		Receiver receiver4 = receiver3;
		if (itemObj == null)
		{
			return false;
		}
		Equip cmpt = itemObj.GetCmpt<Equip>();
		if (cmpt == null)
		{
			return false;
		}
		if (!PeGender.IsMatch(cmpt.sex, base.Entity.ExtGetSex()))
		{
			return false;
		}
		if (mItemList.Contains(itemObj))
		{
			return false;
		}
		SkillTreeUnitMgr cmpt2 = base.Entity.GetCmpt<SkillTreeUnitMgr>();
		if (!netRequest && cmpt2 != null && RandomMapConfig.useSkillTree && !cmpt2.CheckEquipEnable(cmpt.protoData.itemClassId, cmpt.itemObj.level))
		{
			return false;
		}
		m_TakeOffEquip.Clear();
		for (int i = 0; i < mItemList.Count; i++)
		{
			ItemObject itemObject = mItemList[i];
			Equip cmpt3 = itemObject.GetCmpt<Equip>();
			if (cmpt3 != null && Convert.ToBoolean(cmpt.equipPos & cmpt3.equipPos))
			{
				m_TakeOffEquip.Add(itemObject);
			}
		}
		for (int j = 0; j < mEquipments.Count; j++)
		{
			if (m_TakeOffEquip.Contains(mEquipments[j].m_ItemObj) && !netRequest && !mEquipments[j].CanTakeOff())
			{
				return false;
			}
		}
		if (!netRequest && addToReceiver && receiver4 != null && !receiver4.CanAddItemList(m_TakeOffEquip))
		{
			if (receiver4 is NpcPackageCmpt)
			{
				PeTipMsg.Register(PELocalization.GetString(82209013), PeTipMsg.EMsgLevel.Warning);
			}
			else
			{
				PeTipMsg.Register(PELocalization.GetString(82209001), PeTipMsg.EMsgLevel.Warning);
			}
			return false;
		}
		for (int num = mItemList.Count - 1; num >= 0; num--)
		{
			if (m_TakeOffEquip.Contains(mItemList[num]))
			{
				mItemList.RemoveAt(num);
			}
		}
		mItemList.Add(itemObj);
		if (addToReceiver)
		{
			receiver4?.AddItemList(m_TakeOffEquip);
		}
		for (int k = 0; k < m_TakeOffEquip.Count; k++)
		{
			ItemObject itemObject2 = m_TakeOffEquip[k];
			RemoveItemEff(itemObject2);
			RemoveModel(itemObject2);
			mEquipType &= ~itemObject2.protoData.equipType;
			EventArg eventArg = new EventArg();
			eventArg.isAdd = false;
			eventArg.itemObj = itemObject2;
			changeEventor.Dispatch(eventArg, this);
		}
		ApplyItemEff(itemObj);
		AddModel(itemObj);
		mEquipType |= itemObj.protoData.equipType;
		EventArg eventArg2 = new EventArg();
		eventArg2.isAdd = true;
		eventArg2.itemObj = itemObj;
		changeEventor.Dispatch(eventArg2, this);
		if ((itemObj.protoData.equipPos & 0x10) != 0)
		{
			mainHandEquipment = itemObj;
		}
		if (this.OnEquipmentChange != null)
		{
			this.OnEquipmentChange();
		}
		if (base.Entity.IsMainPlayer)
		{
			InGameAidData.CheckPutOnEquip(itemObj.protoId);
		}
		m_EquipmentDirty = true;
		return true;
	}

	public bool TryTakeOffEquipment(ItemObject itemObj, bool addToReceiver = true, Receiver receiver = null)
	{
		Receiver receiver3;
		if (receiver == null)
		{
			Receiver receiver2 = mItemReciver;
			receiver3 = receiver2;
		}
		else
		{
			receiver3 = receiver;
		}
		Receiver receiver4 = receiver3;
		if (receiver4 == null)
		{
			return false;
		}
		if (mItemList.Contains(itemObj))
		{
			for (int i = 0; i < mEquipments.Count; i++)
			{
				if (mEquipments[i].m_ItemObj == itemObj && !mEquipments[i].CanTakeOff())
				{
					return false;
				}
			}
			m_TakeOffEquip.Clear();
			m_TakeOffEquip.Add(itemObj);
			if (addToReceiver && !receiver4.CanAddItemList(m_TakeOffEquip))
			{
				return false;
			}
		}
		return true;
	}

	public bool TakeOffEquipment(ItemObject itemObj, bool addToReceiver = true, Receiver receiver = null, bool netRequest = false)
	{
		if (!netRequest && null != mViewCmpt && mViewCmpt.IsRagdoll)
		{
			return false;
		}
		Receiver receiver3;
		if (receiver == null)
		{
			Receiver receiver2 = mItemReciver;
			receiver3 = receiver2;
		}
		else
		{
			receiver3 = receiver;
		}
		Receiver receiver4 = receiver3;
		if (receiver4 == null)
		{
			return false;
		}
		if (mItemList.Contains(itemObj))
		{
			for (int i = 0; i < mEquipments.Count; i++)
			{
				if (mEquipments[i].m_ItemObj == itemObj && !netRequest && !mEquipments[i].CanTakeOff())
				{
					return false;
				}
			}
			m_TakeOffEquip.Clear();
			m_TakeOffEquip.Add(itemObj);
			if (addToReceiver && !receiver4.CanAddItemList(m_TakeOffEquip))
			{
				if (receiver4 is NpcPackageCmpt)
				{
					PeTipMsg.Register(PELocalization.GetString(82209013), PeTipMsg.EMsgLevel.Warning);
				}
				else
				{
					PeTipMsg.Register(PELocalization.GetString(82209001), PeTipMsg.EMsgLevel.Warning);
				}
				return false;
			}
			mItemList.Remove(itemObj);
			if (addToReceiver)
			{
				receiver4.AddItemList(m_TakeOffEquip);
			}
			RemoveItemEff(itemObj);
			RemoveModel(itemObj);
			mEquipType &= ~itemObj.protoData.equipType;
			EventArg eventArg = new EventArg();
			eventArg.isAdd = false;
			eventArg.itemObj = itemObj;
			changeEventor.Dispatch(eventArg, this);
			if ((itemObj.protoData.equipPos & 0x10) != 0)
			{
				mainHandEquipment = null;
			}
			if (this.OnEquipmentChange != null)
			{
				this.OnEquipmentChange();
			}
			m_EquipmentDirty = true;
			return true;
		}
		return false;
	}

	private void DestoryItemObj(int itemId)
	{
		PeSingleton<ItemMgr>.Instance.DestroyItem(itemId);
	}

	public void ModelDestroy()
	{
		for (int i = 0; i < mEquipments.Count; i++)
		{
			if (null != mEquipments[i])
			{
				mEquipments[i].RemoveEquipment();
			}
		}
		mEquipments.Clear();
	}

	public void ApplyEquipment(ItemObject[] itemList)
	{
		int count = mItemList.Count;
		for (int i = 0; i < count; i++)
		{
			TakeOffEquipment(mItemList[0]);
		}
		for (int j = 0; j < itemList.Length; j++)
		{
			PutOnEquipment(itemList[j], addToReceiver: false);
		}
	}

	private void ApplyItemEff(ItemObject itemObj)
	{
		Equip cmpt = itemObj.GetCmpt<Equip>();
		if (cmpt != null)
		{
			if (mSkEntity == null)
			{
				mSkEntity = GetComponent<SkEntity>();
			}
			cmpt.AddBuff(mSkEntity);
		}
	}

	private void RemoveItemEff(ItemObject itemObj)
	{
		itemObj.GetCmpt<Equip>()?.RemoveBuff(mSkEntity);
	}

	private bool ISAvatarModel(ItemObject itemObj)
	{
		return itemObj.protoData.equipReplacePos != 0;
	}

	private void AddModel(ItemObject itemObj)
	{
		if (ISAvatarModel(itemObj))
		{
			AddAvatarModel(itemObj.protoData.equipReplacePos, itemObj.protoData.resourcePath);
		}
		AddEquipment(itemObj);
	}

	private void AddAvatarModel(int partMask, string path)
	{
		mViewCmpt.AddPart(partMask, path);
	}

	private void CreateLogic(ItemObject itemObj)
	{
		if (mLogics.ContainsKey(itemObj.instanceId))
		{
			return;
		}
		Equip cmpt = itemObj.GetCmpt<Equip>();
		if (cmpt == null)
		{
			return;
		}
		GameObject gameObject = cmpt.CreateLogicObj();
		if (!(null == gameObject))
		{
			PEEquipmentLogic component = gameObject.GetComponent<PEEquipmentLogic>();
			if (null == component)
			{
				Debug.LogError("Equip can't find:" + itemObj.nameText);
				UnityEngine.Object.Destroy(gameObject);
			}
			else
			{
				gameObject.transform.parent = m_EquipmentLogicParent;
				component.InitEquipment(base.Entity, itemObj);
				mLogics[itemObj.instanceId] = component;
			}
		}
	}

	private void CreateModel(ItemObject itemObj)
	{
		if (mViewCmpt == null)
		{
			mViewCmpt = base.Entity.biologyViewCmpt;
		}
		if (null == mViewCmpt.modelTrans)
		{
			return;
		}
		Equip cmpt = itemObj.GetCmpt<Equip>();
		if (cmpt == null)
		{
			return;
		}
		GameObject gameObject = cmpt.CreateGameObj();
		if (!(null == gameObject))
		{
			PEEquipment component = gameObject.GetComponent<PEEquipment>();
			if (null == component)
			{
				Debug.LogError("Equip can't find:" + itemObj.nameText);
				UnityEngine.Object.Destroy(gameObject);
				return;
			}
			component.InitEquipment(base.Entity, itemObj);
			mEquipments.Add(component);
			HideEquipmentByFirstPerson(component, m_HideEquipmentByFirstPerson);
			HideEquipmentByVehicle(component, m_HideEquipmentByVehicle);
			HidEquipmentByRagdoll(component, m_HideEquipmentByRagdoll);
			mMotionEquip.SetEquipment(component, isPutOn: true);
			PreLoadEquipmentEffect(component);
		}
	}

	private void PreLoadEquipmentEffect(PEEquipment equipment)
	{
		if (!(equipment is PEGun))
		{
			return;
		}
		PEGun pEGun = equipment as PEGun;
		SkData value = null;
		SkData.s_SkillTbl.TryGetValue(pEGun.m_ShootSoundID, out value);
		if (value != null && value._effMainOneTime != null && value._effMainOneTime._seId > 0)
		{
			int seId = value._effMainOneTime._seId;
			SESoundBuff sESoundData = SESoundBuff.GetSESoundData(seId);
			if (sESoundData != null && AudioManager.instance != null)
			{
				AudioManager.instance.GetAudioClip(sESoundData.mName);
			}
		}
		value = null;
		SkData.s_SkillTbl.TryGetValue(pEGun.GetSkillID(), out value);
		if (value == null || value._effMainOneTime == null || value._effMainOneTime._effId == null || value._effMainOneTime._effId.Length <= 0)
		{
			return;
		}
		int[] effId = value._effMainOneTime._effId;
		foreach (int num in effId)
		{
			if (num > 0)
			{
				EffectData effCastData = EffectData.GetEffCastData(num);
				if (effCastData != null && !string.IsNullOrEmpty(effCastData.m_path) && Singleton<EffectBuilder>.Instance != null)
				{
					Singleton<EffectBuilder>.Instance.GetEffect(effCastData.m_path);
				}
			}
		}
	}

	private void AddEquipment(ItemObject itemObj)
	{
		if (itemObj != null)
		{
			CreateLogic(itemObj);
			CreateModel(itemObj);
		}
	}

	private void RemoveModel(ItemObject itemObj)
	{
		if (itemObj.protoData.equipReplacePos != 0)
		{
			RemoveAvatarModel(itemObj.protoData.equipReplacePos);
		}
		RemoveEquipModel(itemObj);
	}

	private void RemoveAvatarModel(int partMask)
	{
		mViewCmpt.RemovePart(partMask);
	}

	private void RemoveEquipModel(ItemObject itemObj)
	{
		if (itemObj == null)
		{
			return;
		}
		if (mLogics.ContainsKey(itemObj.instanceId))
		{
			if (null != mLogics[itemObj.instanceId])
			{
				mLogics[itemObj.instanceId].RemoveEquipment();
			}
			mLogics.Remove(itemObj.instanceId);
		}
		for (int num = mEquipments.Count - 1; num >= 0; num--)
		{
			PEEquipment pEEquipment = mEquipments[num];
			if (null == pEEquipment)
			{
				mEquipments.RemoveAt(num);
			}
			else if (pEEquipment.m_ItemObj == itemObj)
			{
				mMotionEquip.SetEquipment(pEEquipment, isPutOn: false);
				mEquipments.Remove(pEEquipment);
				pEEquipment.RemoveEquipment();
				break;
			}
		}
	}

	private void HideEquipmentByFirstPerson(PEEquipment equ, bool hide)
	{
		if (null != equ)
		{
			equ.HideEquipmentByFirstPerson(hide);
		}
	}

	private void HideEquipmentByFirstPerson(bool hide)
	{
		m_HideEquipmentByFirstPerson = hide;
		for (int i = 0; i < mEquipments.Count; i++)
		{
			HideEquipmentByFirstPerson(mEquipments[i], m_HideEquipmentByFirstPerson);
		}
	}

	private void HideEquipmentByVehicle(PEEquipment equ, bool hide)
	{
		if (null != equ)
		{
			equ.HideEquipmentByVehicle(hide);
		}
	}

	public void HideEquipmentByVehicle(bool hide)
	{
		m_HideEquipmentByVehicle = hide;
		for (int i = 0; i < mEquipments.Count; i++)
		{
			HideEquipmentByVehicle(mEquipments[i], hide);
		}
	}

	private void HidEquipmentByRagdoll(PEEquipment equ, bool hide)
	{
		if (null != equ)
		{
			equ.HideEquipmentByVehicle(hide);
		}
	}

	public void HidEquipmentByRagdoll(bool hide)
	{
		m_HideEquipmentByVehicle = hide;
		for (int i = 0; i < mEquipments.Count; i++)
		{
			HideEquipmentByVehicle(mEquipments[i], hide);
		}
	}

	public override void Serialize(BinaryWriter _out)
	{
		_out.Write(1);
		_out.Write(mItemList.Count);
		foreach (ItemObject mItem in mItemList)
		{
			if (mItem != null)
			{
				_out.Write(mItem.instanceId);
			}
		}
	}

	public override void Deserialize(BinaryReader _in)
	{
		_in.ReadInt32();
		int num = _in.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int id = _in.ReadInt32();
			ItemObject item = PeSingleton<ItemMgr>.Instance.Get(id);
			m_InitItems.Add(item);
		}
	}

	public ItemSample GetDroppableItemAt(int idx)
	{
		return mItemList[idx];
	}

	public void AddDroppableItem(ItemSample item)
	{
		if (item is ItemObject item2)
		{
			mItemList.Add(item2);
		}
	}

	public void RemoveDroppableItem(ItemSample item)
	{
		if (item is ItemObject itemObj)
		{
			TakeOffEquipment(itemObj, addToReceiver: false);
		}
	}

	public void RemoveDroppableItemAll()
	{
		int num = 0;
		while (num < mItemList.Count)
		{
			if (!TakeOffEquipment(mItemList[num], addToReceiver: false))
			{
				num++;
			}
		}
	}

	public void OnMsg(EMsg msg, params object[] args)
	{
		switch (msg)
		{
		case EMsg.View_Prefab_Build:
			mShowModel = true;
			ResetModels();
			break;
		case EMsg.View_Prefab_Destroy:
			mShowModel = false;
			ModelDestroy();
			break;
		case EMsg.Battle_EquipAttack:
			if ((!(base.Entity != PeSingleton<PeCreature>.Instance.mainPlayer) || !(null != mNPC) || mNPC.HasConsume) && (base.Entity.proto != EEntityProto.Monster || base.Entity.Race != ERace.Mankind) && !PeGameMgr.IsBuild)
			{
				ReduceWeaponDurability((ItemObject)args[0]);
			}
			break;
		case EMsg.Battle_BeAttacked:
			if ((!(base.Entity != PeSingleton<PeCreature>.Instance.mainPlayer) || !(null != mNPC) || mNPC.HasConsume) && (base.Entity.proto != EEntityProto.Monster || base.Entity.Race != ERace.Mankind) && !PeGameMgr.IsBuild)
			{
				ReduceArmorDurability((float)args[0], (SkEntity)args[1]);
			}
			break;
		case EMsg.View_FirstPerson:
			HideEquipmentByFirstPerson((bool)args[0]);
			break;
		}
	}

	public void DestroyAllEquipment()
	{
		for (int i = 0; i < mItemList.Count; i++)
		{
			RemoveItemEff(mItemList[i]);
			RemoveModel(mItemList[i]);
			mEquipType &= ~mItemList[i].protoData.equipType;
			PeSingleton<ItemMgr>.Instance.DestroyItem(mItemList[i].instanceId);
		}
		mItemList.Clear();
	}

	public void ResetModels()
	{
		ModelDestroy();
		foreach (ItemObject mItem in mItemList)
		{
			if (!ISAvatarModel(mItem))
			{
				CreateModel(mItem);
			}
		}
		foreach (PEEquipmentLogic value in mLogics.Values)
		{
			if (null != value)
			{
				value.OnModelRebuild();
			}
		}
	}

	public void CheckEXBuffs()
	{
		if (!m_EquipmentDirty)
		{
			return;
		}
		m_EquipmentDirty = false;
		if (null == mSkEntity)
		{
			return;
		}
		if (m_EXBuffs != null)
		{
			for (int i = 0; i < m_EXBuffs.Count; i++)
			{
				mSkEntity.CancelBuffById(m_EXBuffs[i]);
			}
		}
		m_EXBuffs.Clear();
		m_SuitSetMatchDatas.Clear();
		EquipSetData.GetSuitSetEffect(_ItemList, ref m_EXBuffs);
		SuitSetData.GetSuitSetEffect(_ItemList, ref m_EXBuffs, ref m_SuitSetMatchDatas);
		for (int j = 0; j < m_EXBuffs.Count; j++)
		{
			SkEntity.MountBuff(mSkEntity, m_EXBuffs[j], null, null);
		}
		if (onSuitSetChange != null)
		{
			onSuitSetChange(m_SuitSetMatchDatas);
		}
	}

	private IEnumerator PreLoad()
	{
		if (mItemList == null)
		{
			yield break;
		}
		for (int i = 0; i < mItemList.Count; i++)
		{
			ItemObject itemObj = mItemList[i];
			if (!ISAvatarModel(itemObj))
			{
				Equip equip = itemObj.GetCmpt<Equip>();
				if (equip != null)
				{
					AssetsLoader.Instance.AddReq(new AssetReq(equip.protoData.resourcePath));
					yield return new WaitForSeconds(0.2f);
				}
			}
		}
	}
}
