using UnityEngine;

public class UIWorkShopListItem : MonoBehaviour
{
	public UILabel mLbText;

	public UISlicedSprite mMouseSprite;

	public UISlicedSprite mSelectedSprite;

	public int mIndex = -1;

	public bool isSelected;

	public event OnGuiIndexBaseCallBack ItemClick;

	public void SetText(string _text)
	{
		mLbText.text = _text;
	}

	public void SetIndex(int index)
	{
		mIndex = index;
	}

	public void SetSelected(bool isSelected)
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

	private void ListItemOnClick()
	{
		if (this.ItemClick != null)
		{
			this.ItemClick(mIndex);
		}
	}
}
