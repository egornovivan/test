using System.IO;
using PETools;

namespace ItemAsset;

public class Arrow : Cmpt
{
	public int index;

	public override byte[] Export()
	{
		return Serialize.Export(delegate(BinaryWriter w)
		{
			w.Write(index);
		}, 800);
	}

	public override void Export(BinaryWriter w)
	{
		base.Export(w);
		BufferHelper.Serialize(w, index);
	}

	public override void Import(byte[] buff)
	{
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			index = r.ReadInt32();
		});
	}

	public override void Import(BinaryReader r)
	{
		base.Import(r);
		index = BufferHelper.ReadInt32(r);
	}
}
