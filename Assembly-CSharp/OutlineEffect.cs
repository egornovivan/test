using UnityEngine;

public class OutlineEffect : MonoBehaviour
{
	public delegate void outliningEventHandler(bool active);

	public LayerMask layerMask;

	public int stencilBufferDepth = 16;

	public int downsampleFactor = 4;

	public int iterations = 2;

	public float blurMinSpread = 0.65f;

	public float blurSpread = 0.25f;

	public float blurIntensity = 0.3f;

	private static Shader m_BlurShader;

	private static Material m_BlurMat;

	private static Shader m_CompShader;

	private static Material m_CompMat;

	private RenderTexture m_StencilBuffer;

	private GameObject m_StencilRendererGo;

	private static Shader blurShader
	{
		get
		{
			if (m_BlurShader == null)
			{
				m_BlurShader = Shader.Find("wuyiqiu/Outline/Blur");
			}
			return m_BlurShader;
		}
	}

	private static Material blurMaterial
	{
		get
		{
			if (m_BlurMat == null)
			{
				m_BlurMat = new Material(blurShader);
				m_BlurMat.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_BlurMat;
		}
	}

	private static Shader compShader
	{
		get
		{
			if (m_CompShader == null)
			{
				m_CompShader = Shader.Find("wuyiqiu/Outline/Composite");
			}
			return m_CompShader;
		}
	}

	private static Material compMaterial
	{
		get
		{
			if (m_CompMat == null)
			{
				m_CompMat = new Material(compShader);
				m_CompMat.hideFlags = HideFlags.DontSave;
			}
			return m_CompMat;
		}
	}

	public static event outliningEventHandler OutliningEventHandler;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnDestroy()
	{
		if (m_StencilRendererGo != null)
		{
			Object.DestroyImmediate(m_StencilRendererGo);
		}
		if (m_StencilBuffer != null)
		{
			RenderTexture.ReleaseTemporary(m_StencilBuffer);
		}
		if (m_BlurMat != null)
		{
			Object.DestroyImmediate(m_BlurMat);
		}
		if (m_CompMat != null)
		{
			Object.DestroyImmediate(m_CompMat);
		}
	}

	private void OnPreRender()
	{
		if (m_StencilBuffer != null)
		{
			RenderTexture.ReleaseTemporary(m_StencilBuffer);
			m_StencilBuffer = null;
		}
		if (base.enabled && base.gameObject.activeInHierarchy && OutlineEffect.OutliningEventHandler != null)
		{
			OutlineEffect.OutliningEventHandler(active: true);
			m_StencilBuffer = RenderTexture.GetTemporary(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, stencilBufferDepth, RenderTextureFormat.ARGB32);
			Camera camera = null;
			if (m_StencilRendererGo == null)
			{
				m_StencilRendererGo = new GameObject("Stencil renderer camera" + GetInstanceID(), typeof(Camera), typeof(Skybox));
			}
			camera = m_StencilRendererGo.GetComponent<Camera>();
			camera.enabled = false;
			camera.CopyFrom(GetComponent<Camera>());
			camera.cullingMask = layerMask;
			camera.renderingPath = RenderingPath.VertexLit;
			camera.rect = new Rect(0f, 0f, 1f, 1f);
			camera.hdr = false;
			camera.useOcclusionCulling = false;
			camera.backgroundColor = Color.clear;
			camera.clearFlags = CameraClearFlags.Color;
			camera.targetTexture = m_StencilBuffer;
			camera.Render();
			if (OutlineEffect.OutliningEventHandler != null)
			{
				OutlineEffect.OutliningEventHandler(active: false);
			}
		}
	}

	public void FourTapCone(RenderTexture source, RenderTexture dest, int iteration)
	{
		float value = blurMinSpread + (float)iteration * blurSpread;
		blurMaterial.SetFloat("_OffsetScale", value);
		Graphics.Blit(source, dest, blurMaterial);
	}

	private void DownSample4x(RenderTexture source, RenderTexture dest)
	{
		float value = 1f;
		blurMaterial.SetFloat("_OffsetScale", value);
		Graphics.Blit(source, dest, blurMaterial);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (m_StencilBuffer == null)
		{
			Graphics.Blit(source, destination);
			return;
		}
		int width = source.width / downsampleFactor;
		int height = source.height / downsampleFactor;
		RenderTexture temporary = RenderTexture.GetTemporary(width, height, stencilBufferDepth, RenderTextureFormat.ARGB32);
		RenderTexture temporary2 = RenderTexture.GetTemporary(width, height, stencilBufferDepth, RenderTextureFormat.ARGB32);
		DownSample4x(m_StencilBuffer, temporary);
		bool flag = false;
		for (int i = 0; i < iterations; i++)
		{
			if (!flag)
			{
				FourTapCone(temporary, temporary2, i);
			}
			else
			{
				FourTapCone(temporary2, temporary, i);
			}
			flag = !flag;
		}
		compMaterial.SetTexture("_StencilTex", m_StencilBuffer);
		compMaterial.SetTexture("_BlurTex", (!flag) ? temporary2 : temporary);
		Graphics.Blit(source, destination, compMaterial);
		RenderTexture.ReleaseTemporary(temporary);
		RenderTexture.ReleaseTemporary(temporary2);
	}
}
