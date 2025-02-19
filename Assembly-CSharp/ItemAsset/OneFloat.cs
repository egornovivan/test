using System.IO;
using PETools;

namespace ItemAsset;

public abstract class OneFloat : Cmpt
{
	protected PeFloatRangeNum mValue;

	private float mMaxValue = -1f;

	private bool mInit;

	public float valueMax
	{
		get
		{
			if (!mInit)
			{
				mInit = true;
				mMaxValue = GetRawMax();
			}
			return mMaxValue;
		}
	}

	public PeFloatRangeNum floatValue
	{
		get
		{
			if (mValue == null)
			{
				CreateValue();
			}
			return mValue;
		}
	}

	public void SetMax(float v)
	{
		mInit = true;
		mMaxValue = v;
		CreateValue();
	}

	public abstract float GetRawMax();

	protected void CreateValue()
	{
		float current = valueMax;
		if (mValue != null)
		{
			current = mValue.current;
		}
		mValue = new PeFloatRangeNum(current, 0f, valueMax);
	}

	public override byte[] Export()
	{
		return Serialize.Export(delegate(BinaryWriter w)
		{
			w.Write(floatValue.current);
		}, 10);
	}

	public override void Export(BinaryWriter w)
	{
		base.Export(w);
		BufferHelper.Serialize(w, floatValue.current);
	}

	public override void Import(byte[] buff)
	{
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			mValue = new PeFloatRangeNum(r.ReadSingle(), 0f, valueMax);
		});
	}

	public override void Import(BinaryReader r)
	{
		base.Import(r);
		float cur = BufferHelper.ReadSingle(r);
		mValue = new PeFloatRangeNum(cur, 0f, valueMax);
	}
}
