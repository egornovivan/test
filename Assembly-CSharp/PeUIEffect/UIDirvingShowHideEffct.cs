using System;
using UnityEngine;

namespace PeUIEffect;

public class UIDirvingShowHideEffct : UIEffect
{
	[Serializable]
	public class TargetSpr
	{
		public Transform mTargetTs;

		private Vector3 mPosFrom;

		private Vector3 mPosTo;

		private UISprite mSpr;

		public void Init()
		{
			if (mTargetTs != null)
			{
				mPosTo = mTargetTs.localPosition;
				mPosFrom = new Vector3(0f, mPosTo.y, mPosTo.z);
				mSpr = mTargetTs.GetComponent<UISprite>();
			}
		}

		public void Update(float acValue, Color color)
		{
			if (!(mTargetTs == null))
			{
				int num = Convert.ToInt32(acValue * mPosTo.x);
				mTargetTs.localPosition = new Vector3(num, mPosTo.y, mPosTo.z);
				if (mSpr != null)
				{
					mSpr.color = color;
				}
			}
		}

		public void OnEffectEnd(bool forward)
		{
			if (mTargetTs != null)
			{
				mTargetTs.localPosition = ((!forward) ? mPosFrom : mPosTo);
			}
		}
	}

	[SerializeField]
	private AcEffect mAcEffctPosx;

	[SerializeField]
	private TargetSpr mTopLeft;

	[SerializeField]
	private TargetSpr mTopRight;

	[SerializeField]
	private TargetSpr mButtomLeft;

	[SerializeField]
	private TargetSpr mButtomRight;

	private float time;

	public override void Play(bool forward)
	{
		time = 0f;
		base.Play(forward);
	}

	public override void End()
	{
		mTopLeft.OnEffectEnd(mForward);
		mTopRight.OnEffectEnd(mForward);
		mButtomLeft.OnEffectEnd(mForward);
		mButtomRight.OnEffectEnd(mForward);
		base.End();
	}

	private void Start()
	{
		mTopLeft.Init();
		mTopRight.Init();
		mButtomLeft.Init();
		mButtomRight.Init();
	}

	private void Update()
	{
		if (m_Runing)
		{
			if (mAcEffctPosx.bActive)
			{
				time += Time.deltaTime;
				float num = ((!mForward) ? (1f - mAcEffctPosx.GetAcValue(time)) : mAcEffctPosx.GetAcValue(time));
				Color color = ((!mForward) ? new Color(1f, 1f, 1f, num * num) : new Color(1f, 1f, 1f, 0.5f + num));
				mTopLeft.Update(num, color);
				mTopRight.Update(num, color);
				mButtomLeft.Update(num, color);
				mButtomRight.Update(num, color);
			}
			if (time > mAcEffctPosx.EndTime)
			{
				End();
			}
		}
	}
}
