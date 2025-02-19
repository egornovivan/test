using System.IO;
using PETools;

namespace Pathea;

public class Money
{
	private const int MoneyItemProtoId = 229;

	public static bool Digital = true;

	private int mCur;

	private PackageCmpt mPkg;

	public int current
	{
		get
		{
			if (Digital)
			{
				return digitalCur;
			}
			return itemCur;
		}
		set
		{
			if (Digital)
			{
				digitalCur = value;
			}
			else
			{
				itemCur = value;
			}
		}
	}

	private int digitalCur
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

	private int itemCur
	{
		get
		{
			return mPkg.GetItemCount(229);
		}
		set
		{
			mPkg.Set(229, value);
		}
	}

	public Money()
	{
	}

	public Money(PackageCmpt pkg)
	{
		mPkg = pkg;
	}

	public void SetCur(int n)
	{
		mCur = n;
	}

	public byte[] Export()
	{
		return Serialize.Export(delegate(BinaryWriter w)
		{
			w.Write(mCur);
		});
	}

	public void Import(byte[] data)
	{
		Serialize.Import(data, delegate(BinaryReader r)
		{
			mCur = r.ReadInt32();
		});
	}
}
