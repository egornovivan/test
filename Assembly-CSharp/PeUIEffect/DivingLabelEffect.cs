using System;
using System.Collections.Generic;
using UnityEngine;

namespace PeUIEffect;

public class DivingLabelEffect : UIEffect
{
	[SerializeField]
	private float mSpeed = 2f;

	[SerializeField]
	private int mMaxLineWidth = 250;

	[SerializeField]
	private List<UILabel> mLbList = new List<UILabel>();

	private UIDirvingShowHideEffct dirvingEffct;

	private void Start()
	{
		dirvingEffct = GetComponent<UIDirvingShowHideEffct>();
		dirvingEffct.e_OnEnd += OnUIDringShow;
	}

	private void OnUIDringShow(UIEffect effct)
	{
		if (effct.Forward)
		{
			Play();
		}
	}

	public override void Play()
	{
		foreach (UILabel mLb in mLbList)
		{
			mLb.lineWidth = 0;
		}
		base.Play();
	}

	private void Update()
	{
		if (!m_Runing)
		{
			return;
		}
		foreach (UILabel mLb in mLbList)
		{
			mLb.lineWidth += Convert.ToInt32(mSpeed * (float)mLb.text.Length);
			if (mLb.lineWidth > mMaxLineWidth)
			{
				End();
			}
		}
	}
}
