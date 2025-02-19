using System;
using Pathea;
using Pathea.Effect;
using SkillSystem;
using UnityEngine;

public class PEAnimatorTrigger : StateMachineBehaviour
{
	public enum EAnimEffect
	{
		None,
		Music,
		Skill,
		Particle,
		Max
	}

	[Serializable]
	public class AnimEffect
	{
		public EAnimEffect effect;

		public string data;
	}

	[Serializable]
	public class AnimTrigger
	{
		public float time;

		public AnimEffect[] effects;

		private bool m_Dirty;

		private AudioController m_Audio;

		public bool dirty
		{
			set
			{
				m_Dirty = value;
			}
		}

		private void OnTrigger(PeEntity entity, Animator animator, AnimatorStateInfo stateInfo)
		{
			try
			{
				for (int i = 0; i < effects.Length; i++)
				{
					if (effects[i].effect == EAnimEffect.Music)
					{
						if (stateInfo.loop)
						{
							m_Audio = AudioManager.instance.Create(animator.transform.position, int.Parse(effects[i].data), null, isPlay: true, isDelete: false);
						}
						else
						{
							AudioManager.instance.Create(animator.transform.position, int.Parse(effects[i].data));
						}
					}
					else if (effects[i].effect == EAnimEffect.Particle)
					{
						Singleton<EffectBuilder>.Instance.Register(int.Parse(effects[i].data), null, animator.transform);
					}
					else if (effects[i].effect == EAnimEffect.Skill && entity != null)
					{
						entity.StartSkill(null, int.Parse(effects[i].data));
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(string.Concat("[", animator.name, "]<", ex, ">"));
			}
		}

		public void OnDeath()
		{
			if (m_Audio != null)
			{
				m_Audio.Delete();
			}
			m_Audio = null;
		}

		public void OnDisable()
		{
			if (m_Audio != null)
			{
				m_Audio.Delete();
			}
			m_Audio = null;
		}

		public void OnStateEnter(PeEntity entity, Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
		}

		public void OnStateUpdate(PeEntity entity, Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (stateInfo.normalizedTime >= time && !m_Dirty)
			{
				m_Dirty = true;
				OnTrigger(entity, animator, stateInfo);
			}
		}

		public void OnStateExit(PeEntity entity, Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (m_Audio != null && stateInfo.loop)
			{
				m_Audio.Delete();
			}
			m_Audio = null;
		}
	}

	public AnimTrigger[] triggers;

	private PeEntity m_Entity;

	private void OnDeath(SkEntity sk1, SkEntity sk2)
	{
		for (int i = 0; i < triggers.Length; i++)
		{
			triggers[i].OnDeath();
		}
	}

	private void OnDisable()
	{
		for (int i = 0; i < triggers.Length; i++)
		{
			triggers[i].OnDisable();
		}
		if (m_Entity != null && m_Entity.aliveEntity != null)
		{
			m_Entity.aliveEntity.deathEvent -= OnDeath;
		}
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (m_Entity == null)
		{
			m_Entity = animator.GetComponentInParent<PeEntity>();
			if (m_Entity != null && m_Entity.aliveEntity != null)
			{
				m_Entity.aliveEntity.deathEvent += OnDeath;
			}
		}
		for (int i = 0; i < triggers.Length; i++)
		{
			triggers[i].dirty = false;
			triggers[i].OnStateEnter(m_Entity, animator, stateInfo, layerIndex);
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		for (int i = 0; i < triggers.Length; i++)
		{
			triggers[i].OnStateUpdate(m_Entity, animator, stateInfo, layerIndex);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		for (int i = 0; i < triggers.Length; i++)
		{
			triggers[i].dirty = false;
			triggers[i].OnStateExit(m_Entity, animator, stateInfo, layerIndex);
		}
	}
}
