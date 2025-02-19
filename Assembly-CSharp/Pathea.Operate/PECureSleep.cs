using System.Collections.Generic;
using UnityEngine;

namespace Pathea.Operate;

public class PECureSleep : MonoBehaviour
{
	private IOperator Operator;

	private PeEntity mSleepEntity;

	private float mStartLayTime;

	private bool mIsPase;

	public float REOVE_TIME;

	private bool hasOperator => Operator != null && !Operator.Equals(null);

	private void Start()
	{
	}

	private void Update()
	{
		if (hasOperator && mSleepEntity == null)
		{
			OperateCmpt operateCmpt = Operator as OperateCmpt;
			if (operateCmpt != null)
			{
				mSleepEntity = operateCmpt.Entity;
			}
		}
		if (!(mSleepEntity != null) || !(mSleepEntity.Alnormal != null) || REOVE_TIME == 0f || !(Time.time - mStartLayTime >= REOVE_TIME) || mIsPase)
		{
			return;
		}
		if (Operator != null && !Operator.Equals(null) && Operator.Operate != null && !Operator.Operate.Equals(null))
		{
			if (Operator.Operate.StopOperate(Operator, EOperationMask.Sleep) && mSleepEntity != null && mSleepEntity.NpcCmpt != null)
			{
				mSleepEntity.NpcCmpt.IsNeedMedicine = false;
				EndAlnormal(mSleepEntity);
				mSleepEntity = null;
			}
		}
		else if (mSleepEntity != null && mSleepEntity.NpcCmpt != null)
		{
			mSleepEntity.NpcCmpt.IsNeedMedicine = false;
			EndAlnormal(mSleepEntity);
			mSleepEntity = null;
		}
	}

	private void EndAlnormal(PeEntity entity)
	{
		List<PEAbnormalType> activeAbnormalList = entity.Alnormal.GetActiveAbnormalList();
		if (activeAbnormalList != null)
		{
			for (int i = 0; i < activeAbnormalList.Count; i++)
			{
				entity.Alnormal.EndAbnormalCondition(activeAbnormalList[i]);
			}
		}
	}

	public void AddOperator(IOperator oper)
	{
		Operator = oper;
		mStartLayTime = ((!mIsPase) ? Time.time : mStartLayTime);
		mIsPase = false;
	}

	public void RemveOperator()
	{
		Operator = null;
		mIsPase = !(Time.time - mStartLayTime >= REOVE_TIME);
		mStartLayTime = ((!mIsPase) ? 0f : mStartLayTime);
	}
}
