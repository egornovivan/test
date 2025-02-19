using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SeaPlaneGen : MonoBehaviour
{
	public int m_CellCount = 200;

	public float m_CellSize = 1f;

	private MeshFilter m_MeshFilter;

	private Mesh m_Mesh;

	private void Start()
	{
		m_MeshFilter = GetComponent<MeshFilter>();
		m_Mesh = new Mesh();
		int num = m_CellCount + 1;
		int num2 = num * num;
		int num3 = m_CellCount * m_CellCount * 2;
		int num4 = num3 * 3;
		Vector3[] array = new Vector3[num2];
		Vector3[] array2 = new Vector3[num2];
		int[] array3 = new int[num4];
		Vector4[] array4 = new Vector4[num2];
		for (int i = 0; i <= m_CellCount; i++)
		{
			for (int j = 0; j <= m_CellCount; j++)
			{
				ref Vector3 reference = ref array[i * num + j];
				reference = new Vector3((float)j * m_CellSize, 0f, (float)i * m_CellSize);
				ref Vector3 reference2 = ref array2[i * num + j];
				reference2 = Vector3.up;
				ref Vector4 reference3 = ref array4[i * num + j];
				reference3 = new Vector4(1f, 0f, 0f, 1f);
				int num5 = (i * m_CellCount + j) * 6;
				if (i < m_CellCount && j < m_CellCount)
				{
					array3[num5] = i * num + j;
					array3[num5 + 1] = (i + 1) * num + j;
					array3[num5 + 2] = (i + 1) * num + (j + 1);
					array3[num5 + 3] = (i + 1) * num + (j + 1);
					array3[num5 + 4] = i * num + (j + 1);
					array3[num5 + 5] = i * num + j;
				}
			}
		}
		m_Mesh.vertices = array;
		m_Mesh.normals = array2;
		m_Mesh.tangents = array4;
		m_Mesh.SetTriangles(array3, 0);
		m_MeshFilter.mesh = m_Mesh;
	}

	private void OnDestroy()
	{
		if (m_Mesh != null)
		{
			Object.Destroy(m_Mesh);
			m_Mesh = null;
		}
	}
}
