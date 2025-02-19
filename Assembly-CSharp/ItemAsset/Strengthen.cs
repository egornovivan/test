using System.IO;
using Pathea;
using PETools;

namespace ItemAsset;

public class Strengthen : Cmpt
{
	private Property mProperty;

	private Durability mDurability;

	private LifeLimit mLifeLimit;

	private int mStrengthenTime;

	public int strengthenTime => mStrengthenTime;

	public override void Init()
	{
		base.Init();
		mProperty = itemObj.GetCmpt<Property>();
		mDurability = itemObj.GetCmpt<Durability>();
		mLifeLimit = itemObj.GetCmpt<LifeLimit>();
		ApplyStrengthenTime();
	}

	private void ApplyStrengthenTime()
	{
		if (mProperty != null)
		{
			mProperty.SetFactor(GetStrengthFactor(mStrengthenTime));
		}
		if (mDurability != null)
		{
			mDurability.SetMax(mDurability.GetRawMax() * GetStrengthFactor(mStrengthenTime));
		}
		if (mLifeLimit != null)
		{
			mLifeLimit.SetMax(mLifeLimit.GetRawMax() * GetStrengthFactor(mStrengthenTime));
		}
	}

	public void LevelUp()
	{
		mStrengthenTime++;
		ApplyStrengthenTime();
	}

	public float GetStrengthFactor()
	{
		return GetStrengthFactor(mStrengthenTime);
	}

	public static float GetStrengthFactor(int time)
	{
		return 1.2f - 0.2f / (float)(1 << time);
	}

	public float GetNextLevelProperty(AttribType pro)
	{
		return GetPropertyByStrengthTime(pro, strengthenTime + 1);
	}

	public float GetCurLevelProperty(AttribType pro)
	{
		return GetPropertyByStrengthTime(pro, strengthenTime);
	}

	private float GetPropertyByStrengthTime(AttribType pro, int time)
	{
		float num = 0f;
		if (mProperty != null)
		{
			num = mProperty.GetRawProperty(pro);
		}
		float strengthFactor = GetStrengthFactor(time);
		return num * strengthFactor;
	}

	public float GetNextMaxDurability()
	{
		if (mDurability == null)
		{
			return -1f;
		}
		return GetStrengthFactor(mStrengthenTime + 1) * mDurability.GetRawMax();
	}

	public float GetNextMaxLife()
	{
		if (mLifeLimit == null)
		{
			return -1f;
		}
		return GetStrengthFactor(mStrengthenTime + 1) * mLifeLimit.GetRawMax();
	}

	public float GetCurMaxDurability()
	{
		if (mDurability == null)
		{
			return -1f;
		}
		return mDurability.valueMax;
	}

	public float GetCurMaxLife()
	{
		if (mLifeLimit == null)
		{
			return -1f;
		}
		return mLifeLimit.valueMax;
	}

	public override byte[] Export()
	{
		return Serialize.Export(delegate(BinaryWriter w)
		{
			w.Write(mStrengthenTime);
		}, 20);
	}

	public override void Export(BinaryWriter w)
	{
		base.Export(w);
		BufferHelper.Serialize(w, mStrengthenTime);
	}

	public override void Import(byte[] buff)
	{
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			mStrengthenTime = r.ReadInt32();
			ApplyStrengthenTime();
		});
	}

	public override void Import(BinaryReader r)
	{
		base.Import(r);
		mStrengthenTime = BufferHelper.ReadInt32(r);
		ApplyStrengthenTime();
	}

	public MaterialItem[] GetMaterialItems()
	{
		if (base.protoData.strengthenMaterialList != null)
		{
			return base.protoData.strengthenMaterialList.ToArray();
		}
		return null;
	}
}
