using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class TrigrassGenTest : MonoBehaviour
{
	public int m_GrassCount = 4000;

	public Color m_GrassColor = Color.white;

	public int m_ProtoType0 = 14;

	public int m_ProtoType1 = 13;

	public int m_ProtoType2 = 15;

	public int m_RandSeed;

	private MeshFilter m_Mesh;

	public Vector3 m_StartCoord = Vector3.zero;

	public float m_GenAreaSize = 128f;

	private List<GrassInstance> m_Grasses = new List<GrassInstance>();

	public bool m_RegenerateNow;

	private void ReGen()
	{
		m_Grasses.Clear();
		SimplexNoise simplexNoise = new SimplexNoise();
		for (int i = 0; i < m_GrassCount; i++)
		{
			Vector3 origin = new Vector3(Random.value, 1f, Random.value) * m_GenAreaSize + m_StartCoord;
			origin.y = 512f;
			if (Physics.Raycast(origin, Vector3.down, out var hitInfo, 1024f, 4096))
			{
				float f = (float)simplexNoise.Noise(hitInfo.point.x / 32f, hitInfo.point.y / 32f, hitInfo.point.z / 32f) + 1f;
				float f2 = (float)simplexNoise.Noise((hitInfo.point.x + m_GenAreaSize) / 32f, (hitInfo.point.y + m_GenAreaSize) / 32f, (hitInfo.point.z + m_GenAreaSize) / 32f) + 1f;
				float f3 = (float)simplexNoise.Noise(hitInfo.point.y / 16f, hitInfo.point.z / 16f, hitInfo.point.x / 16f) + 1f;
				f = Mathf.Pow(f, 15f);
				f2 = Mathf.Pow(f2, 15f);
				f3 = Mathf.Pow(f3, 15f);
				float num = f + f2 + f3;
				f /= num;
				f2 /= num;
				f2 += f;
				f3 /= num;
				f3 += f2;
				GrassInstance grassInstance = new GrassInstance();
				grassInstance.Position = hitInfo.point;
				grassInstance.Normal = hitInfo.normal;
				grassInstance.ColorF = m_GrassColor;
				float value = Random.value;
				if (value < f)
				{
					grassInstance.Prototype = m_ProtoType0;
				}
				else if (value < f2)
				{
					grassInstance.Prototype = m_ProtoType1;
				}
				else if (value < f3)
				{
					grassInstance.Prototype = m_ProtoType2;
				}
				m_Grasses.Add(grassInstance);
			}
		}
		TrigrassMeshComputer.ComputeMesh(m_Grasses, 0, m_Mesh);
		m_Grasses.Clear();
	}

	private void OnEnable()
	{
		ReGen();
	}

	private void Start()
	{
		m_Mesh = GetComponent<MeshFilter>();
	}

	private void Update()
	{
		if (m_RegenerateNow)
		{
			ReGen();
			m_RegenerateNow = false;
		}
	}
}
