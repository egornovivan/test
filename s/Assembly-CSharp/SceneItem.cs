using ItemAsset;

public class SceneItem : SceneObject
{
	protected ItemObject _item;

	public ItemObject Item => _item;

	public SceneItem()
	{
		_type = ESceneObjType.ITEM;
	}

	public void SetItem(ItemObject item)
	{
		_item = item;
	}
}
