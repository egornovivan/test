using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RedGrass;

public class RGPrototypeMgr : MonoBehaviour
{
	public const int s_PrototypeRowCount = 8;

	public const int s_PrototypeColCount = 8;

	public const int s_MapResolution = 256;

	public const int s_PrototypeCount = 64;

	[SerializeField]
	public List<RGPrototype> m_Prototypes;

	[SerializeField]
	public Texture2D m_DiffuseMap;

	[SerializeField]
	public Texture2D m_ParticleMap;

	[SerializeField]
	public Texture2D m_PropertyMap;

	[SerializeField]
	public string m_DiffuseMapFileName = "Diffuse2048";

	[SerializeField]
	public string m_ParticleMapFileName = "Particle2048";

	[SerializeField]
	public string m_PropertyMapFileName = "Property64x4";

	public void GenerateTextures()
	{
		for (int i = 0; i < 64; i++)
		{
			if (m_Prototypes[i].m_Diffuse != null)
			{
				m_DiffuseMap.SetPixels(i % 8 * 256, i / 8 * 256, 256, 256, m_Prototypes[i].m_Diffuse.GetPixels(0, 0, 256, 256));
			}
			else
			{
				m_DiffuseMap.SetPixels(i % 8 * 256, i / 8 * 256, 256, 256, new Color[65536]);
			}
		}
		for (int j = 0; j < 64; j++)
		{
			m_PropertyMap.SetPixel(j, 0, new Color(m_Prototypes[j].m_MinSize.x * 0.5f, m_Prototypes[j].m_MinSize.y * 0.5f, m_Prototypes[j].m_MaxSize.x * 0.5f, m_Prototypes[j].m_MaxSize.y * 0.5f));
			m_PropertyMap.SetPixel(j, 1, new Color(m_Prototypes[j].m_BendFactor * 2f, (m_Prototypes[j].m_LODBias + 4f) / 8f, 0f, 1f));
			m_PropertyMap.SetPixel(j, 2, m_Prototypes[j].m_ParticleTintColor);
			m_PropertyMap.SetPixel(j, 3, new Color(0f, 0f, 0f, 1f));
		}
		m_DiffuseMap.Apply();
		byte[] array = null;
		array = m_DiffuseMap.EncodeToPNG();
		File.WriteAllBytes("Assets/RedGrass/Textures/" + m_DiffuseMapFileName + ".png", array);
		array = m_PropertyMap.EncodeToPNG();
		File.WriteAllBytes("Assets/RedGrass/Textures/" + m_PropertyMapFileName + ".png", array);
	}
}
