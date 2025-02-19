using UnityEngine;

public class PageGridBox : MonoBehaviour
{
	public UILabel mTextLabel;

	public UIGrid mGrid;

	public int mPageNum;

	protected int mPagIndex;

	protected int mMaxPagIndex;

	[HideInInspector]
	public event OnGuiBtnClicked PageBtnLeft;

	[HideInInspector]
	public event OnGuiBtnClicked PageBtnRight;

	[HideInInspector]
	public event OnGuiBtnClicked PageBtnLeftEnd;

	[HideInInspector]
	public event OnGuiBtnClicked PageBtnRightEnd;

	private void BtnLeftEndOnClick()
	{
		if (mPagIndex >= 1)
		{
			mPagIndex = 0;
			UpdateList();
			if (this.PageBtnLeftEnd != null)
			{
				this.PageBtnLeftEnd();
			}
		}
	}

	private void BtnLeftOnClick()
	{
		if (mPagIndex >= 1)
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

	protected virtual void UpdateList()
	{
		UpdatePagText();
	}

	private void UpdatePagText()
	{
		int length = (mMaxPagIndex + 1).ToString().Length;
		int length2 = (mPagIndex + 1).ToString().Length;
		string text = string.Empty;
		for (int i = 0; i < length - length2; i++)
		{
			text += "0";
		}
		mTextLabel.text = text + (mPagIndex + 1) + "/" + (mMaxPagIndex + 1);
	}
}
