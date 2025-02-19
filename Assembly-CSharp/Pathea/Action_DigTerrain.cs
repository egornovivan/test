using System;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_DigTerrain : PEAction
{
	public HumanPhyCtrl m_PhyCtrl;

	private PEDigTool m_DigTool;

	public float m_MaxDigDis = 3f;

	private Vector3 m_CurrentPos;

	private Vector3 m_DigPos;

	private bool m_CanDig;

	private bool m_EndDig;

	private bool m_EndAnim;

	private UTimer m_FixTimer;

	private string m_AnimName;

	private bool m_FirstDig;

	public override PEActionType ActionType => PEActionType.Dig;

	public PEDigTool digTool
	{
		get
		{
			return m_DigTool;
		}
		set
		{
			if (null == value)
			{
				base.motionMgr.EndImmediately(ActionType);
			}
			m_DigTool = value;
		}
	}

	public PECrusher crusher => digTool as PECrusher;

	public Vector3 digPos
	{
		get
		{
			return m_DigPos;
		}
		private set
		{
			m_CurrentPos = value;
		}
	}

	public Action_DigTerrain()
	{
		m_FixTimer = new UTimer();
		m_FixTimer.ElapseSpeed = -1f;
	}

	public void UpdateDigPos()
	{
		if (null != digTool && base.motionMgr.Entity == PeSingleton<PeCreature>.Instance.mainPlayer)
		{
			if (null != digTool.m_Indicator)
			{
				digTool.m_Indicator.show = base.motionMgr.GetMaskState(digTool.m_HandChangeAttr.m_HoldActionMask) && digTool.durability > float.Epsilon;
				digTool.m_Indicator.disEnable = Vector3.Distance(digTool.m_Indicator.digPos, base.trans.position + Vector3.up) < m_MaxDigDis;
				m_CanDig = digTool.m_Indicator.active;
				m_CurrentPos = digTool.m_Indicator.digPos;
			}
			else
			{
				m_CanDig = false;
			}
		}
	}

	public override bool CanDoAction(PEActionParam para = null)
	{
		if (null == digTool || !m_CanDig)
		{
			return false;
		}
		if (digTool.durability <= float.Epsilon)
		{
			base.motionMgr.Entity.SendMsg(EMsg.Action_DurabilityDeficiency);
			return false;
		}
		return base.CanDoAction(para);
	}

	public override void DoAction(PEActionParam para = null)
	{
		if (null != digTool)
		{
			base.motionMgr.SetMaskState(digTool.m_DigMask, state: true);
			m_AnimName = digTool.m_AnimName;
			base.anim.ResetTrigger("ResetFullBody");
			base.anim.SetBool(m_AnimName, value: true);
			m_EndDig = false;
			m_EndAnim = false;
		}
		m_DigPos = m_CurrentPos;
		if (null != m_PhyCtrl)
		{
			m_PhyCtrl.m_SubAcc = Vector3.down;
		}
		m_FirstDig = true;
		ApplyStaminaCost();
	}

	public override bool Update()
	{
		if (null == m_DigTool)
		{
			base.motionMgr.EndImmediately(ActionType);
			return true;
		}
		if (m_EndDig)
		{
			m_FixTimer.Update(Time.deltaTime);
			if (m_EndAnim || m_FixTimer.Second <= 0.0)
			{
				OnEndAction();
				return true;
			}
		}
		if (null != crusher)
		{
			crusher.UpdateEnCost();
		}
		base.skillCmpt._lastestTimeOfConsumingStamina = Time.time;
		return false;
	}

	public override void EndAction()
	{
		base.anim.SetBool(m_AnimName, value: false);
		m_EndDig = true;
		m_FixTimer.Second = 5.0;
	}

	public override void EndImmediately()
	{
		base.anim.SetBool(m_AnimName, value: false);
		base.anim.SetTrigger("ResetFullBody");
		OnEndAction();
	}

	private void OnEndAction()
	{
		base.anim.speed = 1f;
		base.motionMgr.SetMaskState(PEActionMask.Dig, state: false);
		base.motionMgr.SetMaskState(PEActionMask.DrillingDig, state: false);
		if (null != m_PhyCtrl)
		{
			m_PhyCtrl.m_SubAcc = Vector3.zero;
		}
	}

	private void DigTerrain()
	{
		if ((m_FirstDig || m_CanDig) && null != digTool && null != base.skillCmpt && null != base.packageCmpt)
		{
			if (!m_FirstDig)
			{
				m_DigPos = m_CurrentPos;
			}
			if (null != base.skillCmpt && digTool.m_SkillID != 0)
			{
				base.skillCmpt.StartSkill(VFVoxelTerrain.self, digTool.m_SkillID);
			}
			base.motionMgr.Entity.SendMsg(EMsg.Battle_EquipAttack, digTool.m_ItemObj);
			if (digTool.durability <= float.Epsilon)
			{
				base.motionMgr.EndAction(ActionType);
				base.motionMgr.Entity.SendMsg(EMsg.Action_DurabilityDeficiency);
			}
			if (!m_EndDig)
			{
				ApplyStaminaCost();
			}
		}
		m_FirstDig = false;
	}

	private void ApplyStaminaCost()
	{
		if (null == digTool)
		{
			return;
		}
		float num = base.entity.GetAttribute(AttribType.Stamina) - digTool.m_StaminaCost * base.entity.GetAttribute(AttribType.StaminaReducePercent);
		base.entity.SetAttribute(AttribType.Stamina, num, offEvent: false);
		float num2 = digTool.m_AnimSpeed;
		for (int i = 0; i < digTool.m_AnimDownThreshold.Length; i++)
		{
			if (num <= digTool.m_AnimDownThreshold[i])
			{
				num2 *= 0.9f;
			}
		}
		base.anim.speed = Mathf.Clamp(num2, 0.5f, 1.5f);
	}

	protected override void OnAnimEvent(string eventParam)
	{
		switch (eventParam)
		{
		case "DigTerrain":
			DigTerrain();
			break;
		case "DigEnd":
		case "OnEndFullAnim":
			m_EndAnim = true;
			break;
		case "ChangeTarget":
			if (!m_FirstDig && m_CanDig)
			{
				m_DigPos = m_CurrentPos;
			}
			break;
		}
	}
}
