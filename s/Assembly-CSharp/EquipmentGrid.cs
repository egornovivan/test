using ItemAsset;

public class EquipmentGrid
{
	private ItemObject _itemObj;

	private int _gridMask = -1;

	public ItemObject _ItemObj => _itemObj;

	public int _GridMask => _gridMask;

	public ItemObject SetItem(ItemObject itemObj)
	{
		ItemObject itemObj2 = _itemObj;
		_itemObj = itemObj;
		return itemObj2;
	}

	public void SetGridMask(int mask)
	{
		_gridMask = mask;
	}
}
