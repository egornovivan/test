using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CrossLine : MonoBehaviour
{
	private Mesh m_LineMesh;

	public Vector3 m_Begin;

	public Vector3 m_End;

	public float m_Thickness = 0.1f;

	public int m_Segment = 4;

	private Vector3 _lastBegin;

	private Vector3 _lastEnd;

	private float _thickness;

	private float _seg;

	private void Start()
	{
	}

	private void Update()
	{
		if (Vector3.Distance(m_Begin, _lastBegin) > 0.001f || Vector3.Distance(m_End, _lastEnd) > 0.001f || m_Thickness != _thickness || (float)m_Segment != _seg)
		{
			Refresh();
		}
	}

	private void OnDestroy()
	{
		FreeMesh();
	}

	public void FreeMesh()
	{
		if (m_LineMesh != null)
		{
			UnityEngine.Object.Destroy(m_LineMesh);
			m_LineMesh = null;
		}
	}

	public void Refresh()
	{
		_lastBegin = m_Begin;
		_lastEnd = m_End;
		_thickness = m_Thickness;
		_seg = m_Segment;
		FreeMesh();
		if (m_Begin != m_End && _thickness != 0f && _seg > 0f)
		{
			m_LineMesh = new Mesh();
			int num = m_Segment * 4;
			int num2 = m_Segment * 6;
			Vector3[] array = new Vector3[num];
			Vector2[] array2 = new Vector2[num];
			int[] array3 = new int[num2];
			float num3 = (float)Math.PI / (float)m_Segment;
			base.transform.position = (m_End + m_Begin) * 0.5f;
			base.transform.LookAt(m_End);
			float num4 = (m_End - m_Begin).magnitude * 0.5f;
			for (int i = 0; i < m_Segment; i++)
			{
				float num5 = num3 * (float)i;
				float num6 = Mathf.Cos(num5) * m_Thickness * 0.5f;
				float num7 = Mathf.Sin(num5) * m_Thickness * 0.5f;
				float num8 = num4;
				ref Vector3 reference = ref array[i * 4];
				reference = new Vector3(0f - num6, 0f - num7, 0f - num8);
				ref Vector3 reference2 = ref array[i * 4 + 1];
				reference2 = new Vector3(num6, num7, 0f - num8);
				ref Vector3 reference3 = ref array[i * 4 + 2];
				reference3 = new Vector3(num6, num7, num8);
				ref Vector3 reference4 = ref array[i * 4 + 3];
				reference4 = new Vector3(0f - num6, 0f - num7, num8);
				ref Vector2 reference5 = ref array2[i * 4];
				reference5 = new Vector2(0f - num8 + num5 * 2f, -1f);
				ref Vector2 reference6 = ref array2[i * 4 + 1];
				reference6 = new Vector2(0f - num8 + num5 * 2f, 1f);
				ref Vector2 reference7 = ref array2[i * 4 + 2];
				reference7 = new Vector2(num8 + num5 * 2f, 1f);
				ref Vector2 reference8 = ref array2[i * 4 + 3];
				reference8 = new Vector2(num8 + num5 * 2f, -1f);
				array3[i * 6] = i * 4;
				array3[i * 6 + 1] = i * 4 + 1;
				array3[i * 6 + 2] = i * 4 + 2;
				array3[i * 6 + 3] = i * 4 + 2;
				array3[i * 6 + 4] = i * 4 + 3;
				array3[i * 6 + 5] = i * 4;
			}
			m_LineMesh.vertices = array;
			m_LineMesh.uv = array2;
			m_LineMesh.SetTriangles(array3, 0);
			GetComponent<MeshFilter>().mesh = m_LineMesh;
		}
	}
}
