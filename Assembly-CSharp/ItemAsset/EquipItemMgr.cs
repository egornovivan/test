using System.Collections.Generic;

namespace ItemAsset;

public class EquipItemMgr
{
	private List<ItemObject>[] mItemsDic;

	private ItemObject[] mEquipObjs;

	private List<ItemObject>[] mAtkItems;

	public EquipItemMgr()
	{
		Init();
	}

	private void Init()
	{
		mItemsDic = new List<ItemObject>[6];
		mItemsDic[1] = new List<ItemObject>();
		mItemsDic[2] = new List<ItemObject>();
		mItemsDic[3] = new List<ItemObject>();
		mItemsDic[4] = new List<ItemObject>();
		mItemsDic[5] = new List<ItemObject>();
		mAtkItems = new List<ItemObject>[2];
		mAtkItems[0] = new List<ItemObject>();
		mAtkItems[1] = new List<ItemObject>();
	}

	public bool Add(ItemObject obj)
	{
		if (obj == null)
		{
			return false;
		}
		EquipInfo equipInfo = SelectItem.SwichEquipInfo(obj);
		if (equipInfo != null)
		{
			mItemsDic[(int)equipInfo._selectType].Add(obj);
			if (obj.protoData.weaponInfo != null && !mAtkItems[(int)obj.protoData.weaponInfo.attackModes[0].type].Contains(obj))
			{
				mAtkItems[(int)obj.protoData.weaponInfo.attackModes[0].type].Add(obj);
			}
			return true;
		}
		return false;
	}

	public bool ReMove(ItemObject obj)
	{
		if (obj == null)
		{
			return false;
		}
		EquipInfo equipInfo = SelectItem.SwichEquipInfo(obj);
		if (equipInfo != null)
		{
			mItemsDic[(int)equipInfo._selectType].Remove(obj);
			if (obj.protoData.weaponInfo != null && mAtkItems[(int)obj.protoData.weaponInfo.attackModes[0].type].Contains(obj))
			{
				mAtkItems[(int)obj.protoData.weaponInfo.attackModes[0].type].Remove(obj);
			}
			return true;
		}
		return false;
	}

	public bool hasAtkEquip(AttackType type)
	{
		return mAtkItems[(int)type].Count > 0;
	}

	public List<ItemObject> GetAtkEquips(AttackType type)
	{
		return mAtkItems[(int)type];
	}

	public void Clear()
	{
		mItemsDic[1].Clear();
		mItemsDic[2].Clear();
		mItemsDic[3].Clear();
		mItemsDic[4].Clear();
		mItemsDic[5].Clear();
		mAtkItems[0].Clear();
		mAtkItems[1].Clear();
	}

	public List<ItemObject> GetEquipItemObjs(EeqSelect selet)
	{
		if (mItemsDic == null)
		{
			return null;
		}
		return mItemsDic[(int)selet];
	}
}
