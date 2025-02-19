using UnityEngine;

public class Light_flares : MonoBehaviour
{
	public Light m_light;

	public float totalLifeTime;

	public float lightUp;

	public float lightDuration;

	public float decay;

	public float outBreak;

	public float extinguish = 1f;

	public float intensity0;

	public float intensity1;

	public float intensity2;

	public float intensity3;

	public float intensity4;

	public float intensity5;

	public float range0;

	public float range1;

	public float range2;

	public float range3;

	public float range4;

	public float range5;

	private float elapsedTime;

	private void Update()
	{
		elapsedTime += Time.deltaTime;
		float num = elapsedTime / totalLifeTime;
		if (num < 1f)
		{
			if (num <= lightUp)
			{
				m_light.intensity = Mathf.Lerp(intensity0, intensity1, num / lightUp);
				m_light.range = Mathf.Lerp(range0, range1, num / lightUp);
			}
			else if (num <= lightDuration)
			{
				m_light.intensity = Mathf.Lerp(intensity1, intensity2, (num - lightUp) / (lightDuration - lightUp));
				m_light.range = Mathf.Lerp(range1, range2, (num - lightUp) / (lightDuration - lightUp));
			}
			else if (num <= decay)
			{
				m_light.intensity = Mathf.Lerp(intensity2, intensity3, (num - lightDuration) / (decay - lightDuration));
				m_light.range = Mathf.Lerp(range2, range3, (num - lightDuration) / (decay - lightDuration));
			}
			else if (num <= outBreak)
			{
				m_light.intensity = Mathf.Lerp(intensity3, intensity4, (num - decay) / (outBreak - decay));
				m_light.range = Mathf.Lerp(range3, range4, (num - decay) / (outBreak - decay));
			}
			else
			{
				m_light.intensity = Mathf.Lerp(intensity4, intensity5, (num - outBreak) / (extinguish - outBreak));
				m_light.range = Mathf.Lerp(range4, range5, (num - outBreak) / (extinguish - outBreak));
			}
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}
