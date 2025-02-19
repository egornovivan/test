using System;
using UnityEngine;

public class UITalkScrollBoxCtrl : MonoBehaviour
{
	public UISlicedSprite mWndBg;

	public float mScrollBoxWidth;

	public float mScrollBoxHeight;

	private UIScrollBox mSrollboxCtrl;

	private void Start()
	{
		mSrollboxCtrl = base.gameObject.GetComponent<UIScrollBox>();
	}

	private void Update()
	{
		if (mSrollboxCtrl != null)
		{
			UpdateScollBoxState();
		}
	}

	private void UpdateScollBoxState()
	{
		if (mScrollBoxWidth != mWndBg.gameObject.transform.localScale.x - 70f)
		{
			mScrollBoxWidth = mWndBg.gameObject.transform.localScale.x - 70f;
			mSrollboxCtrl.m_Width = Convert.ToInt32(mScrollBoxWidth);
		}
		if (mScrollBoxHeight != mWndBg.gameObject.transform.localScale.y - 10f)
		{
			mScrollBoxHeight = mWndBg.gameObject.transform.localScale.y - 10f;
			Vector3 localPosition = base.gameObject.transform.localPosition;
			base.gameObject.transform.localPosition = new Vector3(localPosition.x, mWndBg.gameObject.transform.localScale.y, localPosition.z);
			mSrollboxCtrl.m_Height = Convert.ToInt32(mScrollBoxHeight);
		}
	}
}
