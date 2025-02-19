using System;
using System.Collections.Generic;
using ItemAsset;
using Mono.Data.SqliteClient;
using Pathea.PeEntityExtNpcPackage;
using PETools;

namespace Pathea;

public class NpcEatDb
{
	public class Item
	{
		[DbReader.DbField("sort", false)]
		public int _sort;

		[DbReader.DbField("attrID", false)]
		public int _typeID;

		[DbReader.DbField("eatmin", false)]
		public float _eatmin;

		[DbReader.DbField("eatmax", false)]
		public float _eatmax;

		[DbReader.DbField("attrper", false)]
		public float _attrper;

		[DbReader.DbField("PrototypeItem_ID", false)]
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
			Item item = DbReader.ReadItem<Item>(sqliteDataReader);
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

	public static bool CanEat(PeEntity entity)
	{
		if (entity.UseItem == null)
		{
			return false;
		}
		if (IsWantEat(entity, out var AttrPers))
		{
			for (int i = 0; i < AttrPers.Count; i++)
			{
				int[] eatIDs = GetEatIDs(AttrPers[i].mTypeId, AttrPers[i].mCurPercent);
				ItemObject canEatItemFromPackage = GetCanEatItemFromPackage(entity, eatIDs);
				if (canEatItemFromPackage != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool CanEatByAttr(PeEntity entity, AttribType type, AttribType typeMax, bool bContinue)
	{
		if (entity.UseItem == null)
		{
			return false;
		}
		AttrPer attrPer = WantByType(entity, type, typeMax, bContinue);
		if (attrPer == null)
		{
			return false;
		}
		int[] eatIDs = GetEatIDs(attrPer.mTypeId, attrPer.mCurPercent);
		ItemObject canEatItemFromPackage = GetCanEatItemFromPackage(entity, eatIDs);
		if (canEatItemFromPackage != null)
		{
			return true;
		}
		return false;
	}

	public static bool CanEat(PeEntity entity, out ItemObject item)
	{
		item = null;
		if (entity.UseItem == null)
		{
			return false;
		}
		if (IsWantEat(entity, out var AttrPers))
		{
			for (int i = 0; i < AttrPers.Count; i++)
			{
				int[] eatIDs = GetEatIDs(AttrPers[i].mTypeId, AttrPers[i].mCurPercent);
				item = GetCanEatItemFromPackage(entity, eatIDs);
				if (item != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool CanEatFromStorage(PeEntity entity, CSStorage storage)
	{
		ItemObject itemObject = null;
		if (IsWantEat(entity, out var AttrPers))
		{
			for (int i = 0; i < AttrPers.Count; i++)
			{
				int[] eatIDs = GetEatIDs(AttrPers[i].mTypeId, AttrPers[i].mCurPercent);
				itemObject = GetCanEatItemFromCSStorage(entity, storage, eatIDs);
				if (itemObject != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static List<int> GetWantEatIds(PeEntity entity)
	{
		List<int> list = new List<int>();
		if (IsWantEat(entity, out var AttrPers))
		{
			for (int i = 0; i < AttrPers.Count; i++)
			{
				int[] eatIDs = GetEatIDs(AttrPers[i].mTypeId, AttrPers[i].mCurPercent);
				for (int j = 0; j < eatIDs.Length; j++)
				{
					list.Add(eatIDs[j]);
				}
			}
		}
		return list;
	}

	public static bool CanEatFromStorage(PeEntity entity, CSStorage storage, out ItemObject item)
	{
		item = null;
		if (IsWantEat(entity, out var AttrPers))
		{
			for (int i = 0; i < AttrPers.Count; i++)
			{
				int[] eatIDs = GetEatIDs(AttrPers[i].mTypeId, AttrPers[i].mCurPercent);
				item = GetCanEatItemFromCSStorage(entity, storage, eatIDs);
				if (item != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool CanEatSthFromStorages(PeEntity entity, List<CSCommon> storages)
	{
		if (storages == null)
		{
			return false;
		}
		for (int i = 0; i < storages.Count; i++)
		{
			if (storages[i] is CSStorage storage && IsContinueEatFromStorage(entity, storage, out var _))
			{
				return true;
			}
		}
		return false;
	}

	public static bool CanEatSthFromStorages(PeEntity entity, List<CSCommon> storages, out ItemObject item)
	{
		item = null;
		if (storages == null)
		{
			return false;
		}
		for (int i = 0; i < storages.Count; i++)
		{
			if (storages[i] is CSStorage storage && IsContinueEatFromStorage(entity, storage, out item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool CanEatSthFromStorages(PeEntity entity, List<CSCommon> storages, out ItemObject item, out CSStorage storage)
	{
		item = null;
		storage = null;
		if (storages == null)
		{
			return false;
		}
		for (int i = 0; i < storages.Count; i++)
		{
			storage = storages[i] as CSStorage;
			if (storage != null && IsContinueEatFromStorage(entity, storage, out item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsContinueEatFromStorage(PeEntity entity, CSStorage storage, out ItemObject item)
	{
		item = null;
		if (IsWantContinueEat(entity, out var AttrPers))
		{
			for (int i = 0; i < AttrPers.Count; i++)
			{
				int[] eatIDs = GetEatIDs(AttrPers[i].mTypeId, AttrPers[i].mCurPercent);
				item = GetCanEatItemFromCSStorage(entity, storage, eatIDs);
				if (item != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsContinueEat(PeEntity entity, out ItemObject item)
	{
		item = null;
		if (entity.UseItem == null)
		{
			return false;
		}
		if (IsWantContinueEat(entity, out var AttrPers))
		{
			for (int i = 0; i < AttrPers.Count; i++)
			{
				int[] eatIDs = GetEatIDs(AttrPers[i].mTypeId, AttrPers[i].mCurPercent);
				item = GetCanEatItemFromPackage(entity, eatIDs);
				if (item != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static ItemObject GetCanEatItemFromPackage(PeEntity entity, int[] eatIds)
	{
		if (entity.UseItem == null)
		{
			return null;
		}
		ItemObject itemObject = null;
		if (eatIds != null && eatIds.Length > 0)
		{
			for (int i = 0; i < eatIds.Length; i++)
			{
				itemObject = entity.GetBagItemObj(eatIds[i]);
				if (itemObject != null)
				{
					break;
				}
			}
		}
		return itemObject;
	}

	private static ItemObject GetCanEatItemFromCSStorage(PeEntity entity, CSStorage storage, int[] eatIds)
	{
		ItemObject itemObject = null;
		for (int i = 0; i < eatIds.Length; i++)
		{
			itemObject = storage.m_Package.FindItemByProtoId(eatIds[i]);
			if (itemObject != null)
			{
				break;
			}
		}
		return itemObject;
	}

	public static List<AttrPer> IsNeedEatAttr(PeEntity entity)
	{
		List<AttrPer> AttrPers = null;
		IsWantEat(entity, out AttrPers);
		return AttrPers;
	}

	public static bool IsNeedEatsth(PeEntity entity)
	{
		List<AttrPer> AttrPers = null;
		return IsWantEat(entity, out AttrPers);
	}

	public static bool IsNeedContineEat(PeEntity entity)
	{
		List<AttrPer> AttrPers = null;
		return IsWantContinueEat(entity, out AttrPers);
	}

	private static bool IsWantEat(PeEntity entity, out List<AttrPer> AttrPers)
	{
		AttrPers = new List<AttrPer>();
		if (entity == null)
		{
			return false;
		}
		AttrPer attrPer = WantByType(entity, AttribType.Hp, AttribType.HpMax);
		if (attrPer != null)
		{
			AttrPers.Add(attrPer);
		}
		attrPer = WantByType(entity, AttribType.Comfort, AttribType.ComfortMax);
		if (attrPer != null)
		{
			AttrPers.Add(attrPer);
		}
		attrPer = WantByType(entity, AttribType.Hunger, AttribType.HungerMax);
		if (attrPer != null)
		{
			AttrPers.Add(attrPer);
		}
		return AttrPers.Count > 0;
	}

	private static bool IsWantContinueEat(PeEntity entity, out List<AttrPer> AttrPers)
	{
		AttrPers = new List<AttrPer>();
		if (entity == null)
		{
			return false;
		}
		AttrPer attrPer = WantByType(entity, AttribType.Hp, AttribType.HpMax, _bContinue: true);
		if (attrPer != null)
		{
			AttrPers.Add(attrPer);
		}
		attrPer = WantByType(entity, AttribType.Comfort, AttribType.ComfortMax, _bContinue: true);
		if (attrPer != null)
		{
			AttrPers.Add(attrPer);
		}
		attrPer = WantByType(entity, AttribType.Hunger, AttribType.HungerMax, _bContinue: true);
		if (attrPer != null)
		{
			AttrPers.Add(attrPer);
		}
		return AttrPers.Count > 0;
	}

	public static AttrPer WantByType(PeEntity entity, AttribType _type, AttribType _typeMax, bool _bContinue = false)
	{
		AttrPer result = null;
		float num = 1f;
		Items iitems = GetIitems((int)_type);
		if (iitems == null)
		{
			return null;
		}
		float num2 = ((!_bContinue) ? iitems.mEatMin : iitems.mEatMax);
		float attribute = entity.GetAttribute(_type);
		float attribute2 = entity.GetAttribute(_typeMax);
		num = attribute / attribute2;
		if (num < iitems.mEatMax)
		{
			result = new AttrPer((int)_type, num);
		}
		return result;
	}

	public static AttrPer WantByType(PeEntity entity, AttribType _type, AttribType _typeMax, float _eatMin, float _eatMax, bool _bContinue = false)
	{
		AttrPer result = null;
		float num = 1f;
		float num2 = ((!_bContinue) ? _eatMin : _eatMax);
		float attribute = entity.GetAttribute(_type);
		float attribute2 = entity.GetAttribute(_typeMax);
		num = attribute / attribute2;
		if (num < _eatMax)
		{
			result = new AttrPer((int)_type, num);
		}
		return result;
	}
}
