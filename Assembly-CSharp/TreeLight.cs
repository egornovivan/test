using PETools;
using UnityEngine;

public class TreeLight : MonoBehaviour
{
	public float m_MaxIntensity = 1f;

	public float m_FadeDist = 64f;

	public float m_Falloff = 8f;

	public Color m_MaxFlareColor = Color.black;

	private LensFlare m_LensFlare;

	private Light m_Light;

	private void Start()
	{
		m_Light = GetComponent<Light>();
		m_MaxIntensity = m_Light.intensity;
		m_Light.enabled = false;
		m_LensFlare = GetComponent<LensFlare>();
		if (m_LensFlare != null)
		{
			m_MaxFlareColor = m_LensFlare.color;
		}
	}

	private void Update()
	{
		if (!(PEUtil.MainCamTransform != null))
		{
			return;
		}
		Vector3 position = PEUtil.MainCamTransform.position;
		float sqrMagnitude = (base.transform.position - position).sqrMagnitude;
		float num = m_FadeDist * m_FadeDist;
		float num2 = (m_FadeDist - m_Falloff) * (m_FadeDist - m_Falloff);
		if (sqrMagnitude < num)
		{
			m_Light.enabled = true;
			if (m_LensFlare != null)
			{
				m_LensFlare.enabled = true;
			}
			float num3 = Mathf.Clamp01((num - sqrMagnitude) / (num - num2));
			float num4 = 1f;
			float num5 = Mathf.Clamp01((num4 + 0.1f) * 2f);
			m_Light.intensity = m_MaxIntensity * num3 * num5;
			if (m_LensFlare != null)
			{
				m_LensFlare.color = m_MaxFlareColor * num5;
			}
		}
		else
		{
			m_Light.enabled = false;
			if (m_LensFlare != null)
			{
				m_LensFlare.enabled = false;
			}
		}
	}
}
