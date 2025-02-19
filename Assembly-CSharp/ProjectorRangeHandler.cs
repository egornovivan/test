using UnityEngine;

[RequireComponent(typeof(Projector))]
public class ProjectorRangeHandler : MonoBehaviour
{
	public Material m_SourceMat;

	private Material m_ProjMat;

	private Projector m_Projector;

	private void Start()
	{
		m_ProjMat = Object.Instantiate(m_SourceMat);
		m_Projector = GetComponent<Projector>();
		m_Projector.material = m_ProjMat;
		m_Projector.orthographic = true;
	}

	private void Update()
	{
		m_ProjMat.SetVector("_CenterAndRadius", new Vector4(base.transform.position.x, base.transform.position.y - m_Projector.farClipPlane * 0.4f, base.transform.position.z, m_Projector.orthographicSize));
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
