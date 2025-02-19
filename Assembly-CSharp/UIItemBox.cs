using System.Collections.Generic;

public class UIItemBox : UIBaseWnd
{
	public ItemGridPGBWnd_N mItemPGB;

	private ItemBox mOpBox;

	public ItemBox OpBox => mOpBox;

	private void Start()
	{
		Show();
	}

	protected override void InitWindow()
	{
		base.InitWindow();
		mItemPGB.Init();
		foreach (Grid_N grid in mItemPGB.GridList)
		{
			grid.onRightMouseClicked = OnRightMouseCliked;
		}
	}

	public override void Show()
	{
		base.Show();
		Invoke("Reposition", 0.1f);
	}

	private void Reposition()
	{
		mItemPGB.mGrid.Reposition();
	}

	public void SetItemList(ItemBox itemBox, List<int> items)
	{
		mOpBox = itemBox;
		mItemPGB.SetItemList(items, ItemPlaceType.IPT_ItemBox);
	}

	public void OnRightMouseCliked(Grid_N grid)
	{
		if (grid.ItemObj != null && null != mOpBox)
		{
			mOpBox.SendItemToPlayer(grid.ItemObj);
		}
	}

	private void BtnGetOnClick()
	{
		if (null != mOpBox)
		{
			mOpBox.SendAllItemToPlayer();
		}
		Hide();
	}

	private void Update()
	{
	}
}
