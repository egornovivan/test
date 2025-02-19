public class ItemSpwanPoint : SpawnPoint
{
	public int ItemObjId;

	public bool CanPickup;

	public bool isNew = true;

	public ISceneObject m_Agent;

	public ItemSpwanPoint()
	{
	}

	public ItemSpwanPoint(WEItem obj)
		: base(obj)
	{
		ItemObjId = -1;
		CanPickup = obj.CanPickup;
	}

	public ItemSpwanPoint(EffectSpwanPoint sp)
		: base(sp)
	{
	}
}
