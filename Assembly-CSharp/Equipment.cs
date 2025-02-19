using System.Collections.Generic;
using ItemAsset;
using SkillAsset;
using UnityEngine;

public class Equipment : MonoBehaviour
{
	[HideInInspector]
	public ItemObject mItemObj;

	[HideInInspector]
	public SkillRunner mSkillRunner;

	protected IHuman mHuman;

	protected bool mMainPlayerEquipment;

	protected int mSkillIndex;

	protected int mCastSkillId;

	public List<int> mSkillMaleId;

	public List<int> mSkillFemaleId;

	public EquipType mEquipType => mItemObj.protoData.equipType;

	public bool OpEnable => false;

	public virtual void InitEquipment(SkillRunner runner, ItemObject item)
	{
		mSkillRunner = runner;
		mHuman = mSkillRunner as IHuman;
		mItemObj = item;
		mSkillIndex = 0;
	}

	public virtual void InitEquipment(ItemObject item)
	{
		mItemObj = item;
	}

	public virtual void RemoveEquipment()
	{
	}

	public virtual bool CostSkill(ISkillTarget target = null, int sex = 2, bool buttonDown = false, bool buttonPressed = false)
	{
		LifeLimit cmpt = mItemObj.GetCmpt<LifeLimit>();
		if (cmpt == null)
		{
			return false;
		}
		if (cmpt.lifePoint.current < float.Epsilon)
		{
			return false;
		}
		if (mSkillMaleId.Count == 0 || mSkillFemaleId.Count == 0)
		{
			return false;
		}
		mCastSkillId = 0;
		switch (sex)
		{
		case 1:
		{
			mCastSkillId = mSkillFemaleId[0];
			mSkillIndex = 0;
			for (int j = 0; j < mSkillFemaleId.Count - 1; j++)
			{
				if (mSkillRunner.IsEffRunning(mSkillFemaleId[j]))
				{
					mCastSkillId = mSkillFemaleId[j + 1];
					mSkillIndex = j + 1;
				}
			}
			break;
		}
		case 2:
		{
			mCastSkillId = mSkillMaleId[0];
			mSkillIndex = 0;
			for (int i = 0; i < mSkillMaleId.Count - 1; i++)
			{
				if (mSkillRunner.IsEffRunning(mSkillMaleId[i]))
				{
					mCastSkillId = mSkillMaleId[i + 1];
					mSkillIndex = i + 1;
				}
			}
			break;
		}
		}
		EffSkillInstance effSkillInstance = CostSkill(mSkillRunner, mCastSkillId, target);
		if (null != mSkillRunner && target != null && effSkillInstance != null && mSkillRunner != target)
		{
			Vector3 vector = target.GetPosition() - mSkillRunner.transform.position;
			mSkillRunner.transform.LookAt(mSkillRunner.transform.position + vector, Vector3.up);
		}
		return null != effSkillInstance;
	}

	protected virtual EffSkillInstance CostSkill(SkillRunner coster, int id, ISkillTarget target)
	{
		return coster.RunEff(id, target);
	}

	protected virtual void CheckMainPlayerCtrl()
	{
		if (PeInput.Get(PeInput.LogicFunction.Item_Drag))
		{
			CostSkill();
		}
	}

	protected virtual void Update()
	{
		if (mMainPlayerEquipment)
		{
			CheckMainPlayerCtrl();
		}
	}

	public void TakeOffEquip()
	{
	}

	public bool CanTakeOff()
	{
		return true;
	}
}
