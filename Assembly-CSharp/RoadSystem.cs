using System.Collections.Generic;
using Pathea.Maths;
using UnityEngine;

public class RoadSystem : MonoBehaviour
{
	private static RoadSystem s_Instance;

	private RSDataSource m_DataSource;

	public int m_TileExtend = 1;

	private Dictionary<int, RSTileRenderer> m_Renderers;

	public RSTileRenderer m_RendererResource;

	public Transform m_RendererGroup;

	public INTVECTOR2 m_Center = INTVECTOR2.zero;

	public static RoadSystem Instance => s_Instance;

	public static RSDataSource DataSource => (!(s_Instance != null)) ? null : s_Instance.m_DataSource;

	private void UpdateCenter()
	{
	}

	private void Awake()
	{
		s_Instance = this;
		m_DataSource = new RSDataSource();
		m_Renderers = new Dictionary<int, RSTileRenderer>();
		int num = 4096;
		RSTileRenderer.EmptyTexture = new Color32[num];
		RSTileRenderer.EmptyHeightTexture = new Color32[num];
		for (int i = 0; i < num; i++)
		{
			ref Color32 reference = ref RSTileRenderer.EmptyTexture[i];
			reference = new Color32(0, 0, 0, 0);
			ref Color32 reference2 = ref RSTileRenderer.EmptyHeightTexture[i];
			reference2 = new Color32(0, 0, 0, byte.MaxValue);
		}
		RSTile.OnTileCreate += OnTileCreate;
		RSTile.OnTileDestroy += OnTileDestroy;
	}

	private void OnDestroy()
	{
		s_Instance = null;
		m_DataSource.Destroy();
		RSTile.OnTileCreate -= OnTileCreate;
		RSTile.OnTileDestroy -= OnTileDestroy;
	}

	private void Update()
	{
		UpdateCenter();
		UpdateRenderers();
	}

	private void UpdateRenderers()
	{
	}

	private void AddRenderer(INTVECTOR2 pos)
	{
		int hash = pos.hash;
		if (!m_Renderers.ContainsKey(hash))
		{
			RSTileRenderer rSTileRenderer = Object.Instantiate(m_RendererResource);
			rSTileRenderer.gameObject.name = "Road Tile " + pos.ToString();
			rSTileRenderer.transform.parent = m_RendererGroup;
			rSTileRenderer.transform.position = new Vector3(pos.x * 256, 0f, pos.y * 256);
			rSTileRenderer.transform.rotation = Quaternion.identity;
			rSTileRenderer.transform.localScale = Vector3.one;
			rSTileRenderer.m_DataSource = m_DataSource;
			rSTileRenderer.m_Pos = pos;
			m_Renderers.Add(hash, rSTileRenderer);
		}
	}

	private void RemoveFarRenderers()
	{
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, RSTileRenderer> renderer in m_Renderers)
		{
			if (Mathf.Abs(renderer.Value.m_Pos.x - m_Center.x) > m_TileExtend || Mathf.Abs(renderer.Value.m_Pos.y - m_Center.y) > m_TileExtend)
			{
				list.Add(renderer.Key);
			}
		}
		foreach (int item in list)
		{
			m_Renderers[item].BeforeDestroy();
			Object.Destroy(m_Renderers[item].gameObject);
			m_Renderers.Remove(item);
		}
	}

	public void RefreshRenderers()
	{
		RemoveFarRenderers();
		for (int i = m_Center.x - m_TileExtend; i <= m_Center.x + m_TileExtend; i++)
		{
			for (int j = m_Center.y - m_TileExtend; j <= m_Center.y + m_TileExtend; j++)
			{
				AddRenderer(new INTVECTOR2(i, j));
			}
		}
	}

	private void OnTileCreate(RSTile tile)
	{
		int hash = tile.Hash;
		if (m_Renderers.ContainsKey(hash))
		{
			tile.OnTileRefresh = m_Renderers[hash].RefreshTextures;
			tile.OnTileDot = m_Renderers[hash].DotTextures;
		}
		else
		{
			tile.OnTileRefresh = null;
			tile.OnTileDot = null;
		}
	}

	private void OnTileDestroy(RSTile tile)
	{
		tile.OnTileRefresh = null;
		tile.OnTileDot = null;
	}
}
