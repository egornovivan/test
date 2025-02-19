using System.IO;
using PETools;

namespace ItemAsset;

public class GunAmmo : Cmpt
{
	public int index;

	public int count = -1;

	public override byte[] Export()
	{
		return Serialize.Export(delegate(BinaryWriter w)
		{
			w.Write(index);
			w.Write(count);
		}, 10);
	}

	public override void Export(BinaryWriter w)
	{
		base.Export(w);
		BufferHelper.Serialize(w, index);
		BufferHelper.Serialize(w, count);
	}

	public override void Import(byte[] buff)
	{
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			index = r.ReadInt32();
			count = r.ReadInt32();
		});
	}

	public override void Import(BinaryReader r)
	{
		base.Import(r);
		index = BufferHelper.ReadInt32(r);
		count = BufferHelper.ReadInt32(r);
	}
}
