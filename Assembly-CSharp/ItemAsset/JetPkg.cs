using System.IO;
using PETools;

namespace ItemAsset;

public class JetPkg : Cmpt
{
	public float energy;

	public override byte[] Export()
	{
		return Serialize.Export(delegate(BinaryWriter w)
		{
			w.Write(energy);
		}, 800);
	}

	public override void Export(BinaryWriter w)
	{
		base.Export(w);
		BufferHelper.Serialize(w, energy);
	}

	public override void Import(byte[] buff)
	{
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			energy = r.ReadSingle();
		});
	}

	public override void Import(BinaryReader r)
	{
		base.Import(r);
		BufferHelper.ReadSingle(r);
	}
}
