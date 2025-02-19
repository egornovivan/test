using Pathea;
using PETools;
using UnityEngine;

public class StandardAlphaAnimator : MonoBehaviour, ICloneModelHelper
{
	public enum BlendMode
	{
		Opaque,
		Cutout,
		Fade,
		Transparent
	}

	public enum EMode
	{
		None,
		FadeIn,
		FadeOut
	}

	public delegate void DNotify();

	public static bool s_Enable = true;

	public bool _GenFadeIn;

	public bool _GenFadeOut;

	public bool _RestShader;

	public bool LensFade;

	public SkinnedMeshRenderer m_SkinnedRenderer;

	private SkinnedMeshRenderer _copySkinRenderer;

	private Transform m_Trans;

	public BlendMode TransparentMode = BlendMode.Transparent;

	private bool _needRevert;

	public float MinDis = 1.5f;

	public float HeightOffset = 1.4f;

	public Material DepthOnlyMat;

	private bool _nearCam;

	private float m_FadeTime = 5f;

	private float m_CurTime;

	private float m_CurAlpha = 1f;

	private EMode m_Mode;

	private bool _customFading;

	private bool _disable;

	public event DNotify onFadeIn;

	public event DNotify onFadeOut;

	private void Awake()
	{
		m_Trans = base.transform;
	}

	private void OnDisable()
	{
		if (_copySkinRenderer != null)
		{
			_copySkinRenderer.enabled = false;
		}
	}

	private void Update()
	{
		if (!s_Enable)
		{
			return;
		}
		if (m_SkinnedRenderer == null)
		{
			m_SkinnedRenderer = base.gameObject.GetComponent<SkinnedMeshRenderer>();
		}
		if (m_SkinnedRenderer == null)
		{
			return;
		}
		CustomFadeUpdate();
		if (LensFade)
		{
			UpdateLensFadeInOut();
		}
		else
		{
			_nearCam = false;
		}
		if (!_nearCam && !_customFading)
		{
			if (_needRevert)
			{
				RevertMode();
				_needRevert = false;
			}
		}
		else
		{
			_needRevert = true;
		}
		if (_GenFadeIn)
		{
			FadeIn();
			_GenFadeIn = false;
		}
		if (_GenFadeOut)
		{
			FadeOut();
			_GenFadeOut = false;
		}
	}

	private void UpdateLensFadeInOut()
	{
		if (Camera.main == null || _disable)
		{
			return;
		}
		Camera main = Camera.main;
		Vector3 position = m_Trans.position;
		position.y += HeightOffset;
		float minDis = MinDis;
		float num = minDis * 1.2f;
		float num2 = Vector3.SqrMagnitude(position - main.transform.position);
		if (num2 < num * num)
		{
			if (num2 < minDis * minDis)
			{
				if (_copySkinRenderer != null)
				{
					_copySkinRenderer.enabled = m_SkinnedRenderer.enabled;
				}
				_nearCam = true;
				CreateCopySkinnedRenderer();
				Material[] materials = m_SkinnedRenderer.materials;
				foreach (Material material in materials)
				{
					SetupMaterialWithBlendMode(material, TransparentMode);
				}
			}
			else
			{
				_nearCam = false;
			}
		}
		else
		{
			_nearCam = false;
		}
	}

	private void CustomFadeUpdate()
	{
		if (m_Mode == EMode.None)
		{
			return;
		}
		CreateCopySkinnedRenderer();
		Material[] materials = m_SkinnedRenderer.materials;
		foreach (Material material in materials)
		{
			SetupMaterialWithBlendMode(material, TransparentMode);
		}
		_needRevert = true;
		m_CurTime += Time.deltaTime;
		if (m_Mode == EMode.FadeIn)
		{
			_disable = false;
			if (Mathf.Abs(m_CurAlpha - 1f) <= 0.001f)
			{
				m_Mode = EMode.None;
				m_CurTime = 0f;
				_customFading = false;
				if (this.onFadeIn != null)
				{
					this.onFadeIn();
				}
				return;
			}
			m_CurAlpha = Mathf.Lerp(0f, 1f, m_CurTime / m_FadeTime);
			Material[] materials2 = m_SkinnedRenderer.materials;
			foreach (Material material2 in materials2)
			{
				Color color = material2.GetColor("_Color");
				color.a = m_CurAlpha;
				material2.SetColor("_Color", color);
			}
			_customFading = true;
		}
		else
		{
			if (m_Mode != EMode.FadeOut)
			{
				return;
			}
			_disable = true;
			if (m_CurAlpha < 0.001f)
			{
				m_Mode = EMode.None;
				m_CurTime = 0f;
				if (this.onFadeOut != null)
				{
					this.onFadeOut();
				}
				if (_copySkinRenderer != null)
				{
					_copySkinRenderer.enabled = false;
				}
				return;
			}
			m_CurAlpha = Mathf.Lerp(1f, 0f, m_CurTime / m_FadeTime);
			Material[] materials3 = m_SkinnedRenderer.materials;
			foreach (Material material3 in materials3)
			{
				Color color2 = material3.GetColor("_Color");
				color2.a = m_CurAlpha;
				material3.SetColor("_Color", color2);
			}
			_customFading = true;
		}
	}

