using System.Collections.Generic;

namespace ItemAsset;

public class ItemSkillMar
{
	private Dictionary<int, ItemSample> mItems = new Dictionary<int, ItemSample>(100);

	private float mFillcount;

	public float Fillcount
	{
		get
		{
			return mFillcount;
		}
		set
		{
			mFillcount = value;
		}
	}

	public float GetFillcount(int id)
	{
		ItemSample itemSample = Get(id);
		if (itemSample == null)
		{
			return 0f;
		}
		return mFillcount;
	}

	public ItemSample Get(int id)
	{
		if (mItems.ContainsKey(id))
		{
			return mItems[id];
		}
		return null;
	}

	public void AddSkillCD(ItemSample Item)
	{
		if (Item != null && Item.protoData.skillId != 0)
		{
			mItems[Item.protoId] = Item;
		}
	}

	public bool DelateSkillCD(int protoId)
	{
		ItemSample itemSample = Get(protoId);
		if (itemSample == null)
		{
			return false;
		}
		return mItems.Remove(protoId);
	}

	public bool IsInSkillCD(int protoId)
	{
		ItemSample itemSample = Get(protoId);
		if (itemSample == null)
		{
			return false;
		}
		return true;
	}
}
