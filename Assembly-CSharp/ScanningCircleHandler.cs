using UnityEngine;

public class ScanningCircleHandler : MonoBehaviour
{
	public float m_Brightness = 1f;

	public float m_CircleBrightness = 0.5f;

	private Renderer m_Renderer;

	private void Start()
	{
		m_Renderer = GetComponentInChildren<Renderer>();
	}

	private void Update()
	{
		TakeEffect();
	}

	public void TakeEffect()
	{
		if (m_Renderer == null)
		{
			m_Renderer = GetComponentInChildren<Renderer>();
		}
		if (m_Renderer != null)
		{
			m_Renderer.material.SetFloat("_Brightness", m_Brightness);
			m_Renderer.material.SetFloat("_CircleBrightness", m_CircleBrightness);
		}
	}
}
