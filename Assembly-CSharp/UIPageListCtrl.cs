using System.Collections.Generic;
using UnityEngine;

public class UIPageListCtrl : MonoBehaviour
{
	public delegate void OnClickPageBtnFunc();

	public delegate void FuncForIndex(int index);

	public string[] mHeaderTexts;

	public int[] mHeaderWidth;

	public GameObject mHerderPrefab;

	public GameObject mHerderContent;

	public GameObject mItemPrefab;

	public GameObject mListContent;

	public float mItemHeight;

	public GameObject mLeftBtns;

	public GameObject mRightBtns;

	public UILabel mPageTextLabel;

	public GameObject mSortUp;

	public GameObject mSortDn;

	public GameObject mSortDefault;

	public int mLockSortState;

	public bool CanSelected;

	public int BtnMovePos = 12;

	public int mPagIndex;

	public int mMaxPagIndex;

	public int mMaxListCount = 9;

	public List<UIListItemCtrl> mItemCtrls = new List<UIListItemCtrl>();

	public int mSelectedIndex = -1;

	public List<PageListItem> mItems = new List<PageListItem>();

	public int mUISeletedIndex = -1;

	public List<UIListHeaderItem> mHeaderItems = new List<UIListHeaderItem>();

	private int tempResult;

	public event OnClickPageBtnFunc PageBtnLeft;

	public event OnClickPageBtnFunc PageBtnRight;

	public event OnClickPageBtnFunc PageBtnLeftEnd;

	public event OnClickPageBtnFunc PageBtnRightEnd;

	public event FuncForIndex CheckItem;

	public event FuncForIndex DoubleClickItem;

	public bool AddItem(List<string> _data)
	{
		if (_data.Count > mHeaderTexts.Length)
		{
			return false;
		}
		PageListItem pageListItem = new PageListItem();
		pageListItem.mData = _data;
		pageListItem.mColor = Color.white;
		mItems.Add(pageListItem);
		return true;
	}

	public bool AddItem(List<string> _data, Color _color)
	{
		if (_data.Count > mHeaderTexts.Length)
		{
			return false;
		}
		PageListItem pageListItem = new PageListItem();
		pageListItem.mData = _data;
		pageListItem.mColor = _color;
		mItems.Add(pageListItem);
		return true;
	}

	public bool AddItem(PageListItem item)
	{
		if (item.mData.Count > mHeaderTexts.Length)
		{
			return false;
		}
		mItems.Add(item);
		return true;
	}

	public void SetColor(int _index, Color _color)
	{
		mItems[_index].mColor = _color;
		int num = _index - mMaxListCount * mPagIndex;
		if (num >= 0 && num < mMaxListCount && mItemCtrls.Count != 0)
		{
			mItemCtrls[num].SetColor(_color);
		}
	}

	public void DeleteItem()
	{
		if (mSelectedIndex != -1 && mSelectedIndex < mItems.Count)
		{
			mItems.RemoveAt(mSelectedIndex);
			UpdateList();
		}
	}

	public void ClearSelected()
	{
		for (int i = 0; i < mItemCtrls.Count; i++)
		{
			mItemCtrls[i].SetSelected(_isSelected: false);
		}
	}

	public void SetLockUIState(int sortState)
	{
		if (!(mSortUp == null) && !(mSortDn == null) && !(mSortDefault == null))
		{
			switch (sortState)
			{
			case 0:
				mSortUp.SetActive(value: false);
				mSortDn.SetActive(value: false);
				mSortDefault.SetActive(value: true);
				break;
			case 1:
				mSortUp.SetActive(value: true);
				mSortDn.SetActive(value: false);
				mSortDefault.SetActive(value: false);
				break;
			case 2:
				mSortUp.SetActive(value: false);
				mSortDn.SetActive(value: true);
				mSortDefault.SetActive(value: false);
				break;
			}
			mLockSortState = sortState;
		}
	}

