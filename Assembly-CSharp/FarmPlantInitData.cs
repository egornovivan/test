using UnityEngine;

public class FarmPlantInitData
{
	public int mPlantInstanceId;

	public int mTypeID;

	public Vector3 mPos;

	public Quaternion mRot;

	public double mPutOutGameTime;

	public double mLife;

	public double mWater;

	public double mClean;

	public bool mDead;

	public int mGrowTimeIndex;

	public double mCurGrowTime;

	public byte mTerrianType;

	public float mGrowRate = 1f;

	public float mExtraGrowRate;

	public float mNpcGrowRate;

	public double mLastUpdateTime;
}
