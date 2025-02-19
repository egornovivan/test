using System.Collections;
using ItemAsset;
using Pathea;
using PeMap;
using SkillSystem;
using UnityEngine;

public class ItemScript_Tower : ItemScript
{
	private TowerCmpt mTower;

	private SkAliveEntity mAlive;

	private Tower mItemTower;

	private Energy mItemEnergy;

	private LifeLimit mLifeLimit;

	private Durability mDurability;

	private int lastReqeustTime;

	private float totalCost;

	public TowerCmpt tower
	{
		get
		{
			if (mTower == null)
			{
				mTower = GetComponent<TowerCmpt>();
				if (null != mTower)
				{
					mTower.onConsumeChange += OnAttrChanged;
				}
			}
			return mTower;
		}
	}

	public Energy energy
	{
		get
		{
			if (mItemEnergy == null)
			{
				mItemEnergy = mItemObj.GetCmpt<Energy>();
			}
			return mItemEnergy;
		}
	}

	public SkAliveEntity alive
	{
		get
		{
			if (mAlive == null)
			{
				mAlive = GetComponent<SkAliveEntity>();
			}
			return mAlive;
		}
	}

	public int ammoItemProtoId => tower.ItemID;

	public int ammoCount
	{
		get
		{
			if (tower.CostType == ECostType.Item)
			{
				return tower.ItemCount;
			}
			if (tower.CostType == ECostType.Energy)
			{
				return tower.EnergyCount;
			}
			return int.MaxValue;
		}
	}

	public int ammoMaxCount
	{
		get
		{
			if (tower.CostType == ECostType.Item)
			{
				return tower.ItemCountMax;
			}
			if (tower.CostType == ECostType.Energy)
			{
				return tower.EnergyCountMax;
			}
			return int.MaxValue;
		}
	}

	public float CurDurabilityValue => (mDurability != null) ? mDurability.floatValue.current : ((mLifeLimit != null) ? mLifeLimit.floatValue.current : 0f);

	public float MaxDurabilityValue => (mDurability != null) ? mDurability.valueMax : ((mLifeLimit != null) ? mLifeLimit.valueMax : 0f);

	public override void SetItemObject(ItemObject itemObj)
	{
		base.SetItemObject(itemObj);
		mItemTower = mItemObj.GetCmpt<Tower>();
		mItemEnergy = mItemObj.GetCmpt<Energy>();
		mLifeLimit = mItemObj.GetCmpt<LifeLimit>();
		mDurability = mItemObj.GetCmpt<Durability>();
		if (!PeGameMgr.IsMulti && null != tower && mLifeLimit != null)
		{
			tower.Entity.SetAttribute(AttribType.Hp, mLifeLimit.floatValue.current);
		}
	}

	private void Start()
	{
		RegisterDeadEvent();
		LoadFromItem();
	}

	private void OnDestroy()
	{
		OnAttrChanged(0f);
		if (null != mTower)
		{
			mTower.onConsumeChange -= OnAttrChanged;
		}
	}

	private void RegisterDeadEvent()
	{
		if (!(alive == null))
		{
			alive.onHpChange += OnHpChange;
			alive.deathEvent += OnDeath;
		}
	}

	private void OnHpChange(SkEntity caster, float hpChange)
	{
		float attribute = alive.GetAttribute(AttribType.Hp);
		if (mItemObj != null)
		{
			if (mLifeLimit != null)
			{
				mLifeLimit.floatValue.current = attribute;
			}
			if (mDurability != null)
			{
				mDurability.floatValue.current = attribute;
			}
		}
	}

	private void OnDeath(SkEntity caster, SkEntity target)
	{
		TowerMark towerMark = PeSingleton<TowerMark.Mgr>.Instance.Find((TowerMark tower) => base.itemObjectId == tower.ID);
		if (towerMark != null)
		{
			PeSingleton<LabelMgr>.Instance.Remove(towerMark);
			PeSingleton<TowerMark.Mgr>.Instance.Remove(towerMark);
		}
		Invoke("TowerDestroy", 10f);
	}

	private void TowerDestroy()
	{
		PeSingleton<ItemMgr>.Instance.DestroyItem(base.itemObjectId);
		DragArticleAgent.Destory(id);
	}

	private void OnAttrChanged(float cost = 0f)
	{
		if (mItemObj != null && mItemTower != null)
		{
			if (tower.ConsumeType == ECostType.Item)
			{
				mItemTower.curCostValue = tower.ItemCount;
			}
			else if (tower.ConsumeType == ECostType.Energy && mItemEnergy != null)
			{
				mItemEnergy.energy.current = tower.EnergyCount;
			}
			if (GameConfig.IsMultiMode && cost != 0f)
			{
				StartCoroutine(DelayRequestAttrChanged(cost));
			}
		}
	}

	private IEnumerator DelayRequestAttrChanged(float cost)
	{
		totalCost += cost;
		if (lastReqeustTime == 0)
		{
			for (lastReqeustTime = 3; lastReqeustTime != 0; lastReqeustTime--)
			{
				yield return new WaitForSeconds(1f);
			}
			if (null != PlayerNetwork.mainPlayer)
			{
				PlayerNetwork.mainPlayer.RequestAttrChanged(tower.Entity.Id, base.itemObjectId, totalCost, ammoItemProtoId);
			}
			totalCost = 0f;
		}
	}

	private void LoadFromItem()
	{
		if (mItemObj == null || mItemTower == null)
		{
			return;
		}
		if (mItemTower.costType == ECostType.Item)
		{
			tower.ItemCount = mItemTower.curCostValue;
		}
		else if (mItemTower.costType == ECostType.Energy && mItemEnergy != null)
		{
			if (mItemEnergy.energy.current == -1f)
			{
				mItemEnergy.energy.SetToMax();
			}
			tower.EnergyCount = (int)mItemEnergy.energy.current;
		}
	}

	public bool CostLimited()
	{
		if (tower.ConsumeType == ECostType.Item || tower.ConsumeType == ECostType.Energy)
		{
			return true;
		}
		return false;
	}

	public bool CanRefill()
	{
		if (tower.ConsumeType != ECostType.Item)
		{
			return false;
		}
		return true;
	}

	public void Refill(int number)
	{
		if (CanRefill())
		{
			tower.ItemCount += number;
			OnAttrChanged(0f);
		}
	}
}
