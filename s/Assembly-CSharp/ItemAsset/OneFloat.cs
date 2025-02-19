using System.IO;

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

	public void Reset()
	{
		floatValue.SetToMax();
		OnCmptChanged();
	}

	public bool SetValue(float v)
	{
		if (floatValue.current != v)
		{
			floatValue.current = v;
			OnCmptChanged();
			return true;
		}
		return false;
	}

	public bool ChangeValue(float v)
	{
		if (floatValue.Change(v))
		{
			OnCmptChanged();
			return true;
		}
		return false;
	}

	public override void Export(BinaryWriter w)
	{
		BufferHelper.Serialize(w, floatValue.current);
	}

	public override void Import(BinaryReader r)
	{
		float cur = BufferHelper.ReadSingle(r);
		mValue = new PeFloatRangeNum(cur, 0f, valueMax);
	}
}
