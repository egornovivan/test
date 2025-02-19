using Pathea;
using Pathea.Operate;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcBaseWork), "NpcBaseWork")]
public class BTNpcBaseWork : BTNormal
{
	private Vector3 mStandPostion;

	private bool mChangePlace;

	private float startEndActionTime;

	private float endActionTime = 2f;

	private PEMachine mMachine;

	private PEWork mWork;

	private void StopWork()
	{
		SetNpcState(ENpcState.UnKnown);
		if (base.Operator != null && base.Operator.Operate != null && base.Operator.Operate.ContainsOperator(base.Operator))
		{
			base.Operator.Operate.StopOperate(base.Operator, EOperationMask.Work);
		}
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
				_operation.StopOperate(_IOperator, EOperationMask.Work);
			}
			else if (_IOperator != null && !_IOperator.Equals(null) && _IOperator.Operate != null && !_IOperator.Equals(null) && _IOperator.Operate.ContainsOperator(_IOperator))
			{
				_IOperator.Operate.StopOperate(_IOperator, EOperationMask.Work);
			}
			return BehaveResult.Running;
		}
		startEndActionTime = 0f;
		return BehaveResult.Failure;
	}

	private BehaveResult Init(Tree sender)
	{
		if (!base.IsNpcBase || base.NpcJob != ENpcJob.Worker)
		{
			return BehaveResult.Failure;
		}
		if (base.Work == null || base.Work.Equals(null) || base.Operator == null || base.Operator.Equals(null))
		{
			return BehaveResult.Failure;
		}
		mMachine = base.Work as PEMachine;
		if (mMachine == null)
		{
			return BehaveResult.Failure;
		}
		mWork = mMachine.GetStartOperate(EOperationMask.Work) as PEWork;
		if (mWork == null)
		{
			return BehaveResult.Failure;
		}
		SetNpcState(ENpcState.Prepare);
		mStandPostion = mWork.Trans.position;
		mChangePlace = false;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!base.IsNpcBase || base.NpcJob != ENpcJob.Worker || base.hasAnyRequest || !Enemy.IsNullOrInvalid(base.attackEnemy) || base.Work == null || base.Work.Equals(null) || base.Operator == null || base.Operator.Equals(null))
		{
			return EndOperate(base.Work, base.Operator);
		}
		if (mChangePlace)
		{
			return EndOperate(base.Work, base.Operator);
		}
		if (base.Work != null && base.Work.ContainsOperator(base.Operator) && !IsMotionRunning(PEActionType.Operation))
		{
			if (IsActionRunning(PEActionType.Sleep))
			{
				EndAction(PEActionType.Sleep);
			}
			mWork.StopOperate(base.Operator, EOperationMask.Work);
		}
		SetNpcAiType(ENpcAiType.NpcBaseWorker);
		PEMachine pEMachine = base.Work as PEMachine;
		if (base.NpcJobStae == ENpcState.Work && pEMachine != null && !mMachine.Equals(pEMachine))
		{
			mChangePlace = true;
			mStandPostion = mWork.Trans.position;
		}
		if (!base.Work.ContainsOperator(base.Operator))
		{
			if (!base.Work.CanOperate(base.transform))
			{
				bool flag = PEUtil.IsUnderBlock(base.entity);
				bool flag2 = PEUtil.IsForwardBlock(base.entity, base.entity.peTrans.forward, 2f);
				if (flag)
				{
					SetPosition(mWork.Trans.position);
				}
				else
				{
					MoveToPosition(mWork.Trans.position, SpeedState.Run);
				}
				if (flag2)
				{
					SetPosition(mWork.Trans.position, neeedrepair: false);
				}
				if (Stucking())
				{
					SetPosition(mWork.Trans.position, neeedrepair: false);
				}
				if (IsReached(base.position, mWork.Trans.position))
				{
					SetPosition(mWork.Trans.position, neeedrepair: false);
				}
			}
			else
			{
				SetNpcState(ENpcState.Work);
				MoveToPosition(Vector3.zero);
				mWork = mMachine.GetStartOperate(EOperationMask.Work) as PEWork;
				mStandPostion = mWork.Trans.position;
				mChangePlace = false;
				mWork.StartOperate(base.Operator, EOperationMask.Work);
			}
		}
		else if (base.position != mWork.Trans.position)
		{
			SetPosition(mWork.Trans.position, neeedrepair: false);
			SetRotation(mWork.Trans.rotation);
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (base.Creater != null && base.Creater.Assembly != null && base.NpcJobStae == ENpcState.Work && base.Operator != null && base.Operator.Operate != null && base.Operator.Operate.ContainsOperator(base.Operator))
		{
			base.Operator.Operate.StopOperate(base.Operator, EOperationMask.Work);
		}
		SetNpcState(ENpcState.UnKnown);
	}
}
