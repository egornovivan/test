using System.IO;
using Pathea;

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
			OnCmptChanged();
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

	public override void Export(BinaryWriter w)
	{
		BufferHelper.Serialize(w, mCurCostValue);
	}

	public override void Import(BinaryReader r)
	{
		mCurCostValue = BufferHelper.ReadInt32(r);
	}
}
