using System.Collections.Generic;
using Pathea;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcFollowerLoot), "NpcFollowerLoot")]
public class BTNpcFollowerLoot : BTNormal
{
	private class Data
	{
		[Behave]
		public float LootRadius;

		public float lootTime = 1f;

		public float startLootTime;
	}

	private Data m_Data;

	private PeEntity mLootEntity;

	private ItemDropPeEntity mItemDrop;

	private bool bReached;

	private BehaveResult Init(Tree sender)
	{
		return BehaveResult.Failure;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (bReached && Time.time - m_Data.startLootTime > m_Data.lootTime)
		{
			mLootEntity = null;
			bReached = false;
			EndAction(PEActionType.Leisure);
		}
		if (!bReached && mLootEntity == null)
		{
			if (!Enemy.IsNullOrInvalid(base.attackEnemy))
			{
				return BehaveResult.Failure;
			}
			List<PeEntity> entities = LootItemDropPeEntity.GetEntities(base.position, m_Data.LootRadius);
			if (entities.Count <= 0)
			{
				return BehaveResult.Failure;
			}
			mLootEntity = LootItemDropPeEntity.GetLootEntity(base.position, m_Data.LootRadius);
			mItemDrop = mLootEntity.GetComponent<ItemDropPeEntity>();
			if (mItemDrop == null || !mItemDrop.NpcCanFetchAll(base.entity.NpcCmpt.NpcPackage))
			{
				return BehaveResult.Failure;
			}
			LootItemDropPeEntity.RemovePeEntity(mLootEntity);
		}
		if (!bReached)
		{
			if (IsReached(base.position, mLootEntity.peTrans.position) || Stucking())
			{
				StopMove();
				FaceDirection(mLootEntity.peTrans.position - base.position);
				PEActionParamS param = PEActionParamS.param;
				param.str = "Gather";
				DoAction(PEActionType.Leisure, param);
				mItemDrop.NpcFetchAll(base.entity.NpcCmpt.NpcPackage);
				m_Data.startLootTime = Time.time;
				bReached = true;
			}
			else
			{
				MoveToPosition(mLootEntity.peTrans.position, SpeedState.Run);
			}
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
	}
}
