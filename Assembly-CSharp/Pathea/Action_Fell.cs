using System;
using NaturalResAsset;
using PETools;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_Fell : PEAction
{
	private const float MaxHp = 255f;

	private const float FellDis = 2f;

	public PEAxe m_Axe;

	private bool m_EndAction;

	private bool m_EndAnim;

	private GlobalTreeInfo mOpTreeInfo;

	private GlobalTreeInfo mFindTreeInfo;

	private float m_FixTime;

	private float m_MSGCD = 1f;

	private float m_LastShowTime;

	public override PEActionType ActionType => PEActionType.Fell;

	public GlobalTreeInfo treeInfo
	{
		get
		{
			return mOpTreeInfo;
		}
		set
		{
			mOpTreeInfo = (mFindTreeInfo = value);
		}
	}

	public event Action<TreeInfo> startFell;

	public event Action endFell;

	public event Action<TreeInfo, float> hpChange;

	public bool UpdateOPTreeInfo()
	{
		if (null == m_Axe || m_Axe.durability <= float.Epsilon)
		{
			return false;
		}
		mFindTreeInfo = null;
		GlobalTreeInfo globalTreeInfo = PEUtil.RayCastTree(PeCamera.mouseRay.origin - 0.5f * PeCamera.mouseRay.direction, PeCamera.mouseRay.direction, 100f);
		GlobalTreeInfo globalTreeInfo2 = PEUtil.RayCastTree(base.trans.existent.position + Vector3.up, base.trans.existent.forward, 2f);
		if (globalTreeInfo == null || globalTreeInfo2 == null || globalTreeInfo._treeInfo != globalTreeInfo2._treeInfo)
		{
			return false;
		}
		if (globalTreeInfo != null)
		{
			NaturalRes terrainResData = NaturalRes.GetTerrainResData(globalTreeInfo._treeInfo.m_protoTypeIdx + 1000);
			if (terrainResData != null && terrainResData.m_type == 9)
			{
				mFindTreeInfo = globalTreeInfo;
			}
		}
		return null != mFindTreeInfo;
	}

	public override bool CanDoAction(PEActionParam para = null)
	{
		if (null == m_Axe || null == base.trans)
		{
			return false;
		}
		if (m_Axe.durability <= float.Epsilon)
		{
			if (Time.time - m_LastShowTime >= m_MSGCD)
			{
				m_LastShowTime = Time.time;
				base.motionMgr.Entity.SendMsg(EMsg.Action_DurabilityDeficiency);
			}
			return false;
		}
		if (mFindTreeInfo == null)
		{
			return false;
		}
		return base.CanDoAction(para);
	}

	public override void PreDoAction()
	{
		base.PreDoAction();
		mOpTreeInfo = mFindTreeInfo;
	}

	public override void DoAction(PEActionParam para = null)
	{
		if (!(null == m_Axe))
		{
			base.motionMgr.SetMaskState(PEActionMask.Fell, state: true);
			m_EndAction = false;
			m_EndAnim = false;
			if (null != base.anim)
			{
				base.anim.ResetTrigger("ResetFullBody");
				base.anim.SetBool(m_Axe.fellAnim, value: true);
			}
			m_FixTime = 3f;
			ApplyStaminaCost();
			if (this.startFell != null)
			{
				this.startFell(treeInfo._treeInfo);
			}
			if (this.hpChange != null)
			{
				this.hpChange(treeInfo._treeInfo, SkEntitySubTerrain.Instance.GetTreeHP(treeInfo.WorldPos) / 255f);
			}
		}
	}

	public override bool Update()
	{
		m_FixTime -= Time.deltaTime;
		base.skillCmpt._lastestTimeOfConsumingStamina = Time.time;
		if (null != base.anim)
		{
			if (m_EndAction && (m_EndAnim || m_FixTime < 0f))
			{
				OnEndAction();
				return true;
			}
		}
		else if (m_EndAction)
		{
			OnEndAction();
			return true;
		}
		return false;
	}

	public override void EndAction()
	{
		m_EndAction = true;
		if (null != base.anim && null != m_Axe)
		{
			base.anim.SetBool(m_Axe.fellAnim, value: false);
		}
	}

	public override void EndImmediately()
	{
		if (null != base.anim && null != m_Axe)
		{
			base.anim.SetTrigger("ResetFullBody");
			base.anim.SetBool(m_Axe.fellAnim, value: false);
		}
		OnEndAction();
	}

	private void OnEndAction()
	{
		base.motionMgr.SetMaskState(PEActionMask.Fell, state: false);
		if (null != m_Axe && m_Axe.durability <= float.Epsilon)
		{
			base.motionMgr.EndAction(PEActionType.EquipmentHold);
		}
		if (this.endFell != null)
		{
			this.endFell();
		}
		base.anim.speed = 1f;
	}

	private void FellTree()
	{
		if (treeInfo != null && null != base.skillCmpt && null != m_Axe)
		{
			if (SkEntitySubTerrain.Instance.GetTreeHP(treeInfo.WorldPos) <= float.Epsilon)
			{
				base.motionMgr.EndAction(ActionType);
				if (this.hpChange != null)
				{
					this.hpChange(treeInfo._treeInfo, 0f);
				}
				return;
			}
			if (null != base.skillCmpt && m_Axe.m_FellSkillID != 0)
			{
				base.skillCmpt.StartSkill(SkEntitySubTerrain.Instance, m_Axe.m_FellSkillID);
			}
			if (this.hpChange != null)
			{
				this.hpChange(treeInfo._treeInfo, SkEntitySubTerrain.Instance.GetTreeHP(treeInfo.WorldPos) / 255f);
			}
			if (SkEntitySubTerrain.Instance.GetTreeHP(treeInfo.WorldPos) <= 0f || m_Axe.durability <= float.Epsilon)
			{
				base.motionMgr.EndAction(ActionType);
			}
			base.motionMgr.Entity.SendMsg(EMsg.Battle_EquipAttack, m_Axe.ItemObj);
		}
		m_FixTime = 3f;
		if (!m_EndAction)
		{
			ApplyStaminaCost();
		}
	}

	private void ApplyStaminaCost()
	{
		if (null == m_Axe)
		{
			return;
		}
		float num = base.entity.GetAttribute(AttribType.Stamina) - m_Axe.m_StaminaCost * base.entity.GetAttribute(AttribType.StaminaReducePercent);
		base.entity.SetAttribute(AttribType.Stamina, num, offEvent: false);
		float num2 = m_Axe.m_AnimSpeed;
		for (int i = 0; i < m_Axe.m_AnimDownThreshold.Length; i++)
		{
			if (num <= m_Axe.m_AnimDownThreshold[i])
			{
				num2 *= 0.9f;
			}
		}
		base.anim.speed = Mathf.Clamp(num2, 0.5f, 1.5f);
	}

	protected override void OnAnimEvent(string eventParam)
	{
		if (base.motionMgr.IsActionRunning(ActionType))
		{
			switch (eventParam)
			{
			case "Fell":
				FellTree();
				break;
			case "FellEnd":
			case "OnEndFullAnim":
				m_EndAnim = true;
				break;
			}
		}
	}
}
