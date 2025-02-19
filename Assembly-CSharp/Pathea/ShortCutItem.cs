using System.IO;
using ItemAsset;
using PETools;

namespace Pathea;

public class ShortCutItem : ItemSample
{
	private int m_ItemInstanceId;

	public int itemInstanceId
	{
		get
		{
			return m_ItemInstanceId;
		}
		set
		{
			m_ItemInstanceId = value;
		}
	}

	public bool UseProtoID => base.protoData != null && base.protoData.maxStackNum > 1;

	public override void Export(BinaryWriter w)
	{
		PETools.Serialize.WriteData(base.Export, w);
		w.Write(itemInstanceId);
	}

	public override void Import(byte[] buff)
	{
		PETools.Serialize.Import(buff, delegate(BinaryReader r)
		{
			byte[] buff2 = PETools.Serialize.ReadBytes(r);
			base.Import(buff2);
			itemInstanceId = r.ReadInt32();
		});
	}
}
