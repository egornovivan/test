using System.Collections.Generic;
using UnityEngine;

public class VCMatManager : MonoBehaviour
{
	private static VCMatManager s_Instance;

	public Dictionary<ulong, Material> m_mapMaterials;

	public Dictionary<ulong, RenderTexture> m_mapDiffuseTexs;

	public Dictionary<ulong, RenderTexture> m_mapBumpTexs;

	public Dictionary<ulong, Texture2D> m_mapPropertyTexs;

	public Dictionary<ulong, int> m_mapMatRefCounters;

	public static VCMatManager Instance => s_Instance;

	private void Awake()
	{
		s_Instance = this;
	}

	private void Start()
	{
		m_mapMaterials = new Dictionary<ulong, Material>();
		m_mapDiffuseTexs = new Dictionary<ulong, RenderTexture>();
		m_mapBumpTexs = new Dictionary<ulong, RenderTexture>();
		m_mapPropertyTexs = new Dictionary<ulong, Texture2D>();
		m_mapMatRefCounters = new Dictionary<ulong, int>();
	}

	private void Update()
	{
	}

	private void OnDestroy()
	{
		s_Instance = null;
	}
}
