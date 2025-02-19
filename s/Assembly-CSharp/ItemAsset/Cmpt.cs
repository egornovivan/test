using System.IO;
using PETools;

namespace ItemAsset;

public abstract class Cmpt
{
	public ItemObject itemObj;

	public ItemProto protoData => itemObj.protoData;

	public byte[] Export()
	{
		return Serialize.Export(delegate(BinaryWriter w)
		{
			Export(w);
		});
	}

	public virtual void Export(BinaryWriter w)
	{
	}

	public void Import(byte[] buff)
	{
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			Import(r);
		});
	}

	public virtual void Import(BinaryReader r)
	{
	}

	public string GetTypeName()
	{
		return GetType().ToString();
	}

	public virtual void Init()
	{
	}

	protected void OnCmptChanged()
	{
		itemObj.OnChange();
	}
}
