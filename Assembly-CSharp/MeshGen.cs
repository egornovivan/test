using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshGen : MonoBehaviour
{
	public int m_BillboardCount = 1000;

	public Mesh m_Mesh;

	public Vector3 m_StartCoord = Vector3.zero;

	private void Start()
	{
		m_Mesh = GetComponent<MeshFilter>().mesh;
		ReGen();
		base.enabled = false;
	}

	private void Update()
	{
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(30f, 30f, 100f, 30f), "Re-Generate"))
		{
			ReGen();
		}
	}

	private void ReGen()
	{
		m_Mesh.Clear();
		Vector3[] array = new Vector3[m_BillboardCount * 4];
		Vector3[] array2 = new Vector3[m_BillboardCount * 4];
		Vector2[] array3 = new Vector2[m_BillboardCount * 4];
		Vector2[] array4 = new Vector2[m_BillboardCount * 4];
		int[] array5 = new int[m_BillboardCount * 6];
		Vector3 vector = new Vector3(-1f, -1f, 0f);
		Vector3 vector2 = new Vector3(-1f, 1f, 0f);
		Vector3 vector3 = new Vector3(1f, 1f, 0f);
		Vector3 vector4 = new Vector3(1f, -1f, 0f);
		m_StartCoord = base.transform.position - Vector3.one * 128f;
		for (int i = 0; i < m_BillboardCount; i++)
		{
			array[i * 4] = vector;
			array[i * 4 + 1] = vector2;
			array[i * 4 + 2] = vector3;
			array[i * 4 + 3] = vector4;
			array5[i * 6] = i * 4;
			array5[i * 6 + 1] = i * 4 + 1;
			array5[i * 6 + 2] = i * 4 + 2;
			array5[i * 6 + 3] = i * 4 + 2;
			array5[i * 6 + 4] = i * 4 + 3;
			array5[i * 6 + 5] = i * 4;
			Vector3 origin = new Vector3(Random.value, 1f, Random.value) * 256f + m_StartCoord;
			Vector3 vector5 = Vector3.up;
			if (Physics.Raycast(origin, Vector3.down, out var hitInfo, 1024f, 4096))
			{
				Vector3 point = hitInfo.point;
				ref Vector3 reference = ref array2[i * 4 + 3];
				ref Vector3 reference2 = ref array2[i * 4 + 2];
				ref Vector3 reference3 = ref array2[i * 4 + 1];
				reference = (reference2 = (reference3 = (array2[i * 4] = point)));
				vector5 = hitInfo.normal;
			}
			else
			{
				Vector3 zero = Vector3.zero;
				ref Vector3 reference4 = ref array2[i * 4 + 3];
				ref Vector3 reference5 = ref array2[i * 4 + 2];
				ref Vector3 reference6 = ref array2[i * 4 + 1];
				reference4 = (reference5 = (reference6 = (array2[i * 4] = zero)));
			}
			Vector3 normalized = (vector5 * 1.25f + Vector3.up * -0.25f).normalized;
			Vector3 normalized2 = (vector5 * 0.5f + Vector3.up * 0.5f).normalized;
			ref Vector2 reference7 = ref array3[i * 4];
			reference7 = new Vector2(normalized.x, normalized.z);
			ref Vector2 reference8 = ref array3[i * 4 + 1];
			reference8 = new Vector2(normalized2.x, normalized2.z);
			ref Vector2 reference9 = ref array3[i * 4 + 2];
			reference9 = new Vector2(normalized2.x, normalized2.z);
			ref Vector2 reference10 = ref array3[i * 4 + 3];
			reference10 = new Vector2(normalized.x, normalized.z);
			float x = 0f;
			ref Vector2 reference11 = ref array4[i * 4];
			reference11 = new Vector2(x, 0f);
			ref Vector2 reference12 = ref array4[i * 4 + 1];
			reference12 = new Vector2(x, 0f);
			ref Vector2 reference13 = ref array4[i * 4 + 2];
			reference13 = new Vector2(x, 0f);
			ref Vector2 reference14 = ref array4[i * 4 + 3];
			reference14 = new Vector2(x, 0f);
		}
		m_Mesh.vertices = array;
		m_Mesh.triangles = array5;
		m_Mesh.normals = array2;
		m_Mesh.uv = array3;
		m_Mesh.uv2 = array4;
	}
}
