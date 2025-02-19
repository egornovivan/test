using System;
using ItemAsset;
using Pathea;
using UnityEngine;

public class UICompoundWndListItem : MonoBehaviour
{
	public delegate void ClickFunc(int index);

	public UIAtlas mAtlasUI;

	public UIAtlas mAtlasIcon;

	public UILabel mText;

	public UILabel mItemName;

	public UITexture mContentTexture;

	public UISprite[] mContentSprites;

	public UISlicedSprite mItemLine;

	public UISprite mNewMask;

	public int mItemId;

	public UISlicedSprite mMouseSprite;

	public UISlicedSprite mSelectedSprite;

	public BoxCollider mIcoCollider;

	public BoxCollider mRinghtCollider;

	public GameObject mPanel;

	public GameObject mScrolBox;

	public bool isSelected;

	public int mIndex;

	public ListItemType mType;

	private ItemSample mItemSample;

	private bool IsEnableCollider;

	private int mPaneHeight;

	private ItemObject _itemObj;

	public event ClickFunc mItemClick;

	public void SetItem(string itemName, int id, bool bNew, string[] strContentSprite, string text, int index, ListItemType itemType)
	{
		mItemName.text = itemName;
		mItemId = id;
		mText.text = text;
		SetCotent(strContentSprite);
		mIndex = index;
		mType = itemType;
		mNewMask.enabled = bNew;
	}

	public void SetTextColor(Color mColor)
	{
		mText.color = mColor;
	}

	private void ActiveBoxClider(bool isActive)
	{
		mIcoCollider.enabled = isActive;
		mRinghtCollider.enabled = isActive;
		mItemName.enabled = isActive;
		mItemLine.enabled = isActive;
	}

	private void SetCotent(string[] sprNameList)
	{
		if (mContentSprites == null || mContentSprites.Length <= 0)
		{
			return;
		}
		for (int i = 0; i < sprNameList.Length; i++)
		{
			if (i < mContentSprites.Length)
			{
				if (sprNameList[i] == "0")
				{
					mContentSprites[i].gameObject.SetActive(value: false);
					continue;
				}
				mContentSprites[i].spriteName = sprNameList[i];
				mContentSprites[i].gameObject.SetActive(value: true);
			}
		}
	}

	private void SetCotent(Texture _contentTexture)
	{
		mContentTexture.mainTexture = _contentTexture;
		mContentTexture.gameObject.SetActive(value: true);
		if (mContentSprites != null && mContentSprites.Length > 0)
		{
			for (int i = 0; i < mContentSprites.Length; i++)
			{
				mContentSprites[i].gameObject.SetActive(value: false);
			}
		}
	}

	private void OnMouseOver()
	{
		if (!isSelected)
		{
			mMouseSprite.enabled = true;
		}
	}

	private void OnMoseOut()
	{
		if (!isSelected)
		{
			mMouseSprite.enabled = false;
		}
	}

	private void OnClick()
	{
		if (this.mItemClick != null)
		{
			this.mItemClick(mIndex);
		}
		mNewMask.enabled = false;
	}

	public void SetSelectmState(bool isSelected)
	{
		if (isSelected)
		{
			isSelected = true;
			mMouseSprite.enabled = false;
			mSelectedSprite.enabled = true;
		}
		else
		{
			isSelected = false;
			mSelectedSprite.enabled = false;
		}
	}

	private void Start()
	{
		UIScrollBox component = mScrolBox.GetComponent<UIScrollBox>();
		mPaneHeight = component.m_Height;
		ActiveBoxClider(isActive: false);
	}

	private void Update()
	{
		if (isShowInPanel() && !IsEnableCollider)
		{
			ActiveBoxClider(isActive: true);
			IsEnableCollider = true;
		}
		else if (!isShowInPanel() && IsEnableCollider)
		{
			ActiveBoxClider(isActive: false);
			IsEnableCollider = false;
		}
	}

	private bool isShowInPanel()
	{
		float num = mPanel.transform.localPosition.y - 3f;
		float y = base.gameObject.transform.localPosition.y;
		float num2 = Math.Abs(y) - num;
		if (num2 >= -3f && num2 <= (float)mPaneHeight)
		{
			return true;
		}
		return false;
	}

	private void OnTooltip(bool show)
	{
		if (mType == ListItemType.mItem)
		{
			if (show && mItemSample == null && mItemId != 0)
			{
				mItemSample = new ItemSample(mItemId);
			}
			else if (!show)
			{
				mItemSample = null;
			}
			if (mItemSample != null)
			{
				_itemObj = PeSingleton<ItemMgr>.Instance.CreateItem(mItemId);
				string tooltip = _itemObj.GetTooltip();
				ToolTipsMgr.ShowText(tooltip);
			}
			else
			{
				PeSingleton<ItemMgr>.Instance.DestroyItem(_itemObj);
				ToolTipsMgr.ShowText(null);
			}
		}
	}
}
