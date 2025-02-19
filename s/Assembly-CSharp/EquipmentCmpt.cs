using System.Collections.Generic;
using System.IO;
using System.Linq;
using ItemAsset;

public class EquipmentCmpt : DataCmpt
{
	protected List<ItemObject> mEquips = new List<ItemObject>();

	public int EquipCount => mEquips.Count;

	public int[] EquipIds => mEquips.Select((ItemObject iter) => iter.instanceId).ToArray();

	public ItemObject this[int id] => mEquips.Find((ItemObject iter) => iter.instanceId == id);

	public ItemObject[] EquipItems => mEquips.ToArray();

	public EquipmentCmpt()
	{
		mType = ECmptType.Equipment;
	}

	public override void Export(BinaryWriter w)
	{
		base.Export(w);
		BufferHelper.Serialize(w, mEquips.Count);
		foreach (ItemObject mEquip in mEquips)
		{
			byte[] value = mEquip.Export();
			BufferHelper.Serialize(w, value);
		}
	}

	public override void Import(BinaryReader r)
	{
		base.Import(r);
		int num = BufferHelper.ReadInt32(r);
		for (int i = 0; i < num; i++)
		{
			byte[] array = BufferHelper.ReadBytes(r);
			if (array != null && array.Length > 0)
			{
				ItemObject itemObject = ItemObject.Deserialize(array);
				if (itemObject != null)
				{
					mEquips.Add(itemObject);
				}
			}
		}
	}

	public void Clear()
	{
		mEquips.Clear();
	}

	public int FindEffectEquipCount(int equipPos)
	{
		return mEquips.Count((ItemObject iter) => (iter.protoData.equipPos & equipPos) != 0);
	}

	public int FindEffectEquipCount(ItemObject equip)
	{
		if (equip == null || equip.protoData == null)
		{
			return 0;
		}
		return FindEffectEquipCount(equip.protoData.equipPos);
	}

	public bool HasEquip(int id)
	{
		return mEquips.Exists((ItemObject iter) => iter.instanceId == id);
	}

	public bool HasEquip(ItemObject item)
	{
		return item != null && mEquips.Contains(item);
	}

	public bool Remove(ItemObject equip)
	{
		return equip != null && mEquips.Contains(equip) && mEquips.Remove(equip);
	}

	public bool Add(ItemObject equip)
	{
		if (equip == null)
		{
			return false;
		}
		if (mEquips.Contains(equip))
		{
			return false;
		}
		mEquips.Add(equip);
		return true;
	}

	public ItemObject[] GetNotBindEquips()
	{
		return mEquips.Where((ItemObject iter) => !iter.bind).ToArray();
	}

	public IEnumerable<ItemObject> GetEffectEquips(int equipPos)
	{
		List<ItemObject> list = new List<ItemObject>();
		foreach (ItemObject mEquip in mEquips)
		{
			if (mEquip != null && mEquip.protoData != null && (mEquip.protoData.equipPos & equipPos) != 0)
			{
				list.Add(mEquip);
			}
		}
		return list;
	}

	public bool PutOnEquip(ItemObject equip)
	{
		if (equip == null || equip.protoData == null || mEquips.Contains(equip))
		{
			return false;
		}
		IEnumerable<ItemObject> effectEquips = GetEffectEquips(equip.protoData.equipPos);
		foreach (ItemObject item in effectEquips)
		{
			mEquips.Remove(item);
		}
		mEquips.Add(equip);
		return true;
	}

	public bool PutOnEquip(ItemObject equip, ref List<ItemObject> effEquips)
	{
		if ((equip == null && equip.protoData == null) || mEquips.Contains(equip))
		{
			return false;
		}
		IEnumerable<ItemObject> effectEquips = GetEffectEquips(equip.protoData.equipPos);
		foreach (ItemObject item in effectEquips)
		{
			mEquips.Remove(item);
			effEquips.Add(item);
		}
		mEquips.Add(equip);
		return true;
	}

	public bool TakeOffEquip(ItemObject equip, ref List<ItemObject> effEquips)
	{
		if ((equip == null && equip.protoData == null) || !mEquips.Contains(equip))
		{
			return false;
		}
		IEnumerable<ItemObject> effectEquips = GetEffectEquips(equip.protoData.equipPos);
		foreach (ItemObject item in effectEquips)
		{
			mEquips.Remove(item);
			effEquips.Add(item);
		}
		return true;
	}

	public bool TakeOffEquip(ItemObject equip)
	{
		if ((equip == null && equip.protoData == null) || !mEquips.Contains(equip))
		{
			return false;
		}
		IEnumerable<ItemObject> effectEquips = GetEffectEquips(equip.protoData.equipPos);
		foreach (ItemObject item in effectEquips)
		{
			mEquips.Remove(item);
		}
		return true;
	}
}
