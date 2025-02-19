using System.IO;

namespace ItemAsset;

public class JetPkg : Cmpt
{
	protected float mEnergy;

	public float energy
	{
		get
		{
			return mEnergy;
		}
		set
		{
			if (mEnergy != value)
			{
				mEnergy = value;
				OnCmptChanged();
			}
		}
	}

	public override void Export(BinaryWriter w)
	{
		BufferHelper.Serialize(w, energy);
	}

	public override void Import(BinaryReader r)
	{
		energy = BufferHelper.ReadSingle(r);
	}
}
