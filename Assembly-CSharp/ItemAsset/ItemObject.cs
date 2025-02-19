using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mono.Data.SqliteClient;
using Pathea;
using PETools;
using uLink;
using UnityEngine;

namespace ItemAsset;

public class ItemObject : ItemSample, IEnumerable, IEnumerable<Cmpt>
{
	public const int VERSION_0000 = 0;

	public const int CURRENT_VERSION = 0;

	public int Version;

	private List<Cmpt> mListCmpt = new List<Cmpt>(10);

	private int mInstanceId;

	private PeFloatRangeNum mFloatFactor;

	private static Dictionary<int, string[]> dicItemType;

	public int instanceId => mInstanceId;

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

	private ItemObject(int protoId)
		: base(protoId)
	{
	}

	IEnumerator<Cmpt> IEnumerable<Cmpt>.GetEnumerator()
	{
		return mListCmpt.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return mListCmpt.GetEnumerator();
	}

	public void SetInstanceId(int id)
	{
		mInstanceId = id;
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

	public string[] GetCmptNames()
	{
		List<string> list = mListCmpt.ConvertAll((Cmpt c) => c.GetTypeName());
		return list.ToArray();
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

	public void Serialize(BinaryWriter w)
	{
		w.Write(base.protoId);
		w.Write(instanceId);
		PETools.Serialize.WriteData(Export, w);
	}

	public new static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
	{
		try
		{
			byte[] data = stream.ReadBytes();
			return NetImport(data);
		}
		catch (Exception message)
		{
			Debug.LogWarning(message);
			return null;
		}
	}

	public new static void Serialize(uLink.BitStream stream, object value, params object[] codecOptions)
	{
		try
		{
			ItemObject itemObj = value as ItemObject;
			if (itemObj != null)
			{
				byte[] value2 = PETools.Serialize.Export(delegate(BinaryWriter w)
				{
					itemObj.Export4Net(w);
				});
				stream.WriteBytes(value2);
			}
		}
		catch (Exception message)
		{
			Debug.LogWarning(message);
		}
	}

	public static ItemObject Deserialize(byte[] buff)
	{
		if (buff == null || buff.Length <= 0)
		{
			Debug.LogError("buff is null");
			return null;
		}
		using MemoryStream input = new MemoryStream(buff, writable: false);
		using BinaryReader binaryReader = new BinaryReader(input);
		int num = binaryReader.ReadInt32();
		int id = binaryReader.ReadInt32();
		byte[] array = PETools.Serialize.ReadBytes(binaryReader);
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(id);
		if (itemObject == null)
		{
			itemObject = Create(num);
			if (itemObject != null && array != null && array.Length > 0)
			{
				itemObject.SetInstanceId(id);
				itemObject.Import(array);
				PeSingleton<ItemMgr>.Instance.Add(itemObject);
			}
		}
		else
		{
			itemObject.Import(array);
		}
		return itemObject;
	}

	public static byte[] Serialize(ItemObject item)
	{
		if (item == null)
		{
			Debug.LogError("item is null");
			return null;
		}
		using MemoryStream memoryStream = new MemoryStream(100);
		using (BinaryWriter w = new BinaryWriter(memoryStream))
		{
			item.Serialize(w);
		}
		return memoryStream.ToArray();
	}

	public static ItemObject NetImport(byte[] data)
	{
		if (data == null || data.Length == 0)
		{
			Debug.LogWarning("buff is null");
			return null;
		}
		ItemObject item = null;
		PETools.Serialize.Import(data, delegate(BinaryReader r)
		{
			int num = BufferHelper.ReadInt32(r);
			int num2 = BufferHelper.ReadInt32(r);
			int num3 = BufferHelper.ReadInt32(r);
			BufferHelper.ReadBoolean(r);
			int num4 = BufferHelper.ReadInt32(r);
			item = PeSingleton<ItemMgr>.Instance.Get(num3);
			if (item == null)
			{
				item = Create(num, num3);
				if (item == null)
				{
					Debug.LogWarningFormat("id:{0}, prototype id:{1} import error.", num3, num);
					return;
				}
				PeSingleton<ItemMgr>.Instance.Add(item);
			}
			item.SetStackCount(num2);
			for (int i = 0; i < num4; i++)
			{
				string text = BufferHelper.ReadString(r);
				Cmpt cmpt = item.GetCmpt(text);
				if (cmpt == null)
				{
					Debug.LogWarningFormat("item id:{0}, item id:{1} does not contain cmpt:{2}", num3, num, text);
					break;
				}
				cmpt.Import(r);
			}
		});
		return item;
	}

	public override void Export(BinaryWriter w)
	{
		PETools.Serialize.WriteData(base.Export, w);
		int count = mListCmpt.Count;
		w.Write(bind);
		w.Write(count);
		for (int i = 0; i < count; i++)
		{
			Cmpt cmpt = mListCmpt[i];
			w.Write(cmpt.GetTypeName());
			PETools.Serialize.WriteBytes(cmpt.Export(), w);
		}
	}

	public override void Export4Net(BinaryWriter w)
	{
		base.Export4Net(w);
		int count = mListCmpt.Count;
		BufferHelper.Serialize(w, mInstanceId);
		BufferHelper.Serialize(w, bind);
		BufferHelper.Serialize(w, count);
		for (int i = 0; i < count; i++)
		{
			Cmpt cmpt = mListCmpt[i];
			BufferHelper.Serialize(w, cmpt.GetTypeName());
			cmpt.Export(w);
		}
	}

	public override void Import(byte[] buffer)
	{
		PETools.Serialize.Import(buffer, delegate(BinaryReader r)
		{
			byte[] buff = PETools.Serialize.ReadBytes(r);
			base.Import(buff);
			bind = r.ReadBoolean();
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				string cmptName = r.ReadString();
				byte[] array = PETools.Serialize.ReadBytes(r);
				if (array != null && array.Length > 0)
				{
					GetCmpt(cmptName)?.Import(array);
				}
			}
		});
		Init();
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
		ShopData shopDataByItemId = ShopRespository.GetShopDataByItemId(base.protoId);
		if (shopDataByItemId == null)
		{
			return 0;
		}
		return GetPrice(shopDataByItemId.m_Price, priceFactor);
	}

	public int GetBuyPrice(ShopData data)
	{
		if (data == null)
		{
			return 0;
		}
		return GetPrice(data.m_Price, priceFactor);
	}

	private string GetOriginText()
	{
		return PELocalization.GetString(base.protoData.descriptionStringId);
	}

	private string CmptProcess(string text)
	{
		foreach (Cmpt item in mListCmpt)
		{
			text = item.ProcessTooltip(text);
		}
		return text;
	}

	private string SellPriceProcess(string text)
	{
		return text.Replace("[SP]", GetSellPrice().ToString());
	}

	private string BuyPriceProcess(string text)
	{
		return text.Replace("[BP]", GetBuyPrice().ToString());
	}

	private string GetCreationTooltip()
	{
		if (base.protoId >= 100000000)
		{
			CreationData creation = CreationMgr.GetCreation(base.protoId);
			if (creation != null)
			{
				return creation.AttrDescString(this);
			}
		}
		return null;
	}

	public override string GetTooltip()
	{
		string creationTooltip = GetCreationTooltip();
		if (creationTooltip != null)
		{
			return creationTooltip;
		}
		string originText = GetOriginText();
		originText = CmptProcess(originText);
		originText = SellPriceProcess(originText);
		return BuyPriceProcess(originText);
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
		if (100000000 <= protoId)
		{
			SteamWorkShop.GetCreateionHead(protoId);
		}
		ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(protoId);
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
			try
			{
				if (!string.IsNullOrEmpty(text) && !(text == "0"))
				{
					Type type = Type.GetType("ItemAsset." + text);
					Cmpt cmpt = Activator.CreateInstance(type) as Cmpt;
					itemObject.Add(cmpt);
				}
			}
			catch (Exception message)
			{
				Debug.LogError("create failed, item:" + protoId + " cmpt:" + text);
				Debug.LogWarning(message);
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
