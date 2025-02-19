using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mono.Data.SqliteClient;
using PETools;
using uLink;
using UnityEngine;

namespace ItemAsset;

public class ItemObject : ItemSample, IEnumerable, IEnumerable<Cmpt>
{
	public const int VERSION_0000 = 0;

	public const int CURRENT_VERSION = 0;

	protected int mInstanceId;

	protected int mVersion;

	protected bool mChangedFLag;

	protected List<Cmpt> mListCmpt = new List<Cmpt>(10);

	private PeFloatRangeNum mFloatFactor;

	private static Dictionary<int, string[]> dicItemType;

	public int instanceId => mInstanceId;

	public bool changedFlag => mChangedFLag;

	public int CmptCount => mListCmpt.Count;

	public float priceFactor
	{
		get
		{
			if (mFloatFactor == null)
			{
				return 1f;
			}
			return mFloatFactor.percent;
		}
	}

	public bool bind { get; set; }

	public ItemObject()
	{
	}

	public ItemObject(int protoId)
		: base(protoId)
	{
	}

	public ItemObject(int instanceId, int prototypeId)
		: base(prototypeId)
	{
		mInstanceId = instanceId;
	}

	IEnumerator<Cmpt> IEnumerable<Cmpt>.GetEnumerator()
	{
		return mListCmpt.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return mListCmpt.GetEnumerator();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ItemObject))
		{
			return false;
		}
		return instanceId == ((ItemObject)obj).instanceId;
	}

	public override int GetHashCode()
	{
		return instanceId;
	}

	public int GetTabIndex()
	{
		return base.protoData.tabIndex;
	}

	public override bool CountUp(int num)
	{
		base.CountUp(num);
		OnChange();
		return true;
	}

	public override bool CountDown(int num)
	{
		if (!base.CountDown(num))
		{
			OnChange();
			return false;
		}
		OnChange();
		return true;
	}

	public bool CreateFromItem(ItemObject item)
	{
		if (item == null)
		{
			return false;
		}
		int num = mInstanceId;
		byte[] buff = item.Export();
		Import(buff);
		SetInstanceId(num);
		return true;
	}

	public void SetInstanceId(int id)
	{
		mInstanceId = id;
	}

	public void ResetChangeFlag()
	{
		mChangedFLag = false;
	}

	public void OnChange()
	{
		if (!mChangedFLag)
		{
			mChangedFLag = true;
			ItemManager.AddUpdateItem(this);
		}
	}

	public void Init()
	{
		if (mListCmpt == null)
		{
			return;
		}
		foreach (Cmpt item in mListCmpt)
		{
			item.Init();
		}
	}

	public void Add(Cmpt cmpt)
	{
		if (cmpt != null)
		{
			cmpt.itemObj = this;
			mListCmpt.Add(cmpt);
		}
	}

	public bool Remove(Cmpt cmpt)
	{
		if (!mListCmpt.Remove(cmpt))
		{
			return false;
		}
		return true;
	}

	public bool Remove(string cmptName)
	{
		Cmpt cmpt = GetCmpt(cmptName);
		return Remove(cmpt);
	}

	public bool Contains(string cmptName)
	{
		return null != GetCmpt(cmptName);
	}

	public Cmpt GetCmpt(string cmptName)
	{
		return mListCmpt.Find((Cmpt c) => (c.GetTypeName() == cmptName) ? true : false);
	}

	public T GetCmpt<T>() where T : Cmpt
	{
		Cmpt cmpt = mListCmpt.Find((Cmpt c) => c is T);
		return cmpt as T;
	}

	public new static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
	{
		byte[] buff = stream.ReadBytes();
		return Deserialize(buff);
	}

	public new static void Serialize(uLink.BitStream stream, object value, params object[] codecOptions)
	{
		ItemObject itemObject = value as ItemObject;
		byte[] value2 = itemObject.Export();
		stream.WriteBytes(value2);
	}

	public static ItemObject Deserialize(byte[] buff)
	{
		if (buff == null)
		{
			Debug.LogError("buff is null");
			return null;
		}
		ItemObject item = null;
		PETools.Serialize.Import(buff, delegate(BinaryReader r)
		{
			int num = BufferHelper.ReadInt32(r);
			int num2 = BufferHelper.ReadInt32(r);
			int num3 = BufferHelper.ReadInt32(r);
			BufferHelper.ReadBoolean(r);
			int num4 = BufferHelper.ReadInt32(r);
			item = ItemManager.GetItemByID(num3);
			if (item == null)
			{
				item = Create(num);
				item.SetInstanceId(num3);
				ItemManager.AddItem(item, newItem: false);
			}
			item.SetStackCount(num2);
			for (int i = 0; i < num4; i++)
			{
				string text = BufferHelper.ReadString(r);
				Cmpt cmpt = item.GetCmpt(text);
				if (cmpt == null)
				{
					Debug.LogErrorFormat("item id:{0}, item id:{1} does not contain cmpt:{2}", num3, num, text);
					break;
				}
				cmpt.Import(r);
			}
		});
		return item;
	}

	public override void Export(BinaryWriter w)
	{
		base.Export(w);
		BufferHelper.Serialize(w, mInstanceId);
		BufferHelper.Serialize(w, bind);
		BufferHelper.Serialize(w, CmptCount);
		foreach (Cmpt item in mListCmpt)
		{
			BufferHelper.Serialize(w, item.GetTypeName());
			item.Export(w);
		}
	}

	public override void Import(BinaryReader r)
	{
		base.Import(r);
		mInstanceId = BufferHelper.ReadInt32(r);
		bind = BufferHelper.ReadBoolean(r);
		int num = BufferHelper.ReadInt32(r);
		for (int i = 0; i < num; i++)
		{
			string text = BufferHelper.ReadString(r);
			Cmpt cmpt = GetCmpt(text);
			if (cmpt != null)
			{
				cmpt.Import(r);
				continue;
			}
			Debug.LogErrorFormat("item id:{0}, item id:{1} does not contain cmpt:{2}", instanceId, base.protoId, text);
			break;
		}
	}

	public void SetPriceFactor(PeFloatRangeNum f)
	{
		mFloatFactor = f;
	}

	private static int GetPrice(int basePrice, float factor)
	{
		return (int)((float)basePrice * (0.8f * factor + 0.2f));
	}

	public int GetSellPrice()
	{
		return GetPrice(base.protoData.currency, priceFactor);
	}

	public int GetBuyPrice()
	{
		ShopData shopData = ShopRespository.GetShopData(base.protoId);
		if (shopData == null)
		{
			return 0;
		}
		return GetPrice(shopData.m_Price, priceFactor);
	}

	private static void LoadItemType()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("item_class");
		while (sqliteDataReader.Read())
		{
			int key = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("classname"));
			dicItemType[key] = @string.Split(',');
		}
	}

	private static ItemObject Create(int protoId)
	{
		ItemProto itemProto = ItemProto.Mgr.Instance.Get(protoId);
		if (itemProto == null)
		{
			Debug.LogError("cant find prorotype data by prototype id:" + protoId);
			return null;
		}
		int itemClassId = itemProto.itemClassId;
		if (dicItemType == null)
		{
			dicItemType = new Dictionary<int, string[]>(50);
			LoadItemType();
		}
		ItemObject itemObject = new ItemObject(protoId);
		if (!dicItemType.ContainsKey(itemClassId))
		{
			return itemObject;
		}
		string[] array = dicItemType[itemClassId];
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (!string.IsNullOrEmpty(text) && !(text == "0"))
			{
				Type type = Type.GetType("ItemAsset." + text);
				Cmpt cmpt = Activator.CreateInstance(type) as Cmpt;
				itemObject.Add(cmpt);
			}
		}
		itemObject.Init();
		return itemObject;
	}

	public static ItemObject Create(int prototypeId, int instanceId)
	{
		ItemObject itemObject = Create(prototypeId);
		if (itemObject == null)
		{
			return null;
		}
		itemObject.mInstanceId = instanceId;
		return itemObject;
	}
}
