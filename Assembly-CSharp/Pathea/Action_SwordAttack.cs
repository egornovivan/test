using System;
using System.Collections;
using System.Collections.Generic;
using Pathea.Effect;
using PETools;
using SkillSystem;
using UnityEngine;

namespace Pathea;

[Serializable]
public class Action_SwordAttack : PEAction
{
	private PeSword m_Sword;

	public float m_MoveAttackSpeed = 4f;

	public float m_SprintAttackSpeed = 6f;

	public float m_RotateAcc = 3f;

	[HideInInspector]
	public bool m_UseStamina;

	protected bool m_AttackInAir;

	protected bool m_AttackInWater;

	protected bool m_AttackInMove;

	protected bool m_AttackInSprint;

	protected Vector3 m_AttackDir;

	protected Vector3 m_PreDir;

	protected bool m_WaitInput;

	protected bool m_TstAttack;

	protected int m_Farmcount;

	protected SkInst m_SkillInst;

	protected int m_AttackModeIndex;

	private PEAttackTrigger m_AttackTrigger;

	private Collider m_AttackCol;

	public LayerMask m_AttackLayer;

	[HideInInspector]
	public bool firstPersonAttack;

	public float m_AttackHeight = 1.5f;

	public float m_LockMaxAngle = 20f;

	public float m_AttackChecRange = 3f;

	private Vector3 m_AttackTargetDir;

	private bool m_EndAction;

	[Header("AnimSpeed")]
	public AnimationCurve animScaleCurve;

	[Range(0f, 5f)]
	public float animScaleTime = 0.15f;

	public float animLerpF = 5f;

	private float animScaleStartTime;

	[Header("Comb")]
	private bool attackInAnimTime = true;

	protected int m_CombTime;

	private bool m_CombAttack;

	private float m_LastAtackTime;

	private GameObject m_EffectObj;

	private EffectLateupdateHelperEX_Part1 m_EffectHelper;

	public override PEActionType ActionType => PEActionType.SwordAttack;

	public HumanPhyCtrl phyMotor { get; set; }

	public SkEntity targetEntity { get; set; }

	public PeSword sword
	{
		get
		{
			return m_Sword;
		}
		set
		{
			m_Sword = value;
			if (null == m_Sword)
			{
				base.motionMgr.EndImmediately(ActionType);
			}
		}
	}

	public override bool CanDoAction(PEActionParam para = null)
	{
		PEActionParamVVNN pEActionParamVVNN = para as PEActionParamVVNN;
		int n = pEActionParamVVNN.n2;
		if (n >= sword.m_AttackSkill.Length || n >= sword.m_AttackSkill.Length)
		{
			return false;
		}
		return true;
	}

	public override void DoAction(PEActionParam para = null)
	{
		if (!(null == base.skillCmpt) && !(null == sword))
		{
			PEActionParamVVNN pEActionParamVVNN = para as PEActionParamVVNN;
			base.trans.position = pEActionParamVVNN.vec1;
			m_AttackDir = pEActionParamVVNN.vec2;
			if (m_AttackDir == Vector3.zero)
			{
				m_AttackDir = base.trans.forward;
			}
			m_CombTime = pEActionParamVVNN.n1;
			m_AttackModeIndex = pEActionParamVVNN.n2;
			ApplySkill();
			base.anim.ResetTrigger("ResetFullBody");
			base.anim.SetBool("AttackLand", value: false);
			base.motionMgr.SetMaskState(PEActionMask.SwordAttack, state: true);
			m_WaitInput = false;
			m_TstAttack = false;
			m_EndAction = false;
			OnStartAttack();
		}
	}

	public override void ResetAction(PEActionParam para = null)
	{
		if (!m_EndAction && m_WaitInput && (null != phyMotor && phyMotor.spineInWater) == m_AttackInWater && base.motionMgr.GetMaskState(PEActionMask.InAir) == m_AttackInAir)
		{
			if (attackInAnimTime)
			{
				m_CombAttack = true;
			}
			else
			{
				OnComboAttack();
			}
		}
		PEActionParamVVNN pEActionParamVVNN = para as PEActionParamVVNN;
		m_PreDir = pEActionParamVVNN.vec2;
	}

