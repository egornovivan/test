using System;
using ItemAsset;
using UnityEngine;

public class CSUI_GetItem : MonoBehaviour
{
	[SerializeField]
	private UISprite mItemSprite;

	[SerializeField]
	private UILabel mPerviteNumLb;

	public UIInput mInputNuLabel;

	private float mCurrentNum;

	private int mprotoId;

	private ItemSample mItemSample;

	private ListItemType mType;

	private int mPackageNum;

	public float CurrentNum
	{
		get
		{
			return mCurrentNum;
		}
		set
		{
			mCurrentNum = value;
			mInputNuLabel.text = mCurrentNum.ToString();
		}
	}

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

	public void SetCurrentNum(int MaxNum)
	{
		mCurrentNum = MaxNum;
		mInputNuLabel.text = mCurrentNum.ToString();
	}

	public void SetTexture(string icon)
	{
		mItemSprite.spriteName = icon;
		mItemSprite.MakePixelPerfect();
	}

	private void Update()
	{
		if (string.Empty == mInputNuLabel.text)
		{
			mCurrentNum = 0f;
		}
		else
		{
			mCurrentNum = Convert.ToInt32(mInputNuLabel.text);
		}
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
