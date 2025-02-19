using System.IO;

public abstract class PeMoney
{
	public abstract int Current { get; set; }

	public PeMoney()
	{
	}

	public abstract bool Add(int i);

	public abstract bool Remove(int i);

	public virtual void Import(BinaryReader r)
	{
		if (r != null)
		{
		}
	}

	public virtual bool Export(BinaryWriter w)
	{
		if (w == null)
		{
			return false;
		}
		return true;
	}
}
