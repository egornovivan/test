using ItemAsset;

public interface IDroppableItemList
{
	int DroppableItemCount { get; }

	ItemSample GetDroppableItemAt(int idx);

	void AddDroppableItem(ItemSample item);

	void RemoveDroppableItem(ItemSample item);

	void RemoveDroppableItemAll();
}
