using System.Collections;
using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BRDialogue), "RDialogue")]
public class BRDialogue : BTNormal
{
	private class Data
	{
		[Behave]
		public string[] anims = new string[0];

		public float m_LastTime;

		public float m_CurrentTime;

		public string m_curAction;

		public float GetCurrentTime()
		{
			return Random.Range(3f, 10f);
		}
	}

	private Data m_Data;

	private RQDialogue m_Dialogue;

	private string m_RqAction = string.Empty;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		m_Dialogue = GetRequest(EReqType.Dialogue) as RQDialogue;
		if (m_Dialogue == null)
		{
			return BehaveResult.Failure;
		}
		StopMove();
		m_Data.m_LastTime = Time.time;
		m_Data.m_CurrentTime = m_Data.GetCurrentTime();
		m_RqAction = m_Dialogue.RqAction;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		SetNpcAiType(ENpcAiType.RDialogue);
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!(GetRequest(EReqType.Dialogue) is RQDialogue rQDialogue))
		{
			if (m_RqAction != string.Empty)
			{
				EndAction(PEActionType.Leisure);
			}
			return BehaveResult.Success;
		}
		RQSalvation rQSalvation = GetRequest(EReqType.Salvation) as RQSalvation;
		if (rQDialogue != null && rQSalvation != null)
		{
			RemoveRequest(EReqType.Dialogue);
			return BehaveResult.Success;
		}
		if (rQDialogue.RqRatePos != Vector3.zero)
		{
			Vector3 dir = rQDialogue.RqRatePos - base.position;
			if (PEUtil.InAimDistance(rQDialogue.RqRatePos, base.position, 1f, 10f) && PEUtil.InAimAngle(rQDialogue.RqRatePos, base.position, base.existent.forward))
			{
				SetIKFadeInTime(NPCConstNum.IK_Aim_FadeInTime_0);
				SetIKFadeOutTime(NPCConstNum.IK_Aim_FadeOutTime_0);
				SetIKActive(active: true);
				SetIKTargetPos(PEUtil.CalculateAimPos(rQDialogue.RqRatePos, base.position));
			}
			else
			{
				if (!PEUtil.InAimDistance(rQDialogue.RqRatePos, base.position, 1f, 10f))
				{
					SetIKActive(active: false);
				}
				FaceDirection(dir);
			}
		}
		if (!Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			base.entity.target.ClearEnemy();
		}
		m_RqAction = rQDialogue.RqAction;
		if (rQDialogue.CanDoAction())
		{
			PEActionParamS param = PEActionParamS.param;
			param.str = m_RqAction;
			DoAction(PEActionType.Leisure, param);
		}
		if (m_Data.anims.Length > 0 && Time.time - m_Data.m_LastTime > m_Data.m_CurrentTime)
		{
			m_Data.m_LastTime = Time.time;
			m_Data.m_CurrentTime = m_Data.GetCurrentTime();
			SetBool(m_Data.anims[Random.Range(0, m_Data.anims.Length)], value: true);
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (IsMotionRunning(PEActionType.Leisure))
		{
			EndAction(PEActionType.Leisure);
		}
		if (base.entity != null && base.entity.IKCmpt != null && base.entity.IKCmpt.m_IKAimCtrl != null && base.entity.IKCmpt.iKAimCtrl.active)
		{
			SetIKActive(active: false);
			base.entity.StartCoroutine(endSth(NPCConstNum.IK_Aim_FadeOutTime_0));
		}
	}

	private IEnumerator endSth(float time)
	{
		yield return new WaitForSeconds(time);
		SetIKFadeInTime(NPCConstNum.IK_Aim_FadeInTimedefault);
		SetIKFadeOutTime(NPCConstNum.IK_Aim_FadeOutTimedefault);
	}
}
