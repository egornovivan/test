using Pathea;
using Pathea.Operate;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(NpcInHospital), "NpcInHospital")]
public class NpcInHospital : BTNormal
{
	private class Data
	{
		[Behave]
		public string WaitAnim;

		[Behave]
		public float WaitTime;
	}

	private Data m_Data;

	private bool IsReadyTent;

	private bool HasReached;

	private bool HasLay;

	private CSMedicalTent m_CSMedicalTent;

	private Vector3 m_TentPos;

	private Sickbed m_sickbed;

	private float m_Roate;

	private float m_waitingTime;

	private Vector3 m_WaitingPos;

	private Vector3 GetPosition(Vector3 pos)
	{
		return PEUtil.GetRandomPositionOnGroundForWander(pos, 15f, 6f) + Vector3.up * 2f;
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!base.IsNpcBase)
		{
			return BehaveResult.Failure;
		}
		if (!base.IsNeedMedicine)
		{
			return BehaveResult.Failure;
		}
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Cure))
		{
			return BehaveResult.Failure;
		}
		if (!NpcThinkDb.CanDo(base.entity, EThinkingType.Cure))
		{
			return BehaveResult.Failure;
		}
		if (base.NpcMedicalState == ENpcMedicalState.Cure)
		{
			return BehaveResult.Success;
		}
		if (base.NpcMedicalState != ENpcMedicalState.SearchHospital)
		{
			return BehaveResult.Failure;
		}
		m_CSMedicalTent = CSMain.FindMedicalTent(out IsReadyTent, base.entity, out m_sickbed);
		if (m_CSMedicalTent == null || m_sickbed == null || m_sickbed.bedLay == null || m_sickbed.bedLay.m_StandTrans == null || !m_CSMedicalTent.IsDoctorReady())
		{
			m_waitingTime = Time.time;
			SetMedicineSate(ENpcMedicalState.WaitForTent);
			return BehaveResult.Success;
		}
		if (IsReadyTent && base.Sleep != null && base.Sleep.ContainsOperator(base.Operator))
		{
			base.Sleep.StopOperate(base.Operator, EOperationMask.Sleep);
		}
		m_Roate = m_CSMedicalTent.workTrans[0].rotation.eulerAngles.y;
		if (base.NpcMedicalState == ENpcMedicalState.In_Hospital)
		{
			if (!CSMain.TryGetTent(base.entity))
			{
				return BehaveResult.Failure;
			}
			SetPosition(m_sickbed.bedLay.m_StandTrans.position);
			SetRotation(m_sickbed.bedLay.m_StandTrans.rotation);
			m_sickbed.bedLay.StartOperate(base.Operator, EOperationMask.Lay);
			HasReached = true;
			return BehaveResult.Success;
		}
		if (!IsReadyTent)
		{
			m_waitingTime = Time.time;
			SetMedicineSate(ENpcMedicalState.WaitForTent);
		}
		HasReached = false;
		return BehaveResult.Success;
	}

	private BehaveResult Tick(Tree sender)
	{
		SetNpcAiType(ENpcAiType.NpcInHospital);
		if (!base.IsNpcBase)
		{
			return BehaveResult.Failure;
		}
		if (!base.IsNeedMedicine)
		{
			return BehaveResult.Failure;
		}
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Cure) && m_sickbed != null)
		{
			if (m_sickbed.bedLay.ContainsOperator(base.Operator))
			{
				m_sickbed.bedLay.StopOperate(base.Operator, EOperationMask.Lay);
			}
			CSMain.KickOutFromHospital(base.entity);
			return BehaveResult.Failure;
		}
		if (base.NpcMedicalState == ENpcMedicalState.Cure || base.NpcMedicalState == ENpcMedicalState.SearchTreat || base.NpcMedicalState == ENpcMedicalState.SearchDiagnos)
		{
			if (m_sickbed != null && m_sickbed.bedLay.ContainsOperator(base.Operator))
			{
				m_sickbed.bedLay.StopOperate(base.Operator, EOperationMask.Lay);
			}
			return BehaveResult.Success;
		}
		if (base.NpcMedicalState != ENpcMedicalState.In_Hospital && !NpcThinkDb.CanDoing(base.entity, EThinkingType.Cure))
		{
			return BehaveResult.Failure;
		}
		if (base.NpcMedicalState == ENpcMedicalState.WaitForTent)
		{
			if (Time.time - m_waitingTime >= m_Data.WaitTime)
			{
				SetMedicineSate(ENpcMedicalState.SearchHospital);
				return BehaveResult.Failure;
			}
			if (base.Operator != null && base.Operator.Operate != null && base.Operator.Operate.ContainsOperator(base.Operator) && IsMotionRunning(PEActionType.Cure))
			{
				base.Operator.Operate.StopOperate(base.Operator, EOperationMask.Lay);
			}
			if (base.Sleep != null && !base.Sleep.ContainsOperator(base.Operator))
			{
				if (!base.Sleep.CanOperate(base.transform))
				{
					MoveToPosition(base.Sleep.Trans.position);
					if (Stucking(3f))
					{
						SetPosition(base.Sleep.Trans.position);
					}
				}
				else
				{
					PESleep pESleep = base.Sleep as PESleep;
					if (null != pESleep)
					{
						PEActionParamVQNS param = PEActionParamVQNS.param;
						param.vec = pESleep.transform.position;
						param.q = pESleep.transform.rotation;
						param.n = 0;
						param.str = m_Data.WaitAnim;
						pESleep.StartOperate(base.Operator, EOperationMask.Sleep, param);
					}
				}
			}
			if (base.Sleep != null && base.Sleep.ContainsOperator(base.Operator) && !IsMotionRunning(PEActionType.Sleep))
			{
				base.Sleep.StopOperate(base.Operator, EOperationMask.Sleep);
			}
			return BehaveResult.Running;
		}
		if (HasReached && base.NpcMedicalState == ENpcMedicalState.SearchHospital)
		{
			if (!CSMain.TryGetTent(base.entity))
			{
				m_waitingTime = Time.time;
				SetMedicineSate(ENpcMedicalState.WaitForTent);
				HasReached = false;
				return BehaveResult.Running;
			}
			if (m_sickbed != null && m_sickbed.bedLay.CanOperateMask(EOperationMask.Lay) && !m_sickbed.bedLay.ContainsOperator(base.Operator))
			{
				SetPosition(m_sickbed.bedLay.m_StandTrans.position);
				SetRotation(m_sickbed.bedLay.m_StandTrans.rotation);
				m_sickbed.bedLay.StartOperate(base.Operator, EOperationMask.Lay);
			}
		}
		if (base.NpcMedicalState == ENpcMedicalState.In_Hospital)
		{
			if (m_sickbed != null && m_sickbed.bedLay.CanOperateMask(EOperationMask.Lay) && !m_sickbed.bedLay.ContainsOperator(base.Operator))
			{
				SetPosition(m_sickbed.bedLay.m_StandTrans.position);
				SetRotation(m_sickbed.bedLay.m_StandTrans.rotation);
				m_sickbed.bedLay.StartOperate(base.Operator, EOperationMask.Lay);
			}
			return BehaveResult.Running;
		}
		CSMedicalTent cSMedicalTent = CSMain.FindTentMachine(base.entity);
		if (cSMedicalTent == null)
		{
			if (base.Operator != null && base.Operator.Operate != null && base.Operator.Operate.ContainsOperator(base.Operator))
			{
				base.Operator.Operate.StopOperate(base.Operator, EOperationMask.Lay);
			}
			SetMedicineSate(ENpcMedicalState.WaitForTent);
			return BehaveResult.Failure;
		}
		if (m_sickbed != null && m_Roate != cSMedicalTent.workTrans[0].rotation.eulerAngles.y && base.NpcMedicalState == ENpcMedicalState.In_Hospital)
		{
			m_sickbed.bedLay.StopOperate(base.Operator, EOperationMask.Lay);
			if (!IsMotionRunning(PEActionType.Cure))
			{
				m_sickbed.bedLay.StartOperate(base.Operator, EOperationMask.Lay);
				m_Roate = cSMedicalTent.workTrans[0].rotation.eulerAngles.y;
				m_sickbed.ReleaseMachine();
			}
		}
		if (!HasReached && (ReachToPostion(cSMedicalTent.workTrans[1].position) || Stucking(5f)))
		{
			StopMove();
			HasReached = true;
			if (m_sickbed != null && m_sickbed.bedLay != null && m_sickbed.bedLay.ContainsOperator(base.Operator))
			{
				m_sickbed.bedLay.StopOperate(base.Operator, EOperationMask.Lay);
			}
		}
		return BehaveResult.Running;
	}
}
