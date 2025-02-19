using Pathea;
using Pathea.Operate;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(NpcBaseDoctor), "NpcBaseDoctor")]
public class NpcBaseDoctor : BTNormal
{
	private Vector3 m_MoveToPostion;

	private bool mChangeAction;

	private static float endActionTime = 2f;

	private float startEndActionTime;

	private Vector3 GetCSWokePosition(CSEntity WokeEnity)
	{
		return WokeEnity.Position;
	}

	private BehaveResult EndOperate()
	{
		if (((base.Cured != null && base.Cured.ContainsOperator(base.Operator)) || IsMotionRunning(PEActionType.Operation)) && startEndActionTime == 0f)
		{
			startEndActionTime = Time.time;
		}
		if (Time.time - startEndActionTime < endActionTime)
		{
			if (base.Cured != null && base.Cured.ContainsOperator(base.Operator))
			{
				base.Cured.StopOperate(base.Operator, EOperationMask.Cure);
			}
			else if (base.Operator != null && !base.Operator.Equals(null) && base.Operator.Operate != null && !base.Operator.Equals(null) && base.Operator.Operate.ContainsOperator(base.Operator))
			{
				base.Operator.Operate.StopOperate(base.Operator, EOperationMask.Cure);
			}
			return BehaveResult.Running;
		}
		startEndActionTime = 0f;
		return BehaveResult.Failure;
	}

	private BehaveResult Init(Tree sender)
	{
		if (!base.IsNpcBase || base.NpcJob != ENpcJob.Doctor)
		{
			return BehaveResult.Failure;
		}
		if (base.WorkEntity == null || base.WorkEntity.workTrans == null)
		{
			return BehaveResult.Failure;
		}
		if (base.Cured == null || base.Cured.Equals(null) || base.Operator == null || base.Operator.Equals(null))
		{
			return BehaveResult.Failure;
		}
		m_MoveToPostion = base.WorkEntity.workTrans[0].position;
		SetNpcState(ENpcState.Prepare);
		mChangeAction = false;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		SetNpcAiType(ENpcAiType.NpcBaseJobDoctor);
		if (!base.IsNpcBase || base.NpcJob != ENpcJob.Doctor)
		{
			return EndOperate();
		}
		if (base.WorkEntity == null || base.WorkEntity.gameLogic == null)
		{
			return EndOperate();
		}
		if (base.hasAnyRequest || !Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return EndOperate();
		}
		if (mChangeAction)
		{
			return EndOperate();
		}
		if (base.Cured != null && base.Cured.ContainsOperator(base.Operator) && !IsActionRunning(PEActionType.Operation))
		{
			if (IsActionRunning(PEActionType.Sleep))
			{
				EndAction(PEActionType.Sleep);
			}
			base.Cured.StopOperate(base.Operator, EOperationMask.Cure);
		}
		if (base.WorkEntity != null && m_MoveToPostion != base.WorkEntity.workTrans[0].position)
		{
			mChangeAction = true;
			m_MoveToPostion = base.WorkEntity.workTrans[0].position;
		}
		if (!base.Cured.ContainsOperator(base.Operator))
		{
			if (!base.Cured.CanOperate(base.transform))
			{
				Vector3 vector = ((base.WorkEntity.workTrans.Length != 2) ? m_MoveToPostion : base.WorkEntity.workTrans[1].position);
				MoveToPosition(vector, SpeedState.Run);
				if (IsReached(base.position, vector))
				{
					SetPosition(vector);
				}
				if (Stucking())
				{
					SetPosition(vector);
				}
			}
			else
			{
				StopMove();
				SetPosition(base.WorkEntity.workTrans[0].position);
				SetRotation(base.WorkEntity.workTrans[0].rotation);
				base.Cured.StartOperate(base.Operator, EOperationMask.Cure);
				SetNpcState(ENpcState.Work);
				mChangeAction = false;
			}
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (base.Creater != null && base.Creater.Assembly != null && base.NpcJobStae == ENpcState.Work && base.Cured != null && base.Cured.ContainsOperator(base.Operator))
		{
			base.Cured.StopOperate(base.Operator, EOperationMask.Cure);
		}
		m_MoveToPostion = Vector3.zero;
		SetNpcState(ENpcState.UnKnown);
	}
}
