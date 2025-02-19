using System.IO;
using UnityEngine;

public class VCDecalAsset
{
	public const int MAX_TEX_RESOLUTION = 512;

	public ulong m_Guid;

	public byte[] m_TexData;

	public Texture2D m_Tex;

	private static string s_DefaultTexPath = "Decals/Unknown";

	public string GUIDString => m_Guid.ToString("X").PadLeft(16, '0');

	public void Destroy()
	{
		if (m_Tex != null)
		{
			Object.Destroy(m_Tex);
			m_Tex = null;
		}
		m_TexData = null;
	}

	public void Import(byte[] buffer)
	{
		Destroy();
		m_Guid = CRC64.Compute(buffer);
		using (MemoryStream memoryStream = new MemoryStream(buffer))
		{
			m_TexData = new byte[(int)memoryStream.Length];
			memoryStream.Read(m_TexData, 0, (int)memoryStream.Length);
		}
		m_Tex = new Texture2D(4, 4);
		if (!m_Tex.LoadImage(m_TexData))
		{
			Destroy();
			Texture2D texture2D = Resources.Load(s_DefaultTexPath) as Texture2D;
			m_TexData = texture2D.EncodeToPNG();
			m_Tex = new Texture2D(4, 4);
			if (!m_Tex.LoadImage(m_TexData))
			{
				Debug.LogError("Can't find default decal texture!");
				Destroy();
				m_Tex = new Texture2D(16, 16, TextureFormat.ARGB32, mipmap: false);
				m_TexData = m_Tex.EncodeToPNG();
			}
		}
		m_Tex.filterMode = FilterMode.Trilinear;
		m_Tex.wrapMode = TextureWrapMode.Clamp;
	}

	public byte[] Export()
	{
		if (m_TexData != null)
		{
			return m_TexData;
		}
		return new byte[0];
	}

	public void CalcGUID()
	{
		m_Guid = CRC64.Compute(Export());
	}
}
