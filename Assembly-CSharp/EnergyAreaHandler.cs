using UnityEngine;

public class EnergyAreaHandler : MonoBehaviour
{
	public Material m_SourceMat;

	private Material m_ProjMat;

	private Projector m_Projector;

	public float m_StreamSpeed = 0.5f;

	public float m_EnergyScale = 1f;

	private float m_UnitBodyIntens;

	private float m_UnitStreamIntens;

	private void Start()
	{
		m_ProjMat = Object.Instantiate(m_SourceMat);
		m_UnitBodyIntens = m_ProjMat.GetFloat("_BodyIntensity");
		m_UnitStreamIntens = m_ProjMat.GetFloat("_StreamIntensity");
		m_Projector = GetComponent<Projector>();
		m_Projector.material = m_ProjMat;
		m_ProjMat.name = "Energy Area - " + base.transform.position.ToString();
	}

	private void Update()
	{
		m_ProjMat.SetVector("_CenterAndRadius", new Vector4(base.transform.position.x, base.transform.position.y - m_Projector.farClipPlane * 0.5f, base.transform.position.z, m_Projector.orthographicSize));
		m_ProjMat.SetFloat("_Speed", m_StreamSpeed);
		m_ProjMat.SetFloat("_BodyIntensity", m_UnitBodyIntens * m_EnergyScale);
		m_ProjMat.SetFloat("_StreamIntensity", m_UnitStreamIntens * m_EnergyScale);
		m_ProjMat.SetFloat("_ExhaustEffect", Mathf.Clamp01((0.25f - m_EnergyScale) * 4f));
	}

	private void OnDestroy()
	{
		if (m_Projector != null)
		{
			m_Projector.material = null;
		}
		Object.Destroy(m_ProjMat);
	}
}
