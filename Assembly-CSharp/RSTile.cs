using System.Collections.Generic;
using System.IO;
using Pathea.Maths;
using UnityEngine;

public class RSTile
{
	public delegate void DTileNotify(RSTile tile);

	public delegate void DRefreshNotify(RSTile tile);

	public delegate void DDotNotify(int hash, RoadCell rc);

	public const int TILE_SIZE_SHIFT = 8;

	public const int CELL_SIZE_SHIFT = 2;

	public const int CELL_YSIZE_SHIFT = 2;

	public const int MAX_HEIGHT_SHIFT = 10;

	public const int TILE_SIZE = 256;

	public const int CELL_SIZE = 4;

	public const int CELL_YSIZE = 4;

	public const int MAX_HEIGHT = 1024;

	public const float CELL_SIZEF = 4f;

	public const float CELL_YSIZEF = 4f;

	public const float TILE_SIZEF = 256f;

	public const float MAX_HEIGHTF = 1024f;

	public const int CELL_AXIS_COUNT = 64;

	public const int CELL_YAXIS_COUNT = 256;

	private INTVECTOR2 m_Pos;

	private Dictionary<int, RoadCell> m_Road;

	private bool m_Register;

	public DRefreshNotify OnTileRefresh;

	public DDotNotify OnTileDot;

	public byte[] CacheData;

	public INTVECTOR2 Pos => m_Pos;

	public int Hash => m_Pos.hash;

	public Dictionary<int, RoadCell> Road => m_Road;

	public byte[] Data
	{
		get
		{
			int value = 1;
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(value);
			binaryWriter.Write(m_Road.Count);
			foreach (KeyValuePair<int, RoadCell> item in m_Road)
			{
				binaryWriter.Write(item.Key);
				item.Value.WriteToStream(binaryWriter);
			}
			binaryWriter.Close();
			return memoryStream.ToArray();
		}
		set
		{
			m_Road.Clear();
			if (value == null || value.Length < 8)
			{
				return;
			}
			MemoryStream memoryStream = new MemoryStream(value);
			BinaryReader binaryReader = new BinaryReader(memoryStream);
			int num = binaryReader.ReadInt32();
			int num2 = num;
			if (num2 == 1)
			{
				int num3 = binaryReader.ReadInt32();
				for (int i = 0; i < num3; i++)
				{
					int key = binaryReader.ReadInt32();
					RoadCell value2 = default(RoadCell);
					value2.ReadFromStream(binaryReader);
					m_Road.Add(key, value2);
				}
			}
			binaryReader.Close();
			memoryStream.Close();
			if (OnTileRefresh != null)
			{
				OnTileRefresh(this);
			}
		}
	}

	public static event DTileNotify OnTileCreate;

	public static event DTileNotify OnTileDestroy;

	public RSTile(int hash, bool register)
	{
		m_Pos.hash = hash;
		m_Road = new Dictionary<int, RoadCell>();
		m_Register = register;
		if (m_Register && RSTile.OnTileCreate != null)
		{
			RSTile.OnTileCreate(this);
		}
	}

	public static INTVECTOR2 QueryTile(Vector3 world_pos)
	{
		return new INTVECTOR2(world_pos.x / 256f, world_pos.z / 256f);
	}

	public Vector3 WorldPosToLocalPos(Vector3 wpos)
	{
		wpos.x -= (float)m_Pos.x * 256f;
		wpos.z -= (float)m_Pos.y * 256f;
		return wpos;
	}

	public INTVECTOR3 WorldPosToRoadCellPos(Vector3 wpos)
	{
		Vector3 vector = WorldPosToLocalPos(wpos);
		return new INTVECTOR3(vector.x / 4f, vector.y / 4f, vector.z / 4f);
	}

	public static INTVECTOR3 WorldPosToRoadCellPos_s(int hash, Vector3 wpos)
	{
		INTVECTOR2 iNTVECTOR = default(INTVECTOR2);
		iNTVECTOR.hash = hash;
		wpos.x -= (float)iNTVECTOR.x * 256f;
		wpos.z -= (float)iNTVECTOR.y * 256f;
		return new INTVECTOR3(wpos.x / 4f, wpos.y / 4f, wpos.z / 4f);
	}

	public bool CheckRoadCellPos(INTVECTOR3 rcpos)
	{
		if (rcpos.x < 0)
		{
			return false;
		}
		if (rcpos.y < 0)
		{
			return false;
		}
		if (rcpos.z < 0)
		{
			return false;
		}
		if (rcpos.x >= 64)
		{
			return false;
		}
		if (rcpos.y >= 256)
		{
			return false;
		}
		if (rcpos.z >= 64)
		{
			return false;
		}
		return true;
	}

	public RoadCell GetRoadCell(int hash)
	{
		if (m_Road.ContainsKey(hash))
		{
			return m_Road[hash];
		}
		return new RoadCell(0);
	}

	public RoadCell GetRoadCell(Vector3 world_pos)
	{
		int hash = WorldPosToRoadCellPos(world_pos).hash;
		if (m_Road.ContainsKey(hash))
		{
			return m_Road[hash];
		}
		return new RoadCell(0);
	}

	public RoadCell GetRoadCell(INTVECTOR3 rcpos)
	{
		int hash = rcpos.hash;
		if (m_Road.ContainsKey(hash))
		{
			return m_Road[hash];
		}
		return new RoadCell(0);
	}

	public void SetRoadCell(int hash, RoadCell rc)
	{
		bool flag = false;
		if (rc.type > 0)
		{
			if (m_Road.ContainsKey(hash))
			{
				RoadCell roadCell = m_Road[hash];
				if (!rc.Equals(roadCell))
				{
					m_Road[hash] = rc;
					flag = true;
				}
			}
			else
			{
				m_Road.Add(hash, rc);
				flag = true;
			}
		}
		else if (m_Road.ContainsKey(hash))
		{
			m_Road.Remove(hash);
			flag = true;
		}
		if (flag && OnTileDot != null)
		{
			OnTileDot(hash, rc);
		}
	}

	public void SetRoadCell(Vector3 world_pos, RoadCell rc)
	{
		int hash = WorldPosToRoadCellPos(world_pos).hash;
		SetRoadCell(hash, rc);
	}

	public void SetRoadCell(INTVECTOR3 rcpos, RoadCell rc)
	{
		int hash = rcpos.hash;
		SetRoadCell(hash, rc);
	}

	public void Clear()
	{
		m_Road.Clear();
		if (OnTileRefresh != null)
		{
			OnTileRefresh(this);
		}
	}

	public void Destroy()
	{
		Clear();
		if (m_Register && RSTile.OnTileDestroy != null)
		{
			RSTile.OnTileDestroy(this);
		}
	}
}
