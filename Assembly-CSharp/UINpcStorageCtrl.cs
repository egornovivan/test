using PeUIEffect;
using UnityEngine;

public class UINpcStorageCtrl : UIBaseWnd
{
	public GameObject mContent;

	public UISprite mContentSprite;

	public UITexture mContentTexture;

	public UILabel mNpcNameText;

	public UICheckbox[] PageTitles;

	private static UINpcStorageCtrl mInstance;

	[SerializeField]
	private UISpriteScaleEffect effect;

	private int mPageIndex;

	public static UINpcStorageCtrl Instance => mInstance;

	public event OnGuiBtnClicked btnClose;

	public event OnGuiBtnClicked btnPageItem;

	public event OnGuiBtnClicked btnPageEquipment;

	public event OnGuiBtnClicked btnPageResource;

	public override void Show()
	{
		if (effect != null)
		{
			effect.Play();
		}
		base.Show();
	}

	public void SetNpcName(string strName)
	{
		mNpcNameText.text = strName + " Stroage";
	}

	public void SetICO(string _sprName)
	{
		if (!(mContentSprite == null))
		{
			mContentSprite.spriteName = _sprName;
			mContentSprite.gameObject.SetActive(value: true);
			mContentTexture.gameObject.SetActive(value: false);
		}
	}

	public void SetICO(Texture _contentTexture)
	{
		if (!(mContentTexture == null))
		{
			mContentTexture.mainTexture = _contentTexture;
			mContentTexture.gameObject.SetActive(value: true);
			mContentSprite.gameObject.SetActive(value: false);
		}
	}

	public bool SetTabIndex(int index)
	{
		if (index < 0 || index >= 3)
		{
			return false;
		}
		if (index == mPageIndex)
		{
			return false;
		}
		PageTitles[index].isChecked = true;
		return true;
	}

	protected override void InitWindow()
	{
		base.InitWindow();
		mInstance = this;
	}

	private void BtnCloseOnClick()
	{
		if (this.btnClose != null)
		{
			this.btnClose();
		}
		Hide();
	}

	private void BtnPageItemOnClick(bool isActive)
	{
		if (isActive)
		{
			mPageIndex = 0;
			if (this.btnPageItem != null)
			{
				this.btnPageItem();
			}
		}
	}

	private void BtnPageEquipmentOnClick(bool isActive)
	{
		if (isActive)
		{
			mPageIndex = 1;
			if (this.btnPageEquipment != null)
			{
				this.btnPageEquipment();
			}
		}
	}

	private void BtnPageResourceOnClick(bool isActive)
	{
		if (isActive)
		{
			mPageIndex = 2;
			if (this.btnPageResource != null)
			{
				this.btnPageResource();
			}
		}
	}
}
