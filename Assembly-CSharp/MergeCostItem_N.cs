using ItemAsset;
using Pathea;
using UnityEngine;

public class MergeCostItem_N : MonoBehaviour
{
	public Grid_N mGrid;

	public UILabel mNumCost;

	private int mCosPerItem;

	public void SetItem(ItemSample itemsp, int cosPerItem)
	{
		mGrid.SetItem(itemsp);
		mCosPerItem = cosPerItem;
	}

	public void UpdateNum(int num)
	{
		if (num < 1)
		{
			num = 1;
		}
		if (mGrid.Item != null)
		{
			mNumCost.text = (mCosPerItem * num).ToString();
			PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
			HaveEnoughItem(cmpt.package.GetCount(mGrid.Item.protoId) >= mCosPerItem * num);
		}
		else
		{
			mNumCost.text = string.Empty;
		}
	}

	public void HaveEnoughItem(bool enough)
	{
		if (enough)
		{
			mNumCost.color = Color.white;
		}
		else
		{
			mNumCost.color = Color.red;
		}
	}
}
