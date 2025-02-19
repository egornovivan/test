using Pathea;
using UnityEngine;

public class UIMallItem : MonoBehaviour
{
	public delegate void MallItem(int index, UIMallItem item);

	public UICheckbox mCheckBox;

	public UILabel mLbText;

	public UILabel mLbPice;

	public int mIndex;

	public UISprite mSpr;

	public BoxCollider mCollider;

	public GameObject mBtnBuy;

	public GameObject mBtnExport;

	public UISprite mSprDiscount;

	public UILabel mLbDisCount;

	public UILabel mLbCount;

	public MallItemData mData;

	public event MallItem e_ItemBuy;

	public event MallItem e_OnClick;

	public event MallItem e_ItemExport;

	public void SetInfo(MallItemData data, int index)
	{
		mLbText.text = data.GetName();
		mLbPice.text = data.GetPrice() + " [C8C800]PP";
		mSpr.spriteName = data.GetSprName();
		mSpr.MakePixelPerfect();
		mIndex = index;
		mCollider.enabled = true;
		mData = data;
		mLbCount.text = mData.GetCount().ToString();
	}

	public void ClearInfo()
	{
		mSpr.spriteName = "Null";
		mLbPice.text = string.Empty;
		mLbText.text = string.Empty;
		mCollider.enabled = false;
		mCheckBox.isChecked = false;
		mLbDisCount.text = string.Empty;
		mSprDiscount.enabled = false;
		mLbCount.text = string.Empty;
		mData = null;
	}

	private void BtnBuy_OnClick()
	{
		if (this.e_ItemBuy != null)
		{
			this.e_ItemBuy(mIndex, this);
		}
	}

	private void BtnExport_OnClick()
	{
		if (this.e_ItemExport != null)
		{
			this.e_ItemExport(mIndex, this);
		}
	}

	private void MallItem_OnClick()
	{
		if (this.e_OnClick != null)
		{
			this.e_OnClick(mIndex, this);
		}
	}

	private void Update()
	{
		if (UIMallWnd.Instance == null)
		{
			return;
		}
		if (UIMallWnd.Instance.mCurrentTab == Mall_Tab.tab_Item || UIMallWnd.Instance.mCurrentTab == Mall_Tab.tab_Equip)
		{
			mBtnBuy.SetActive(value: false);
			mBtnExport.SetActive(PeSingleton<PeFlowMgr>.Instance.curScene == PeFlowMgr.EPeScene.GameScene);
		}
		else
		{
			mBtnBuy.SetActive(value: true);
			mBtnExport.SetActive(value: false);
		}
		if (UIMallWnd.Instance.mCurrentTab == Mall_Tab.tab_Hot)
		{
			if (mData != null)
			{
				mSprDiscount.enabled = mData.ShowDiscount();
				mLbDisCount.enabled = mData.ShowDiscount();
				mLbDisCount.text = mData.GetDiscount() + "%";
			}
			else
			{
				mSprDiscount.enabled = false;
				mLbDisCount.text = string.Empty;
			}
		}
		else
		{
			mSprDiscount.enabled = false;
			mLbDisCount.text = string.Empty;
		}
		if (mData != null)
		{
			mLbPice.enabled = mData.GetPrice() != -1;
			return;
		}
		mBtnBuy.SetActive(value: false);
		mLbPice.text = string.Empty;
	}
}
