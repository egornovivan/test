using System.IO;

namespace ItemAsset;

public abstract class Cmpt
{
	public ItemObject itemObj;

	public ItemProto protoData => itemObj.protoData;

	public virtual byte[] Export()
	{
		return null;
	}

	public virtual void Export(BinaryWriter w)
	{
	}

	public virtual void Import(byte[] buff)
	{
	}

	public virtual void Import(BinaryReader r)
	{
	}

	public string GetTypeName()
	{
		return GetType().ToString();
	}

	public virtual string ProcessTooltip(string text)
	{
		return text;
	}

	public virtual void Init()
	{
	}
}
