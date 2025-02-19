using System.Collections.Generic;
using ItemAsset;
using Pathea.Effect;
using PeEvent;
using SkillSystem;
using WhiteCat;

namespace Pathea;

public class UseItemCmpt : PeCmpt
{
	public class EventArg : PeEvent.EventArg
	{
		public ItemObject itemObj;
	}

	private const int ReviveItemProtoId = 937;

	private const int LearnEffectId = 88;

	private const int LearnSoundId = 19;

	private Event<EventArg> mEventor = new Event<EventArg>();

	private NetCmpt mNet;

	private PackageCmpt mPkg;

	private SkEntity mSkEntity;

	public Event<EventArg> eventor => mEventor;

	private SkEntity skEntity
	{
		get
		{
			if (mSkEntity == null)
			{
				mSkEntity = base.Entity.GetGameObject().GetComponent<SkEntity>();
			}
			return mSkEntity;
		}
	}

	public override void Start()
	{
		base.Start();
		mPkg = base.Entity.GetCmpt<PackageCmpt>();
		if (NetworkInterface.IsClient)
		{
			mNet = base.Entity.GetCmpt<NetCmpt>();
		}
	}

	private bool ExtractBundle(Bundle bundle)
	{
		if (bundle == null)
		{
			return false;
		}
		PackageCmpt cmpt = base.Entity.GetCmpt<PackageCmpt>();
		if (null == cmpt)
		{
			return false;
		}
		IEnumerable<ItemObject> enumerable = bundle.Extract();
		if (enumerable == null)
		{
			return false;
		}
		List<ItemObject> items = new List<ItemObject>(enumerable);
		if (!cmpt.CanAddItemList(items))
		{
			return false;
		}
		cmpt.AddItemList(items);
		return true;
	}

	private bool ConsumeItem(Consume consume)
	{
		if (consume == null)
		{
			return false;
		}
		if (null == skEntity)
		{
			return false;
		}
		return null != consume.StartSkSkill(skEntity);
	}

	private bool LearnReplicatorFormula(ReplicatorFormula formula, bool bLearn = true)
	{
		if (formula == null || formula.formulaId == null || formula.formulaId.Length <= 0)
		{
			return false;
		}
		ReplicatorCmpt cmpt = base.Entity.GetCmpt<ReplicatorCmpt>();
		if (null == cmpt)
		{
			return false;
		}
		Replicator replicator = cmpt.replicator;
		if (replicator == null)
		{
			return false;
		}
		if (bLearn)
		{
			bool flag = false;
			for (int i = 0; i < formula.formulaId.Length; i++)
			{
				if (replicator.AddFormula(formula.formulaId[i]))
				{
					flag = true;
				}
			}
			if (!flag)
			{
				new PeTipMsg(PELocalization.GetString(4000001), PeTipMsg.EMsgLevel.Warning);
				return flag;
			}
		}
		LearnEffectAndSound();
		return true;
	}

	public void LearnEffectAndSound()
	{
		PeTrans peTrans = base.Entity.peTrans;
		if (!(peTrans == null))
		{
			Singleton<EffectBuilder>.Instance.Register(88, null, peTrans.position, peTrans.rotation);
			AudioManager.instance.Create(peTrans.position, 19);
		}
	}

	private bool LearnMetalScan(MetalScan metalScan, bool bLearn = true)
	{
		if (metalScan == null)
		{
			return false;
		}
		if (base.Entity.Id != PeSingleton<PeCreature>.Instance.mainPlayerId)
		{
			return false;
		}
		int[] metalIds = metalScan.metalIds;
		foreach (int metalId in metalIds)
		{
			if (!MetalScanData.HasMetal(metalId))
			{
				if (bLearn)
				{
					MetalScanData.AddMetalScan(metalScan.metalIds);
				}
				LearnEffectAndSound();
				return true;
			}
		}
		return false;
	}

	private bool TakeOnEquipment(Equip equip)
	{
		if (equip == null)
		{
			return false;
		}
		EquipmentCmpt cmpt = base.Entity.GetCmpt<EquipmentCmpt>();
		if (null == cmpt)
		{
			return false;
		}
		if (cmpt.PutOnEquipment(equip.itemObj))
		{
			GameUI.Instance.PlayPutOnEquipAudio();
			return true;
		}
		return false;
	}

