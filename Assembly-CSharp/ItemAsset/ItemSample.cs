using System.IO;
using Pathea;
using PETools;
using uLink;
using UnityEngine;

namespace ItemAsset;

public class ItemSample
{
	private int mProtoId;

	private int mStackCount;

	private ItemProto mProtoData;

	private bool mClick;

	private float mClickedTime;

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
				mProtoData = PeSingleton<ItemProto.Mgr>.Instance.Get(protoId);
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
				return protoData.name;
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

	public bool Click
	{
		get
		{
			return mClick;
		}
		set
		{
			mClick = value;
		}
	}

	public float ClickedTime
	{
		get
		{
			return mClickedTime;
		}
		set
		{
			mClickedTime = value;
		}
	}

	public Texture2D iconTex
	{
		get
		{
			if (protoData == null)
			{
				return null;
			}
			return protoData.iconTex;
		}
	}

	public string iconString0
	{
		get
		{
			if (protoData == null)
			{
				Debug.LogError("protoData == null" + protoId);
				return "protoData == null" + protoId;
			}
			if (protoData.icon == null)
			{
				Debug.LogError("icon==null" + protoId);
				return "icon==null" + protoId;
			}
			return protoData.icon[0];
		}
	}

	public string iconString1 => protoData.icon[1];

	public string iconString2
	{
		get
		{
			string text = protoData.icon[2];
			if ("0" != text)
			{
				return text;
			}
			return "Null";
		}
	}

	public int level => protoData.level;

	public ItemSample()
	{
	}

	public ItemSample(int itemId, int stackCount = 1)
	{
		mProtoId = itemId;
		mStackCount = stackCount;
	}

	public virtual string GetTooltip()
	{
		return nameText;
	}

	public int GetCount()
	{
		return stackCount;
	}

	public virtual int GetStackMax()
	{
		return protoData.maxStackNum;
	}

	public bool IncreaseStackCount(int num)
	{
		if (mStackCount + num > GetStackMax())
		{
			return false;
		}
		mStackCount += num;
		return false;
	}

	public bool SetStackCount(int num)
	{
		if (num > GetStackMax() || num < 0)
		{
			return false;
		}
		mStackCount = num;
		return true;
	}

	public bool DecreaseStackCount(int num)
	{
		if (num > mStackCount)
		{
			return false;
		}
		mStackCount -= num;
		return true;
	}

	public virtual void Export(BinaryWriter w)
	{
		w.Write(mProtoId);
		w.Write(mStackCount);
	}

	public virtual void Export4Net(BinaryWriter w)
	{
		BufferHelper.Serialize(w, mProtoId);
		BufferHelper.Serialize(w, mStackCount);
	}

	public virtual void Import(byte[] buff)
	{
		PETools.Serialize.Import(buff, delegate(BinaryReader r)
		{
			mProtoId = r.ReadInt32();
			mStackCount = r.ReadInt32();
		});
	}

	public virtual void Import(BinaryReader r)
	{
		mProtoId = BufferHelper.ReadInt32(r);
		mStackCount = BufferHelper.ReadInt32(r);
	}

	public static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
	{
		byte[] buff = stream.ReadBytes();
		ItemSample itemObj = new ItemSample();
		PETools.Serialize.Import(buff, delegate(BinaryReader r)
		{
			itemObj.Import(r);
		});
		return itemObj;
	}

	public static void Serialize(uLink.BitStream stream, object value, params object[] codecOptions)
	{
		ItemSample itemObj = value as ItemSample;
		byte[] value2 = PETools.Serialize.Export(delegate(BinaryWriter w)
		{
			itemObj.Export4Net(w);
		});
		stream.WriteBytes(value2);
	}

	public virtual bool CanDrag()
	{
		return "0" != protoData.icon[1];
	}
}
