using Pathea;
using UnityEngine;

public class PEAnimatorAudio_Human : PEAnimatorAudio
{
	public int id;

	public float maleDelayTime;

	public float femaleDelayTime;

	private int m_Gender;

	private bool m_Trigger;

	private int[] m_Sounds;

	private float GetDelayTime(int gender)
	{
		return gender switch
		{
			1 => femaleDelayTime, 
			2 => maleDelayTime, 
			_ => 10f, 
		};
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		m_Trigger = false;
		if (m_Gender == 0)
		{
			PeEntity componentInParent = animator.GetComponentInParent<PeEntity>();
			if (componentInParent != null && componentInParent.commonCmpt != null)
			{
				m_Gender = (int)componentInParent.commonCmpt.sex;
			}
		}
		if ((m_Gender == 1 || m_Gender == 2) && m_Sounds == null)
		{
			m_Sounds = HumanSoundData.GetSoundID(id, m_Gender);
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if ((m_Gender == 1 || m_Gender == 2) && id > 0 && m_Sounds != null && m_Sounds.Length != 0 && !m_Trigger && stateInfo.normalizedTime >= GetDelayTime(m_Gender))
		{
			m_Trigger = true;
			for (int i = 0; i < m_Sounds.Length; i++)
			{
				AudioManager.instance.Create(animator.transform.position, m_Sounds[i]);
			}
		}
	}
}
