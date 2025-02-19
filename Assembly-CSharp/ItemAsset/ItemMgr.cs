using System;
using System.Collections.Generic;
using System.IO;
using Pathea;
using PETools;

namespace ItemAsset;

public class ItemMgr : MonoLikeSingleton<ItemMgr>
{
	private IdGenerator mIdGenerator = new IdGenerator(0, 0, 100000000);

	private Dictionary<int, ItemObject> mItems = new Dictionary<int, ItemObject>(100);

	public Action<int> DestoryItemEvent;

	public bool Add(ItemObject item)
	{
		if (item == null)
		{
			return false;
		}
		if (mItems.ContainsKey(item.instanceId))
		{
			return false;
		}
		mItems.Add(item.instanceId, item);
		return true;
	}

	public ItemObject Get(int id)
	{
		if (mItems.ContainsKey(id))
		{
			return mItems[id];
		}
		return null;
	}

	public bool DestroyItem(int id)
	{
		ItemObject itemObject = Get(id);
		if (itemObject == null)
		{
			return false;
		}
		ExecDestoryItemEvent(id);
		return mItems.Remove(id);
	}

	public bool DestroyItem(ItemObject item)
	{
		if (item == null)
		{
			return false;
		}
		ExecDestoryItemEvent(item.instanceId);
		return mItems.Remove(item.instanceId);
	}

	public static bool IsCreationItem(int protoId)
	{
		if (protoId < 100000000)
		{
			return false;
		}
		return true;
	}

	public ItemObject CreateItem(int prototypeId)
	{
		int num = 0;
		num = (IsCreationItem(prototypeId) ? prototypeId : mIdGenerator.Fetch());
		ItemObject itemObject = ItemObject.Create(prototypeId, num);
		Add(itemObject);
		return itemObject;
	}

	public void Export(BinaryWriter w)
	{
		Serialize.WriteData(mIdGenerator.Export, w);
		w.Write(mItems.Count);
		foreach (ItemObject value in mItems.Values)
		{
			if (value != null)
			{
				Serialize.WriteData(value.Serialize, w);
			}
			else
			{
				Serialize.WriteData(null, w);
			}
		}
		w.Write(KillNPC.ashBox_inScene);
	}

	public void Import(byte[] buffer)
	{
		Serialize.Import(buffer, delegate(BinaryReader r)
		{
			mIdGenerator.Import(Serialize.ReadBytes(r));
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				ItemObject item = ItemObject.Deserialize(Serialize.ReadBytes(r));
				Add(item);
			}
			if (PeSingleton<ArchiveMgr>.Instance.GetCurArvhiveVersion() > 2)
			{
				KillNPC.ashBox_inScene = r.ReadInt32();
			}
		});
	}

	private void Clear()
	{
		mIdGenerator.Cur = 0;
		mItems.Clear();
	}

	private void ExecDestoryItemEvent(int instanceId)
	{
		if (DestoryItemEvent != null)
		{
			DestoryItemEvent(instanceId);
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		Clear();
	}
}
