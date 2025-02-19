using UnityEngine;

public class VCEGizmoMesh : MonoBehaviour
{
	public float m_BorderSize = 0.003f;

	public float m_BorderUVSize = 0.46f;

	public float m_MeshSizeX = 0.01f;

	public float m_MeshSizeY = 0.01f;

	private MeshFilter m_MeshFilter;

	private Mesh m_GizmoMesh;

	private void Start()
	{
		m_MeshFilter = GetComponent<MeshFilter>();
		m_GizmoMesh = m_MeshFilter.mesh;
	}

	private void Update()
	{
		m_GizmoMesh.Clear();
		Vector3[] array = new Vector3[54];
		Vector3[] array2 = new Vector3[54];
		Vector2[] array3 = new Vector2[54];
		int[] array4 = new int[54];
		float num = (0f - m_MeshSizeX) / 2f;
		float num2 = num + m_BorderSize;
		float num3 = m_MeshSizeX / 2f;
		float num4 = num3 - m_BorderSize;
		float num5 = (0f - m_MeshSizeY) / 2f;
		float num6 = num5 + m_BorderSize;
		float num7 = m_MeshSizeY / 2f;
		float num8 = num7 - m_BorderSize;
		float num9 = 0f;
		float borderUVSize = m_BorderUVSize;
		float num10 = 1f;
		float num11 = 1f - m_BorderUVSize;
		float[] array5 = new float[4] { num, num2, num4, num3 };
		float[] array6 = new float[4] { num5, num6, num8, num7 };
		float[] array7 = new float[4] { num9, borderUVSize, num11, num10 };
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				int num12 = (i * 3 + j) * 6;
				ref Vector3 reference = ref array[num12];
				reference = new Vector3(array5[i], array6[j], 0f);
				ref Vector3 reference2 = ref array[num12 + 1];
				reference2 = new Vector3(array5[i + 1], array6[j], 0f);
				ref Vector3 reference3 = ref array[num12 + 2];
				reference3 = new Vector3(array5[i + 1], array6[j + 1], 0f);
				ref Vector3 reference4 = ref array[num12 + 3];
				reference4 = new Vector3(array5[i + 1], array6[j + 1], 0f);
				ref Vector3 reference5 = ref array[num12 + 4];
				reference5 = new Vector3(array5[i], array6[j + 1], 0f);
				ref Vector3 reference6 = ref array[num12 + 5];
				reference6 = new Vector3(array5[i], array6[j], 0f);
				ref Vector2 reference7 = ref array3[num12];
				reference7 = new Vector2(array7[i], array7[j]);
				ref Vector2 reference8 = ref array3[num12 + 1];
				reference8 = new Vector2(array7[i + 1], array7[j]);
				ref Vector2 reference9 = ref array3[num12 + 2];
				reference9 = new Vector2(array7[i + 1], array7[j + 1]);
				ref Vector2 reference10 = ref array3[num12 + 3];
				reference10 = new Vector2(array7[i + 1], array7[j + 1]);
				ref Vector2 reference11 = ref array3[num12 + 4];
				reference11 = new Vector2(array7[i], array7[j + 1]);
				ref Vector2 reference12 = ref array3[num12 + 5];
				reference12 = new Vector2(array7[i], array7[j]);
				for (int k = 0; k < 6; k++)
				{
					ref Vector3 reference13 = ref array2[num12 + k];
					reference13 = Vector3.forward;
					array4[num12 + k] = num12 + k;
				}
			}
		}
		m_GizmoMesh.vertices = array;
		m_GizmoMesh.normals = array2;
		m_GizmoMesh.uv = array3;
		m_GizmoMesh.SetTriangles(array4, 0);
	}
}
