using ItemAsset;
using Pathea;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTRunawayFromEnemy), "RunawayFromEnemy")]
public class BTRunawayFromEnemy : BTNormal
{
	private class Data
	{
		[Behave]
		public float RunRadius;

		[Behave]
		public float minHpPercent;
	}

	private Data m_Data;

	private FindHidePos mFind;

	private float startRunTime;

	private float CHECK_TIME = 10f;

	private float startHideTime;

	private float CHECK_Hide_TIME = 1f;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		mFind = new FindHidePos(m_Data.RunRadius, needHide: false, m_Data.RunRadius);
		startRunTime = Time.time;
		SetCambat(value: false);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (Time.time - startHideTime > CHECK_Hide_TIME)
		{
			Vector3 hideDir = mFind.GetHideDir(PeSingleton<PeCreature>.Instance.mainPlayer.peTrans.position, base.position, base.Enemies);
			if (mFind.bNeedHide)
			{
				Vector3 pos = base.position + hideDir.normalized * m_Data.RunRadius;
				MoveToPosition(pos, SpeedState.Run);
			}
			else
			{
				StopMove();
				FaceDirection(PeSingleton<PeCreature>.Instance.mainPlayer.peTrans.position - base.position);
			}
			startHideTime = Time.time;
		}
		if (Time.time - startRunTime > CHECK_TIME || base.entity.HPPercent > m_Data.minHpPercent || SelectItem.HasCanEquip(base.entity, EeqSelect.combat, AttackType.Ranged))
		{
			SetCambat(value: true);
			return BehaveResult.Success;
		}
		if (NpcEatDb.IsContinueEat(base.entity, out var item) && base.entity.UseItem.GetCdByItemProtoId(item.protoId) < float.Epsilon)
		{
			UseItem(item);
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		startHideTime = 0f;
	}
}
