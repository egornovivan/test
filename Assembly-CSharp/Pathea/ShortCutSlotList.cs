using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ItemAsset;
using ItemAsset.PackageHelper;
using PETools;

namespace Pathea;

public class ShortCutSlotList : IEnumerable, IEnumerable<ShortCutItem>
{
	private ItemPackage mPkg;

	private EquipmentCmpt mEquip;

	private ShortCutItem[] mShortCutItems;

	public int length => mShortCutItems.Length;

	public event Action onListUpdate;

	public ShortCutSlotList(int count, ItemPackage pkg, EquipmentCmpt equip)
	{
		mShortCutItems = new ShortCutItem[count];
		mPkg = pkg;
		mEquip = equip;
	}

	IEnumerator<ShortCutItem> IEnumerable<ShortCutItem>.GetEnumerator()
	{
		return mShortCutItems.AsEnumerable().GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return mShortCutItems.GetEnumerator();
	}

	public ItemObject GetItemObj(int index)
	{
		ShortCutItem item = GetItem(index);
		if (item == null)
		{
			return null;
		}
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(item.itemInstanceId);
		if (itemObject == null)
		{
			if (item.UseProtoID && mPkg != null)
			{
				return mPkg.GetItemByProtoID(item.protoId);
			}
			return null;
		}
		if (mPkg != null)
		{
			SlotList slotList = mPkg.GetSlotList(item.protoId);
			if (slotList != null && slotList.HasItem(item.itemInstanceId))
			{
				return itemObject;
			}
		}
		if (null != mEquip)
		{
			for (int i = 0; i < mEquip._ItemList.Count; i++)
			{
				if (mEquip._ItemList[i].instanceId == item.itemInstanceId)
				{
					return mEquip._ItemList[i];
				}
			}
		}
		return null;
	}

	public ShortCutItem GetItem(int index)
	{
		if (index >= mShortCutItems.Length || index < 0)
		{
			return null;
		}
		return mShortCutItems[index];
	}

	public void PutItemObj(ItemObject itemObj, int index)
	{
		if (itemObj != null)
		{
			ShortCutItem shortCutItem = new ShortCutItem();
			shortCutItem.protoId = itemObj.protoId;
			shortCutItem.itemInstanceId = itemObj.instanceId;
			ShortCutItem item = shortCutItem;
			PutItem(item, index);
		}
	}

	public void PutItem(ShortCutItem item, int index, bool updateList = true)
	{
		if (index < mShortCutItems.Length && index >= 0)
		{
			mShortCutItems[index] = item;
			if (updateList)
			{
				UpdateShortCut();
			}
		}
	}

	public void UpdateShortCut()
	{
		if (mPkg == null)
		{
			return;
		}
		for (int i = 0; i < mShortCutItems.Length; i++)
		{
			ShortCutItem shortCutItem = mShortCutItems[i];
			if (shortCutItem == null || shortCutItem.protoData == null)
			{
				continue;
			}
			if (shortCutItem.protoData.maxStackNum > 1 && 60 != shortCutItem.protoId)
			{
				int count = mPkg.GetCount(shortCutItem.protoId);
				if (count == 0)
				{
					PutItem(null, i, updateList: false);
				}
				else
				{
					shortCutItem.SetStackCount(count);
				}
				continue;
			}
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(shortCutItem.itemInstanceId);
			if (itemObject == null)
			{
				PutItem(null, i, updateList: false);
				continue;
			}
			if (itemObject.protoData.maxStackNum > 1)
			{
				shortCutItem.SetStackCount(itemObject.GetCount());
			}
			CheckEquipInPackage(shortCutItem, i);
		}
		SendUpdateMsg();
	}

	public void UpdateShortCut(ItemObject itemObj)
	{
		if (mPkg == null || itemObj.protoData.maxStackNum <= 1)
		{
			return;
		}
		for (int i = 0; i < mShortCutItems.Length; i++)
		{
			ShortCutItem shortCutItem = mShortCutItems[i];
			if (shortCutItem != null && shortCutItem.protoId == itemObj.protoId)
			{
				if (shortCutItem.protoId == 60)
				{
					shortCutItem.SetStackCount(itemObj.GetCount());
				}
				else
				{
					shortCutItem.SetStackCount(mPkg.GetCount(shortCutItem.protoId));
				}
				break;
			}
		}
	}

	private void CheckEquipInPackage(ShortCutItem item, int index)
	{
		if (mPkg != null)
		{
			SlotList slotList = mPkg.GetSlotList(item.protoId);
			if (slotList != null && slotList.HasItem(item.itemInstanceId))
			{
				return;
			}
		}
		if (!(null != mEquip) || !mEquip.IsEquipNow(item.itemInstanceId))
		{
			PutItem(null, index, updateList: false);
		}
	}

	private void SendUpdateMsg()
	{
		if (this.onListUpdate != null)
		{
			this.onListUpdate();
		}
	}

	public void Export(BinaryWriter w)
	{
		w.Write(mShortCutItems.Length);
		for (int i = 0; i < mShortCutItems.Length; i++)
		{
			if (mShortCutItems[i] != null)
			{
				Serialize.WriteData(mShortCutItems[i].Export, w);
			}
			else
			{
				Serialize.WriteData(null, w);
			}
		}
	}

	public void Import(byte[] buff)
	{
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			int num = r.ReadInt32();
			mShortCutItems = new ShortCutItem[num];
			for (int i = 0; i < num; i++)
			{
				byte[] array = Serialize.ReadBytes(r);
				if (array == null)
				{
					mShortCutItems[i] = null;
				}
				else
				{
					mShortCutItems[i] = new ShortCutItem();
					mShortCutItems[i].Import(array);
					if (mShortCutItems[i].protoData == null)
					{
						mShortCutItems[i] = null;
					}
				}
			}
			SendUpdateMsg();
		});
	}
}
