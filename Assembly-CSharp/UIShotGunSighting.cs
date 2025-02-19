using System;
using UnityEngine;

public class UIShotGunSighting : UIBaseSighting
{
	[SerializeField]
	private int mMaxAddWidth;

	[SerializeField]
	private int mMaxAddHeight;

	[SerializeField]
	private float mTime_t = 0.2f;

	[SerializeField]
	private UISprite mSprTop;

	[SerializeField]
	private UISprite mSprButtom;

	[SerializeField]
	private UISprite mSprLeft;

	[SerializeField]
	private UISprite mSprRight;

	private Vector3 mTopPos;

	private Vector3 mButtomPos;

	private Vector3 mLeftPos;

	private Vector3 mRightPos;

	protected override void Start()
	{
		base.Start();
		mTopPos = mSprTop.transform.localPosition;
		mButtomPos = mSprButtom.transform.localPosition;
		mLeftPos = mSprLeft.transform.localPosition;
		mRightPos = mSprRight.transform.localPosition;
	}

	protected override void Update()
	{
		base.Update();
		int num = 0;
		int num2 = 0;
		num2 = Convert.ToInt32(mTopPos.y + (float)mMaxAddHeight * base.Value);
		Vector3 localPosition = mSprTop.transform.localPosition;
		mSprTop.transform.localPosition = Vector3.Lerp(localPosition, new Vector3(mTopPos.x, num2, mTopPos.z), mTime_t);
		num2 = Convert.ToInt32(mButtomPos.y - (float)mMaxAddHeight * base.Value);
		localPosition = mSprButtom.transform.localPosition;
		mSprButtom.transform.localPosition = Vector3.Lerp(localPosition, new Vector3(mButtomPos.x, num2, mButtomPos.z), mTime_t);
		num = Convert.ToInt32(mLeftPos.x - (float)mMaxAddWidth * base.Value);
		localPosition = mSprLeft.transform.localPosition;
		mSprLeft.transform.localPosition = Vector3.Lerp(localPosition, new Vector3(num, mLeftPos.y, mLeftPos.z), mTime_t);
		num = Convert.ToInt32(mRightPos.x + (float)mMaxAddWidth * base.Value);
		localPosition = mSprRight.transform.localPosition;
		mSprRight.transform.localPosition = Vector3.Lerp(localPosition, new Vector3(num, mRightPos.y, mRightPos.z), mTime_t);
	}
}
