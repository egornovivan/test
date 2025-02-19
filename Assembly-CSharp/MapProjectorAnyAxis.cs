using UnityEngine;

[RequireComponent(typeof(Projector))]
public class MapProjectorAnyAxis : MonoBehaviour
{
	public float Size = 50f;

	public int ColorIndex;

	public float Brightness = 1f;

	public float AlphaCoef = 1f;

	public Texture2D MapTex;

	public float Depth = 100f;

	[SerializeField]
	private Color[] ColorPreset;

	private Projector m_Projector;

	private Transform m_Trans;

	private Material m_Material;

	public Projector projector => m_Projector;

	public float NearClip
	{
		get
		{
			return m_Projector.nearClipPlane;
		}
		set
		{
			m_Projector.nearClipPlane = value;
		}
	}

	private void Awake()
	{
		m_Projector = base.gameObject.GetComponent<Projector>();
		m_Material = Object.Instantiate(projector.material);
		projector.material = m_Material;
		m_Trans = base.transform;
	}

	private void OnDestroy()
	{
		if (m_Material != null)
		{
			Object.Destroy(m_Material);
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (!(m_Material == null))
		{
			m_Projector.orthographicSize = Size;
			m_Projector.orthographic = true;
			Vector3 position = m_Trans.position;
			Vector3 forward = m_Trans.forward;
			m_Material.SetTexture("_MainTex", MapTex);
			m_Material.SetVector("_CenterAndSize", new Vector4(position.x, position.y, position.z, Size));
			m_Material.SetVector("_Direction", new Vector4(forward.x, forward.y, forward.z));
			m_Material.SetColor("_TintColor", ColorPreset[ColorIndex % ColorPreset.Length]);
			m_Material.SetFloat("_Brightness", Brightness);
			m_Material.SetFloat("_AlphaCoef", AlphaCoef);
			m_Material.SetFloat("_Depth", Depth);
		}
	}
}
