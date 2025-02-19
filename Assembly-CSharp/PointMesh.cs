using UnityEngine;

public class PointMesh : MonoBehaviour
{
	public int vertex_count = 1000;

	private MeshFilter mf;

	private void Start()
	{
		mf = GetComponent<MeshFilter>();
		mf.mesh.Clear();
		Vector3[] array = new Vector3[6];
		Vector3[] array2 = new Vector3[6];
		Vector2[] array3 = new Vector2[6];
		int[] array4 = new int[6];
		ref Vector3 reference = ref array[0];
		reference = new Vector3(-0.5f, 0f, -0.5f);
		ref Vector3 reference2 = ref array[1];
		reference2 = new Vector3(0.5f, 0f, -0.5f);
		ref Vector3 reference3 = ref array[2];
		reference3 = new Vector3(0.5f, 0f, 0.5f);
		ref Vector3 reference4 = ref array[3];
		reference4 = new Vector3(0.5f, 0f, 0.5f);
		ref Vector3 reference5 = ref array[4];
		reference5 = new Vector3(-0.5f, 0f, 0.5f);
		ref Vector3 reference6 = ref array[5];
		reference6 = new Vector3(-0.5f, 0f, -0.5f);
		ref Vector3 reference7 = ref array2[0];
		reference7 = new Vector3(0f, 1f, 0f);
		ref Vector3 reference8 = ref array2[1];
		reference8 = new Vector3(0f, 1f, 0f);
		ref Vector3 reference9 = ref array2[2];
		reference9 = new Vector3(0f, 1f, 0f);
		ref Vector3 reference10 = ref array2[3];
		reference10 = new Vector3(0f, 1f, 0f);
		ref Vector3 reference11 = ref array2[4];
		reference11 = new Vector3(0f, 1f, 0f);
		ref Vector3 reference12 = ref array2[5];
		reference12 = new Vector3(0f, 1f, 0f);
		array4[0] = 0;
		array4[1] = 1;
		array4[2] = 2;
		array4[3] = 3;
		array4[4] = 4;
		array4[5] = 5;
		ref Vector2 reference13 = ref array3[0];
		reference13 = new Vector2(0f, 0f);
		ref Vector2 reference14 = ref array3[1];
		reference14 = new Vector2(1f, 0f);
		ref Vector2 reference15 = ref array3[2];
		reference15 = new Vector2(1f, 1f);
		ref Vector2 reference16 = ref array3[3];
		reference16 = new Vector2(1f, 1f);
		ref Vector2 reference17 = ref array3[4];
		reference17 = new Vector2(0f, 1f);
		ref Vector2 reference18 = ref array3[5];
		reference18 = new Vector2(0f, 0f);
		mf.mesh.vertices = array;
		mf.mesh.normals = array2;
		mf.mesh.SetTriangles(array4, 0);
		mf.mesh.uv = array3;
	}

	private void Update()
	{
	}
}
