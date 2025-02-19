using ItemAsset;
using Pathea;
using UnityEngine;

public class UIGridItemCtrl : MonoBehaviour
{
	public delegate void ChickFunc(int index);

	public UISprite[] mContentSprites;

	public UITexture mContentTexture;

	public UILabel mTextCount;

	public BoxCollider mBoxCollider;

	public UIAtlas mAtlasButton;

	public UIAtlas mAtlasIcon;

	public int mIndex = -1;

	public int mItemId;

	private ListItemType mType;

	private ItemSample mItemSample;

	private ItemObject _itemObj;

	public event ChickFunc mItemClick;

	public void SetToolTipInfo(ListItemType type, int id)
	{
		mType = type;
		mItemId = id;
	}

	public void SetTextCount(int count)
	{
		if (!(mTextCount == null))
		{
			mTextCount.text = count.ToString();
		}
	}

	public void SetCotent(string[] ico)
	{
		if (mContentSprites == null || mContentSprites.Length <= 0)
		{
			return;
		}
		for (int i = 0; i < ico.Length; i++)
		{
			if (i < mContentSprites.Length)
			{
				if (ico[i] == "0")
				{
					mContentSprites[i].gameObject.SetActive(value: false);
					continue;
				}
				mContentSprites[i].spriteName = ico[i];
				mContentSprites[i].gameObject.SetActive(value: true);
			}
		}
		if (mContentTexture != null)
		{
			mContentTexture.gameObject.SetActive(value: false);
		}
	}

	public void SetCotent(Texture _contentTexture)
	{
		if (mContentTexture == null)
		{
			return;
		}
		mContentTexture.mainTexture = _contentTexture;
		mContentTexture.gameObject.SetActive(value: true);
		if (mContentSprites == null || mContentSprites.Length <= 0)
		{
			return;
		}
		for (int i = 0; i < mContentSprites.Length; i++)
		{
			if (mContentSprites[i] != null)
			{
				mContentSprites[i].gameObject.SetActive(value: false);
			}
		}
	}

	private void OnClickItem()
	{
		if (UICamera.currentTouchID == -1 && this.mItemClick != null)
		{
			this.mItemClick(mIndex);
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
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