	public override bool Update()
	{
		if (null == base.skillCmpt || null == sword || EndAirAttack())
		{
			EndImmediately();
			return true;
		}
		UpdateAnimSpeed();
		UpdateEffect();
		if (!UpdateSkillState())
		{
			return false;
		}
		OnEndAction();
		return true;
	}

	public override void EndAction()
	{
		m_EndAction = true;
	}

	public override void EndImmediately()
	{
		m_EndAction = true;
		if (null != base.anim)
		{
			base.anim.SetTrigger("ResetFullBody");
		}
		if (null != phyMotor)
		{
			phyMotor.velocity = new Vector3(0f, phyMotor.velocity.y, 0f);
			phyMotor.ResetInertiaVelocity();
		}
		OnEndAction();
	}

	private void StartEffect(int effectID = 0)
	{
		EffectBuilder.EffectRequest effectRequest = Singleton<EffectBuilder>.Instance.Register(effectID, null, base.viewCmpt.modelTrans);
		effectRequest.SpawnEvent += OnEffectSpawn;
	}

	private void UpdateEffect()
	{
		if (!(null != m_EffectHelper))
		{
			return;
		}
		for (int i = 0; i < m_EffectHelper.particleSystems.Length; i++)
		{
			if (null != m_EffectHelper.particleSystems[i])
			{
				m_EffectHelper.particleSystems[i].playbackSpeed = base.anim.speed;
			}
		}
	}

	private void ApplySkill()
	{
		m_AttackInAir = base.motionMgr.GetMaskState(PEActionMask.InAir);
		m_AttackInWater = null != phyMotor && phyMotor.spineInWater;
		if (m_AttackInAir)
		{
			m_SkillInst = base.skillCmpt.StartSkill(targetEntity, sword.m_AttackSkill[m_AttackModeIndex].m_SkillInAir);
			return;
		}
		if (m_AttackInWater)
		{
			m_SkillInst = base.skillCmpt.StartSkill(targetEntity, sword.m_AttackSkill[m_AttackModeIndex].m_SkillInWater);
			return;
		}
		m_AttackInMove = null != base.move && base.move.velocity.magnitude > m_MoveAttackSpeed;
		m_AttackInSprint = null != base.move && base.move.velocity.magnitude > m_SprintAttackSpeed;
		if (m_AttackInSprint)
		{
			m_SkillInst = base.skillCmpt.StartSkill(targetEntity, sword.m_AttackSkill[m_AttackModeIndex].m_SkillInSprint);
		}
		else if (m_AttackInMove)
		{
			m_SkillInst = base.skillCmpt.StartSkill(targetEntity, sword.m_AttackSkill[m_AttackModeIndex].m_SkillInMove);
		}
		else
		{
			m_SkillInst = base.skillCmpt.StartSkill(targetEntity, sword.m_AttackSkill[m_AttackModeIndex].m_SkillID);
		}
	}

	private bool UpdateSkillState()
	{
		if (m_SkillInst != null && base.skillCmpt.IsSkillRunning(m_SkillInst.SkillID))
		{
			if (null != phyMotor)
			{
				Vector3 vector = Vector3.ProjectOnPlane(base.anim.m_LastMove / Time.deltaTime, Vector3.up);
				Vector3 vector2 = Vector3.ProjectOnPlane(phyMotor.velocity, Vector3.up);
				Vector3 vector3 = vector - vector2;
				if (vector3.magnitude >= 12f)
				{
					vector = vector3.normalized * 12f + vector2;
				}
				phyMotor.ApplyMoveRequest(vector);
				if (m_AttackInAir)
				{
					phyMotor.ApplyMoveRequest(base.anim.GetFloat("AirAttackDownSpeed") * Vector3.up);
				}
			}
			if ((bool)base.ikCmpt && Vector3.zero != m_AttackTargetDir)
			{
				if (firstPersonAttack)
				{
					m_AttackTargetDir = Vector3.Normalize(base.ikCmpt.aimTargetPos - PEUtil.MainCamTransform.position);
				}
				else
				{
					Vector3 forward = m_AttackTargetDir;
					if (!firstPersonAttack)
					{
						base.ikCmpt.aimTargetPos = base.trans.position + m_AttackHeight * Vector3.up + 10f * m_AttackTargetDir;
					}
					else
					{
						forward = base.ikCmpt.aimTargetPos - (base.trans.position + m_AttackHeight * Vector3.up);
						base.ikCmpt.aimTargetPos += 10f * m_AttackTargetDir;
					}
					if (null == phyMotor || !phyMotor.spineInWater)
					{
						forward.y = 0f;
					}
					base.trans.rotation = Quaternion.LookRotation(forward, Vector3.up);
				}
			}
			else if (m_AttackDir != Vector3.zero)
			{
				base.trans.rotation = Quaternion.Lerp(base.trans.rotation, Quaternion.LookRotation(m_AttackDir), m_RotateAcc * Time.deltaTime);
			}
			return false;
		}
		if (!m_EndAction && m_UseStamina)
		{
			ApplySkill();
			return false;
		}
		return true;
	}

