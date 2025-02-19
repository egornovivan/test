using PETools;
using UnityEngine;

public class WaterEffectHelper : MonoBehaviour
{
	public GameObject m_UnderWaterEffect;

	public GameObject m_WaterSurfaceEffect;

	public GameObject m_AboveWaterEffect;

	public float m_SurfaceHeight = 1f;

	private void Start()
	{
		if (PEUtil.CheckPositionUnderWater(base.transform.position))
		{
			if (Physics.Raycast(base.transform.position + m_SurfaceHeight * Vector3.up, Vector3.down, out var _, 2f, 1048592))
			{
				m_UnderWaterEffect.SetActive(value: false);
				m_WaterSurfaceEffect.SetActive(value: true);
				m_AboveWaterEffect.SetActive(value: false);
			}
			else
			{
				m_UnderWaterEffect.SetActive(value: true);
				m_WaterSurfaceEffect.SetActive(value: false);
				m_AboveWaterEffect.SetActive(value: false);
			}
		}
		else
		{
			m_UnderWaterEffect.SetActive(value: false);
			m_WaterSurfaceEffect.SetActive(value: false);
			m_AboveWaterEffect.SetActive(value: true);
		}
	}
}