	public bool RequestRevive()
	{
		if (!NetworkInterface.IsClient)
		{
			Revive();
		}
		else if (null != PlayerNetwork.mainPlayer)
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_PlayerRevive, base.Entity.position);
			return true;
		}
		return false;
	}

	public bool ReviveServent(bool usePlayer = true)
	{
		NpcPackageCmpt component = GetComponent<NpcPackageCmpt>();
		if (component == null)
		{
			return false;
		}
		SlotList slotList = component.GetSlotList();
		ItemObject itemObject = slotList.FindItemByProtoId(937);
		if (itemObject == null)
		{
			slotList = component.GetHandinList();
			itemObject = slotList.FindItemByProtoId(937);
		}
		if (itemObject == null && !usePlayer)
		{
			return false;
		}
		if (itemObject == null)
		{
			if (GameUI.Instance.mMainPlayer == null)
			{
				return false;
			}
			PlayerPackageCmpt cmpt = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();
			if (cmpt == null)
			{
				return false;
			}
			ItemObject itemObject2 = cmpt.package.FindItemByProtoId(937);
			if (itemObject2 == null)
			{
				return false;
			}
			return Use(itemObject2, cmpt);
		}
		return Use(itemObject);
	}

	public bool Revive()
	{
		PlayerPackageCmpt playerPackageCmpt = mPkg as PlayerPackageCmpt;
		if (playerPackageCmpt == null)
		{
			return false;
		}
		ItemObject itemObject = playerPackageCmpt.package.FindItemByProtoId(937);
		if (itemObject == null)
		{
			return false;
		}
		return Use(itemObject);
	}

	public bool Request(ItemObject item)
	{
		if (item == null)
		{
			return false;
		}
		if (item.protoId == 937)
		{
			return false;
		}
		SkAliveEntity cmpt = base.Entity.GetCmpt<SkAliveEntity>();
		if (cmpt != null && cmpt.isDead)
		{
			return false;
		}
		if (NetworkInterface.IsClient && null != mNet)
		{
			float cdByItemProtoId = GetCdByItemProtoId(item.protoId);
			if (cdByItemProtoId > 0f)
			{
				return false;
			}
			Equip cmpt2 = item.GetCmpt<Equip>();
			if (cmpt2 != null && mNet.network is PlayerNetwork)
			{
				EquipmentCmpt cmpt3 = (mNet.network as PlayerNetwork).PlayerEntity.GetCmpt<EquipmentCmpt>();
				if (null != cmpt3 && !cmpt3.NetTryPutOnEquipment(item))
				{
					return false;
				}
			}
			LearnReplicatorFormula(item.GetCmpt<ReplicatorFormula>(), bLearn: false);
			LearnMetalScan(item.GetCmpt<MetalScan>(), bLearn: false);
			mNet.RequestUseItem(item.instanceId);
			return true;
		}
		return Use(item);
	}

	public bool Use(ItemObject item, PlayerPackageCmpt UsePkg)
	{
		CheckMainPlayerUseItem(item.protoId);
		bool flag = false;
		flag = ExtractBundle(item.GetCmpt<Bundle>()) || flag;
		flag = ConsumeItem(item.GetCmpt<Consume>()) || flag;
		flag = LearnReplicatorFormula(item.GetCmpt<ReplicatorFormula>()) || flag;
		flag = LearnMetalScan(item.GetCmpt<MetalScan>()) || flag;
		bool flag2 = TakeOnEquipment(item.GetCmpt<Equip>());
		if (UsePkg != null)
		{
			if (flag)
			{
				UsePkg.DestroyItem(item, 1);
			}
			else if (flag2)
			{
				UsePkg.Remove(item);
			}
		}
		bool flag3 = flag || flag2;
		if (flag3)
		{
			eventor.Dispatch(new EventArg
			{
				itemObj = item
			}, this);
		}
		return flag3;
	}

	public bool Use(ItemObject item)
	{
		CheckMainPlayerUseItem(item.protoId);
		bool flag = false;
		flag = ExtractBundle(item.GetCmpt<Bundle>()) || flag;
		flag = ConsumeItem(item.GetCmpt<Consume>()) || flag;
		flag = LearnReplicatorFormula(item.GetCmpt<ReplicatorFormula>()) || flag;
		flag = LearnMetalScan(item.GetCmpt<MetalScan>()) || flag;
		bool flag2 = TakeOnEquipment(item.GetCmpt<Equip>());
		if (mPkg != null)
		{
			if (flag)
			{
				mPkg.DestroyItem(item, 1);
			}
			else if (flag2)
			{
				mPkg.Remove(item);
			}
		}
		bool flag3 = flag || flag2;
		if (flag3)
		{
			eventor.Dispatch(new EventArg
			{
				itemObj = item
			}, this);
		}
		return flag3;
	}

	public void UseFromNet(ItemObject item)
	{
		ConsumeItem(item.GetCmpt<Consume>());
	}

	private void CheckMainPlayerUseItem(int itemID)
	{
		if (base.Entity.IsMainPlayer)
		{
			InGameAidData.CheckUseItem(itemID);
		}
	}

	public float GetCd(int skillId)
	{
		return SkInst.GetSkillCoolingPercent(skEntity, skillId);
	}

	public float GetNpcSkillCd(SkEntity npcSkentiy, int SkillId)
	{
		return SkInst.GetSkillCoolingPercent(npcSkentiy, SkillId);
	}

	public float GetCdByItemProtoId(int itemProtoId)
	{
		ItemProto proto = PeSingleton<ItemProto.Mgr>.Instance.Get(itemProtoId);
		return GetCdByItemProto(proto);
	}

	public float GetCdByItemProto(ItemProto proto)
	{
		if (proto == null)
		{
			return 0f;
		}
		return GetCd(proto.skillId);
	}

	public float GetCdByItemInstanceId(int itemInstanceId)
	{
		ItemObject itemObj = PeSingleton<ItemMgr>.Instance.Get(itemInstanceId);
		return GetCdByItemInstance(itemObj);
	}

	public float GetCdByItemInstance(ItemObject itemObj)
	{
		if (itemObj == null)
		{
			return 0f;
		}
		return GetCd(itemObj.protoData.skillId);
	}

	public void RightMouseClickArmorItem(ItemObject item)
	{
		GetComponent<PlayerArmorCmpt>().QuickEquipArmorPartFromPackage(item);
	}
}
