using System.Collections.Generic;
using ItemAsset;
using SkillAsset;
using UnityEngine;
using WhiteCat;

public class Gun : ShootEquipment
{
	public VCPGunHandle mGunHandle;

	public List<VCPGunMuzzle> mGunMuzzle;

	private List<float> mCoolDownTime;

	protected int mCurrentIndex;

	private int mLastIndex;

	public float mRuntimeAccuracyScale = 1f;

	public override void InitEquipment(SkillRunner runner, ItemObject item)
	{
		mCurrentIndex = 0;
		mLastIndex = 0;
		mCoolDownTime = new List<float>();
		foreach (VCPGunMuzzle item2 in mGunMuzzle)
		{
			mCoolDownTime.Add(Time.time);
		}
		base.InitEquipment(runner, item);
	}

	public virtual EArmType GetArmType()
	{
		return mGunMuzzle[0].ArmType;
	}

	public override bool CostSkill(ISkillTarget target, int sex = 2, bool buttonDown = false, bool buttonPressed = false)
	{
		if (!base.CostSkill(target, sex, buttonDown, buttonPressed))
		{
			return false;
		}
		if (mShootState == ShootState.Aim || mShootState == ShootState.Fire)
		{
			for (int i = 0; i < mGunMuzzle.Count; i++)
			{
				int index = (i + mCurrentIndex + 1) % mGunMuzzle.Count;
				if (((mGunMuzzle[index].Multishot && Time.time - mCoolDownTime[index] > mGunMuzzle[index].FireInterval && (buttonDown || buttonPressed)) || (!mGunMuzzle[index].Multishot && buttonDown)) && mHuman.CheckAmmoCost(mGunMuzzle[index].ArmType, mGunMuzzle[index].CostItemId))
				{
					mCurrentIndex = index;
					Transform end = mGunMuzzle[mCurrentIndex].End;
					float num = 0.5f;
					if (mGunMuzzle[mCurrentIndex] != null)
					{
						num = mGunMuzzle[mCurrentIndex].Accuracy;
					}
					float num2 = mRuntimeAccuracyScale * num;
					float x = (Random.value - 0.5f) * 1.8f * num2;
					float y = (Random.value - 0.5f) * 1.8f * num2;
					end.localEulerAngles = new Vector3(x, y, 0f);
					EffSkillInstance effSkillInstance = CostSkill(mSkillRunner, mGunMuzzle[index].SkillId, target);
					if (effSkillInstance != null)
					{
						mHuman.ApplyDurabilityReduce(0);
						mCoolDownTime[index] = Time.time;
						mGunMuzzle[index].PlayMuzzleEffect();
						mHuman.ApplyAmmoCost(mGunMuzzle[index].ArmType, mGunMuzzle[index].CostItemId);
						return true;
					}
				}
			}
		}
		return false;
	}
}
