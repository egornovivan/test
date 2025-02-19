using System.IO;
using PETools;

public abstract class DataCmpt
{
	protected ECmptType mType;

	protected NetInterface mNet;

	public ECmptType CmptType => mType;

	public NetInterface Net => mNet;

	public virtual void Export(BinaryWriter w)
	{
		BufferHelper.Serialize(w, (int)mType);
	}

	public virtual void Import(BinaryReader r)
	{
	}

	public void LinkNet(NetInterface net)
	{
		mNet = net;
	}

	public byte[] ExportData()
	{
		return Serialize.Export(delegate(BinaryWriter w)
		{
			Export(w);
		});
	}

	public void ImportData(byte[] data)
	{
		Serialize.Import(data, delegate(BinaryReader r)
		{
			Import(r);
		});
	}
}
