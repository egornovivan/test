using Pathea;
using Pathea.Operate;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(NpcMedicalDiagnose), "NpcMedicalDiagnose")]
public class NpcMedicalDiagnose : BTNormal
{
	private class Data
	{
		[Behave]
		public string WaitAnim;

		[Behave]
		public float WaitTime;
	}

	private Data m_Data;

	private bool IsReadyDiagnose;

	private bool HasReached;

	private CSMedicalCheck m_CSMedicalCheck;

	private Vector3 m_Moveposition;

	private float m_Roate;

	private float m_WaitingTime;

	private Vector3 m_WaitingPos;

	private Vector3 GetPosition(Vector3 pos)
	{
		return PEUtil.GetRandomPosition(pos, 7f, 7f) + Vector3.up * 2f;
	}

	private bool EndCureSleep()
	{
		if (base.Sleep != null && !base.Sleep.Equals(null) && base.Sleep.ContainsOperator(base.Operator))
		{
			return base.Sleep.StopOperate(base.Operator, EOperationMask.Sleep);
		}
		if (base.Operator != null && !base.Operator.Equals(null) && !base.Operator.Operate.Equals(null) && base.Operator.Operate.ContainsOperator(base.Operator))
		{
			return base.Operator.Operate.StartOperate(base.Operator, EOperationMask.Sleep);
		}
		return true;
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
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Cure))
		{
			return BehaveResult.Failure;
		}
		if (!NpcThinkDb.CanDo(base.entity, EThinkingType.Cure))
		{
			return BehaveResult.Failure;
		}
		if (base.NpcMedicalState == ENpcMedicalState.SearchTreat || base.NpcMedicalState == ENpcMedicalState.SearchHospital || base.NpcMedicalState == ENpcMedicalState.Treating)
		{
			return BehaveResult.Success;
		}
		if (!base.IsNeedMedicine)
		{
			return BehaveResult.Failure;
		}
		m_CSMedicalCheck = CSMain.FindMedicalCheck(out IsReadyDiagnose, base.entity);
		if (m_CSMedicalCheck == null || m_CSMedicalCheck.resultTrans == null || !m_CSMedicalCheck.IsDoctorReady())
		{
			m_WaitingTime = Time.time;
			SetMedicineSate(ENpcMedicalState.WaitForDiagnos);
			return BehaveResult.Success;
		}
		if (IsReadyDiagnose && base.Sleep != null && base.Sleep.ContainsOperator(base.Operator))
		{
			base.Sleep.StopOperate(base.Operator, EOperationMask.Sleep);
		}
		m_Moveposition = m_CSMedicalCheck.resultTrans[1].position;
		m_Roate = m_CSMedicalCheck.resultTrans[1].rotation.eulerAngles.y;
		if (base.NpcMedicalState == ENpcMedicalState.Diagnosing)
		{
			if (!CSMain.TryGetCheck(base.entity))
			{
				return BehaveResult.Failure;
			}
			SetPosition(m_CSMedicalCheck.resultTrans[1].position);
			SetRotation(m_CSMedicalCheck.resultTrans[1].rotation);
			m_CSMedicalCheck.pePatient.StartOperate(base.Operator, EOperationMask.Lay);
			SetPosition(m_CSMedicalCheck.resultTrans[0].position);
			HasReached = true;
			return BehaveResult.Success;
		}
		m_WaitingTime = 0f;
		if (IsReadyDiagnose)
		{
			SetMedicineSate(ENpcMedicalState.SearchDiagnos);
		}
		else
		{
			m_WaitingTime = Time.time;
			SetMedicineSate(ENpcMedicalState.WaitForDiagnos);
		}
		HasReached = false;
		return BehaveResult.Success;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!base.IsNpcBase || base.Sleep == null || base.Sleep.Equals(null))
		{
			if (EndCureSleep())
			{
				return BehaveResult.Failure;
			}
			return BehaveResult.Running;
		}
		if (!base.IsNeedMedicine)
		{
			if (base.Operator != null && base.Operator.Operate != null && base.Operator.Operate.ContainsOperator(base.Operator) && IsMotionRunning(PEActionType.Cure))
			{
				base.Operator.Operate.StopOperate(base.Operator, EOperationMask.Lay);
			}
			if (base.Sleep != null && base.Sleep.ContainsOperator(base.Operator) && IsMotionRunning(PEActionType.Sleep))
			{
				base.Sleep.StopOperate(base.Operator, EOperationMask.Sleep);
			}
			return BehaveResult.Failure;
		}
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Cure) && m_CSMedicalCheck != null)
		{
			if (m_CSMedicalCheck.pePatient.ContainsOperator(base.Operator))
			{
				m_CSMedicalCheck.pePatient.StopOperate(base.Operator, EOperationMask.Lay);
			}
			CSMain.KickOutFromHospital(base.entity);
			return BehaveResult.Failure;
		}
		if (base.NpcMedicalState == ENpcMedicalState.Cure || base.NpcMedicalState == ENpcMedicalState.SearchTreat || base.NpcMedicalState == ENpcMedicalState.Treating || base.NpcMedicalState == ENpcMedicalState.SearchHospital)
		{
			if (base.Operator != null && base.Operator.Operate != null && base.Operator.Operate.ContainsOperator(base.Operator) && IsMotionRunning(PEActionType.Cure))
			{
				base.Operator.Operate.StopOperate(base.Operator, EOperationMask.Lay);
			}
			return BehaveResult.Success;
		}
		if (base.NpcMedicalState != ENpcMedicalState.Diagnosing && !NpcThinkDb.CanDoing(base.entity, EThinkingType.Cure))
		{
			if (EndCureSleep())
			{
				return BehaveResult.Failure;
			}
			return BehaveResult.Running;
		}
		if (base.NpcMedicalState == ENpcMedicalState.WaitForDiagnos)
		{
			if (Time.time - m_WaitingTime > m_Data.WaitTime)
			{
				SetMedicineSate(ENpcMedicalState.SearchDiagnos);
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
					if (IsReached(base.position, base.Sleep.Trans.position))
					{
						SetPosition(base.Sleep.Trans.position);
					}
				}
				else
				{
					PEBed pEBed = base.Sleep as PEBed;
					PESleep pESleep = pEBed.GetStartOperate(EOperationMask.Sleep) as PESleep;
					if (null != pESleep)
					{
						PEActionParamVQNS param = PEActionParamVQNS.param;
						param.vec = pESleep.transform.position;
						param.q = pESleep.transform.rotation;
						param.n = 0;
						param.str = m_Data.WaitAnim;
						pEBed.StartOperate(base.Operator, EOperationMask.Sleep, param);
					}
				}
			}
			if (base.Sleep != null && base.Sleep.ContainsOperator(base.Operator) && !IsMotionRunning(PEActionType.Sleep))
			{
				base.Sleep.StopOperate(base.Operator, EOperationMask.Sleep);
			}
			return BehaveResult.Running;
		}
		if (HasReached && base.NpcMedicalState == ENpcMedicalState.SearchDiagnos)
		{
			if (!CSMain.TryGetCheck(base.entity))
			{
				m_WaitingTime = Time.time;
				SetMedicineSate(ENpcMedicalState.WaitForDiagnos);
				HasReached = false;
				return BehaveResult.Running;
			}
			if (!m_CSMedicalCheck.pePatient.ContainsOperator(base.Operator))
			{
				SetPosition(m_CSMedicalCheck.resultTrans[1].position);
				SetRotation(m_CSMedicalCheck.resultTrans[1].rotation);
				m_CSMedicalCheck.pePatient.StartOperate(base.Operator, EOperationMask.Lay);
				SetPosition(m_CSMedicalCheck.resultTrans[0].position);
			}
		}
		if (base.NpcMedicalState == ENpcMedicalState.Diagnosing)
		{
			if (!m_CSMedicalCheck.pePatient.ContainsOperator(base.Operator))
			{
				SetPosition(m_CSMedicalCheck.resultTrans[1].position);
				SetRotation(m_CSMedicalCheck.resultTrans[1].rotation);
				m_CSMedicalCheck.pePatient.StartOperate(base.Operator, EOperationMask.Lay);
				SetPosition(m_CSMedicalCheck.resultTrans[0].position);
			}
			return BehaveResult.Running;
		}
		if (base.NpcMedicalState == ENpcMedicalState.Cure)
		{
			m_CSMedicalCheck.pePatient.StopOperate(base.Operator, EOperationMask.Lay);
			return BehaveResult.Success;
		}
		CSMedicalCheck cSMedicalCheck = CSMain.FindCheckMachine(base.entity);
		if (cSMedicalCheck == null)
		{
			if (base.Operator != null && base.Operator.Operate != null && base.Operator.Operate.ContainsOperator(base.Operator))
			{
				base.Operator.Operate.StopOperate(base.Operator, EOperationMask.Lay);
			}
			SetMedicineSate(ENpcMedicalState.WaitForDiagnos);
			return BehaveResult.Failure;
		}
		if (cSMedicalCheck != null && base.NpcMedicalState == ENpcMedicalState.Diagnosing && m_Roate != cSMedicalCheck.resultTrans[1].rotation.eulerAngles.y)
		{
			cSMedicalCheck.pePatient.StopOperate(base.Operator, EOperationMask.Lay);
			if (!IsMotionRunning(PEActionType.Cure))
			{
				cSMedicalCheck.pePatient.StartOperate(base.Operator, EOperationMask.Lay);
				m_Roate = cSMedicalCheck.resultTrans[1].rotation.eulerAngles.y;
			}
		}
		if (!HasReached && Stucking(5f))
		{
			StopMove();
			HasReached = true;
			if (m_CSMedicalCheck.pePatient.ContainsOperator(base.Operator))
			{
				m_CSMedicalCheck.pePatient.StopOperate(base.Operator, EOperationMask.Lay);
			}
		}
		if (!HasReached && ReachToPostion(m_Moveposition))
		{
			StopMove();
			HasReached = true;
			if (m_CSMedicalCheck.pePatient.ContainsOperator(base.Operator))
			{
				m_CSMedicalCheck.pePatient.StopOperate(base.Operator, EOperationMask.Lay);
			}
		}
		return BehaveResult.Running;
	}
}
