using System.Collections.Generic;
using Pathea.Maths;
using UnityEngine;

public class RSTileRenderer : MonoBehaviour
{
	private bool m_Started;

	public RSDataSource m_DataSource;

	public INTVECTOR2 m_Pos;

	public Projector m_Projector;

	public Material m_TileMatResource;

	public Material m_TileMat;

	public Texture2D m_RoadGraph;

	public Texture2D m_HeightGraph0;

	public Texture2D m_HeightGraph1;

	public static Color32[] EmptyTexture;

	public static Color32[] EmptyHeightTexture;

	private void Awake()
	{
		CreateTextures();
		ClearTextures();
		ApplyTexturesChange();
		m_Started = false;
	}

	private void Start()
	{
		if (m_DataSource != null)
		{
			m_TileMat = Object.Instantiate(m_TileMatResource);
			m_TileMat.SetVector("_TileOffset", new Vector4((float)m_Pos.x * 256f, 0f, (float)m_Pos.y * 256f, 1f));
			m_TileMat.SetFloat("_TileSize", 256f);
			m_TileMat.SetFloat("_MaxHeight", 1024f);
			m_TileMat.SetTexture("_xzMask", m_RoadGraph);
			m_TileMat.SetTexture("_y0Mask", m_HeightGraph0);
			m_TileMat.SetTexture("_y1Mask", m_HeightGraph1);
			m_Projector.nearClipPlane = 0f;
			m_Projector.farClipPlane = 1028f;
			m_Projector.orthographicSize = 128f;
			m_Projector.material = m_TileMat;
			m_Projector.transform.localPosition = new Vector3(128f, 1024f, 128f);
			RSTile tile = m_DataSource.GetTile(m_Pos);
			if (tile != null)
			{
				tile.OnTileDot = DotTextures;
				tile.OnTileRefresh = RefreshTextures;
				tile.Data = tile.CacheData;
			}
			m_Started = true;
		}
	}

	private void Update()
	{
		if (m_DataSource != null)
		{
			RSTile tile = m_DataSource.GetTile(m_Pos);
			if (tile != null)
			{
				m_Projector.gameObject.SetActive(value: true);
			}
			else
			{
				m_Projector.gameObject.SetActive(value: false);
			}
		}
	}

	public void BeforeDestroy()
	{
		RSTile tile = m_DataSource.GetTile(m_Pos);
		if (tile != null)
		{
			tile.OnTileDot = null;
			tile.OnTileRefresh = null;
			tile.CacheData = tile.Data;
			tile.Clear();
		}
	}

	private void OnDestroy()
	{
		DestroyTextures();
	}

	private void CreateTextures()
	{
		m_RoadGraph = new Texture2D(64, 64, TextureFormat.ARGB32, mipmap: false);
		m_HeightGraph0 = new Texture2D(64, 64, TextureFormat.ARGB32, mipmap: false);
		m_HeightGraph1 = new Texture2D(64, 64, TextureFormat.ARGB32, mipmap: false);
	}

	private void DestroyTextures()
	{
		if (m_RoadGraph != null)
		{
			Object.Destroy(m_RoadGraph);
			m_RoadGraph = null;
		}
		if (m_HeightGraph0 != null)
		{
			Object.Destroy(m_HeightGraph0);
			m_HeightGraph0 = null;
		}
		if (m_HeightGraph1 != null)
		{
			Object.Destroy(m_HeightGraph1);
			m_HeightGraph1 = null;
		}
		if (m_TileMat != null)
		{
			Object.Destroy(m_TileMat);
			m_TileMat = null;
		}
	}

	private void ClearTextures()
	{
		m_RoadGraph.SetPixels32(EmptyTexture);
		m_HeightGraph0.SetPixels32(EmptyHeightTexture);
		m_HeightGraph1.SetPixels32(EmptyHeightTexture);
	}

	private void WriteTileToTextures(RSTile tile)
	{
		foreach (KeyValuePair<int, RoadCell> item in tile.Road)
		{
			WriteRoadCellToTextures(item.Key, item.Value);
		}
	}

