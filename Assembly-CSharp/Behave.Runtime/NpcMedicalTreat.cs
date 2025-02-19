using Pathea;
using Pathea.Operate;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(NpcMedicalTreat), "NpcMedicalTreat")]
public class NpcMedicalTreat : BTNormal
{
	private class Data
	{
		[Behave]
		public string WaitAnim;

		[Behave]
		public float WaitTime;
	}

	private Data m_Data;

	private bool IsReadyTreat;

	private bool HasReached;

	private CSMedicalTreat m_CSMedicalTreat;

	private Vector3 m_Moveposition;

	private float m_Roate;

	private float m_StartTime;

	private PEPatients m_pePatitents;

	private float mWaitingTime;

	private Vector3 m_WaitingPos;

	private Vector3 GetPosition(Vector3 pos)
	{
		return PEUtil.GetRandomPosition(pos, 10f, 10f) + Vector3.up * 2f;
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
		if (base.NpcMedicalState == ENpcMedicalState.SearchHospital)
		{
			return BehaveResult.Success;
		}
		if (base.NpcMedicalState != ENpcMedicalState.Treating && base.NpcMedicalState != ENpcMedicalState.SearchTreat)
		{
			return BehaveResult.Failure;
		}
		m_CSMedicalTreat = CSMain.FindMedicalTreat(out IsReadyTreat, base.entity);
		if (m_CSMedicalTreat == null || m_CSMedicalTreat.resultTrans == null || !m_CSMedicalTreat.IsDoctorReady() || !m_CSMedicalTreat.IsMedicineReady())
		{
			mWaitingTime = Time.time;
			SetMedicineSate(ENpcMedicalState.WaitForTreat);
			return BehaveResult.Success;
		}
		if (IsReadyTreat && base.Sleep != null && base.Sleep.ContainsOperator(base.Operator))
		{
			base.Sleep.StopOperate(base.Operator, EOperationMask.Sleep);
		}
		m_Moveposition = m_CSMedicalTreat.resultTrans[1].position;
		m_Roate = m_CSMedicalTreat.resultTrans[1].rotation.eulerAngles.y;
		m_pePatitents = m_CSMedicalTreat.pePatient;
		if (base.NpcMedicalState == ENpcMedicalState.Treating)
		{
			if (!CSMain.TryGetTreat(base.entity))
			{
				return BehaveResult.Failure;
			}
			SetPosition(m_CSMedicalTreat.resultTrans[1].position);
			SetRotation(m_CSMedicalTreat.resultTrans[1].rotation);
			m_CSMedicalTreat.pePatient.StartOperate(base.Operator, EOperationMask.Lay);
			SetPosition(m_CSMedicalTreat.resultTrans[0].position);
			HasReached = true;
			return BehaveResult.Success;
		}
		if (!IsReadyTreat)
		{
			mWaitingTime = Time.time;
			SetMedicineSate(ENpcMedicalState.WaitForTreat);
		}
		HasReached = false;
		m_StartTime = Time.time;
		return BehaveResult.Success;
	}

	private BehaveResult Tick(Tree sender)
	{
		SetNpcAiType(ENpcAiType.NpcMedicalTreat);
		if (!base.IsNpcBase)
		{
			return BehaveResult.Failure;
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
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Cure) && m_CSMedicalTreat != null)
		{
			if (m_CSMedicalTreat.pePatient.ContainsOperator(base.Operator))
			{
				m_CSMedicalTreat.pePatient.StopOperate(base.Operator, EOperationMask.Lay);
			}
			CSMain.KickOutFromHospital(base.entity);
			return BehaveResult.Failure;
		}
		if (base.NpcMedicalState == ENpcMedicalState.Cure || base.NpcMedicalState == ENpcMedicalState.SearchHospital || base.NpcMedicalState == ENpcMedicalState.SearchDiagnos)
		{
			if (m_CSMedicalTreat != null && m_CSMedicalTreat.pePatient.ContainsOperator(base.Operator))
			{
				m_CSMedicalTreat.pePatient.StopOperate(base.Operator, EOperationMask.Lay);
			}
			return BehaveResult.Success;
		}
		if (base.NpcMedicalState != ENpcMedicalState.Treating && !NpcThinkDb.CanDoing(base.entity, EThinkingType.Cure))
		{
			return BehaveResult.Failure;
		}
		if (base.NpcMedicalState == ENpcMedicalState.WaitForTreat)
		{
			if (Time.time - mWaitingTime >= m_Data.WaitTime)
			{
				SetMedicineSate(ENpcMedicalState.SearchTreat);
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
		if (HasReached && base.NpcMedicalState == ENpcMedicalState.SearchTreat)
		{
			if (!CSMain.TryGetTreat(base.entity))
			{
				mWaitingTime = Time.time;
				SetMedicineSate(ENpcMedicalState.WaitForTreat);
				HasReached = false;
				return BehaveResult.Running;
			}
			if (m_CSMedicalTreat.pePatient.CanOperateMask(EOperationMask.Lay) && !m_CSMedicalTreat.pePatient.ContainsOperator(base.Operator))
			{
				SetPosition(m_CSMedicalTreat.resultTrans[1].position);
				SetRotation(m_CSMedicalTreat.resultTrans[1].rotation);
				m_CSMedicalTreat.pePatient.StartOperate(base.Operator, EOperationMask.Lay);
				SetPosition(m_CSMedicalTreat.resultTrans[0].position);
			}
		}
		if (base.NpcMedicalState == ENpcMedicalState.Treating && m_CSMedicalTreat.pePatient.CanOperateMask(EOperationMask.Lay) && !m_CSMedicalTreat.pePatient.ContainsOperator(base.Operator))
		{
			SetPosition(m_CSMedicalTreat.resultTrans[1].position);
			SetRotation(m_CSMedicalTreat.resultTrans[1].rotation);
			m_CSMedicalTreat.pePatient.StartOperate(base.Operator, EOperationMask.Lay);
			SetPosition(m_CSMedicalTreat.resultTrans[0].position);
		}
		if (base.NpcMedicalState == ENpcMedicalState.Cure)
		{
			m_CSMedicalTreat.pePatient.StopOperate(base.Operator, EOperationMask.Lay);
			return BehaveResult.Success;
		}
		CSMedicalTreat cSMedicalTreat = CSMain.FindTreatMachine(base.entity);
		if (cSMedicalTreat == null)
		{
			if (base.Operator != null && base.Operator.Operate != null && base.Operator.Operate.ContainsOperator(base.Operator))
			{
				base.Operator.Operate.StopOperate(base.Operator, EOperationMask.Lay);
			}
			SetMedicineSate(ENpcMedicalState.WaitForTreat);
			return BehaveResult.Failure;
		}
		if (cSMedicalTreat != null && m_Roate != cSMedicalTreat.resultTrans[1].rotation.eulerAngles.y)
		{
			cSMedicalTreat.pePatient.StopOperate(base.Operator, EOperationMask.Lay);
			if (!IsMotionRunning(PEActionType.Cure))
			{
				cSMedicalTreat.pePatient.StartOperate(base.Operator, EOperationMask.Lay);
				m_Roate = cSMedicalTreat.resultTrans[1].rotation.eulerAngles.y;
			}
		}
		if (!HasReached && (ReachToPostion(m_Moveposition) || Stucking(5f)))
		{
			StopMove();
			HasReached = true;
			if (m_CSMedicalTreat.pePatient.ContainsOperator(base.Operator))
			{
				m_CSMedicalTreat.pePatient.StopOperate(base.Operator, EOperationMask.Lay);
			}
		}
		return BehaveResult.Running;
	}
}
