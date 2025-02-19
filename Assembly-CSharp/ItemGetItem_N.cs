using ItemAsset;
using UnityEngine;

public class ItemGetItem_N : MonoBehaviour
{
	public delegate void EGetItem(ItemGetItem_N item);

	public UILabel mName;

	public Grid_N mGrid;

	public event EGetItem e_GetItem;

	public void SetItem(ItemSample itemSample, int index)
	{
		mGrid.SetItem(itemSample);
		mGrid.SetItemPlace(ItemPlaceType.IPT_ItemGet, index);
		if (itemSample != null)
		{
			mName.text = itemSample.protoData.GetName() + "[30FF30] x " + itemSample.stackCount + "[-]";
		}
		else
		{
			mName.text = string.Empty;
		}
	}

	private void OnGetItem()
	{
		if (this.e_GetItem != null)
		{
			this.e_GetItem(this);
		}
	}
}
