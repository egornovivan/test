using System.IO;
using Pathea;
using PETools;

namespace ItemAsset;

public class Tower : Cmpt
{
	private int mCurCostValue = -1;

	private TowerProtoDb.BulletData mBulletData;

	public int curCostValue
	{
		get
		{
			if (mCurCostValue == -1)
			{
				if (costType == ECostType.Item)
				{
					mCurCostValue = bulletData.bulletMax;
				}
				else if (costType == ECostType.Energy)
				{
					mCurCostValue = bulletData.energyMax;
				}
			}
			return mCurCostValue;
		}
		set
		{
			mCurCostValue = value;
		}
	}

	public int id => base.protoData.towerEntityId;

	public ECostType costType => (ECostType)bulletData.bulletType;

	public TowerProtoDb.BulletData bulletData
	{
		get
		{
			if (mBulletData == null)
			{
				TowerProtoDb.Item item = TowerProtoDb.Get(id);
				if (item != null)
				{
					mBulletData = item.bulletData;
				}
			}
			return mBulletData;
		}
	}

	public override byte[] Export()
	{
		return Serialize.Export(delegate(BinaryWriter w)
		{
			w.Write(mCurCostValue);
		}, 10);
	}

	public override void Export(BinaryWriter w)
	{
		base.Export(w);
		BufferHelper.Serialize(w, mCurCostValue);
	}

	public override void Import(byte[] buff)
	{
		Serialize.Import(buff, delegate(BinaryReader r)
		{
			mCurCostValue = r.ReadInt32();
		});
	}

	public override void Import(BinaryReader r)
	{
		base.Import(r);
		mCurCostValue = BufferHelper.ReadInt32(r);
	}

	public override string ProcessTooltip(string text)
	{
		string text2 = base.ProcessTooltip(text);
		text2 = text2.Replace("$powerMax$", bulletData.energyMax.ToString());
		text2 = text2.Replace("$ammoMax$", bulletData.bulletMax.ToString());
		text2 = text2.Replace("$100000006$", curCostValue.ToString());
		return text2.Replace("$100000007$", curCostValue.ToString());
	}
}
