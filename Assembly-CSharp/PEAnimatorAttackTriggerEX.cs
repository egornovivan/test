using System;
using Pathea;
using PETools;
using UnityEngine;

public class PEAnimatorAttackTriggerEX : PEAnimatorAttack
{
	[Serializable]
	public class Attack
	{
		public float startTime;

		public float endTime;

		public string[] bones;

		[HideInInspector]
		public PEAttackTrigger[] triggers;

		[HideInInspector]
		public bool active;

		[HideInInspector]
		public bool playAttack;
	}

	private AnimatorCtrl m_AnimCtrl;

	[SerializeField]
	private Attack[] m_Attacks;

	internal override void Init(Animator animator)
	{
		base.Init(animator);
		for (int i = 0; i < m_Attacks.Length; i++)
		{
			m_Attacks[i].triggers = new PEAttackTrigger[m_Attacks[i].bones.Length];
			for (int j = 0; j < m_Attacks[i].bones.Length; j++)
			{
				Transform child = PEUtil.GetChild(animator.transform, m_Attacks[i].bones[j]);
				if (child != null)
				{
					PEAttackTrigger component = child.GetComponent<PEAttackTrigger>();
					if (null != component)
					{
						m_Attacks[i].triggers[j] = component;
					}
					else
					{
						Debug.LogError("Can't find PEAttackTrigger:" + m_Attacks[i].bones[j]);
					}
				}
				else
				{
					Debug.LogError("Can't find bone:" + m_Attacks[i].bones[j]);
				}
			}
		}
	}

	private void ClearTrigger(bool value)
	{
		if (m_Attacks == null)
		{
			return;
		}
		for (int i = 0; i < m_Attacks.Length; i++)
		{
			m_Attacks[i].active = value;
			m_Attacks[i].playAttack = false;
			if (m_Attacks[i].triggers == null)
			{
				continue;
			}
			for (int j = 0; j < m_Attacks[i].triggers.Length; j++)
			{
				if (!(null == m_Attacks[i].triggers[j]))
				{
					m_Attacks[i].triggers[j].active = false;
					m_Attacks[i].triggers[j].ClearHitInfo();
				}
			}
		}
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		ClearTrigger(value: true);
		if (null == m_AnimCtrl)
		{
			m_AnimCtrl = animator.GetComponent<AnimatorCtrl>();
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		for (int i = 0; i < m_Attacks.Length; i++)
		{
			if (!m_Attacks[i].active)
			{
				continue;
			}
			if (stateInfo.normalizedTime >= m_Attacks[i].endTime)
			{
				m_Attacks[i].active = false;
				for (int j = 0; j < m_Attacks[i].triggers.Length; j++)
				{
					if (!(null == m_Attacks[i].triggers[j]))
					{
						m_Attacks[i].triggers[j].active = false;
						m_Attacks[i].triggers[j].ResetHitInfo();
					}
				}
				if (null != m_AnimCtrl)
				{
					m_AnimCtrl.AnimEvent("MonsterEndAttack");
				}
				else
				{
					animator.gameObject.SendMessage("AnimatorEvent", "MonsterEndAttack", SendMessageOptions.DontRequireReceiver);
				}
			}
			else
			{
				if (m_Attacks[i].playAttack || !(stateInfo.normalizedTime >= m_Attacks[i].startTime))
				{
					continue;
				}
				m_Attacks[i].playAttack = true;
				for (int k = 0; k < m_Attacks[i].triggers.Length; k++)
				{
					if (!(null == m_Attacks[i].triggers[k]))
					{
						m_Attacks[i].triggers[k].active = true;
					}
				}
			}
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		ClearTrigger(value: false);
	}
}
