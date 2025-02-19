using ItemAsset;
using UnityEngine;

public class SelItemGrid_N : MonoBehaviour
{
	public UISprite mSelectedMask;

	public UISprite mItemIcon;

	[HideInInspector]
	public ItemProto mItemData;

	public string mNpcPath;

	public void SetItem(ItemProto itemdata)
	{
		mItemData = itemdata;
		mItemIcon.spriteName = mItemData.icon[0];
		mItemIcon.MakePixelPerfect();
	}

	public void SetNpc(string path, string icon)
	{
		mNpcPath = path;
		mItemIcon.spriteName = icon;
	}

	public void OnSelected(bool selected)
	{
		mSelectedMask.enabled = selected;
	}

	private void OnDrag(Vector2 delta)
	{
		if (delta.sqrMagnitude > float.Epsilon)
		{
			if (mItemData != null)
			{
				BuildingGui_N.Instance.OnItemPutOut(this);
			}
			else
			{
				BuildingGui_N.Instance.OnNpcPutOut(this);
			}
		}
	}
}
