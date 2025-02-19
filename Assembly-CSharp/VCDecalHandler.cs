using UnityEngine;

public class VCDecalHandler : MonoBehaviour, ISerializationCallbackReceiver
{
	public Projector m_Projector;

	public VCEComponentTool m_Tool;

	public ulong m_Guid;

	public VCIsoData m_Iso;

	public int m_AssetIndex = -1;

	public float m_Depth = 0.01f;

	public float m_Size = 0.1f;

	public bool m_Mirrored;

	public int m_ShaderIndex;

	public Color m_Color = Color.white;

	public Material[] m_DecalMats;

	private static VCIsoData _isoData;

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		_isoData = m_Iso;
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		m_Iso = _isoData;
	}

	private void Start()
	{
		for (int i = 0; i < m_DecalMats.Length; i++)
		{
			if (m_DecalMats[i] != null)
			{
				Material material = Object.Instantiate(m_DecalMats[i]);
				m_DecalMats[i] = material;
			}
		}
		m_Projector.material = m_DecalMats[0];
	}

	private void LateUpdate()
	{
		VCDecalAsset vCDecalAsset = VCEAssetMgr.GetDecal(m_Guid);
		if (vCDecalAsset == null && m_Iso != null && m_Iso.m_DecalAssets != null && m_AssetIndex >= 0 && m_AssetIndex < 4)
		{
			vCDecalAsset = m_Iso.m_DecalAssets[m_AssetIndex];
		}
		if (vCDecalAsset == null)
		{
			m_Projector.gameObject.SetActive(value: false);
			return;
		}
		if (VCEditor.DocumentOpen() && m_Tool != null && m_Tool.m_SelBound != null)
		{
			m_Tool.m_SelBound.transform.localScale = new Vector3(m_Size, m_Size, m_Depth * 2f - 0.002f);
			m_Tool.m_SelBound.transform.localPosition = new Vector3(0f, 0f, 0f);
		}
		Material material = null;
		if (m_ShaderIndex >= 0 && m_ShaderIndex < m_DecalMats.Length)
		{
			material = m_DecalMats[m_ShaderIndex];
		}
		m_Projector.gameObject.SetActive(material != null);
		if (material != null)
		{
			m_Projector.material = material;
			m_Projector.nearClipPlane = 0.001f - m_Depth;
			m_Projector.farClipPlane = m_Depth - 0.001f;
			m_Projector.orthographicSize = m_Size * 0.5f;
			material.SetTexture("_Texture", vCDecalAsset.m_Tex);
			material.SetColor("_TintColor", m_Color);
			material.SetFloat("_Size", m_Size * 0.5f);
			material.SetFloat("_Depth", m_Depth);
			material.SetVector("_Center", new Vector4(base.transform.position.x, base.transform.position.y, base.transform.position.z, 1f));
			material.SetVector("_Forward", new Vector4(base.transform.forward.x, base.transform.forward.y, base.transform.forward.z, 0f));
			if (m_Mirrored)
			{
				material.SetVector("_Right", -new Vector4(base.transform.right.x, base.transform.right.y, base.transform.right.z, 0f));
			}
			else
			{
				material.SetVector("_Right", new Vector4(base.transform.right.x, base.transform.right.y, base.transform.right.z, 0f));
			}
			material.SetVector("_Up", new Vector4(base.transform.up.x, base.transform.up.y, base.transform.up.z, 0f));
		}
	}
}
