using UnityEngine;

public class DoubleDisTrigger : TriggerController
{
	public float mFirstDistnce = 64f;

	public float mSencondDistnce = 128f;

	public Transform mDistanceTarget;

	public string mFirstEnterFuncName = "OnEnterActiveDistance";

	public string mFirstExitFuncName = "OnExitActiveDistance";

	public string mSecondEnterFuncName = "OnEnterViewableDistance";

	public string mSecondExitFuncName = "OnExitViewableDistance";

	private float mCurrentDis;

	private int mDisState;

	protected override void InitDefault()
	{
		base.InitDefault();
		if (!(null == mDistanceTarget))
		{
		}
	}

	protected override bool CheckTrigger()
	{
		if (null != mDistanceTarget)
		{
			float num = mCurrentDis;
			float num2 = (mCurrentDis = Vector3.Distance(mDistanceTarget.position, mTrigerTarget.transform.position));
			if (num >= mFirstDistnce && num2 < mFirstDistnce)
			{
				mDisState = 1;
				return true;
			}
			if (num <= mFirstDistnce && num2 > mFirstDistnce)
			{
				mDisState = 2;
				return true;
			}
			if (num >= mSencondDistnce && num2 < mSencondDistnce)
			{
				mDisState = 3;
				return true;
			}
			if (num <= mSencondDistnce && num2 > mSencondDistnce)
			{
				mDisState = 4;
				return true;
			}
		}
		return false;
	}

	protected override void OnHitTraigger()
	{
		switch (mDisState)
		{
		case 1:
			mTrigerTarget.SendMessage(mFirstEnterFuncName, SendMessageOptions.DontRequireReceiver);
			break;
		case 2:
			mTrigerTarget.SendMessage(mFirstExitFuncName, SendMessageOptions.DontRequireReceiver);
			break;
		case 3:
			mTrigerTarget.SendMessage(mSecondEnterFuncName, SendMessageOptions.DontRequireReceiver);
			break;
		case 4:
			mTrigerTarget.SendMessage(mSecondExitFuncName, SendMessageOptions.DontRequireReceiver);
			break;
		}
	}
}
