using System.Collections.Generic;
using Pathea.Maths;
using UnityEngine;

public class RSDataSource
{
	private Dictionary<int, RSTile> m_Tiles;

	public Dictionary<int, RSTile> Tiles => m_Tiles;

	public RSDataSource()
	{
		m_Tiles = new Dictionary<int, RSTile>();
	}

	~RSDataSource()
	{
		Destroy();
	}

	public void Destroy()
	{
		foreach (KeyValuePair<int, RSTile> tile in m_Tiles)
		{
			if (tile.Value != null)
			{
				tile.Value.Destroy();
				tile.Value.CacheData = null;
			}
		}
		m_Tiles.Clear();
	}

	public RSTile GetTile(INTVECTOR2 pos)
	{
		int hash = pos.hash;
		if (m_Tiles.ContainsKey(hash))
		{
			return m_Tiles[hash];
		}
		return null;
	}

	public void AlterRoadCell(int tile_hash, int cell_hash, RoadCell rc)
	{
		if (rc.type > 0)
		{
			if (!m_Tiles.ContainsKey(tile_hash))
			{
				m_Tiles.Add(tile_hash, new RSTile(tile_hash, register: true));
			}
			m_Tiles[tile_hash].SetRoadCell(cell_hash, rc);
		}
		else if (m_Tiles.ContainsKey(tile_hash))
		{
			RSTile rSTile = m_Tiles[tile_hash];
			rSTile.SetRoadCell(cell_hash, rc);
			if (rSTile.Road.Count == 0)
			{
				rSTile.Destroy();
				m_Tiles.Remove(tile_hash);
			}
		}
	}

	public void AlterRoadCell(Vector3 world_pos, RoadCell rc)
	{
		int hash = RSTile.QueryTile(world_pos).hash;
		int hash2 = RSTile.WorldPosToRoadCellPos_s(hash, world_pos).hash;
		AlterRoadCell(hash, hash2, rc);
	}

	public RoadCell GetRoadCell(int tile_hash, int cell_hash)
	{
		if (m_Tiles.ContainsKey(tile_hash))
		{
			return m_Tiles[tile_hash].GetRoadCell(cell_hash);
		}
		return new RoadCell(0);
	}

	public RoadCell GetRoadCell(Vector3 world_pos)
	{
		int hash = RSTile.QueryTile(world_pos).hash;
		if (m_Tiles.ContainsKey(hash))
		{
			return m_Tiles[hash].GetRoadCell(world_pos);
		}
		return new RoadCell(0);
	}
}
