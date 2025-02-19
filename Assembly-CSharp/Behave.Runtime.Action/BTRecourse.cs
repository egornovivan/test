using Pathea;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTRecourse), "Recourse")]
public class BTRecourse : BTNormal
{
	private class Data
	{
	}

	private PeTrans playerTrans;

	private Data m_Data;

	private float hideStarTime;

	private float HIDE_TIME = 1f;

	private FindHidePos mfind;

	private Vector3 mdir;

	private float checkStarTime;

	private float Check_TIME = 3f;

	private BehaveResult Init(Tree sender)
	{
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Recourse))
		{
			return BehaveResult.Failure;
		}
		float attribute = GetAttribute(AttribType.Hp);
		float attribute2 = GetAttribute(AttribType.HpMax);
		if (attribute / attribute2 > 0.25f)
		{
			return BehaveResult.Failure;
		}
		if (PeSingleton<PeCreature>.Instance != null && PeSingleton<PeCreature>.Instance.mainPlayer != null && playerTrans == null)
		{
			playerTrans = PeSingleton<PeCreature>.Instance.mainPlayer.peTrans;
		}
		SetCambat(value: false);
		hideStarTime = Time.time;
		checkStarTime = Time.time;
		mfind = new FindHidePos(8f, needHide: false);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Recourse))
		{
			return BehaveResult.Failure;
		}
		if (Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Success;
		}
		if (Time.time - checkStarTime > Check_TIME)
		{
			checkStarTime = Time.time;
			if (!base.entity.NpcCmpt.HasEnemyLocked())
			{
				base.entity.target.ClearEnemy();
			}
		}
		float attribute = GetAttribute(AttribType.Hp);
		float attribute2 = GetAttribute(AttribType.HpMax);
		if (attribute / attribute2 >= 0.3f)
		{
			return BehaveResult.Success;
		}
		if (Time.time - hideStarTime > HIDE_TIME)
		{
			mdir = mfind.GetHideDir(playerTrans.position, base.position, base.Enemies);
			hideStarTime = Time.time;
		}
		Vector3 pos = base.position + mdir.normalized * 10f;
		if (mfind.bNeedHide)
		{
			MoveToPosition(pos, SpeedState.Run);
		}
		else
		{
			StopMove();
			FaceDirection(playerTrans.position - base.position);
		}
		if (NpcEatDb.IsContinueEat(base.entity, out var item) && base.entity.UseItem.GetCdByItemProtoId(item.protoId) < float.Epsilon)
		{
			UseItem(item);
		}
		SetCambat(value: false);
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		SetCambat(value: true);
	}
}
