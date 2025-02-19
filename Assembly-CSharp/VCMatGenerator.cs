using System;
using UnityEngine;

public class VCMatGenerator : MonoBehaviour
{
	private static VCMatGenerator s_Instance;

	public Camera m_TextureCam;

	public Transform m_TexturePlanesGroup;

	public GameObject m_TemplatePlane;

	public Material m_TemplatePlaneMat;

	public Material m_TemplateMeshMat;

	public GameObject m_IconGenGroup;

	public Camera m_IconGenCamera;

	public Renderer m_IconCubeRenderer;

	private MeshRenderer[] m_PlaneRenderers;

	private Material[] m_PlaneMaterials;

	public static VCMatGenerator Instance => s_Instance;

	private void Awake()
	{
		s_Instance = this;
	}

	private void Start()
	{
		m_TextureCam.orthographicSize = 2f;
		m_TextureCam.aspect = 1f;
		m_TextureCam.transform.position = new Vector3(2f, 2f, 2f);
		m_PlaneRenderers = new MeshRenderer[16];
		m_PlaneMaterials = new Material[16];
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(m_TemplatePlane);
				gameObject.name = "Render plane [" + i + "," + j + "]";
				gameObject.transform.parent = m_TexturePlanesGroup;
				gameObject.transform.position = new Vector3((float)j + 0.4999f, 0f, (float)i + 0.4999f);
				Material material = UnityEngine.Object.Instantiate(m_TemplatePlaneMat);
				material.name = "Render material [" + i + "," + j + "]";
				gameObject.GetComponent<Renderer>().material = material;
				m_PlaneRenderers[i * 4 + j] = gameObject.GetComponent<MeshRenderer>();
				m_PlaneMaterials[i * 4 + j] = material;
				gameObject.SetActive(value: true);
			}
		}
	}

	private void Update()
	{
		m_TextureCam.orthographicSize = 2f;
		m_TextureCam.aspect = 1f;
	}

	private void OnDestroy()
	{
		if (!(s_Instance != null))
		{
			return;
		}
		try
		{
			if (m_PlaneRenderers != null)
			{
				MeshRenderer[] planeRenderers = m_PlaneRenderers;
				foreach (MeshRenderer meshRenderer in planeRenderers)
				{
					if (meshRenderer != null && meshRenderer.gameObject != null)
					{
						UnityEngine.Object.Destroy(meshRenderer.gameObject);
					}
				}
			}
			if (m_PlaneMaterials != null)
			{
				Material[] planeMaterials = m_PlaneMaterials;
				foreach (Material material in planeMaterials)
				{
					if (material != null)
					{
						UnityEngine.Object.Destroy(material);
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.Log("VCMatGenerator OnDestroy have some problem" + ex.ToString());
		}
		s_Instance = null;
	}

	public ulong GenMeshMaterial(VCMaterial[] mat_list, bool bForEditor = false)
	{
		RenderTexture renderTexture = null;
		RenderTexture renderTexture2 = null;
		Texture2D texture2D = null;
		Vector4 vector = new Vector4(0.25f, 0.25f, 1f, 128f);
		ulong num = VCMaterial.CalcMatGroupHash(mat_list);
		if (bForEditor)
		{
			if (!VCEditor.DocumentOpen())
			{
				return 0uL;
			}
			renderTexture = VCEditor.s_Scene.m_TempIsoMat.m_DiffTex;
			renderTexture2 = VCEditor.s_Scene.m_TempIsoMat.m_BumpTex;
			texture2D = VCEditor.s_Scene.m_TempIsoMat.m_PropertyTex;
			Material editorMat = VCEditor.s_Scene.m_TempIsoMat.m_EditorMat;
			editorMat.SetTexture("_DiffuseTex", renderTexture);
			editorMat.SetTexture("_BumpTex", renderTexture2);
			editorMat.SetTexture("_PropertyTex", texture2D);
			editorMat.SetVector("_Settings1", vector);
		}
		else
		{
			if (VCMatManager.Instance == null)
			{
				return 0uL;
			}
			if (VCMatManager.Instance.m_mapMaterials.ContainsKey(num))
			{
				renderTexture = VCMatManager.Instance.m_mapDiffuseTexs[num];
				renderTexture2 = VCMatManager.Instance.m_mapBumpTexs[num];
				texture2D = VCMatManager.Instance.m_mapPropertyTexs[num];
			}
			else
			{
				renderTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
				renderTexture2 = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
				texture2D = new Texture2D(16, 4, TextureFormat.ARGB32, mipmap: false, linear: false);
				renderTexture.anisoLevel = 4;
				renderTexture.filterMode = FilterMode.Trilinear;
				renderTexture.useMipMap = true;
				renderTexture.wrapMode = TextureWrapMode.Repeat;
				renderTexture2.anisoLevel = 4;
				renderTexture2.filterMode = FilterMode.Trilinear;
				renderTexture2.useMipMap = true;
				renderTexture2.wrapMode = TextureWrapMode.Repeat;
				texture2D.anisoLevel = 0;
				texture2D.filterMode = FilterMode.Point;
				Material material = UnityEngine.Object.Instantiate(m_TemplateMeshMat);
				material.name = "VCMat #" + num.ToString("X").PadLeft(16, '0');
				material.SetTexture("_DiffuseTex", renderTexture);
				material.SetTexture("_BumpTex", renderTexture2);
				material.SetTexture("_PropertyTex", texture2D);
				material.SetVector("_Settings1", vector);
				VCMatManager.Instance.m_mapMaterials.Add(num, material);
				VCMatManager.Instance.m_mapDiffuseTexs.Add(num, renderTexture);
				VCMatManager.Instance.m_mapBumpTexs.Add(num, renderTexture2);
				VCMatManager.Instance.m_mapPropertyTexs.Add(num, texture2D);
				VCMatManager.Instance.m_mapMatRefCounters.Add(num, 0);
			}
		}
		m_TexturePlanesGroup.gameObject.SetActive(value: true);
		for (int i = 0; i < mat_list.Length && i < 16; i++)
		{
			if (mat_list[i] != null)
			{
				m_PlaneRenderers[i].material.SetTexture("_MainTex", mat_list[i].m_DiffuseTex);
			}
			else
			{
				m_PlaneRenderers[i].material.SetTexture("_MainTex", Resources.Load(VCMaterial.s_BlankDiffuseRes) as Texture2D);
			}
		}
		m_TextureCam.targetTexture = renderTexture;
		m_TextureCam.Render();
		for (int j = 0; j < mat_list.Length && j < 16; j++)
		{
			if (mat_list[j] != null)
			{
				m_PlaneRenderers[j].material.SetTexture("_MainTex", mat_list[j].m_BumpTex);
			}
			else
			{
				m_PlaneRenderers[j].material.SetTexture("_MainTex", Resources.Load(VCMaterial.s_BlankBumpRes) as Texture2D);
			}
		}
		m_TextureCam.targetTexture = renderTexture2;
		m_TextureCam.Render();
		for (int k = 0; k < mat_list.Length && k < 16; k++)
		{
			if (mat_list[k] != null)
			{
				texture2D.SetPixel(k, 0, mat_list[k].m_SpecularColor);
				texture2D.SetPixel(k, 1, mat_list[k].m_EmissiveColor);
				texture2D.SetPixel(k, 2, new Color(mat_list[k].m_BumpStrength, mat_list[k].m_SpecularStrength / 2f, mat_list[k].m_SpecularPower / 255f, 1f));
				texture2D.SetPixel(k, 3, new Color(mat_list[k].m_Tile / 17f, 0f, 0f, 1f));
			}
			else
			{
				texture2D.SetPixel(k, 0, Color.black);
				texture2D.SetPixel(k, 1, Color.black);
				texture2D.SetPixel(k, 2, new Color(0f, 0f, 0f, 1f));
				texture2D.SetPixel(k, 3, new Color(0f, 0f, 0f, 1f));
			}
		}
		texture2D.Apply();
		m_TextureCam.targetTexture = null;
		m_TexturePlanesGroup.gameObject.SetActive(value: false);
		return num;
	}

	public void GenMaterialIcon(VCMaterial vcmat)
	{
		m_IconGenGroup.SetActive(value: true);
		if (vcmat.m_Icon == null)
		{
			vcmat.m_Icon = new RenderTexture(64, 64, 24, RenderTextureFormat.ARGB32);
		}
		m_IconCubeRenderer.material.mainTexture = vcmat.m_DiffuseTex;
		m_IconCubeRenderer.material.SetTexture("_BumpMap", vcmat.m_BumpTex);
		m_IconGenCamera.targetTexture = vcmat.m_Icon;
		Color ambientLight = RenderSettings.ambientLight;
		RenderSettings.ambientLight = Color.black;
		m_IconGenCamera.Render();
		RenderSettings.ambientLight = ambientLight;
		m_IconGenGroup.SetActive(value: false);
	}
}
