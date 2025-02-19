using System.Collections.Generic;
using UnityEngine;

public class UIPageGridBox : MonoBehaviour
{
	public delegate void ReflashGridEvent(int starIndex);

	public GameObject mGridItemPrefab;

	public GameObject mLeftBtns;

	public GameObject mRightBtns;

	public UILabel mPageTextLabel;

	public UIGrid mGrid;

	public int BtnMovePos = 12;

	public int mPagIndex;

	public int mMaxPagIndex;

	public int mMaxGridCount = 12;

	public List<GameObject> mItemsObject = new List<GameObject>();

	public int mItemCount;

	[HideInInspector]
	public int mSelectedIndex = -1;

	[HideInInspector]
	public int mUISeletedIndex = -1;

	[HideInInspector]
	public int mStartIndex = -1;

	private bool isInit;

	private int tempResult;

	public event ReflashGridEvent e_RefalshGrid;

	private void Awake()
	{
		InitGrid();
	}

	public void InitGrid()
	{
		if (!isInit)
		{
			for (int i = 0; i < mMaxGridCount; i++)
			{
				GameObject gameObject = Object.Instantiate(mGridItemPrefab);
				gameObject.transform.parent = mGrid.gameObject.transform;
				gameObject.transform.localRotation = Quaternion.identity;
				gameObject.transform.localScale = Vector3.one;
				gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
				mItemsObject.Add(gameObject);
			}
			mGrid.repositionNow = true;
			isInit = true;
		}
	}

	public void ResetItemCount(int _itemCount)
	{
		mItemCount = _itemCount;
	}

	public void ReflashGridCotent()
	{
		if (mItemCount > 0 && mPagIndex == 0)
		{
			mPagIndex = 1;
		}
		if (mItemCount <= mMaxGridCount)
		{
			if (mItemCount == 0)
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
			mMaxPagIndex = (mItemCount - 1) / mMaxGridCount + 1;
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
		mStartIndex = 0;
		if (mPagIndex != 0)
		{
			mStartIndex = mMaxGridCount * (mPagIndex - 1);
		}
		if (this.e_RefalshGrid != null)
		{
			this.e_RefalshGrid(mStartIndex);
		}
	}

	private void BtnLeftEndOnClick()
	{
		if (mPagIndex > 1)
		{
			mPagIndex = 1;
			ReflashGridCotent();
		}
	}

	private void BtnLeftOnClick()
	{
		if (mPagIndex > 1)
		{
			mPagIndex--;
			ReflashGridCotent();
		}
	}

	private void BtnRightEndOnClick()
	{
		if (mPagIndex < mMaxPagIndex)
		{
			mPagIndex = mMaxPagIndex;
			ReflashGridCotent();
		}
	}

	private void BtnRightOnClick()
	{
		if (mPagIndex < mMaxPagIndex)
		{
			mPagIndex++;
			ReflashGridCotent();
		}
	}
}
