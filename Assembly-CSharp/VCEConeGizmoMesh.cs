using System;
using UnityEngine;

public class VCEConeGizmoMesh : MonoBehaviour
{
	private MeshFilter m_TargetMeshFilter;

	private Mesh m_ConeMesh;

	public float m_PositiveScale;

	public float m_NegativeScale = 1f;

	public int m_Segment = 24;

	private float last_pos_scl;

	private float last_neg_scl;

	private Vector3[] m_verts;

	private Vector3[] m_normals;

	private int[] m_indices;

	private void Start()
	{
		m_verts = new Vector3[m_Segment * 12];
		m_normals = new Vector3[m_Segment * 12];
		m_indices = new int[m_Segment * 12];
		m_TargetMeshFilter = GetComponent<MeshFilter>();
		if (m_TargetMeshFilter != null)
		{
			m_ConeMesh = new Mesh();
			m_TargetMeshFilter.mesh = m_ConeMesh;
		}
	}

	private void Update()
	{
		if (m_PositiveScale != last_pos_scl || m_NegativeScale != last_neg_scl)
		{
			last_pos_scl = m_PositiveScale;
			last_neg_scl = m_NegativeScale;
			GenCone();
		}
	}

	private void OnDestroy()
	{
		if (m_ConeMesh != null)
		{
			UnityEngine.Object.Destroy(m_ConeMesh);
			m_ConeMesh = null;
		}
	}

	private void GenCone()
	{
		if (!(m_ConeMesh != null))
		{
			return;
		}
		m_ConeMesh.Clear();
		for (int i = 0; i < m_Segment; i++)
		{
			int num = i * 12;
			float f = (float)i / (float)m_Segment * (float)Math.PI * 2f;
			float f2 = (float)(i + 1) / (float)m_Segment * (float)Math.PI * 2f;
			float num2 = m_NegativeScale * 0.5f;
			float num3 = m_PositiveScale * 0.5f;
			ref Vector3 reference = ref m_verts[num];
			reference = new Vector3(num2 * Mathf.Cos(f), -0.5f, num2 * Mathf.Sin(f));
			ref Vector3 reference2 = ref m_verts[num + 1];
			reference2 = new Vector3(num2 * Mathf.Cos(f2), -0.5f, num2 * Mathf.Sin(f2));
			ref Vector3 reference3 = ref m_verts[num + 2];
			reference3 = new Vector3(num3 * Mathf.Cos(f2), 0.5f, num3 * Mathf.Sin(f2));
			ref Vector3 reference4 = ref m_verts[num + 3];
			reference4 = new Vector3(num3 * Mathf.Cos(f2), 0.5f, num3 * Mathf.Sin(f2));
			ref Vector3 reference5 = ref m_verts[num + 4];
			reference5 = new Vector3(num3 * Mathf.Cos(f), 0.5f, num3 * Mathf.Sin(f));
			ref Vector3 reference6 = ref m_verts[num + 5];
			reference6 = new Vector3(num2 * Mathf.Cos(f), -0.5f, num2 * Mathf.Sin(f));
			ref Vector3 reference7 = ref m_verts[num + 6];
			reference7 = new Vector3(num2 * Mathf.Cos(f2), -0.5f, num2 * Mathf.Sin(f2));
			ref Vector3 reference8 = ref m_verts[num + 7];
			reference8 = new Vector3(num2 * Mathf.Cos(f), -0.5f, num2 * Mathf.Sin(f));
			ref Vector3 reference9 = ref m_verts[num + 8];
			reference9 = new Vector3(0f, -0.5f, 0f);
			ref Vector3 reference10 = ref m_verts[num + 9];
			reference10 = new Vector3(num3 * Mathf.Cos(f), 0.5f, num3 * Mathf.Sin(f));
			ref Vector3 reference11 = ref m_verts[num + 10];
			reference11 = new Vector3(num3 * Mathf.Cos(f2), 0.5f, num3 * Mathf.Sin(f2));
			ref Vector3 reference12 = ref m_verts[num + 11];
			reference12 = new Vector3(0f, 0.5f, 0f);
			ref Vector3 reference13 = ref m_normals[num];
			reference13 = new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f));
			ref Vector3 reference14 = ref m_normals[num + 1];
			reference14 = new Vector3(Mathf.Cos(f2), 0f, Mathf.Sin(f2));
			ref Vector3 reference15 = ref m_normals[num + 2];
			reference15 = new Vector3(Mathf.Cos(f2), 0f, Mathf.Sin(f2));
			ref Vector3 reference16 = ref m_normals[num + 3];
			reference16 = new Vector3(Mathf.Cos(f2), 0f, Mathf.Sin(f2));
			ref Vector3 reference17 = ref m_normals[num + 4];
			reference17 = new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f));
			ref Vector3 reference18 = ref m_normals[num + 5];
			reference18 = new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f));
			ref Vector3 reference19 = ref m_normals[num + 6];
			reference19 = new Vector3(0f, -1f, 0f);
			ref Vector3 reference20 = ref m_normals[num + 7];
			reference20 = new Vector3(0f, -1f, 0f);
			ref Vector3 reference21 = ref m_normals[num + 8];
			reference21 = new Vector3(0f, -1f, 0f);
			ref Vector3 reference22 = ref m_normals[num + 9];
			reference22 = new Vector3(0f, 1f, 0f);
			ref Vector3 reference23 = ref m_normals[num + 10];
			reference23 = new Vector3(0f, 1f, 0f);
			ref Vector3 reference24 = ref m_normals[num + 11];
			reference24 = new Vector3(0f, 1f, 0f);
			for (int j = 0; j < 12; j++)
			{
				m_indices[num + j] = num + j;
			}
		}
		m_ConeMesh.vertices = m_verts;
		m_ConeMesh.normals = m_normals;
		m_ConeMesh.SetTriangles(m_indices, 0);
	}
}
