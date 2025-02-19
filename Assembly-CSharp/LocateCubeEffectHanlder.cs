using UnityEngine;

public class LocateCubeEffectHanlder : MonoBehaviour
{
	public float m_MaxHeightScale = 1f;

	public float m_MaxLengthScale = 0.8f;

	public float m_CubeLen = 1f;

	private Renderer m_Renderer;

	private Transform m_Trans;

	public bool m_Start;

	private float m_CurHeightScale;

	private float m_CurLengthScale;

	private void Start()
	{
		m_Renderer = GetComponent<Renderer>();
		m_Trans = base.transform;
		m_Start = true;
		m_Trans.localScale = new Vector3(m_CurLengthScale, m_CurHeightScale, m_CurLengthScale);
	}

	private void Update()
	{
		if (m_Start)
		{
			m_CurHeightScale = Mathf.Lerp(m_CurHeightScale, m_MaxHeightScale, 0.2f);
			m_CurLengthScale = Mathf.Lerp(m_CurLengthScale, m_MaxLengthScale, 0.2f);
			if (m_MaxHeightScale - m_CurHeightScale < 0.05f)
			{
				m_CurHeightScale = m_MaxHeightScale;
				m_CurLengthScale = m_MaxLengthScale;
				m_Start = false;
			}
			m_Trans.localScale = new Vector3(m_CurLengthScale, m_CurHeightScale, m_CurLengthScale);
		}
		else
		{
			m_CurHeightScale = 0f;
			m_CurLengthScale = 0f;
			m_Trans.localScale = new Vector3(m_MaxLengthScale, m_MaxHeightScale, m_MaxLengthScale);
		}
		m_Renderer.material.SetVector("_CenterWorldPos", base.transform.position);
		m_Renderer.material.SetFloat("_Length", m_Trans.lossyScale.x * m_CubeLen * 0.5f);
		m_Renderer.material.SetFloat("_Height", m_Trans.localScale.y * m_CubeLen * 0.5f);
	}
}
