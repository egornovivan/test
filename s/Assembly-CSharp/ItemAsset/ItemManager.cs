using System.Collections.Generic;
using Mono.Data.SqliteClient;
using Pathea;

namespace ItemAsset;

public class ItemManager
{
	private static Dictionary<int, ItemObject> mItemMap = new Dictionary<int, ItemObject>();

	private static List<ItemObject> mNewItems = new List<ItemObject>();

	private static List<ItemObject> mUpdateItems = new List<ItemObject>();

	private static List<int> mDeleteItems = new List<int>();

	internal static ItemObject GetItemByID(int objectId)
	{
		if (mItemMap.ContainsKey(objectId))
		{
			return mItemMap[objectId];
		}
		return null;
	}

	internal static List<ItemObject> GetItemByProtoId(int protoId)
	{
		List<ItemObject> list = new List<ItemObject>();
		foreach (KeyValuePair<int, ItemObject> item in mItemMap)
		{
			if (item.Value.protoId == protoId)
			{
				list.Add(item.Value);
			}
		}
		return list;
	}

	public static void AddItem(ItemObject obj)
	{
		AddItem(obj, newItem: true);
	}

	public static void AddItem(ItemObject obj, bool newItem)
	{
		mItemMap[obj.instanceId] = obj;
		if (obj.instanceId > IdGenerator.CurItemId && obj.instanceId < 100000000)
		{
			IdGenerator.InitCurItemId(obj.instanceId);
		}
		if (newItem)
		{
			AddNewItem(obj);
		}
	}

	public static void AddNewItem(ItemObject item)
	{
		if (item != null && !mNewItems.Contains(item))
		{
			mNewItems.Add(item);
		}
	}

	internal static void AddUpdateItem(ItemObject item)
	{
		if (item != null && !mUpdateItems.Contains(item))
		{
			mUpdateItems.Add(item);
		}
	}

