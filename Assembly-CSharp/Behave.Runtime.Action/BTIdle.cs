using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTIdle), "Idle")]
public class BTIdle : BTNormal
{
	private class Data
	{
		[Behave]
		public string idle = string.Empty;

		[Behave]
		public float prob;

		[Behave]
		public float minTime;

		[Behave]
		public float maxTime;

		[Behave]
		public float relaxProb;

		[Behave]
		public float relaxTime;

		[Behave]
		public string[] relax = new string[0];

		public float m_StartIdleTime;

		public float m_CurrentIdleTime;

		public float m_StartRestTime;
	}

	private Data m_Data;

	private Enemy m_Escape;

	private Enemy m_Threat;

	public static float _IK_Aim_Time0 = 5f;

	public static float _Ik_Aim_Time1 = 8f;

	public static float _Ik_Aim_Time2 = 3f;

	private float startIkTime;

	private bool inIKAim;

	private bool DoingRelax()
	{
		if (base.entity.commonCmpt.Race != ERace.Mankind)
		{
			return false;
		}
		for (int i = 0; i < m_Data.relax.Length; i++)
		{
			if (base.entity.animCmpt != null && base.entity.animCmpt.animator != null && base.entity.animCmpt.ContainsParameter(m_Data.relax[i]) && base.entity.animCmpt.animator.GetBool(m_Data.relax[i]))
			{
				return true;
			}
		}
		return false;
	}

	private void NpcIKVeer()
	{
		if (base.entity.NpcCmpt != null && Time.time - startIkTime >= _IK_Aim_Time0 && Time.time - startIkTime <= _IK_Aim_Time0 + _Ik_Aim_Time1)
		{
			if (PeSingleton<PeCreature>.Instance != null && PEUtil.InAimDistance(PeSingleton<PeCreature>.Instance.mainPlayer.position, base.position, 1f, 10f) && PEUtil.InAimAngle(PeSingleton<PeCreature>.Instance.mainPlayer.peTrans.existent, base.existent, 60f))
			{
				inIKAim = true;
				SetIKTargetPos(PEUtil.CalculateAimPos(PeSingleton<PeCreature>.Instance.mainPlayer.peTrans.existent.position, base.position));
				SetIKLerpspeed(NPCConstNum.IK_Aim_Lerpspeed_0);
				SetIKActive(inIKAim);
			}
			if (inIKAim && (!PEUtil.InAimDistance(PeSingleton<PeCreature>.Instance.mainPlayer.position, base.position, 1f, 10f) || !PEUtil.InAimAngle(PeSingleton<PeCreature>.Instance.mainPlayer.peTrans.existent, base.existent, 60f)))
			{
				SetIKTargetPos(PEUtil.CalculateAimPos(base.position + base.existent.forward * 3f, base.position));
				SetIKLerpspeed(NPCConstNum.IK_Aim_Lerpspeed_0);
			}
		}
		else if (base.entity.NpcCmpt != null && inIKAim && Time.time - startIkTime > _IK_Aim_Time0 + _Ik_Aim_Time1 && Time.time - startIkTime <= _IK_Aim_Time0 + _Ik_Aim_Time1 + _Ik_Aim_Time2)
		{
			SetIKTargetPos(PEUtil.CalculateAimPos(base.position + base.existent.forward * 3f, base.position));
			SetIKLerpspeed(NPCConstNum.IK_Aim_Lerpspeed_0);
		}
		else if (base.entity.NpcCmpt != null && Time.time - startIkTime > _IK_Aim_Time0 + _Ik_Aim_Time1 + _Ik_Aim_Time2)
		{
			inIKAim = false;
			startIkTime = Time.time;
			SetIKLerpspeed(100f);
			SetIKActive(inIKAim);
		}
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest || base.entity.Food != null || base.entity.IsDarkInDaytime || base.entity.Chat != null)
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.escapeEnemy))
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (base.entity.Treat != null && !base.entity.Treat.IsDeath() && base.entity.Treat.hasView)
		{
			return BehaveResult.Failure;
		}
		if (!EvadePolarShield(base.position))
		{
			return BehaveResult.Failure;
		}
		if (Random.value > m_Data.prob)
		{
			return BehaveResult.Failure;
		}
		if (base.entity.NpcCmpt != null && !NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Leisure))
		{
			return BehaveResult.Failure;
		}
		m_Data.m_StartRestTime = Time.time;
		m_Data.m_StartIdleTime = Time.time;
		startIkTime = Time.time;
		inIKAim = false;
		m_Data.m_CurrentIdleTime = Random.Range(m_Data.relaxTime, m_Data.maxTime);
		StopMove();
		SetBool(m_Data.idle, value: true);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		SetNpcAiType(ENpcAiType.FieldNpcIdle_Idle);
		if (base.entity.NpcCmpt != null && !NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Leisure))
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.escapeEnemy))
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (base.entity.Treat != null && !base.entity.Treat.IsDeath() && base.entity.Treat.hasView)
		{
			return BehaveResult.Failure;
		}
		if (!EvadePolarShield(base.position))
		{
			return BehaveResult.Failure;
		}
		if (IsMotionRunning(PEActionType.HoldShield))
		{
			EndAction(PEActionType.HoldShield);
		}
		if (base.hasAnyRequest || base.entity.Food != null || base.entity.IsDarkInDaytime || base.entity.Chat != null)
		{
			return BehaveResult.Failure;
		}
		NpcIKVeer();
		if (GetBool("BehaveWaiting") || GetBool("Leisureing") || DoingRelax() || inIKAim)
		{
			return BehaveResult.Running;
		}
		if (GetBool("OperateMed"))
		{
			SetBool("OperateMed", value: false);
		}
		if (GetBool("OperateCom"))
		{
			SetBool("OperateCom", value: false);
		}
		if (GetBool("FixLifeboat"))
		{
			SetBool("FixLifeboat", value: false);
		}
		if (Time.time - m_Data.m_StartIdleTime > m_Data.m_CurrentIdleTime)
		{
			return BehaveResult.Success;
		}
		if (Time.time - m_Data.m_StartRestTime > m_Data.relaxTime)
		{
			m_Data.m_StartRestTime = Time.time;
			if (!GetBool("BehaveWaiting") && !GetBool("Leisureing") && !DoingRelax() && Random.value < m_Data.relaxProb)
			{
				SetBool(m_Data.relax[Random.Range(0, m_Data.relax.Length)], value: true);
			}
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return;
		}
		if (m_Data.m_StartIdleTime > float.Epsilon)
		{
			m_Data.m_StartRestTime = 0f;
			m_Data.m_StartIdleTime = 0f;
			m_Data.m_CurrentIdleTime = 0f;
			if (GetBool("BehaveWaiting") || GetBool("Leisureing"))
			{
				SetBool("Interrupt", value: true);
			}
			for (int i = 0; i < m_Data.relax.Length; i++)
			{
				if (GetBool(m_Data.relax[i]))
				{
					SetBool(m_Data.relax[i], value: false);
				}
			}
			SetBool(m_Data.idle, value: false);
		}
		if (base.entity.NpcCmpt != null && !base.entity.NpcCmpt.Req_Contains(EReqType.Dialogue))
		{
			SetIKLerpspeed(100f);
			SetIKActive(active: false);
		}
	}
}
