using System;
using System.IO;
using System.Text;
using UnityEngine;

public class VAMaterial
{
	public const int VERSION = 4096;

	public const int TEX_RESOLUTION = 128;

	public ulong m_Guid;

	public string m_Name = string.Empty;

	public int m_MatterId;

	private int m_ItemId;

	public bool m_UseDefault;

	public float m_BumpStrength;

	public Color32 m_SpecularColor;

	public float m_SpecularStrength;

	public float m_SpecularPower;

	public Color32 m_EmissiveColor;

	public float m_Tile;

	public byte[] m_DiffuseData;

	public byte[] m_BumpData;

	public Texture2D m_DiffuseTex;

	public Texture2D m_BumpTex;

	public RenderTexture m_Icon;

	public static string s_BlankDiffuseRes = "Textures/vc_default_diffuse";

	public static string s_BlankBumpRes = "Textures/vc_default_bump";

	public static string s_BlankBumpRes_ = "Textures/vc_default_bump_";

	public string GUIDString => m_Guid.ToString("X").PadLeft(16, '0');

	public int ItemId
	{
		get
		{
			return m_ItemId;
		}
		set
		{
			m_ItemId = value;
		}
	}

	public void Destroy()
	{
		if (m_DiffuseTex != null)
		{
			UnityEngine.Object.Destroy(m_DiffuseTex);
			m_DiffuseTex = null;
		}
		if (m_BumpTex != null)
		{
			UnityEngine.Object.Destroy(m_BumpTex);
			m_BumpTex = null;
		}
		if (m_Icon != null)
		{
			UnityEngine.Object.Destroy(m_Icon);
			m_Icon = null;
		}
		m_DiffuseData = null;
		m_BumpData = null;
	}

	public void FreeIcon()
	{
		if (m_Icon != null)
		{
			UnityEngine.Object.Destroy(m_Icon);
			m_Icon = null;
		}
	}

	public void Import(byte[] buffer)
	{
		using (MemoryStream memoryStream = new MemoryStream(buffer))
		{
			BinaryReader binaryReader = new BinaryReader(memoryStream);
			int num = binaryReader.ReadInt32();
			int num2 = num;
			if (num2 == 4096)
			{
				m_Name = binaryReader.ReadString();
				m_MatterId = binaryReader.ReadInt32();
				if (VCConfig.s_Matters == null || VCConfig.s_Matters.ContainsKey(m_MatterId))
				{
				}
				m_ItemId = 0;
				m_UseDefault = binaryReader.ReadBoolean();
				m_UseDefault = false;
				if (!m_UseDefault)
				{
					m_BumpStrength = binaryReader.ReadSingle();
					m_SpecularColor.r = binaryReader.ReadByte();
					m_SpecularColor.g = binaryReader.ReadByte();
					m_SpecularColor.b = binaryReader.ReadByte();
					m_SpecularColor.a = binaryReader.ReadByte();
					m_SpecularStrength = binaryReader.ReadSingle();
					m_SpecularPower = binaryReader.ReadSingle();
					m_EmissiveColor.r = binaryReader.ReadByte();
					m_EmissiveColor.g = binaryReader.ReadByte();
					m_EmissiveColor.b = binaryReader.ReadByte();
					m_EmissiveColor.a = binaryReader.ReadByte();
					m_Tile = binaryReader.ReadSingle();
					int num3 = 0;
					num3 = binaryReader.ReadInt32();
					m_DiffuseData = binaryReader.ReadBytes(num3);
					num3 = binaryReader.ReadInt32();
					m_BumpData = binaryReader.ReadBytes(num3);
				}
			}
			binaryReader.Close();
			memoryStream.Close();
		}
		CalcGUID();
	}

