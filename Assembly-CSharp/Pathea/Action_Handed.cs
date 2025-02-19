using System;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_Handed : PEAction
{
	private Vector3 offset = new Vector3(0.4f, 0f, 0.2f);

	private Vector3 axie = new Vector3(-0.5f, 0f, 1f);

	private float rotateSpeed = 15f;

	private float minFixeDis = 0.25f;

	private PeTrans m_TargetTrans;

	private bool m_AnimMatch;

	public override PEActionType ActionType => PEActionType.Handed;

	public bool standAnimEnd { get; set; }

	public override void DoAction(PEActionParam para = null)
	{
		if (!(null == base.trans) && !(null == base.move))
		{
			PEActionParamN pEActionParamN = para as PEActionParamN;
			PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(pEActionParamN.n);
			if (null != peEntity)
			{
				m_TargetTrans = peEntity.peTrans;
			}
			if (!(null == m_TargetTrans))
			{
				m_AnimMatch = false;
				standAnimEnd = false;
				base.motionMgr.SetMaskState(PEActionMask.Hand, state: true);
			}
		}
	}

	public override bool Update()
	{
		if (null == m_TargetTrans || null == base.move)
		{
			return true;
		}
		if (m_AnimMatch)
		{
			Vector3 b = m_TargetTrans.position + m_TargetTrans.rotation * offset;
			if (!standAnimEnd)
			{
				base.trans.position = Vector3.Lerp(base.trans.position, b, 5f * Time.deltaTime);
			}
			else
			{
				Vector3 vector = m_TargetTrans.position + m_TargetTrans.rotation * offset - base.trans.position;
				vector.y = 0f;
				float magnitude = vector.magnitude;
				if (magnitude < 0.25f * minFixeDis)
				{
					vector = Vector3.zero;
				}
				float num = Mathf.Clamp01((magnitude - 0.1f * minFixeDis) / minFixeDis);
				if (null != base.entity.biologyViewCmpt.monoPhyCtrl)
				{
					base.entity.biologyViewCmpt.monoPhyCtrl.netMoveSpeedScale = num;
				}
				base.trans.rotation = Quaternion.LookRotation(Vector3.Lerp(base.trans.forward, m_TargetTrans.rotation * axie, rotateSpeed * Time.deltaTime));
				base.move.Move(vector * num);
			}
		}
		return false;
	}

	public override void EndImmediately()
	{
		base.move.Move(Vector3.zero);
		base.motionMgr.SetMaskState(PEActionMask.Hand, state: false);
		base.anim.SetBool("InjuredSitEX", value: true);
		base.move.baseMoveStyle = MoveStyle.Abnormal;
		if (null != base.entity.biologyViewCmpt.monoPhyCtrl)
		{
			base.entity.biologyViewCmpt.monoPhyCtrl.netMoveSpeedScale = 1f;
		}
	}

	public void OnHand()
	{
		m_AnimMatch = true;
		if (!base.anim.GetBool("InjuredSitEX"))
		{
			standAnimEnd = true;
		}
		else
		{
			base.anim.SetBool("InjuredSitEX", value: false);
		}
		base.move.style = MoveStyle.Abnormal;
	}

	protected override void OnAnimEvent(string eventParam)
	{
		if (base.motionMgr.IsActionRunning(ActionType) && "OnStandAnimEnd" == eventParam)
		{
			standAnimEnd = true;
		}
	}
}
