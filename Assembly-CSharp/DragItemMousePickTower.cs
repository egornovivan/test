using Pathea;
using PeMap;
using UnityEngine;

public class DragItemMousePickTower : DragItemMousePick
{
	private ItemScript_Tower mTower;

	private static DragItemMousePickTower gOperationTower;

	private ItemScript_Tower ammoTower
	{
		get
		{
			if (null == mTower)
			{
				mTower = GetComponent<ItemScript_Tower>();
				if (null != mTower)
				{
					mTower.tower.onConsumeChange += UpdateAmmo;
				}
			}
			return mTower;
		}
	}

	protected override string tipsText
	{
		get
		{
			string text = base.tipsText;
			if (ammoTower != null)
			{
				if (ammoTower.CostLimited())
				{
					string text2 = text;
					text = text2 + "\n" + ammoTower.ammoCount + "/" + ammoTower.ammoMaxCount;
				}
				text += $"\n{PELocalization.GetString(82220001)} {Mathf.FloorToInt(ammoTower.CurDurabilityValue)}/{Mathf.FloorToInt(ammoTower.MaxDurabilityValue)}";
			}
			return text;
		}
	}

	protected override void InitCmd(CmdList cmdList)
	{
		base.InitCmd(cmdList);
		if (CanRefill())
		{
			cmdList.Add("Refill", RefillByUI);
		}
		gOperationTower = this;
	}

	public bool CanRefill()
	{
		if (ammoTower == null)
		{
			return false;
		}
		return ammoTower.CanRefill();
	}

	public int GetMaxRefillNum()
	{
		if (ammoTower == null)
		{
			return 0;
		}
		if (base.pkg == null)
		{
			return 0;
		}
		int count = base.pkg.GetCount(ammoTower.ammoItemProtoId);
		int num = ammoTower.ammoMaxCount - ammoTower.ammoCount;
		return (num <= count) ? num : count;
	}

	private void RefillByUI()
	{
		OnRefill(GameUI.Instance.mItemOp.AmmoNum);
	}

	private void OnRefill(int addAmmoNum)
	{
		if (addAmmoNum <= 0 || ammoTower == null || base.pkg == null)
		{
			return;
		}
		int maxRefillNum = GetMaxRefillNum();
		if (maxRefillNum < addAmmoNum)
		{
			GameUI.Instance.mItemOp.SetRefill(ammoTower.ammoCount, ammoTower.ammoMaxCount, GetMaxRefillNum());
		}
		else if (base.pkg.Destroy(ammoTower.ammoItemProtoId, addAmmoNum))
		{
			if (GameConfig.IsMultiMode)
			{
				PlayerNetwork.mainPlayer.RequestReload(mTower.tower.Entity.Id, mTower.itemObjectId, ammoTower.ammoItemProtoId, ammoTower.ammoItemProtoId, mTower.tower.ItemCountMax);
			}
			ammoTower.Refill(addAmmoNum);
			GameUI.Instance.mItemOp.SetRefill(ammoTower.ammoCount, ammoTower.ammoMaxCount, GetMaxRefillNum());
			PeSingleton<MousePicker>.Instance.UpdateTis();
		}
	}

	protected override void CheckOperate()
	{
		base.CheckOperate();
		if (PeInput.Get(PeInput.LogicFunction.OpenItemMenu) && CanCmd() && CanRefill())
		{
			if (ammoTower == null)
			{
				return;
			}
			GameUI.Instance.mItemOp.SetRefill(ammoTower.ammoCount, ammoTower.ammoMaxCount, GetMaxRefillNum());
		}
		if (PeInput.Get(PeInput.LogicFunction.InteractWithItem) && CanCmd() && CanRefill())
		{
			OnRefill(GetMaxRefillNum());
		}
	}

	public override void DoGetItem()
	{
		if (base.pkg == null || base.itemObj == null || !base.pkg.CanAdd(base.itemObj))
		{
			PeTipMsg.Register(PELocalization.GetString(9500312), PeTipMsg.EMsgLevel.Warning);
			return;
		}
		TowerMark towerMark = PeSingleton<TowerMark.Mgr>.Instance.Find((TowerMark tower) => base.itemObjectId == tower.ID);
		if (towerMark != null)
		{
			PeSingleton<LabelMgr>.Instance.Remove(towerMark);
			PeSingleton<TowerMark.Mgr>.Instance.Remove(towerMark);
		}
		base.DoGetItem();
	}

	private void UpdateAmmo(float cost)
	{
		if (!(ammoTower == null) && GameUI.Instance.mItemOp.Active && gOperationTower == this)
		{
			GameUI.Instance.mItemOp.UpdateAmmoCount(ammoTower.ammoCount, ammoTower.ammoMaxCount, GetMaxRefillNum());
		}
	}
}
