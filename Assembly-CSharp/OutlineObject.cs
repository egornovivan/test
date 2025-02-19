using System.Collections.Generic;
using UnityEngine;

public class OutlineObject : MonoBehaviour
{
	private class RendererCache
	{
		public Renderer m_Renderer;

		public GameObject m_Go;

		private Material[] m_SourceMats;

		private Material[] m_ReplacedMats;

		public RendererCache(Renderer rend, Material[] mats, Material sharedOpaqueMaterial)
		{
			m_Renderer = rend;
			m_SourceMats = mats;
			m_Go = rend.gameObject;
			m_ReplacedMats = new Material[mats.Length];
			for (int i = 0; i < mats.Length; i++)
			{
				m_ReplacedMats[i] = sharedOpaqueMaterial;
			}
		}

		public void SetMaterialsState(bool state)
		{
			m_Renderer.sharedMaterials = ((!state) ? m_SourceMats : m_ReplacedMats);
		}

		public void ReplaceSource(Material old_mat, Material new_mat)
		{
			for (int i = 0; i < m_SourceMats.Length; i++)
			{
				if (m_SourceMats[i] == old_mat)
				{
					m_SourceMats[i] = new_mat;
				}
			}
		}
	}

	public static int highlightingLayer = 26;

	private Color outlineColor = Color.green;

	private static Shader m_OpaqueShader;

	private Material m_OpaqueMat;

	private List<RendererCache> m_RendererCaches;

	private bool m_Once = true;

	private LayerMask[] m_LayerMasks;

	public bool m_Occluder;

	private readonly Color occluderColor = new Color(0f, 0f, 0f, 0.005f);

	private static int _outlinePropertyID = -1;

	public static Shader opaqueShader
	{
		get
		{
			if (m_OpaqueShader == null)
			{
				m_OpaqueShader = Shader.Find("wuyiqiu/Outline/StencilZ");
			}
			return m_OpaqueShader;
		}
	}

	public Material opaqueMat
	{
		get
		{
			if (m_OpaqueMat == null)
			{
				m_OpaqueMat = new Material(opaqueShader);
				m_OpaqueMat.hideFlags = HideFlags.DontSave;
				m_OpaqueMat.SetColor("_Outline", outlineColor);
			}
			return m_OpaqueMat;
		}
	}

	private static int outlinePropertyID
	{
		get
		{
			if (_outlinePropertyID == -1)
			{
				_outlinePropertyID = Shader.PropertyToID("_Outline");
			}
			return _outlinePropertyID;
		}
	}

	public Color color
	{
		get
		{
			return outlineColor;
		}
		set
		{
			outlineColor = value;
			opaqueMat.SetColor(outlinePropertyID, value);
		}
	}

	public void ReplaceInCache(Material old_mat, Material new_mat)
	{
		if (m_RendererCaches == null)
		{
			return;
		}
		foreach (RendererCache rendererCache in m_RendererCaches)
		{
			rendererCache.ReplaceSource(old_mat, new_mat);
		}
	}

	private void InitMaterial()
	{
		MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
		if (componentsInChildren != null)
		{
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Material[] sharedMaterials = componentsInChildren[i].sharedMaterials;
				m_RendererCaches.Add(new RendererCache(componentsInChildren[i], sharedMaterials, opaqueMat));
			}
		}
		SkinnedMeshRenderer[] componentsInChildren2 = GetComponentsInChildren<SkinnedMeshRenderer>();
		if (componentsInChildren2 != null)
		{
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				Material[] sharedMaterials2 = componentsInChildren2[j].sharedMaterials;
				m_RendererCaches.Add(new RendererCache(componentsInChildren2[j], sharedMaterials2, opaqueMat));
			}
		}
	}

	private void OutliningEventHandler(bool active)
	{
		if (active)
		{
			if (m_Once || m_Occluder)
			{
				if (m_Occluder)
				{
					color = occluderColor;
					m_Occluder = false;
				}
				m_RendererCaches = new List<RendererCache>();
				InitMaterial();
				m_Once = false;
				m_LayerMasks = new LayerMask[m_RendererCaches.Count];
			}
			for (int i = 0; i < m_RendererCaches.Count; i++)
			{
				if (m_RendererCaches[i].m_Go == null)
				{
					m_Once = true;
					continue;
				}
				ref LayerMask reference = ref m_LayerMasks[i];
				reference = m_RendererCaches[i].m_Go.layer;
				m_RendererCaches[i].m_Go.layer = highlightingLayer;
				m_RendererCaches[i].SetMaterialsState(state: true);
			}
			return;
		}
		for (int j = 0; j < m_RendererCaches.Count; j++)
		{
			if (!(m_RendererCaches[j].m_Go == null))
			{
				m_RendererCaches[j].m_Go.layer = m_LayerMasks[j];
				m_RendererCaches[j].SetMaterialsState(state: false);
			}
		}
	}

	private void OnEnable()
	{
		m_Once = true;
		OutlineEffect.OutliningEventHandler += OutliningEventHandler;
	}

	private void OnDisable()
	{
		OutlineEffect.OutliningEventHandler -= OutliningEventHandler;
		if (m_OpaqueMat != null)
		{
			Object.DestroyImmediate(m_OpaqueMat);
		}
	}

	private void OnDestroy()
	{
		OutlineEffect.OutliningEventHandler -= OutliningEventHandler;
		if (m_OpaqueMat != null)
		{
			Object.DestroyImmediate(m_OpaqueMat);
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
