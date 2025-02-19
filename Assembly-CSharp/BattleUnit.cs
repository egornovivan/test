using UnityEngine;

public class BattleUnit : MonoBehaviour
{
	public int mID;

	public int mType;

	public float mMaxHp;

	public float mHp;

	public float mMaxEn;

	public float mEn;

	public float mMaxAmmo;

	public float mAmmo;

	public float mAtk;

	public int mAtkType;

	public float mAtkInterval;

	public EffectRange mAtkRange;

	public float mDef;

	public int mDefType;

	public float mRps;

	public float mHealPs;

	public int mHealType;

	public EffectRange mHealRange;

	public float mEnCostPs;

	public float mAmmoCostPs;

	public bool mPlayerForce;

	public float mMoveInterval = 5f;

	private float mMoveCooldownTime;

	public float mSpreadFactor;

	public float mBE;

	private void Start()
	{
		ReCountBE();
		mHp = mMaxHp;
	}

	public void ReCountBE()
	{
		mBE = mAtk * mMaxHp * (mDef + 200f) / mDef;
	}

	public void SetData(BattleUnitData bud)
	{
		mID = bud.mID;
		mType = bud.mType;
		mHp = (mMaxHp = bud.mMaxHp);
		mEn = (mMaxEn = bud.mMaxEn);
		mAmmo = (mMaxAmmo = bud.mMaxAmmo);
		mAtk = bud.mAtk;
		mAtkType = bud.mAtkType;
		mAtkInterval = bud.mAtkInterval;
		mAtkRange = bud.mAtkRange;
		mDef = bud.mDef;
		mDefType = bud.mDefType;
		mRps = bud.mRps;
		mHealPs = bud.mHealPs;
		mHealType = bud.mHealType;
		mHealRange = bud.mHealRange;
		mEnCostPs = bud.mEnCostPs;
		mAmmoCostPs = bud.mAmmoCostPs;
		mPlayerForce = bud.mPlayerForce;
		mMoveInterval = bud.mMoveInterval;
		mSpreadFactor = bud.mSpreadFactor;
		ReCountBE();
	}

	public void Move()
	{
		mMoveCooldownTime = mMoveInterval;
	}

	public bool CanMove()
	{
		return mMoveCooldownTime <= 0f;
	}

	private void Update()
	{
		if (mMoveCooldownTime > 0f)
		{
			mMoveCooldownTime -= Time.deltaTime;
		}
	}
}
