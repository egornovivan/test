using Pathea;
using Pathea.Operate;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(NpcBaseInstructor), "NpcBaseInstructor")]
public class NpcBaseInstructor : BTNormal
{
	private Transform TeachTrans;

	private IOperation mInstructor;

	private Vector3 mDoorPos;

	private float m_Roate;

	private int mIndex;

	private float startEndActionTime;

	private float endActionTime = 2f;

	private void swichWorkTrans(int index)
	{
		TeachTrans = base.WorkEntity.workTrans[index];
		PETrainner pETrainner = base.Trainner as PETrainner;
		if (pETrainner != null)
		{
			mInstructor = pETrainner.Singles[index];
		}
	}

	private void StarTrain()
	{
		StopMove();
		SetPosition(TeachTrans.position);
		SetRotation(TeachTrans.rotation);
		mInstructor.StartOperate(base.Operator, EOperationMask.Practice);
	}

	private void EndTrain()
	{
	}

	private BehaveResult EndOperate(IOperation _operation, IOperator _IOperator)
	{
		if (((_operation != null && _operation.ContainsOperator(_IOperator)) || IsMotionRunning(PEActionType.Operation)) && startEndActionTime == 0f)
		{
			startEndActionTime = Time.time;
		}
		if (Time.time - startEndActionTime < endActionTime)
		{
			if (_operation != null && _operation.ContainsOperator(_IOperator))
			{
				_operation.StopOperate(_IOperator, EOperationMask.Practice);
			}
			else if (_IOperator != null && !_IOperator.Equals(null) && _IOperator.Operate != null && !_IOperator.Equals(null) && _IOperator.Operate.ContainsOperator(_IOperator))
			{
				_IOperator.Operate.StopOperate(_IOperator, EOperationMask.Practice);
			}
			return BehaveResult.Running;
		}
		startEndActionTime = 0f;
		return BehaveResult.Failure;
	}

	private BehaveResult Init(Tree sender)
	{
		if (!base.IsNpcBase)
		{
			return BehaveResult.Failure;
		}
		if (base.NpcJob != ENpcJob.Trainer)
		{
			return BehaveResult.Failure;
		}
		if (base.NpcTrainerType != ETrainerType.Instructor)
		{
			return BehaveResult.Failure;
		}
		if (base.WorkEntity == null || base.WorkEntity.workTrans == null)
		{
			return BehaveResult.Failure;
		}
		if (base.Trainner == null)
		{
			return BehaveResult.Failure;
		}
		mIndex = (int)base.NpcTrainingType;
		swichWorkTrans(mIndex);
		if (TeachTrans == null || mInstructor == null)
		{
			return BehaveResult.Failure;
		}
		if (!base.IsNpcTrainning)
		{
			return BehaveResult.Failure;
		}
		mDoorPos = base.WorkEntity.workTrans[4].position;
		m_Roate = base.WorkEntity.workTrans[4].rotation.eulerAngles.y;
		SetNpcState(ENpcState.Prepare);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		SetNpcAiType(ENpcAiType.NpcBaseJobTrain_Instructor);
		if (!base.IsNpcBase || base.NpcJob != ENpcJob.Trainer)
		{
			return EndOperate(mInstructor, base.Operator);
		}
		if (base.NpcTrainerType != ETrainerType.Instructor)
		{
			return EndOperate(mInstructor, base.Operator);
		}
		if (base.WorkEntity == null || base.WorkEntity.workTrans == null)
		{
			return EndOperate(mInstructor, base.Operator);
		}
		if (TeachTrans == null || base.Trainner == null || TeachTrans == null || mInstructor == null)
		{
			return EndOperate(mInstructor, base.Operator);
		}
		if (m_Roate != base.WorkEntity.workTrans[4].rotation.eulerAngles.y)
		{
			return EndOperate(mInstructor, base.Operator);
		}
		if (!base.IsNpcTrainning)
		{
			return EndOperate(mInstructor, base.Operator);
		}
		if (!mInstructor.ContainsOperator(base.Operator))
		{
			if (!base.Trainner.CanOperate(base.transform))
			{
				MoveToPosition(mDoorPos, SpeedState.Run);
				if (Stucking() && PEUtil.CheckErrorPos(mDoorPos))
				{
					SetPosition(mDoorPos);
				}
				if (IsReached(base.position, mDoorPos))
				{
					SetPosition(mDoorPos);
				}
			}
			else
			{
				StopMove();
				SetPosition(TeachTrans.position);
				SetRotation(TeachTrans.rotation);
				mInstructor.StartOperate(base.Operator, EOperationMask.Practice);
				SetNpcState(ENpcState.Work);
			}
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (base.Creater != null && base.Creater.Assembly != null && base.NpcJobStae == ENpcState.Work && base.Operator != null && base.Operator.Operate != null && base.Operator.Operate.ContainsOperator(base.Operator))
		{
			base.Operator.Operate.StopOperate(base.Operator, EOperationMask.Practice);
		}
		SetNpcState(ENpcState.UnKnown);
	}
}