	private int WriteRoadCellToTextures(int hash, RoadCell rc)
	{
		INTVECTOR3 iNTVECTOR = default(INTVECTOR3);
		iNTVECTOR.hash = hash;
		byte b = (byte)(iNTVECTOR.y + 1);
		if (rc.type > 0)
		{
			m_RoadGraph.SetPixel(iNTVECTOR.x, iNTVECTOR.z, rc.color_type);
			Color32 color = new Color32(0, 0, 0, 0);
			color = SafeColorConvert(m_HeightGraph0.GetPixel(iNTVECTOR.x, iNTVECTOR.z));
			if (color.r == b || color.g == b || color.b == b)
			{
				return -1;
			}
			if (color.r == 0)
			{
				color.r = b;
				m_HeightGraph0.SetPixel(iNTVECTOR.x, iNTVECTOR.z, color);
				return 0;
			}
			if (color.g == 0)
			{
				color.g = b;
				m_HeightGraph0.SetPixel(iNTVECTOR.x, iNTVECTOR.z, color);
				return 0;
			}
			if (color.b == 0)
			{
				color.b = b;
				m_HeightGraph0.SetPixel(iNTVECTOR.x, iNTVECTOR.z, color);
				return 0;
			}
			color = SafeColorConvert(m_HeightGraph1.GetPixel(iNTVECTOR.x, iNTVECTOR.z));
			if (color.r == b || color.g == b || color.b == b)
			{
				return -1;
			}
			if (color.r == 0)
			{
				color.r = b;
				m_HeightGraph1.SetPixel(iNTVECTOR.x, iNTVECTOR.z, color);
				return 1;
			}
			if (color.g == 0)
			{
				color.g = b;
				m_HeightGraph1.SetPixel(iNTVECTOR.x, iNTVECTOR.z, color);
				return 1;
			}
			if (color.b == 0)
			{
				color.b = b;
				m_HeightGraph1.SetPixel(iNTVECTOR.x, iNTVECTOR.z, color);
				return 1;
			}
			return -1;
		}
		m_RoadGraph.SetPixel(iNTVECTOR.x, iNTVECTOR.z, new Color(0f, 0f, 0f, 0f));
		Color32 color2 = new Color32(0, 0, 0, 0);
		color2 = SafeColorConvert(m_HeightGraph0.GetPixel(iNTVECTOR.x, iNTVECTOR.z));
		if (color2.r == b)
		{
			color2.r = 0;
			m_HeightGraph0.SetPixel(iNTVECTOR.x, iNTVECTOR.z, color2);
			return 0;
		}
		if (color2.g == b)
		{
			color2.g = 0;
			m_HeightGraph0.SetPixel(iNTVECTOR.x, iNTVECTOR.z, color2);
			return 0;
		}
		if (color2.b == b)
		{
			color2.b = 0;
			m_HeightGraph0.SetPixel(iNTVECTOR.x, iNTVECTOR.z, color2);
			return 0;
		}
		color2 = SafeColorConvert(m_HeightGraph1.GetPixel(iNTVECTOR.x, iNTVECTOR.z));
		if (color2.r == b)
		{
			color2.r = 0;
			m_HeightGraph1.SetPixel(iNTVECTOR.x, iNTVECTOR.z, color2);
			return 1;
		}
		if (color2.g == b)
		{
			color2.g = 0;
			m_HeightGraph1.SetPixel(iNTVECTOR.x, iNTVECTOR.z, color2);
			return 1;
		}
		if (color2.b == b)
		{
			color2.b = 0;
			m_HeightGraph1.SetPixel(iNTVECTOR.x, iNTVECTOR.z, color2);
			return 1;
		}
		return -1;
	}

	private void ApplyTexturesChange(int mask = 1)
	{
		m_RoadGraph.Apply();
		if (mask >= 0)
		{
			m_HeightGraph0.Apply();
		}
		if (mask >= 1)
		{
			m_HeightGraph1.Apply();
		}
	}

	public void RefreshTextures(RSTile tile)
	{
		ClearTextures();
		WriteTileToTextures(tile);
		ApplyTexturesChange();
	}

	public void DotTextures(int hash, RoadCell rc)
	{
		if (m_Started)
		{
			int mask = WriteRoadCellToTextures(hash, rc);
			ApplyTexturesChange(mask);
		}
	}

	public static Color32 SafeColorConvert(Color c)
	{
		return new Color32((byte)(Mathf.Clamp01(c.r) * 255f + 0.01f), (byte)(Mathf.Clamp01(c.g) * 255f + 0.01f), (byte)(Mathf.Clamp01(c.b) * 255f + 0.01f), (byte)(Mathf.Clamp01(c.a) * 255f + 0.01f));
	}
}
