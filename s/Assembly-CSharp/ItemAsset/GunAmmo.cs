using System.IO;

namespace ItemAsset;

public class GunAmmo : Cmpt
{
	protected int mIndex;

	protected int mCount;

	public int index
	{
		get
		{
			return mIndex;
		}
		set
		{
			if (mIndex != value)
			{
				mIndex = value;
				OnCmptChanged();
			}
		}
	}

	public int count
	{
		get
		{
			return mCount;
		}
		set
		{
			if (mCount != value)
			{
				mCount = value;
				OnCmptChanged();
			}
		}
	}

	public override void Export(BinaryWriter w)
	{
		BufferHelper.Serialize(w, mIndex);
		BufferHelper.Serialize(w, mCount);
	}

	public override void Import(BinaryReader r)
	{
		mIndex = BufferHelper.ReadInt32(r);
		mCount = BufferHelper.ReadInt32(r);
	}
}