	public bool CheckContinueAttack()
	{
		if (m_CombTime > 0)
		{
			m_TstAttack = true;
			m_CombTime--;
		}
		if (m_TstAttack)
		{
			m_AttackDir = m_PreDir;
			if (m_AttackDir == Vector3.zero)
			{
				m_AttackDir = base.trans.forward;
			}
			m_TstAttack = false;
			return true;
		}
		return false;
	}

	private bool EndAirAttack()
	{
		if (m_SkillInst != null && m_AttackInAir)
		{
			return null == base.move || base.move.state != MovementState.Air;
		}
		return false;
	}

	public Vector3 GetHitPos()
	{
		if (m_SkillInst != null)
		{
			return m_SkillInst.GetCollisionContactPoint();
		}
		return Vector3.zero;
	}

	private void CostStamina(int attackModeIndex)
	{
		if (m_UseStamina && null != sword && null != base.skillCmpt && sword.m_StaminaCost != null && sword.m_StaminaCost.Length > attackModeIndex)
		{
			float value = base.skillCmpt.GetAttribute(AttribType.Stamina) - sword.m_StaminaCost[attackModeIndex] * base.motionMgr.Entity.GetAttribute(AttribType.StaminaReducePercent);
			base.skillCmpt.SetAttribute(AttribType.Stamina, value, eventOff: false);
		}
	}

	private void ActiveCol(string colName)
	{
		if (!(null != base.anim.animator))
		{
			return;
		}
		Transform child = PEUtil.GetChild(base.anim.animator.transform, colName);
		if (!(null != child))
		{
			return;
		}
		PEAttackTrigger component = child.GetComponent<PEAttackTrigger>();
		if (null != component)
		{
			if (null != m_AttackTrigger)
			{
				if (component != m_AttackTrigger)
				{
					m_AttackTrigger.ClearHitInfo();
					m_AttackTrigger.onHitTrigger -= OnHitTrigger;
					m_AttackTrigger.active = false;
					m_AttackTrigger = component;
					m_AttackTrigger.onHitTrigger += OnHitTrigger;
					m_AttackTrigger.active = true;
				}
				else
				{
					m_AttackTrigger.ResetHitInfo();
					m_AttackTrigger.active = true;
				}
			}
			else
			{
				m_AttackTrigger = component;
				m_AttackTrigger.onHitTrigger += OnHitTrigger;
				m_AttackTrigger.active = true;
			}
		}
		else
		{
			m_AttackCol = child.GetComponent<Collider>();
			if (null != m_AttackCol)
			{
				m_AttackCol.enabled = true;
			}
		}
	}

	private void InactiveCol()
	{
		if (null != m_AttackTrigger)
		{
			m_AttackTrigger.ResetHitInfo();
			m_AttackTrigger.active = false;
		}
		else if (null != m_AttackCol)
		{
			m_AttackCol.enabled = false;
			m_AttackCol = null;
		}
	}

	private void OnStartAttack()
	{
		CostStamina(m_AttackModeIndex);
		ChangeAttackTarget();
		if (null != sword && sword.m_AttackMode != null && sword.m_AttackMode.Length > m_AttackModeIndex && null != sword && sword.m_AttackMode != null && sword.m_AttackMode.Length > m_AttackModeIndex)
		{
			base.motionMgr.Entity.SendMsg(EMsg.Battle_Attack, sword.m_AttackMode[m_AttackModeIndex]);
			base.motionMgr.Entity.SendMsg(EMsg.Battle_OnAttack, sword.m_AttackMode[m_AttackModeIndex], sword.transform, 0);
		}
		m_CombAttack = false;
		m_LastAtackTime = Time.time;
	}

