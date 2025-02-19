using System;
using System.IO;
using UnityEngine;

namespace RedGrass;

public struct RedGrassInstance
{
	private const int c_RandTableSize = 32;

	private const int c_RandTableMask = 31;

	private int m_Position_x;

	private float m_Position_y;

	private int m_Position_z;

	private float m_Density;

	private float m_Normal_x;

	private float m_Normal_z;

	private byte m_Color_r;

	private byte m_Color_g;

	private byte m_Color_b;

	private byte m_Prototype;

	private static System.Random s_Rand;

	private static Vector2[,,] s_RandTable;

	public Vector3 Position
	{
		get
		{
			return new Vector3(m_Position_x, m_Position_y, m_Position_z);
		}
		set
		{
			m_Position_x = Mathf.FloorToInt(value.x);
			m_Position_y = value.y;
			m_Position_z = Mathf.FloorToInt(value.z);
		}
	}

	public float Density
	{
		get
		{
			return m_Density;
		}
		set
		{
			m_Density = value;
		}
	}

	public Vector2 RandAttr => s_RandTable[m_Position_x & 0x1F, m_Position_z & 0x1F, 0];

	public bool IsParticle => s_RandTable[m_Position_x & 0x1F, (m_Position_x + m_Position_z >> 5) & 0x1F, m_Position_z & 0x1F].x < 0.01f;

	public Vector3 Normal
	{
		get
		{
			return new Vector3(m_Normal_x, Mathf.Sqrt(1f - (m_Normal_x * m_Normal_x + m_Normal_z * m_Normal_z)), m_Normal_z);
		}
		set
		{
			value.Normalize();
			m_Normal_x = value.x;
			m_Normal_z = value.z;
		}
	}

	public int Prototype
	{
		get
		{
			return m_Prototype;
		}
		set
		{
			m_Prototype = (byte)value;
		}
	}

	public int Layer => (m_Prototype & 0x40) >> 6;

	public Color ColorF
	{
		get
		{
			return new Color32(m_Color_r, m_Color_g, m_Color_b, byte.MaxValue);
		}
		set
		{
			Color32 color = value;
			m_Color_r = color.r;
			m_Color_g = color.g;
			m_Color_b = color.b;
		}
	}

	public Color32 ColorDw
	{
		get
		{
			return new Color32(m_Color_r, m_Color_g, m_Color_b, byte.MaxValue);
		}
		set
		{
			m_Color_r = value.r;
			m_Color_g = value.g;
			m_Color_b = value.b;
		}
	}

	public void CopyTo(VoxelGrassInstance rgi)
	{
		m_Position_x = rgi.m_Position_x;
		m_Position_y = rgi.m_Position_y;
		m_Position_z = rgi.m_Position_z;
		m_Density = rgi.m_Density;
		m_Normal_x = rgi.m_Normal_x;
		m_Normal_z = rgi.m_Normal_z;
		m_Color_r = rgi.m_Color_r;
		m_Color_g = rgi.m_Color_g;
		m_Color_b = rgi.m_Color_b;
		m_Prototype = rgi.m_Prototype;
	}

	public static void Init()
	{
		s_Rand = new System.Random(1000);
		s_RandTable = new Vector2[32, 32, 32];
		for (int i = 0; i < 32; i++)
		{
			for (int j = 0; j < 32; j++)
			{
				for (int k = 0; k < 32; k++)
				{
					s_RandTable[i, j, k] = new Vector2((float)s_Rand.NextDouble(), (float)s_Rand.NextDouble());
				}
			}
		}
	}

	public Vector2 RandAttrs(int i)
	{
		return s_RandTable[m_Position_x & 0x1F, m_Position_z & 0x1F, i & 0x1F];
	}

	public Vector3 RandPos(int i)
	{
		Vector2 vector = s_RandTable[m_Position_x & 0x1F, m_Position_z & 0x1F, i & 0x1F];
		float num = vector.x * m_Normal_x + vector.y * m_Normal_z;
		return new Vector3((float)m_Position_x + vector.x, m_Position_y - num * 1.1f, (float)m_Position_z + vector.y);
	}

	public void WriteToStream(BinaryWriter w)
	{
		w.Write(m_Position_x);
		w.Write(m_Position_y);
		w.Write(m_Position_z);
		w.Write(m_Density);
		w.Write(m_Normal_x);
		w.Write(m_Normal_z);
		w.Write(m_Color_r);
		w.Write(m_Color_g);
		w.Write(m_Color_b);
		w.Write(m_Prototype);
	}

	public void ReadFromStream(BinaryReader r)
	{
		m_Position_x = r.ReadInt32();
		m_Position_y = r.ReadSingle();
		m_Position_z = r.ReadInt32();
		m_Density = r.ReadSingle();
		m_Normal_x = r.ReadSingle();
		m_Normal_z = r.ReadSingle();
		m_Color_r = r.ReadByte();
		m_Color_g = r.ReadByte();
		m_Color_b = r.ReadByte();
		m_Prototype = r.ReadByte();
	}
}
