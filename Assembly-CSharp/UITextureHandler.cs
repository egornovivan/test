using UnityEngine;

[ExecuteInEditMode]
public class UITextureHandler : MonoBehaviour
{
	public Material m_Material;

	private Material m_MatInst;

	private void OnEnable()
	{
		if (m_Material != null)
		{
			if (m_MatInst != null)
			{
				Object.DestroyImmediate(m_MatInst);
			}
			m_MatInst = Object.Instantiate(m_Material);
			UITexture component = GetComponent<UITexture>();
			if (component != null)
			{
				component.material = m_MatInst;
			}
		}
	}

	private void OnDestroy()
	{
		if (m_MatInst != null)
		{
			Object.DestroyImmediate(m_MatInst);
			m_MatInst = null;
		}
	}

	private void Update()
	{
	}
}
