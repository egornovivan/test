using System.IO;

public class DigitalMoney : PeMoney
{
	private int mCur;

	public override int Current
	{
		get
		{
			return mCur;
		}
		set
		{
			mCur = value;
		}
	}

	public DigitalMoney()
	{
		mCur = 0;
	}

	public override bool Add(int i)
	{
		if (i <= 0)
		{
			return false;
		}
		Current += i;
		return true;
	}

	public override bool Remove(int i)
	{
		if (i <= 0)
		{
			return false;
		}
		if (Current < i)
		{
			return false;
		}
		Current -= i;
		return true;
	}

	public override bool Export(BinaryWriter w)
	{
		if (!base.Export(w))
		{
			return false;
		}
		w.Write(mCur);
		return true;
	}

	public override void Import(BinaryReader r)
	{
		base.Import(r);
		if (r != null)
		{
			mCur = r.ReadInt32();
		}
	}
}
