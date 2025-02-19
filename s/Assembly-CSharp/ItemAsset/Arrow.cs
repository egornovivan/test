using System.IO;

namespace ItemAsset;

public class Arrow : Cmpt
{
	public int mIndex;

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

	public override void Export(BinaryWriter w)
	{
		BufferHelper.Serialize(w, mIndex);
	}

	public override void Import(BinaryReader r)
	{
		mIndex = BufferHelper.ReadInt32(r);
	}
}
