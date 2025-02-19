using UnityEngine;

public class MetalScanItem_N : MonoBehaviour
{
	public delegate void OnClickEvent(object sender);

	public UICheckbox mCheckBox;

	public UISprite mBgSpr;

	public UISprite mSelecSpr;

	public Color mColor;

	public byte mType;

	public ShowToolTipItem_N mToolTip_N;

	public event OnClickEvent e_OnClick;

	public void SetItem(string name, Color col, byte type, int tipID)
	{
		mType = type;
		mColor = col;
		mBgSpr.spriteName = name;
		mSelecSpr.spriteName = name;
		mToolTip_N.mStrID = tipID;
	}

	private void OnClick()
	{
		if (mType != 0 && this.e_OnClick != null)
		{
			this.e_OnClick(this);
		}
	}
}
