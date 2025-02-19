using UnityEngine;

public class VCEHoloBoard : GLBehaviour
{
	public Vector3 m_Position;

	private float m_FadeFactor;

	private float m_FadeFactorWanted;

	private bool m_MaterialReplaced;

	public void ReplaceMat()
	{
		if (!m_MaterialReplaced)
		{
			m_Material = Object.Instantiate(m_Material);
			m_MaterialReplaced = true;
		}
	}

	public void FadeIn()
	{
		base.gameObject.SetActive(value: true);
		m_FadeFactorWanted = 1f;
		if (m_FadeFactor < 0.001f)
		{
			m_FadeFactor = 0.0011f;
		}
	}

	public void FadeOut()
	{
		m_FadeFactorWanted = 0f;
	}

	private void Start()
	{
	}

	private void OnEnable()
	{
		Update();
	}

	private void Update()
	{
		ReplaceMat();
		if (VCEditor.Instance != null)
		{
			Vector3 normalized = (m_Position - VCEditor.Instance.m_MainCamera.transform.position).normalized;
			base.transform.position = VCEditor.Instance.m_MainCamera.transform.position + normalized * 2f;
			base.transform.eulerAngles = VCEditor.Instance.m_MainCamera.transform.eulerAngles;
			base.transform.localScale = new Vector3(1f, 2f, 1f);
		}
		if (m_FadeFactor > m_FadeFactorWanted)
		{
			m_FadeFactor = Mathf.Lerp(m_FadeFactor, m_FadeFactorWanted, 0.3f);
		}
		else
		{
			m_FadeFactor = Mathf.Lerp(m_FadeFactor, m_FadeFactorWanted, Time.deltaTime * 3f);
		}
		m_Material.SetFloat("_Fade", m_FadeFactor);
		if (m_FadeFactor < 0.001f)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public override void OnGL()
	{
		GL.Begin(7);
		GL.Color(Color.white);
		GL.TexCoord2(0f, 1f);
		GL.Vertex(base.transform.position - base.transform.right * 0.55f);
		GL.TexCoord2(1f, 1f);
		GL.Vertex(base.transform.position + base.transform.right * 1.45f);
		GL.TexCoord2(1f, 0f);
		GL.Vertex(base.transform.position + base.transform.right * 1.45f - base.transform.up * 2f);
		GL.TexCoord2(0f, 0f);
		GL.Vertex(base.transform.position - base.transform.right * 0.55f - base.transform.up * 2f);
		GL.End();
	}
}
