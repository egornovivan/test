using System;
using UnityEngine;

public class UIInviteMsgbox : UIStaticWnd
{
	public delegate void CallBackFunc();

	[SerializeField]
	private UILabel mLbText;

	[SerializeField]
	private TweenPosition mTweenPos;

	public CallBackFunc mJoinFunc;

	public CallBackFunc mCancelFunc;

	public CallBackFunc mIgnorAllFunc;

	public CallBackFunc mTimeOutFunc;

	private UTimer mTimer = new UTimer();

	private bool isHide;

	public void ShowMsg(string msg, CallBackFunc joinFunc, CallBackFunc cancelFunc, CallBackFunc ignorAllFunc, CallBackFunc timeOutFunc)
	{
		mLbText.text = msg;
		mJoinFunc = (CallBackFunc)Delegate.Combine(mJoinFunc, joinFunc);
		mCancelFunc = (CallBackFunc)Delegate.Combine(mCancelFunc, cancelFunc);
		mIgnorAllFunc = (CallBackFunc)Delegate.Combine(mIgnorAllFunc, ignorAllFunc);
		mTimeOutFunc = (CallBackFunc)Delegate.Combine(mTimeOutFunc, timeOutFunc);
		Show();
		mTimer.Second = 60.0;
		mTimer.ElapseSpeed = -1f;
	}

	private void BtnJionOnClick()
	{
		if (mJoinFunc != null)
		{
			mJoinFunc();
		}
		Hide();
	}

	private void BtnCancelOnClick()
	{
		if (mCancelFunc != null)
		{
			mCancelFunc();
		}
		Hide();
	}

	private void BtnIgnorAllOnClick()
	{
		if (mIgnorAllFunc != null)
		{
			mIgnorAllFunc();
		}
		Hide();
	}

	private void Update()
	{
	}

	public override void Show()
	{
		isHide = false;
		base.Show();
		mTweenPos.Play(forward: true);
	}

	protected override void OnHide()
	{
		isHide = true;
		mTweenPos.Play(forward: false);
	}

	private void MoveFinished()
	{
		if (isHide)
		{
			Hide();
		}
	}
}
