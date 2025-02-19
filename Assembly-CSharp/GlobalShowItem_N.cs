using ItemAsset;
using UnityEngine;

public class GlobalShowItem_N : MonoBehaviour
{
	public UILabel mItemName;

	public Grid_N mItem;

	private float mUpspeed = 80f;

	private float mLifeTime = 3f;

	private float mLifElapse;

	private Vector3 mPos;

	private void Update()
	{
		mPos += mUpspeed * Vector3.up * Time.deltaTime;
		base.transform.localPosition = new Vector3(Mathf.Round(mPos.x), Mathf.Round(mPos.y), Mathf.Round(mPos.z));
		mLifElapse += Time.deltaTime;
		GetComponent<UIPanelAlpha>().alpha = 1f - mLifElapse / mLifeTime;
		if (mLifElapse > mLifeTime)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void InitItem(ItemSample itemGrid)
	{
		if (itemGrid != null)
		{
			mItem.SetItem(itemGrid);
			if (itemGrid.protoData.maxStackNum > 1)
			{
				mItemName.text = itemGrid.protoData.GetName() + "+" + itemGrid.GetCount();
			}
			else
			{
				mItemName.text = itemGrid.protoData.GetName();
			}
			mPos = base.transform.localPosition;
		}
	}

	public void InitItem(string str)
	{
		if (!(str == string.Empty))
		{
			mItem.SetItem(null);
			mItem.gameObject.SetActive(value: false);
			mItemName.pivot = UIWidget.Pivot.Center;
			mItemName.text = str;
			mPos = base.transform.localPosition;
		}
	}
}
