using System.IO;
using PETools;

namespace ItemAsset;

public class OwnerData : Cmpt
{
	public static OwnerData deadNPC;

	public int npcID;

	public string npcName = string.Empty;

	public override byte[] Export()
	{
		base.Export();
		return Serialize.Export(delegate(BinaryWriter w)
		{
			w.Write(npcID);
			w.Write(npcName);
		});
	}

	public override void Export(BinaryWriter w)
	{
		base.Export(w);
		BufferHelper.Serialize(w, npcID);
		BufferHelper.Serialize(w, npcName);
	}

	public override void Import(byte[] buff)
	{
		base.Import(buff);
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			npcID = r.ReadInt32();
			npcName = r.ReadString();
		});
	}

	public override void Import(BinaryReader r)
	{
		base.Import(r);
		npcID = BufferHelper.ReadInt32(r);
		npcName = BufferHelper.ReadString(r);
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
