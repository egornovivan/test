using System.IO;
using PETools;

namespace Pathea;

public class AutoIncreaseMoney
{
	private bool mAdded;

	private int mMax;

	private int mIncPerDay;

	private Money mMoney;

	public AutoIncreaseMoney(Money money)
	{
		mMoney = money;
	}

	public AutoIncreaseMoney(Money money, int max, int incPerDay, bool added)
		: this(money)
	{
		mMax = max;
		mIncPerDay = incPerDay;
		mAdded = added;
	}

	public void Update()
	{
		if (GameTime.Timer.SecondInDay < (double)(GameTime.Timer.Day2Sec / 2))
		{
			mAdded = false;
			return;
		}
		if (!mAdded && mMoney != null)
		{
			mMoney.current += mIncPerDay;
		}
		mAdded = true;
	}

	public void Import(byte[] data)
	{
		Serialize.Import(data, delegate(BinaryReader r)
		{
			mMax = r.ReadInt32();
			mIncPerDay = r.ReadInt32();
			mAdded = r.ReadBoolean();
		});
	}

	public byte[] Export()
	{
		return Serialize.Export(delegate(BinaryWriter w)
		{
			w.Write(mMax);
			w.Write(mIncPerDay);
			w.Write(mAdded);
		});
	}

	public override string ToString()
	{
		return $"[PeMoney, cur:{((mMoney != null) ? mMoney.current : 0)}, max{mMax}, incPerDay{mIncPerDay}]";
	}
}