	private void OnComboAttack()
	{
		m_WaitInput = false;
		m_TstAttack = true;
		if (m_SkillInst != null)
		{
			m_SkillInst.SkipWaitAll = true;
		}
		OnStartAttack();
	}

	private void OnEndAction()
	{
		if (null != base.skillCmpt && m_SkillInst != null && base.skillCmpt.IsSkillRunning(m_SkillInst.SkillID))
		{
			base.skillCmpt.CancelSkillById(m_SkillInst.SkillID);
		}
		base.motionMgr.SetMaskState(PEActionMask.SwordAttack, state: false);
		m_WaitInput = false;
		m_TstAttack = false;
		m_SkillInst = null;
		m_AttackTargetDir = Vector3.zero;
		if (null != phyMotor)
		{
			phyMotor.CancelMoveRequest();
			phyMotor.desiredMovementDirection = Vector3.zero;
		}
		if (null != m_AttackTrigger)
		{
			m_AttackTrigger.ClearHitInfo();
			m_AttackTrigger.onHitTrigger -= OnHitTrigger;
			m_AttackTrigger.active = false;
			m_AttackTrigger = null;
		}
		else if (null != m_AttackCol)
		{
			m_AttackCol.enabled = false;
			m_AttackCol = null;
		}
		if (null != base.ikCmpt)
		{
			base.ikCmpt.aimActive = false;
			if (null != base.ikCmpt.m_IKAimCtrl)
			{
				base.ikCmpt.m_IKAimCtrl.EndSyncAimAxie();
			}
		}
		base.anim.speed = 1f;
		DestroyEffect();
	}

	private void ChangeAttackTarget()
	{
		m_AttackTargetDir = Vector3.zero;
		if (!(null != base.ikCmpt) || !(null != base.trans))
		{
			return;
		}
		Vector3 vector = base.trans.position + m_AttackHeight * Vector3.up;
		if (firstPersonAttack)
		{
			m_AttackTargetDir = Vector3.Normalize(base.ikCmpt.aimTargetPos - PEUtil.MainCamTransform.position);
		}
		else
		{
			float num = 100f;
			List<ViewCmpt> list = new List<ViewCmpt>();
			Collider[] array = Physics.OverlapSphere(vector, m_AttackChecRange, m_AttackLayer.value);
			Vector3 attackDir = m_AttackDir;
			attackDir.y = 0f;
			int srcID = Mathf.RoundToInt(base.motionMgr.Entity.GetAttribute(AttribType.DefaultPlayerID));
			Collider[] array2 = array;
			foreach (Collider collider in array2)
			{
				BiologyViewCmpt componentInParent = collider.GetComponentInParent<BiologyViewCmpt>();
				if (!(null != componentInParent) || !(base.viewCmpt != componentInParent) || list.Contains(componentInParent))
				{
					continue;
				}
				list.Add(componentInParent);
				if (componentInParent.IsRagdoll || !Singleton<ForceSetting>.Instance.Conflict(srcID, Mathf.RoundToInt(componentInParent.Entity.GetAttribute(AttribType.DefaultPlayerID))))
				{
					continue;
				}
				PEDefenceTrigger component = collider.GetComponent<PEDefenceTrigger>();
				if (null == component)
				{
					Transform centerTransform = componentInParent.centerTransform;
					Vector3 from = centerTransform.position - vector;
					float magnitude = from.magnitude;
					from.y = 0f;
					if (null != centerTransform && magnitude < num && Vector3.Angle(from, attackDir) < m_LockMaxAngle)
					{
						num = magnitude;
						m_AttackTargetDir = Vector3.Normalize(centerTransform.position - vector);
					}
				}
				else
				{
					if (!component.GetClosest(vector, m_AttackChecRange, out var result))
					{
						continue;
					}
					Vector3 vector2 = result.hitPos - vector;
					float magnitude2 = vector2.magnitude;
					Vector3 from2 = vector2;
					from2.y = 0f;
					if (magnitude2 < num && Vector3.Angle(from2, attackDir) < m_LockMaxAngle)
					{
						float num2 = Vector3.Angle(vector2, attackDir);
						if (num2 < m_LockMaxAngle)
						{
							m_AttackTargetDir = vector2.normalized;
						}
						else
						{
							m_AttackTargetDir = Vector3.Slerp(attackDir.normalized, vector2.normalized, m_LockMaxAngle / num2);
						}
					}
				}
			}
		}
		base.ikCmpt.aimActive = Vector3.zero != m_AttackTargetDir;
		if (base.ikCmpt.aimActive && null != base.ikCmpt.m_IKAimCtrl)
		{
			base.ikCmpt.m_IKAimCtrl.StartSyncAimAxie();
		}
	}

