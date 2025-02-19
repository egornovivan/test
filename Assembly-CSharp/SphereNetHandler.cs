using UnityEngine;

public class SphereNetHandler : MonoBehaviour
{
	private float m_TimeFactor;

	private void Start()
	{
	}

	private void Update()
	{
		m_TimeFactor += Time.deltaTime;
		GetComponent<Renderer>().material.SetFloat("_TimeFactor", m_TimeFactor);
	}
}
