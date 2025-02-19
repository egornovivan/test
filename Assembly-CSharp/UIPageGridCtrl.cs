using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIPageGridCtrl : MonoBehaviour
{
	public delegate void UpdateItem(int index_0);

	public GameObject mLeftBtns;

	public GameObject mRightBtns;

	public UILabel mPageTextLabel;

	public GameObject mGridItem;

	public UIGrid mGrid;

	public int BtnMovePos = 12;

	public int mPagIndex;

	public int mMaxPagIndex;

	public bool IsLocalPage;

	public List<UIWorkShopGridItem> mUIItems = new List<UIWorkShopGridItem>();

	public int mSelectedIndex = -1;

	public int mUISeletedIndex = -1;

	private int itemCount;

	private int tempResult;

	public int mMaxGridCount => (int)((!IsLocalPage) ? UIWorkShopCtrl.GetCurRequestCount() : UIWorkShopCtrl.GetCurLocalShowCount());

	public event OnGuiIndexBaseCallBack ClickLocalFloder;

	public event OnGuiIndexBaseCallBack ClickLocalIso;

	public event OnGuiIndexBaseCallBack ClickWorkShop;

	public event OnGuiIndexBaseCallBack ClickUpload;

	public event OnGuiIndexBaseCallBack ClickWorkShopBtnReLoad;

	public event OnGuiIndexBaseCallBack ClickUpLoadBtnReLoad;

	public event OnGuiIndexBaseCallBack ClickWorkShopBtnDing;

	public event OnGuiIndexBaseCallBack ClickWorkShopBtnCai;

	public event OnGuiIndexBaseCallBack DoubleClickLocalFloder;

	public event OnGuiIndexBaseCallBack DoubleClickLocalIso;

	public event UpdateItem mUpdateGrid;

	private void Awake()
	{
		InitGrid();
	}

	private void Start()
	{
	}

	public void InitGrid()
	{
		mGrid.maxPerLine = ((!IsLocalPage) ? UIWorkShopCtrl.GetCurColumnCount() : (UIWorkShopCtrl.GetCurColumnCount() + 1));
		for (int i = 0; i < mMaxGridCount; i++)
		{
			GameObject gameObject = Object.Instantiate(mGridItem);
			gameObject.transform.parent = mGrid.gameObject.transform;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
			UIWorkShopGridItem component = gameObject.GetComponent<UIWorkShopGridItem>();
			component.index = i;
			component.mClickItem += OnClickItem;
			component.mDoubleClickItem += OnDoubleClickItem;
			component.mBtnReloadOnClick += OnBtnReloadOnClick;
			component.mBtnDingOnClick += OnClickWorkShopBtnDing;
			component.mBtnCaiOnClick += OnClickWorkShopBtnCai;
			mUIItems.Add(component);
		}
	}

	public void ReSetGrid(int _itemCount)
	{
		itemCount = _itemCount;
	}

	public void _UpdatePagText()
	{
		if (itemCount > 0 && mPagIndex == 0)
		{
			mPagIndex = 1;
		}
		if (itemCount <= mMaxGridCount)
		{
			if (itemCount == 0)
			{
				mMaxPagIndex = 0;
			}
			else
			{
				mMaxPagIndex = 1;
			}
			if (mPagIndex > mMaxPagIndex)
			{
				mPagIndex = mMaxPagIndex;
			}
		}
		else
		{
			mMaxPagIndex = (itemCount - 1) / mMaxGridCount + 1;
			if (mPagIndex > mMaxPagIndex)
			{
				mPagIndex = mMaxPagIndex;
			}
		}
		int num = 1;
		int num2 = 1;
		if (mPagIndex > 9)
		{
			int num3 = mPagIndex;
			while (num3 >= 10)
			{
				num3 /= 10;
				num++;
			}
		}
		if (mMaxPagIndex > 9)
		{
			int num4 = mMaxPagIndex;
			while (num4 >= 10)
			{
				num4 /= 10;
				num2++;
			}
		}
		if (tempResult != num2 - 1)
		{
			Vector3 localPosition = mRightBtns.transform.localPosition;
			localPosition.x += BtnMovePos * (num2 - 1 - tempResult);
			mRightBtns.transform.localPosition = localPosition;
			Vector3 localPosition2 = mLeftBtns.transform.localPosition;
			localPosition2.x -= BtnMovePos * (num2 - 1 - tempResult);
			mLeftBtns.transform.localPosition = localPosition2;
			tempResult = num2 - 1;
		}
		string text = string.Empty;
		for (int num5 = num2 - num; num5 > 0; num5--)
		{
			text += "0";
		}
		mPageTextLabel.text = text + mPagIndex + "/" + mMaxPagIndex;
		mSelectedIndex = -1;
		mUISeletedIndex = -1;
		int num6 = 0;
		if (mPagIndex != 0)
		{
			num6 = mMaxGridCount * (mPagIndex - 1);
			if (this.mUpdateGrid != null)
			{
				this.mUpdateGrid(num6);
			}
		}
	}

	public UIWorkShopGridItem GetWorkShopItemByFileName(string fileName)
	{
		if (mUIItems == null)
		{
			return null;
		}
		return mUIItems.FirstOrDefault((UIWorkShopGridItem a) => a != null && a.IsoFileName.Equals(fileName));
	}

	private void BtnLeftEndOnClick()
	{
		if (mPagIndex > 1)
		{
			mPagIndex = 1;
			_UpdatePagText();
		}
	}

	private void BtnLeftOnClick()
	{
		if (mPagIndex > 1)
		{
			mPagIndex--;
			_UpdatePagText();
		}
	}

	private void BtnRightEndOnClick()
	{
		if (mPagIndex < mMaxPagIndex)
		{
			mPagIndex = mMaxPagIndex;
			_UpdatePagText();
		}
	}

	private void BtnRightOnClick()
	{
		if (mPagIndex < mMaxPagIndex)
		{
			mPagIndex++;
			_UpdatePagText();
		}
	}

	private void OnClickItem(WorkGridItemType mType, int UIIndex)
	{
		if (mPagIndex <= 0)
		{
			return;
		}
		int index = UIIndex + mMaxGridCount * (mPagIndex - 1);
		if (UIIndex != mUISeletedIndex)
		{
			if (mUISeletedIndex >= 0 && mUISeletedIndex < mUIItems.Count)
			{
				mUIItems[mUISeletedIndex].SetSelected(Selected: false);
			}
			mUISeletedIndex = UIIndex;
			mUIItems[UIIndex].SetSelected(Selected: true);
		}
		mSelectedIndex = index;
		if (mType == WorkGridItemType.mLocalFloder && this.ClickLocalFloder != null)
		{
			this.ClickLocalFloder(index);
		}
		else if (mType == WorkGridItemType.mLocalIcon && this.ClickLocalIso != null)
		{
			this.ClickLocalIso(index);
		}
		else if (mType == WorkGridItemType.mWorkShop && this.ClickWorkShop != null)
		{
			this.ClickWorkShop(index);
		}
		else if (mType == WorkGridItemType.mUpLoad && this.ClickUpload != null)
		{
			this.ClickUpload(index);
		}
	}

	private void OnDoubleClickItem(WorkGridItemType mType, int UIIndex)
	{
		if (mPagIndex > 0)
		{
			int num = UIIndex + mMaxGridCount * (mPagIndex - 1);
			mSelectedIndex = num;
			if (mType == WorkGridItemType.mLocalFloder && this.DoubleClickLocalFloder != null)
			{
				this.DoubleClickLocalFloder(mSelectedIndex);
			}
			else if (mType == WorkGridItemType.mLocalIcon && this.DoubleClickLocalIso != null)
			{
				this.DoubleClickLocalIso(mSelectedIndex);
			}
		}
	}

	private void OnBtnReloadOnClick(WorkGridItemType mType, int UIIndex)
	{
		if (mPagIndex > 0)
		{
			int index = (mSelectedIndex = UIIndex + mMaxGridCount * (mPagIndex - 1));
			if (mType == WorkGridItemType.mWorkShop && this.ClickWorkShopBtnReLoad != null)
			{
				this.ClickWorkShopBtnReLoad(index);
			}
			else if (mType == WorkGridItemType.mUpLoad && this.ClickUpLoadBtnReLoad != null)
			{
				this.ClickUpLoadBtnReLoad(index);
			}
		}
	}

	private void OnClickWorkShopBtnDing(WorkGridItemType mType, int UIIndex)
	{
		if (mPagIndex > 0)
		{
			int index = (mSelectedIndex = UIIndex + mMaxGridCount * (mPagIndex - 1));
			if (mType == WorkGridItemType.mWorkShop && this.ClickWorkShopBtnDing != null)
			{
				this.ClickWorkShopBtnDing(index);
			}
		}
	}

	private void OnClickWorkShopBtnCai(WorkGridItemType mType, int UIIndex)
	{
		if (mPagIndex > 0)
		{
			int index = (mSelectedIndex = UIIndex + mMaxGridCount * (mPagIndex - 1));
			if (mType == WorkGridItemType.mWorkShop && this.ClickWorkShopBtnCai != null)
			{
				this.ClickWorkShopBtnCai(index);
			}
		}
	}
}