	private void UpdateAnimSpeed()
	{
		if (!(null != base.anim) || !(null != sword))
		{
			return;
		}
		float num = Time.time - animScaleStartTime;
		float attribute = base.entity.GetAttribute(AttribType.Stamina);
		float num2 = sword.m_AnimSpeed;
		for (int i = 0; i < sword.m_AnimDownThreshold.Length; i++)
		{
			if (attribute <= sword.m_AnimDownThreshold[i])
			{
				num2 *= 0.9f;
			}
		}
		num2 = Mathf.Clamp(num2, 0.5f, 1.5f);
		if (num < animScaleTime)
		{
			base.anim.speed = animScaleCurve.Evaluate(num / animScaleTime) * num2;
		}
		else
		{
			base.anim.speed = num2;
		}
		base.skillCmpt._lastestTimeOfConsumingStamina = Time.time;
	}

	private void OnHitTrigger(PEDefenceTrigger hitTrigger, PECapsuleHitResult result)
	{
		if (!hitTrigger.transform.IsChildOf(base.motionMgr.transform))
		{
			animScaleStartTime = Time.time;
		}
	}

	protected override void OnAnimEvent(string eventParam)
	{
		if (!(null != sword) || !base.motionMgr.IsActionRunning(ActionType))
		{
			return;
		}
		string[] array = eventParam.Split('_');
		bool flag = array[0] == "FP";
		bool flag2 = array[0] == "TP";
		bool flag3 = flag || flag2;
		if (flag3 && flag != firstPersonAttack)
		{
			return;
		}
		switch ((!flag3) ? array[0] : array[1])
		{
		case "AnimEffect":
			StartEffect(Convert.ToInt32((!flag3) ? array[1] : array[2]));
			break;
		case "StartAttack":
			if (m_SkillInst != null)
			{
				m_SkillInst.SkipWaitPre = true;
			}
			ActiveCol((!flag3) ? array[1] : array[2]);
			m_Farmcount = Time.frameCount;
			break;
		case "WeightInputStart":
			m_WaitInput = true;
			break;
		case "WeightInputEnd":
			m_WaitInput = false;
			break;
		case "EndAttack":
			m_WaitInput = false;
			base.motionMgr.StartCoroutine(OnEndAttack());
			break;
		case "MonsterEndAttack":
			if (m_SkillInst != null)
			{
				m_SkillInst.SkipWaitAll = true;
			}
			break;
		case "EndAction":
		case "OnEndFullAnim":
			if (Time.time - m_LastAtackTime >= 0.3f)
			{
				m_EndAction = true;
				m_CombAttack = false;
				if (m_SkillInst != null && null != base.skillCmpt)
				{
					base.skillCmpt.CancelSkillById(m_SkillInst.SkillID);
					m_SkillInst = null;
				}
			}
			break;
		}
	}

	private IEnumerator OnEndAttack()
	{
		while (Time.frameCount < m_Farmcount + 2)
		{
			yield return null;
		}
		if (m_SkillInst != null)
		{
			m_SkillInst.SkipWaitMain = true;
		}
		InactiveCol();
		if (attackInAnimTime && m_CombAttack && !m_EndAction)
		{
			OnComboAttack();
		}
	}

	private void DestroyEffect()
	{
		if (null != m_EffectObj)
		{
			UnityEngine.Object.Destroy(m_EffectObj);
		}
		m_EffectObj = null;
	}

	private void OnEffectSpawn(GameObject obj)
	{
		DestroyEffect();
		if (m_EndAction)
		{
			UnityEngine.Object.Destroy(obj);
			return;
		}
		m_EffectObj = obj;
		m_EffectHelper = obj.GetComponent<EffectLateupdateHelperEX_Part1>();
		UpdateEffect();
	}
}