	public static void LoadItems()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		pEDbOp.SetCmdText("SELECT * FROM itemobject;");
		pEDbOp.BindReaderHandler(LoadComplete);
		pEDbOp.Exec();
		pEDbOp = null;
	}

	private static void LoadComplete(SqliteDataReader reader)
	{
		while (reader.Read())
		{
			byte[] buff = (byte[])reader.GetValue(reader.GetOrdinal("data"));
			ItemObject.Deserialize(buff);
		}
	}

	private static void SaveNewItems()
	{
		ItemMgrDbData itemMgrDbData = new ItemMgrDbData();
		itemMgrDbData.ExportData(mNewItems);
		AsyncSqlite.AddRecord(itemMgrDbData);
		mNewItems.Clear();
	}

	private static void SaveDeleteItems()
	{
		ItemMgrDbData itemMgrDbData = new ItemMgrDbData();
		itemMgrDbData.DeleteData(mDeleteItems);
		AsyncSqlite.AddRecord(itemMgrDbData);
		mDeleteItems.Clear();
	}

	private static void SaveUpdataItems()
	{
		ItemMgrDbData itemMgrDbData = new ItemMgrDbData();
		itemMgrDbData.UpdateData(mUpdateItems);
		AsyncSqlite.AddRecord(itemMgrDbData);
		foreach (ItemObject mUpdateItem in mUpdateItems)
		{
			mUpdateItem.ResetChangeFlag();
		}
		mUpdateItems.Clear();
	}

	public static void SaveItems()
	{
		if (mNewItems.Count >= 1)
		{
			SaveNewItems();
		}
		if (mUpdateItems.Count >= 1)
		{
			SaveUpdataItems();
		}
		if (mDeleteItems.Count >= 1)
		{
			SaveDeleteItems();
		}
		SceneObjMgr.SaveSceneIds();
	}

	internal static void CreateItems(int[] ids, ref List<ItemObject> effItems)
	{
		if (ids.Length <= 0)
		{
			return;
		}
		foreach (int num in ids)
		{
			if (num > 0)
			{
				ItemObject itemObject = CreateItem(num, 1);
				if (itemObject != null)
				{
					effItems.Add(itemObject);
				}
			}
		}
	}

	internal static void CreateItems(List<ItemSample> items, ref List<ItemObject> effItems)
	{
		if (items.Count <= 0)
		{
			return;
		}
		foreach (ItemSample item in items)
		{
			if (item != null)
			{
				ItemObject itemObject = CreateItem(item.protoId, item.stackCount);
				if (itemObject != null)
				{
					effItems.Add(itemObject);
				}
			}
		}
	}

	internal static bool ExistID(int objID)
	{
		return mItemMap.ContainsKey(objID);
	}

	public static void RemoveItem(int objId)
	{
		if (!mDeleteItems.Contains(objId))
		{
			mDeleteItems.Add(objId);
		}
		mNewItems.RemoveAll((ItemObject iter) => iter.instanceId == objId);
		mUpdateItems.RemoveAll((ItemObject iter) => iter.instanceId == objId);
		mItemMap.Remove(objId);
		SceneObjMgr.RemoveItem(objId);
		SteamWorks.RemoveCreationData(objId);
	}

	private static ItemObject CreateItem(int itemId)
	{
		int instanceId = ((itemId < 100000000) ? IdGenerator.NewItemId : itemId);
		ItemObject itemObject = ItemObject.Create(itemId, instanceId);
		if (itemObject == null)
		{
			return null;
		}
		AddItem(itemObject);
		return itemObject;
	}

	public static ItemObject CreateItem(int itemId, int num)
	{
		ItemObject itemObject = CreateItem(itemId);
		if (itemObject == null)
		{
			return null;
		}
		itemObject.SetStackCount(num);
		return itemObject;
	}

	public static void CreateItems(int itemId, int num, ref List<ItemObject> effItems)
	{
		ItemProto itemData = ItemProto.GetItemData(itemId);
		if (itemData == null)
		{
			return;
		}
		int num2 = num / itemData.maxStackNum;
		for (int i = 0; i < num2; i++)
		{
			ItemObject itemObject = CreateItem(itemId, itemData.maxStackNum);
			if (itemObject != null)
			{
				effItems.Add(itemObject);
			}
		}
		int num3 = num % itemData.maxStackNum;
		if (num3 != 0)
		{
			ItemObject itemObject2 = CreateItem(itemId, num3);
			if (itemObject2 != null)
			{
				effItems.Add(itemObject2);
			}
		}
	}

	public static ItemObject CreateFromItem(int itemId, int num, ItemObject item)
	{
		ItemObject itemObject = CreateItem(itemId);
		if (itemObject == null)
		{
			return null;
		}
		itemObject.CreateFromItem(item);
		itemObject.SetStackCount(num);
		return itemObject;
	}

	public static void CreateFromItemList(int itemId, int num, ItemObject item, ref List<ItemObject> effItems)
	{
		ItemProto itemData = ItemProto.GetItemData(itemId);
		if (itemData == null)
		{
			return;
		}
		int num2 = num / itemData.maxStackNum;
		for (int i = 0; i < num2; i++)
		{
			ItemObject itemObject = CreateItem(itemId);
			if (itemObject != null)
			{
				itemObject.CreateFromItem(item);
				itemObject.SetStackCount(itemData.maxStackNum);
				effItems.Add(itemObject);
			}
		}
		int num3 = num % itemData.maxStackNum;
		if (num3 != 0)
		{
			ItemObject itemObject2 = CreateItem(itemId);
			if (itemObject2 != null)
			{
				itemObject2.CreateFromItem(item);
				itemObject2.SetStackCount(num3);
				effItems.Add(itemObject2);
			}
		}
	}

	public static int CreateObjectID()
	{
		int num = -1;
		if (mItemMap.ContainsKey(num = IdGenerator.NewItemId + 100000000))
		{
		}
		return num;
	}
}
