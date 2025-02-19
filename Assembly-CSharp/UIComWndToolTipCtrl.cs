using ItemAsset;
using Pathea;
using UnityEngine;

public class UIComWndToolTipCtrl : MonoBehaviour
{
	private ItemSample mItemSample;

	private int mItemId;

	private ListItemType mType;

	private ItemObject _itemObj;

	public void SetToolTipInfo(ListItemType type, int id)
	{
		mType = type;
		mItemId = id;
	}

	public int GetItemID()
	{
		return mItemId;
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
