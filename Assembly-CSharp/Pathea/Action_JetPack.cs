using System;
using System.Collections;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_JetPack : PEAction
{
	public HumanPhyCtrl m_PhyCtrl;

	private PEJetPack m_JetPack;

	public PEJetPackLogic jetPackLogic;

	public float m_RotAcc = 5f;

	private Vector3 m_MoveDir;

	private AudioController m_Audio;

	public override PEActionType ActionType => PEActionType.JetPack;

	public PEJetPack jetPack
	{
		get
		{
			return m_JetPack;
		}
		set
		{
			if (null == value)
			{
				base.motionMgr.EndImmediately(ActionType);
			}
			m_JetPack = value;
		}
	}

	public void SetMoveDir(Vector3 moveDir)
	{
		m_MoveDir = moveDir;
	}

	public override bool CanDoAction(PEActionParam para = null)
	{
		if (null != jetPackLogic && jetPackLogic.enCurrent >= jetPackLogic.m_EnergyThreshold)
		{
			return true;
		}
		return false;
	}

	public override void DoAction(PEActionParam para = null)
	{
		base.motionMgr.SetMaskState(PEActionMask.JetPack, state: true);
		m_MoveDir = Vector3.zero;
		if (null != jetPack)
		{
			jetPack.m_EffectObj.SetActive(value: true);
			if (jetPack.m_StartSoundID != 0 && null != jetPack.m_EffectObj)
			{
				m_Audio = AudioManager.instance.Create(jetPack.m_EffectObj.transform.position, jetPack.m_StartSoundID, jetPack.m_EffectObj.transform);
			}
		}
		if (null != jetPackLogic && null != m_PhyCtrl)
		{
			m_PhyCtrl.ResetSpeed(jetPackLogic.m_BoostHorizonalSpeed);
			m_PhyCtrl.desiredMovementDirection = m_PhyCtrl.currentDesiredMovementDirection;
		}
		base.motionMgr.StartCoroutine(ChangeAudio());
		if (null != base.anim)
		{
			base.anim.ResetTrigger("EndJump");
		}
	}

	private IEnumerator ChangeAudio()
	{
		yield return new WaitForSeconds(1.5f);
		if (base.motionMgr.IsActionRunning(ActionType))
		{
			if (null != m_Audio)
			{
				m_Audio.Delete(0.5f);
				m_Audio = null;
			}
			if (null != jetPack && null != jetPack.m_EffectObj && jetPack.m_SoundID != 0)
			{
				m_Audio = AudioManager.instance.Create(jetPack.m_EffectObj.transform.position, jetPack.m_SoundID, jetPack.m_EffectObj.transform, isPlay: false, isDelete: false);
				m_Audio.PlayAudio(0.5f);
			}
		}
	}

	public override bool Update()
	{
		if (PeGameMgr.IsMulti && null != base.entity.netCmpt && !base.entity.netCmpt.IsController)
		{
			return false;
		}
		if (null != jetPackLogic)
		{
			bool flag = false;
			if (null != m_PhyCtrl)
			{
				if (m_PhyCtrl.grounded)
				{
					flag = true;
				}
				else
				{
					m_PhyCtrl.m_SubAcc = ((!(m_PhyCtrl.velocity.y < jetPackLogic.m_MaxUpSpeed)) ? Vector3.zero : (Vector3.up * jetPackLogic.m_BoostPowerUp));
					if (!m_PhyCtrl.spineInWater && null != base.anim)
					{
						base.anim.SetTrigger("Fall");
					}
				}
			}
			if (!flag)
			{
				jetPackLogic.enCurrent = Mathf.Clamp(jetPackLogic.enCurrent - jetPackLogic.m_CostSpeed * Time.deltaTime, 0f, jetPackLogic.enMax);
				if (jetPackLogic.enCurrent <= float.Epsilon)
				{
					flag = true;
				}
			}
			if (flag)
			{
				EndImmediately();
				return true;
			}
			jetPackLogic.lastUsedTime = Time.time;
		}
		if (null != base.trans && m_MoveDir != Vector3.zero && !base.motionMgr.GetMaskState(PEActionMask.AimEquipHold) && !base.motionMgr.GetMaskState(PEActionMask.GunHold) && !base.motionMgr.GetMaskState(PEActionMask.BowHold))
		{
			base.trans.rotation = Quaternion.Lerp(base.trans.rotation, Quaternion.LookRotation(m_MoveDir, Vector3.up), m_RotAcc * Time.deltaTime);
		}
		if (null != base.trans && null != m_PhyCtrl)
		{
			m_PhyCtrl.desiredMovementDirection = Vector3.Lerp(m_PhyCtrl.desiredMovementDirection, m_MoveDir, 5f * Time.deltaTime);
		}
		return false;
	}

	public override void EndImmediately()
	{
		base.motionMgr.SetMaskState(PEActionMask.JetPack, state: false);
		if (null != jetPackLogic)
		{
			jetPackLogic.lastUsedTime = Time.time;
		}
		if (null != jetPack && null != jetPack.m_EffectObj)
		{
			m_JetPack.m_EffectObj.SetActive(value: false);
		}
		if (null != m_Audio)
		{
			m_Audio.Delete();
			m_Audio = null;
		}
		if (null != m_PhyCtrl)
		{
			m_PhyCtrl.m_SubAcc = Vector3.zero;
			m_PhyCtrl.desiredMovementDirection = Vector3.zero;
		}
		if (null != base.anim)
		{
			base.anim.ResetTrigger("Fall");
		}
	}
}
