using UnityEngine;

public class IKDrive : MonoBehaviour
{
	[HideInInspector]
	public Transform m_LHand;

	[HideInInspector]
	public Transform m_RHand;

	[HideInInspector]
	public float HandFoward = 0.07f;

	[HideInInspector]
	public float HandUp = 0.03f;

	private Animator m_Anim;

	public bool active { get; set; }

	private void Awake()
	{
		m_Anim = GetComponent<Animator>();
		active = false;
	}

	private void OnAnimatorIK()
	{
		if (!(null != m_Anim))
		{
			return;
		}
		m_Anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, (!active) ? 0f : 1f);
		m_Anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, (!active) ? 0f : 1f);
		m_Anim.SetIKPositionWeight(AvatarIKGoal.RightHand, (!active) ? 0f : 1f);
		m_Anim.SetIKRotationWeight(AvatarIKGoal.RightHand, (!active) ? 0f : 1f);
		if (active)
		{
			if (null != m_LHand)
			{
				m_Anim.SetIKPosition(AvatarIKGoal.LeftHand, m_LHand.position - m_LHand.transform.forward * HandFoward + m_LHand.transform.up * HandUp);
				m_Anim.SetIKRotation(AvatarIKGoal.LeftHand, m_LHand.transform.rotation);
			}
			if (null != m_RHand)
			{
				m_Anim.SetIKPosition(AvatarIKGoal.RightHand, m_RHand.position - m_RHand.transform.forward * HandFoward + m_RHand.transform.up * HandUp);
				m_Anim.SetIKRotation(AvatarIKGoal.RightHand, m_RHand.transform.rotation);
			}
		}
	}
}