	public void UpdateList()
	{
		if (mItemCtrls.Count != mMaxListCount)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		if (mItems.Count > 0 && mPagIndex == 0)
		{
			mPagIndex = 1;
		}
		if (mItems.Count <= mMaxListCount)
		{
			if (mItems.Count == 0)
			{
				mMaxPagIndex = 0;
			}
			else
			{
				mMaxPagIndex = 1;
			}
			num = mItems.Count;
			num2 = 0;
			if (mPagIndex > mMaxPagIndex)
			{
				mPagIndex = mMaxPagIndex;
			}
		}
		else
		{
			mMaxPagIndex = (mItems.Count - 1) / mMaxListCount + 1;
			if (mPagIndex > mMaxPagIndex)
			{
				mPagIndex = mMaxPagIndex;
			}
			int num3 = mItems.Count - (mPagIndex - 1) * mMaxListCount;
			num = ((num3 <= mMaxListCount) ? num3 : mMaxListCount);
			num2 = (mPagIndex - 1) * mMaxListCount;
		}
		if (mSelectedIndex >= mItems.Count || mItems.Count == 0)
		{
			mSelectedIndex = -1;
			mUISeletedIndex = -1;
		}
		for (int i = 0; i < mMaxListCount; i++)
		{
			if (i < num)
			{
				mItemCtrls[i].SetItemText(mItems[num2 + i].mData);
				mItemCtrls[i].SetColor(mItems[num2 + i].mColor);
				mItemCtrls[i].SetIconActive(mItems[num2 + i].mEanbleICon);
				if (CanSelected)
				{
					mItemCtrls[i].SetActive(isActive: true);
				}
				else
				{
					mItemCtrls[i].SetActive(isActive: false);
				}
				if (num2 + i == mSelectedIndex)
				{
					mItemCtrls[i].SetSelected(_isSelected: true);
					mUISeletedIndex = i;
				}
				else
				{
					mItemCtrls[i].SetSelected(_isSelected: false);
				}
			}
			else
			{
				mItemCtrls[i].ClearItemText();
				mItemCtrls[i].SetActive(isActive: false);
				mItemCtrls[i].SetSelected(_isSelected: false);
				mItemCtrls[i].SetIconActive(isActive: false);
			}
		}
		UpdatePagText();
	}

	private void UpdatePagText()
	{
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
	}

	private void InitHerderInfo()
	{
		if (mHeaderTexts.Length == mHeaderWidth.Length)
		{
			mHeaderItems.Clear();
			float num = 0f;
			for (int i = 0; i < mHeaderTexts.Length; i++)
			{
				GameObject gameObject = Object.Instantiate(mHerderPrefab);
				gameObject.transform.parent = mHerderContent.transform;
				gameObject.transform.localRotation = Quaternion.identity;
				gameObject.transform.localScale = Vector3.one;
				UIListHeaderItem component = gameObject.GetComponent<UIListHeaderItem>();
				component.Init(mHeaderTexts[i], num, mHeaderWidth[i], i);
				num += (float)mHeaderWidth[i];
				mHeaderItems.Add(component);
			}
		}
	}

	private void InitList()
	{
		for (int i = 0; i < mMaxListCount; i++)
		{
			GameObject gameObject = Object.Instantiate(mItemPrefab);
			gameObject.transform.parent = mListContent.transform;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localPosition = new Vector3(0f, (0f - mItemHeight) * (float)i, 0f);
			UIListItemCtrl component = gameObject.GetComponent<UIListItemCtrl>();
			component.InitItem(mHeaderWidth);
			component.mIndex = i;
			component.SetActive(isActive: false);
			component.mIndex = mItemCtrls.Count;
			component.ListItemChecked += UICheckItem;
			component.listItemDoubleClick += UIDoubleClickItem;
			mItemCtrls.Add(component);
		}
	}

	private void Awake()
	{
		InitHerderInfo();
		InitList();
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void BtnLeftEndOnClick()
	{
		if (mPagIndex > 1)
		{
			mPagIndex = 1;
			UpdateList();
			if (this.PageBtnLeftEnd != null)
			{
				this.PageBtnLeftEnd();
			}
		}
	}

	private void BtnLeftOnClick()
	{
		if (mPagIndex > 1)
		{
			mPagIndex--;
			UpdateList();
			if (this.PageBtnLeft != null)
			{
				this.PageBtnLeft();
			}
		}
	}

	private void BtnRightEndOnClick()
	{
		if (mPagIndex < mMaxPagIndex)
		{
			mPagIndex = mMaxPagIndex;
			UpdateList();
			if (this.PageBtnRightEnd != null)
			{
				this.PageBtnRightEnd();
			}
		}
	}

	private void BtnRightOnClick()
	{
		if (mPagIndex < mMaxPagIndex)
		{
			mPagIndex++;
			UpdateList();
			if (this.PageBtnRight != null)
			{
				this.PageBtnRight();
			}
		}
	}

	private void UICheckItem(int UIIndex)
	{
		if (mUISeletedIndex >= 0 && mUISeletedIndex < mItemCtrls.Count)
		{
			mItemCtrls[mUISeletedIndex].SetSelected(_isSelected: false);
		}
		int index = UIIndex + mMaxListCount * (mPagIndex - 1);
		mUISeletedIndex = UIIndex;
		mItemCtrls[UIIndex].SetSelected(_isSelected: true);
		mSelectedIndex = index;
		if (this.CheckItem != null)
		{
			this.CheckItem(index);
		}
	}

	private void UIDoubleClickItem(int UIIndex)
	{
		int index = UIIndex + mMaxListCount * (mPagIndex - 1);
		mUISeletedIndex = UIIndex;
		mSelectedIndex = index;
		if (this.DoubleClickItem != null)
		{
			this.DoubleClickItem(index);
		}
	}
}
