using ItemAsset;

public interface IItemDrop
{
	ItemSample Get(int index);

	int GetCount();

	void Fetch(int index);

	void FetchAll();
}
