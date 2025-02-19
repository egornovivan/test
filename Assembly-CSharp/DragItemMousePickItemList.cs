using System.Linq;
using ItemAsset;
using Pathea;

public class DragItemMousePickItemList : DragItemMousePick
{
	private ItemScript_ItemList mItemList;

	private PlayerPackageCmpt mPkgCmpt;

	private ItemScript_ItemList itemList
	{
		get
		{
			if (mItemList == null)
			{
				mItemList = GetComponent<ItemScript_ItemList>();
			}
			return mItemList;
		}
	}

	protected PlayerPackageCmpt pkgCmpt
	{
		get
		{
			if (null == mPkgCmpt)
			{
				mPkgCmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
			}
			return mPkgCmpt;
		}
	}

	public override void DoGetItem()
	{
		MaterialItem[] items = itemList.GetItems();
		if (base.pkg == null || !base.pkg.CanAdd(items))
		{
			PeTipMsg.Register(PELocalization.GetString(9500312), PeTipMsg.EMsgLevel.Warning);
			return;
		}
		if (!GameConfig.IsMultiMode)
		{
			for (int i = 0; i < items.Length; i++)
			{
				pkgCmpt.Add(items[i].protoId, items[i].count);
			}
			PeSingleton<ItemMgr>.Instance.DestroyItem(base.itemObjectId);
			DragItemAgent byId = DragItemAgent.GetById(base.id);
			if (byId != null)
			{
				DragItemAgent.Destory(byId);
			}
			GameUI.Instance.mItemPackageCtrl.ResetItem();
		}
		else if (null != PlayerNetwork.mainPlayer)
		{
			ItemSample[] items2 = items.Select((MaterialItem iter) => new ItemSample(iter.protoId, iter.count)).ToArray();
			PlayerNetwork.mainPlayer.RequestGetItemListBack(base.itemObjectId, items2);
		}
		GameUI.Instance.mItemOp.Hide();
	}
}
