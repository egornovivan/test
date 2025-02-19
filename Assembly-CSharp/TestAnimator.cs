using UnityEngine;

public class TestAnimator : MonoBehaviour
{
	public float speed;

	[Range(0f, 1f)]
	public float direction;

	private Animator m_Animator;

	private void Awake()
	{
		m_Animator = GetComponent<Animator>();
	}

	private void Update()
	{
		if (m_Animator != null)
		{
			m_Animator.SetFloat("Speed", speed, 0.15f, Time.deltaTime);
			m_Animator.SetFloat("Direction", direction, 0.15f, Time.deltaTime);
		}
	}
}
