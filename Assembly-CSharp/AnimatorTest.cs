using UnityEngine;

public class AnimatorTest : MonoBehaviour
{
	private Animator m_Animator;

	private void Awake()
	{
		m_Animator = GetComponent<Animator>();
	}

	private void OnAnimatorMove()
	{
		if (m_Animator != null)
		{
			base.transform.position += m_Animator.deltaPosition;
			base.transform.rotation *= m_Animator.deltaRotation;
		}
	}
}
