using System.IO;

namespace ItemAsset;

public class ArmorPart : Cmpt
{
	public override byte[] Export()
	{
		return base.Export();
	}

	public override void Import(byte[] buff)
	{
		base.Import(buff);
	}

	public override void Export(BinaryWriter w)
	{
		base.Export(w);
	}

	public override void Import(BinaryReader r)
	{
		base.Import(r);
	}

	public override void Init()
	{
		base.Init();
	}

	public override string ProcessTooltip(string text)
	{
		return base.ProcessTooltip(text);
	}
}
