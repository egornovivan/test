using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

namespace Pathea;

public class NpcEatDb
{
	public class Item
	{
		public int _sort;

		public int _typeID;

		public float _eatmin;

		public float _eatmax;

		public float _attrper;

		public int _ProtoID;

		public ItemAttr _ItemAtrr;

		public void Init()
		{
			_ItemAtrr = new ItemAttr(_attrper, _ProtoID);
		}
	}

	public class ItemAttr
	{
		public float _percent;

		public int _ProtoID;

		public ItemAttr(float percent, int id)
		{
			_percent = percent;
			_ProtoID = id;
		}
	}

	public class AttrPer
	{
		public int mTypeId;

		public float mCurPercent;

		public AttrPer(int typeid, float percent)
		{
			mTypeId = typeid;
			mCurPercent = percent;
		}
	}

	public class Items
	{
		public int mTypeId;

		public float mEatMin;

		public float mEatMax;

		private float mMaxPer;

		private float mMinPer = 1f;

		private List<Item> mLists;

		public void AddItem(Item item)
		{
			if (mLists == null)
			{
				mLists = new List<Item>();
			}
			mTypeId = item._typeID;
			mEatMin = item._eatmin;
			mEatMax = item._eatmax;
			if (item._attrper > mMaxPer)
			{
				mMaxPer = item._attrper;
			}
			if (item._attrper < mMinPer)
			{
				mMinPer = item._attrper;
			}
			mLists.Add(item);
		}

		public List<Item> GetItems()
		{
			return mLists;
		}

		private List<ItemAttr> GetItemAtts()
		{
			List<ItemAttr> list = new List<ItemAttr>();
			for (int i = 0; i < mLists.Count; i++)
			{
				list.Add(mLists[i]._ItemAtrr);
			}
			return list;
		}

		public int[] GetProtoIdRange(float curpercent)
		{
			List<ItemAttr> itemAtts = GetItemAtts();
			List<ItemAttr> list = new List<ItemAttr>();
			List<int> list2 = new List<int>();
			if (itemAtts != null && itemAtts.Count > 1)
			{
				for (int i = 0; i < itemAtts.Count; i++)
				{
					ItemAttr item = new ItemAttr(Math.Abs(itemAtts[i]._percent - curpercent), itemAtts[i]._ProtoID);
					list.Add(item);
				}
				list.Sort((ItemAttr a, ItemAttr b) => a._percent.CompareTo(b._percent));
				for (int j = 0; j < list.Count; j++)
				{
					list2.Add(list[j]._ProtoID);
				}
			}
			return list2.ToArray();
		}

		public int[] GetProtoItemIds()
		{
			List<int> list = new List<int>();
			for (int i = 0; i < mLists.Count; i++)
			{
				list.Add(mLists[i]._ProtoID);
			}
			return list.ToArray();
		}
	}

	private static Dictionary<int, Items> mData;

	public static void LoadData()
	{
		mData = new Dictionary<int, Items>();
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("NpcEatList");
		while (sqliteDataReader.Read())
		{
			Item item = new Item();
			item._sort = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("sort"));
			item._typeID = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("attrID"));
			item._eatmin = sqliteDataReader.GetFloat(sqliteDataReader.GetOrdinal("eatmin"));
			item._eatmax = sqliteDataReader.GetFloat(sqliteDataReader.GetOrdinal("eatmax"));
			item._attrper = sqliteDataReader.GetFloat(sqliteDataReader.GetOrdinal("attrper"));
			item._ProtoID = sqliteDataReader.GetInt32(sqliteDataReader.GetOrdinal("PrototypeItem_ID"));
			item.Init();
			if (!mData.ContainsKey(item._typeID))
			{
				mData.Add(item._typeID, new Items());
			}
			mData[item._typeID].AddItem(item);
		}
	}

	public static void Release()
	{
		mData = null;
	}

	public static Items GetIitems(int Type)
	{
		return mData[Type];
	}

	public static int[] GetEatIDs(int TypeId, float curPercent)
	{
		return GetIitems(TypeId)?.GetProtoIdRange(curPercent);
	}

	public static int[] GetEatIDs(EEatType type)
	{
		return GetEatIDs((int)type);
	}

	public static int[] GetEatIDs(int TypeId)
	{
		return GetIitems(TypeId)?.GetProtoItemIds();
	}
}