	public byte[] Export()
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(4096);
		binaryWriter.Write(m_Name);
		binaryWriter.Write(m_MatterId);
		binaryWriter.Write(m_UseDefault);
		if (!m_UseDefault)
		{
			binaryWriter.Write(m_BumpStrength);
			binaryWriter.Write(m_SpecularColor.r);
			binaryWriter.Write(m_SpecularColor.g);
			binaryWriter.Write(m_SpecularColor.b);
			binaryWriter.Write(m_SpecularColor.a);
			binaryWriter.Write(m_SpecularStrength);
			binaryWriter.Write(m_SpecularPower);
			binaryWriter.Write(m_EmissiveColor.r);
			binaryWriter.Write(m_EmissiveColor.g);
			binaryWriter.Write(m_EmissiveColor.b);
			binaryWriter.Write(m_EmissiveColor.a);
			binaryWriter.Write(m_Tile);
			if (m_DiffuseData != null && m_DiffuseData.Length > 0)
			{
				binaryWriter.Write(m_DiffuseData.Length);
				binaryWriter.Write(m_DiffuseData, 0, m_DiffuseData.Length);
			}
			else
			{
				int value = 0;
				binaryWriter.Write(value);
			}
			if (m_BumpData != null && m_BumpData.Length > 0)
			{
				binaryWriter.Write(m_BumpData.Length);
				binaryWriter.Write(m_BumpData, 0, m_BumpData.Length);
			}
			else
			{
				int value2 = 0;
				binaryWriter.Write(value2);
			}
		}
		binaryWriter.Close();
		byte[] result = memoryStream.ToArray();
		memoryStream.Close();
		return result;
	}

	public ulong CalcGUID()
	{
		m_Guid = (ulong)Mathf.RoundToInt(m_SpecularPower);
		return m_Guid;
	}

	public void LoadCustomizeTexture(string path_d, string path_n)
	{
		FileStream fileStream = null;
		bool flag = true;
		try
		{
			fileStream = new FileStream(path_d, FileMode.Open, FileAccess.Read);
			m_DiffuseData = new byte[(int)fileStream.Length];
			fileStream.Read(m_DiffuseData, 0, (int)fileStream.Length);
			m_DiffuseTex = new Texture2D(2, 2, TextureFormat.ARGB32, mipmap: false);
			if (!m_DiffuseTex.LoadImage(m_DiffuseData))
			{
				throw new Exception("invalid diffuse");
			}
			fileStream.Close();
			fileStream = null;
			flag = true;
		}
		catch (Exception)
		{
			flag = false;
			m_DiffuseTex = UnityEngine.Object.Instantiate(Resources.Load(s_BlankDiffuseRes)) as Texture2D;
		}
		m_DiffuseData = m_DiffuseTex.EncodeToPNG();
		try
		{
			fileStream = new FileStream(path_n, FileMode.Open, FileAccess.Read);
			m_BumpData = new byte[(int)fileStream.Length];
			fileStream.Read(m_BumpData, 0, (int)fileStream.Length);
			m_BumpTex = new Texture2D(2, 2, TextureFormat.ARGB32, mipmap: false);
			if (!m_BumpTex.LoadImage(m_BumpData))
			{
				throw new Exception("invalid normal map");
			}
			fileStream.Close();
			fileStream = null;
		}
		catch (Exception)
		{
			if (flag)
			{
				m_BumpTex = UnityEngine.Object.Instantiate(Resources.Load(s_BlankBumpRes_)) as Texture2D;
			}
			else
			{
				m_BumpTex = UnityEngine.Object.Instantiate(Resources.Load(s_BlankBumpRes)) as Texture2D;
			}
		}
		m_BumpData = m_BumpTex.EncodeToPNG();
		CalcGUID();
	}

	public static ulong CalcMatGroupHash(VAMaterial[] mats)
	{
		string text = string.Empty;
		for (int i = 0; i < mats.Length; i++)
		{
			text = ((mats[i] == null) ? (text + "null") : (text + mats[i].m_Guid));
		}
		return CRC64.Compute(Encoding.UTF8.GetBytes(text));
	}
}
