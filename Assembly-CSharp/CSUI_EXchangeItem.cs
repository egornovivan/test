using System;
using ItemAsset;
using UnityEngine;

public class CSUI_EXchangeItem : MonoBehaviour
{
	[SerializeField]
	private UILabel mMaxNumLb;

	[SerializeField]
	private UISprite mItemSprit;

	[SerializeField]
	private UILabel mPerviteNumLb;

	public UIInput mInputNuLabel;

	private int m_CurmaxNum;

	private int m_MaxNum;

	private int mprotoId;

	private ItemSample mItemSample;

	private ListItemType mType;

	private int mPackageNum;

	private bool mAddBtnPress;

	private bool mSubBtnPress;

	private float mOpStarTime;

	private float mCurrentNum;

	private float mOpDurNum;

	public int MaxNum
	{
		get
		{
			return m_MaxNum;
		}
		set
		{
			m_MaxNum = value;
		}
	}

	public float CurrentNum => mCurrentNum;

	public int ProtoId
	{
		get
		{
			return mprotoId;
		}
		set
		{
			mprotoId = value;
		}
	}

	public ListItemType Type
	{
		get
		{
			return mType;
		}
		set
		{
			mType = value;
		}
	}

	public int PackageNum
	{
		get
		{
			return mPackageNum;
		}
		set
		{
			mPackageNum = value;
			mPerviteNumLb.text = mPackageNum.ToString();
		}
	}

	private void Start()
	{
	}

	public void SetIcon(string icon)
	{
		mItemSprit.spriteName = icon;
		mItemSprit.MakePixelPerfect();
	}

	public void SetMaxNum(int maxNum)
	{
		mMaxNumLb.text = "X" + maxNum;
		m_MaxNum = maxNum;
	}

	private void Update()
	{
		if (m_MaxNum > mPackageNum)
		{
			m_CurmaxNum = mPackageNum;
		}
		else
		{
			m_CurmaxNum = m_MaxNum;
		}
		if (mAddBtnPress)
		{
			float num = Time.time - mOpStarTime;
			if (num < 0.2f)
			{
				mOpDurNum = 1f;
			}
			else if (num < 1f)
			{
				mOpDurNum += 2f * Time.deltaTime;
			}
			else if (num < 2f)
			{
				mOpDurNum += 4f * Time.deltaTime;
			}
			else if (num < 3f)
			{
				mOpDurNum += 7f * Time.deltaTime;
			}
			else if (num < 4f)
			{
				mOpDurNum += 11f * Time.deltaTime;
			}
			else if (num < 5f)
			{
				mOpDurNum += 16f * Time.deltaTime;
			}
			else
			{
				mOpDurNum += 20f * Time.deltaTime;
			}
			if ((float)m_CurmaxNum >= mOpDurNum + mCurrentNum)
			{
				mInputNuLabel.text = ((int)(mOpDurNum + mCurrentNum)).ToString();
			}
			else
			{
				mInputNuLabel.text = m_CurmaxNum.ToString();
			}
		}
		else if (mSubBtnPress)
		{
			float num2 = Time.time - mOpStarTime;
			if (num2 < 0.5f)
			{
				mOpDurNum = -1f;
			}
			else if (num2 < 1f)
			{
				mOpDurNum -= 2f * Time.deltaTime;
			}
			else if (num2 < 2f)
			{
				mOpDurNum -= 4f * Time.deltaTime;
			}
			else if (num2 < 3f)
			{
				mOpDurNum -= 7f * Time.deltaTime;
			}
			else if (num2 < 4f)
			{
				mOpDurNum -= 11f * Time.deltaTime;
			}
			else if (num2 < 5f)
			{
				mOpDurNum -= 16f * Time.deltaTime;
			}
			else
			{
				mOpDurNum -= 20f * Time.deltaTime;
			}
			if (mOpDurNum + mCurrentNum > 0f)
			{
				mInputNuLabel.text = ((int)(mOpDurNum + mCurrentNum)).ToString();
			}
			else
			{
				mInputNuLabel.text = "0";
			}
		}
		if (string.Empty == mInputNuLabel.text)
		{
			mCurrentNum = 0f;
			return;
		}
		mCurrentNum = Convert.ToInt32(mInputNuLabel.text);
		if (mCurrentNum > (float)m_CurmaxNum)
		{
			mInputNuLabel.text = m_CurmaxNum.ToString();
		}
	}

	public void SetTheCurentNum(float curtent)
	{
	}

	private void OnSubmit()
	{
	}

	private void OnAddBtn()
	{
	}

	private void OnSubtractBtn()
	{
	}

	private void OnAddBtnPress()
	{
		mAddBtnPress = true;
		mOpStarTime = Time.time;
		mOpDurNum = 0f;
	}

	private void OnAddBtnRelease()
	{
		mAddBtnPress = false;
		mCurrentNum = (int)(mCurrentNum + mOpDurNum);
		mOpDurNum = 0f;
		if (mCurrentNum > (float)m_CurmaxNum)
		{
			mCurrentNum = m_CurmaxNum;
		}
		mInputNuLabel.text = ((int)mCurrentNum).ToString();
	}

	private void OnSubstructBtnPress()
	{
		mSubBtnPress = true;
		mOpStarTime = Time.time;
		mOpDurNum = 0f;
	}

	private void OnSubstructBtnRelease()
	{
		mSubBtnPress = false;
		mCurrentNum = (int)(mCurrentNum + mOpDurNum);
		mOpDurNum = 0f;
		if (mCurrentNum < 0f)
		{
			mCurrentNum = 0f;
		}
		mInputNuLabel.text = ((int)mCurrentNum).ToString();
	}

	private void OnTooltip(bool show)
	{
		if (mType == ListItemType.mItem)
		{
			if (show && mItemSample == null && mprotoId != 0)
			{
				mItemSample = new ItemSample(mprotoId);
			}
			else if (!show)
			{
				mItemSample = null;
			}
			if (mItemSample != null)
			{
				string @string = PELocalization.GetString(mItemSample.protoData.descriptionStringId);
				ToolTipsMgr.ShowText(@string);
			}
			else
			{
				ToolTipsMgr.ShowText(null);
			}
		}
	}
}
