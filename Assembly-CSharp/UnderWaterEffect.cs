using UnityEngine;

[ExecuteInEditMode]
public class UnderWaterEffect : MonoBehaviour
{
	public float m_UnderwaterColorFade = 0.125f;

	public Shader m_UnderwaterShader;

	public Color m_BlendColor = new Color(0.07843f, 0.1804f, 0.29804f);

	private Material m_UnderwaterMaterial;

	private void Start()
	{
		if ((bool)m_UnderwaterShader)
		{
			m_UnderwaterMaterial = new Material(m_UnderwaterShader);
			m_UnderwaterMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height);
		m_UnderwaterMaterial.SetColor("_DepthColor", m_BlendColor);
		m_UnderwaterMaterial.SetFloat("_UnderwaterColorFade", m_UnderwaterColorFade);
		m_UnderwaterMaterial.SetVector("offsets", new Vector4(1f, 0f, 0f, 0f));
		Graphics.Blit(source, temporary, m_UnderwaterMaterial, 0);
		m_UnderwaterMaterial.SetVector("offsets", new Vector4(0f, 1f, 0f, 0f));
		Graphics.Blit(temporary, destination, m_UnderwaterMaterial, 0);
		RenderTexture.ReleaseTemporary(temporary);
	}
}
