using System.IO;

namespace ItemAsset;

public class OwnerData : Cmpt
{
	public static OwnerData deadNPC;

	protected int mNpcId;

	protected string mNpcName;

	public int npcID
	{
		get
		{
			return mNpcId;
		}
		set
		{
			if (mNpcId != value)
			{
				mNpcId = value;
				OnCmptChanged();
			}
		}
	}

	public string npcName
	{
		get
		{
			return mNpcName;
		}
		set
		{
			if (!object.Equals(mNpcName, value))
			{
				mNpcName = value;
				OnCmptChanged();
			}
		}
	}

	public override void Export(BinaryWriter w)
	{
		base.Export(w);
		BufferHelper.Serialize(w, mNpcId);
		BufferHelper.Serialize(w, mNpcName);
	}

	public override void Import(BinaryReader r)
	{
		base.Import(r);
		mNpcId = BufferHelper.ReadInt32(r);
		mNpcName = BufferHelper.ReadString(r);
	}

	public override void Init()
	{
		base.Init();
	}
}
