using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class VoxelGrassGenTest : MonoBehaviour
{
	public Color m_GrassColor = Color.white;

	public int m_ProtoType;

	public int m_RandSeed;

	private MeshFilter m_Mesh;

	public Vector3 m_StartCoord = Vector3.zero;

	public float m_GenAreaSize = 32f;

	public float m_Density = 1f;

	private List<VoxelGrassInstance> m_Grasses = new List<VoxelGrassInstance>();

	public bool m_RegenerateNow;

	private void ReGen()
	{
		VoxelGrassMeshComputer.Init();
		m_Grasses.Clear();
		for (float num = m_StartCoord.x + 0.5f; num < m_StartCoord.x + m_GenAreaSize; num += 1f)
		{
			for (float num2 = m_StartCoord.z + 0.5f; num2 < m_StartCoord.z + m_GenAreaSize; num2 += 1f)
			{
				Vector3 origin = new Vector3(num, 512f, num2);
				if (Physics.Raycast(origin, Vector3.down, out var hitInfo, 1024f, 4096))
				{
					VoxelGrassInstance item = default(VoxelGrassInstance);
					item.Position = hitInfo.point;
					item.Density = 1f;
					item.Normal = hitInfo.normal;
					item.ColorF = m_GrassColor;
					item.Prototype = m_ProtoType;
					m_Grasses.Add(item);
				}
			}
		}
		VoxelGrassMeshComputer.ComputeMesh(m_Grasses, 0, m_Mesh, m_Density);
		m_Grasses.Clear();
	}

	private void OnEnable()
	{
		m_Mesh = GetComponent<MeshFilter>();
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
