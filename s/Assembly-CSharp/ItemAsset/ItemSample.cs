using System.IO;
using PETools;
using uLink;
using UnityEngine;

namespace ItemAsset;

public class ItemSample
{
	private int mProtoId;

	private int mStackCount;

	private ItemProto mProtoData;

	public int protoId
	{
		get
		{
			return mProtoId;
		}
		set
		{
			mProtoId = value;
		}
	}

	public ItemProto protoData
	{
		get
		{
			if (mProtoData == null)
			{
				mProtoData = ItemProto.Mgr.Instance.Get(protoId);
			}
			return mProtoData;
		}
	}

	public string nameText
	{
		get
		{
			if (protoData != null)
			{
				if (protoId >= CreationData.ObjectStartID)
				{
					return protoData.englishName;
				}
				return PELocalization.GetString(protoData.nameStringId);
			}
			return null;
		}
	}

	public int stackCount
	{
		get
		{
			return mStackCount;
		}
		set
		{
			mStackCount = value;
		}
	}

	public int MaxStackNum => (protoData != null) ? protoData.maxStackNum : 0;

	public virtual int LeftStackCount => MaxStackNum - stackCount;

	public ItemSample()
	{
	}

	public ItemSample(int itemId, int stackCount = 1)
	{
		mProtoId = itemId;
		mStackCount = stackCount;
	}

	public int GetItemProtoId()
	{
		if (mProtoId > 100000000)
		{
			return CreationHelper.ItemClassIdtoProtoId(protoData.itemClassId);
		}
		return mProtoId;
	}

	public bool SetStackCount(int num)
	{
		mStackCount = Mathf.Min(num, MaxStackNum);
		return true;
	}

	public byte[] Export()
	{
		return PETools.Serialize.Export(delegate(BinaryWriter w)
		{
			Export(w);
		});
	}

	public virtual void Export(BinaryWriter w)
	{
		BufferHelper.Serialize(w, mProtoId);
		BufferHelper.Serialize(w, mStackCount);
	}

	public void Import(byte[] buff)
	{
		PETools.Serialize.Import(buff, delegate(BinaryReader r)
		{
			Import(r);
		});
	}

	public virtual void Import(BinaryReader r)
	{
		mProtoId = BufferHelper.ReadInt32(r);
		mStackCount = BufferHelper.ReadInt32(r);
	}

	public static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
	{
		ItemSample itemSample = new ItemSample();
		byte[] buff = stream.ReadBytes();
		itemSample.Import(buff);
		return itemSample;
	}

	public static void Serialize(uLink.BitStream stream, object value, params object[] codecOptions)
	{
		ItemSample itemSample = value as ItemSample;
		byte[] value2 = itemSample.Export();
		stream.WriteBytes(value2);
	}

	public virtual bool CountUp(int num)
	{
		stackCount = Mathf.Clamp(stackCount + num, 0, MaxStackNum);
		return true;
	}

	public virtual bool CountDown(int num)
	{
		stackCount = Mathf.Clamp(stackCount - num, 0, MaxStackNum);
		return stackCount > 0;
	}
}