	public void SetAlpha(float alpha)
	{
		if (s_Enable)
		{
			if (m_SkinnedRenderer == null)
			{
				m_SkinnedRenderer = base.gameObject.GetComponent<SkinnedMeshRenderer>();
			}
			CreateCopySkinnedRenderer();
			Material[] materials = m_SkinnedRenderer.materials;
			foreach (Material material in materials)
			{
				SetupMaterialWithBlendMode(material, TransparentMode);
			}
			Material[] materials2 = m_SkinnedRenderer.materials;
			foreach (Material material2 in materials2)
			{
				Color color = material2.GetColor("_Color");
				color.a = alpha;
				material2.SetColor("_Color", color);
			}
		}
	}

	public void ResetView()
	{
		if (s_Enable)
		{
			if (m_SkinnedRenderer == null)
			{
				m_SkinnedRenderer = base.gameObject.GetComponent<SkinnedMeshRenderer>();
			}
			Material[] materials = m_SkinnedRenderer.materials;
			foreach (Material material in materials)
			{
				SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));
			}
			Material[] materials2 = m_SkinnedRenderer.materials;
			foreach (Material material2 in materials2)
			{
				Color color = material2.GetColor("_Color");
				color.a = 1f;
				material2.SetColor("_Color", color);
			}
			m_SkinnedRenderer.enabled = true;
		}
	}

	public void FadeIn(float time = 2f)
	{
		m_Mode = EMode.FadeIn;
		m_FadeTime = time;
		m_CurTime = Mathf.InverseLerp(0f, 1f, m_CurAlpha) * m_FadeTime;
	}

	public void FadeOut(float time = 2f)
	{
		m_Mode = EMode.FadeOut;
		m_FadeTime = time;
		m_CurTime = Mathf.InverseLerp(1f, 0f, m_CurAlpha) * m_FadeTime;
	}

	private void RevertMode()
	{
		Material[] materials = m_SkinnedRenderer.materials;
		foreach (Material material in materials)
		{
			Color color = material.GetColor("_Color");
			color.a = 1f;
			material.SetColor("_Color", color);
			SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));
		}
		if (_copySkinRenderer != null)
		{
			_copySkinRenderer.enabled = false;
		}
	}

	private void CreateRagdollGameObject()
	{
		BiologyViewCmpt componentInParent = GetComponentInParent<BiologyViewCmpt>();
		if (componentInParent != null && componentInParent.monoRagdollCtrlr != null)
		{
			Transform child = PEUtil.GetChild(componentInParent.monoRagdollCtrlr.transform, base.name);
			if (child != null)
			{
				GameObject gameObject = new GameObject("Skinned_Copy");
				gameObject.transform.parent = child.parent;
			}
		}
	}

	private void CreateCopySkinnedRenderer()
	{
		if (_copySkinRenderer == null)
		{
			CreateRagdollGameObject();
			GameObject gameObject = new GameObject("Skinned_Copy");
			gameObject.transform.parent = m_SkinnedRenderer.transform.parent;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			_copySkinRenderer = gameObject.AddComponent<SkinnedMeshRenderer>();
			_copySkinRenderer.rootBone = m_SkinnedRenderer.rootBone;
			_copySkinRenderer.bones = m_SkinnedRenderer.bones;
			_copySkinRenderer.localBounds = m_SkinnedRenderer.localBounds;
			_copySkinRenderer.lightmapIndex = m_SkinnedRenderer.lightmapIndex;
			_copySkinRenderer.lightmapScaleOffset = m_SkinnedRenderer.lightmapScaleOffset;
			if (DepthOnlyMat != null)
			{
				Material[] array = new Material[m_SkinnedRenderer.materials.Length];
				for (int i = 0; i < array.Length; i++)
				{
					Material material = Object.Instantiate(DepthOnlyMat);
					material.hideFlags = HideFlags.DontSave;
					array[i] = material;
					CopyOrgMatToDepthMat(m_SkinnedRenderer.materials[i], array[i]);
				}
				_copySkinRenderer.materials = array;
			}
			else
			{
				_copySkinRenderer.materials = m_SkinnedRenderer.materials;
			}
			_copySkinRenderer.sharedMesh = m_SkinnedRenderer.sharedMesh;
			_copySkinRenderer.updateWhenOffscreen = m_SkinnedRenderer.updateWhenOffscreen;
			if (m_SkinnedRenderer.enabled)
			{
				_copySkinRenderer.enabled = true;
			}
			return;
		}
		if (_copySkinRenderer.materials.Length != m_SkinnedRenderer.materials.Length || _copySkinRenderer.sharedMesh == null)
		{
			Material[] materials = _copySkinRenderer.materials;
			foreach (Material obj in materials)
			{
				Object.Destroy(obj);
			}
			Material[] array2 = new Material[m_SkinnedRenderer.materials.Length];
			for (int k = 0; k < array2.Length; k++)
			{
				array2[k] = Object.Instantiate(DepthOnlyMat);
				array2[k].hideFlags = HideFlags.DontSave;
				CopyOrgMatToDepthMat(m_SkinnedRenderer.materials[k], array2[k]);
			}
			_copySkinRenderer.materials = array2;
			_copySkinRenderer.sharedMesh = m_SkinnedRenderer.sharedMesh;
		}
		else
		{
			for (int l = 0; l < _copySkinRenderer.materials.Length; l++)
			{
				CopyOrgMatToDepthMat(m_SkinnedRenderer.materials[l], _copySkinRenderer.materials[l]);
			}
		}
		_copySkinRenderer.rootBone = m_SkinnedRenderer.rootBone;
		_copySkinRenderer.bones = m_SkinnedRenderer.bones;
		_copySkinRenderer.localBounds = m_SkinnedRenderer.localBounds;
		_copySkinRenderer.lightmapIndex = m_SkinnedRenderer.lightmapIndex;
		_copySkinRenderer.lightmapScaleOffset = m_SkinnedRenderer.lightmapScaleOffset;
		_copySkinRenderer.updateWhenOffscreen = m_SkinnedRenderer.updateWhenOffscreen;
		if (m_SkinnedRenderer.enabled)
		{
			_copySkinRenderer.enabled = true;
		}
	}

	private void CopyOrgMatToDepthMat(Material org_mat, Material depth_mat)
	{
		if (depth_mat.HasProperty("_MainTex"))
		{
			depth_mat.SetTexture("_MainTex", org_mat.GetTexture("_MainTex"));
		}
		if (depth_mat.HasProperty("_Cutoff"))
		{
			depth_mat.SetFloat("_Cutoff", org_mat.GetFloat("_Cutoff"));
		}
		BlendMode blendMode = (BlendMode)org_mat.GetFloat("_Mode");
		if (blendMode == BlendMode.Cutout)
		{
			depth_mat.EnableKeyword("_ALPHATEST_ON");
		}
		else
		{
			depth_mat.DisableKeyword("_ALPHATEST_ON");
		}
	}

	public static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
	{
		switch (blendMode)
		{
		case BlendMode.Opaque:
			material.SetInt("_SrcBlend", 1);
			material.SetInt("_DstBlend", 0);
			material.SetInt("_ZWrite", 1);
			material.DisableKeyword("_ALPHATEST_ON");
			material.DisableKeyword("_ALPHABLEND_ON");
			material.DisableKeyword("_ALBEDO_ALPHATEST_ON");
			material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
			material.renderQueue = -1;
			break;
		case BlendMode.Cutout:
			material.SetInt("_SrcBlend", 1);
			material.SetInt("_DstBlend", 10);
			material.SetInt("_ZWrite", 1);
			material.EnableKeyword("_ALPHATEST_ON");
			material.DisableKeyword("_ALPHABLEND_ON");
			material.DisableKeyword("_ALBEDO_ALPHATEST_ON");
			material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
			material.renderQueue = 2450;
			break;
		case BlendMode.Fade:
			material.SetInt("_SrcBlend", 5);
			material.SetInt("_DstBlend", 10);
			material.SetInt("_ZWrite", 0);
			material.DisableKeyword("_ALPHATEST_ON");
			material.EnableKeyword("_ALPHABLEND_ON");
			material.EnableKeyword("_ALBEDO_ALPHATEST_ON");
			material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
			material.renderQueue = 3000;
			break;
		case BlendMode.Transparent:
			material.SetInt("_SrcBlend", 1);
			material.SetInt("_DstBlend", 10);
			material.SetInt("_ZWrite", 0);
			material.DisableKeyword("_ALPHATEST_ON");
			material.DisableKeyword("_ALPHABLEND_ON");
			material.EnableKeyword("_ALBEDO_ALPHATEST_ON");
			material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
			material.renderQueue = 3000;
			break;
		}
	}
}
