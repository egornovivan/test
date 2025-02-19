using UnityEngine;

public class BoundingCubeScale : MonoBehaviour
{
	public Material mat;

	private GameObject go;

	private void Start()
	{
	}

	public void MakeCube(int maxX, int maxY, int maxZ)
	{
		go = new GameObject();
		go.name = "trepassing is naughty";
		go.transform.parent = base.transform;
		go.transform.localPosition = new Vector3(0.5f, 0.5f, 0.5f);
		go.layer = 12;
		MeshFilter meshFilter = go.AddComponent<MeshFilter>();
		MeshCollider meshCollider = go.AddComponent<MeshCollider>();
		MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
		meshRenderer.sharedMaterial = mat;
		Mesh mesh = meshFilter.mesh;
		mesh.name = "bounding cube mesh";
		Vector3[] array = new Vector3[8];
		array[0].x = 0f;
		array[0].y = 0f;
		array[0].z = 0f;
		array[1].x = 1f;
		array[1].y = 0f;
		array[1].z = 0f;
		array[2].x = 0f;
		array[2].y = 1f;
		array[2].z = 0f;
		array[3].x = 1f;
		array[3].y = 1f;
		array[3].z = 0f;
		array[4].x = 0f;
		array[4].y = 0f;
		array[4].z = 1f;
		array[5].x = 1f;
		array[5].y = 0f;
		array[5].z = 1f;
		array[6].x = 0f;
		array[6].y = 1f;
		array[6].z = 1f;
		array[7].x = 1f;
		array[7].y = 1f;
		array[7].z = 1f;
		mesh.vertices = array;
		int[] triangles = new int[30]
		{
			0, 1, 2, 3, 2, 1, 2, 4, 0, 6,
			4, 2, 7, 5, 6, 6, 5, 4, 3, 1,
			5, 7, 3, 5, 5, 1, 4, 4, 1, 0
		};
		mesh.SetTriangles(triangles, 0);
		Vector2[] array2 = new Vector2[8];
		array2[0].x = 0f;
		array2[0].y = 0f;
		array2[1].x = 1f;
		array2[1].y = 0f;
		array2[2].x = 0f;
		array2[2].y = 1f;
		array2[3].x = 1f;
		array2[3].y = 1f;
		array2[4].x = 0f;
		array2[4].y = 0f;
		array2[5].x = 1f;
		array2[5].y = 0f;
		array2[6].x = 0f;
		array2[6].y = 1f;
		array2[7].x = 1f;
		array2[7].y = 1f;
		mesh.uv = array2;
		meshCollider.sharedMesh = mesh;
		Transform component = go.GetComponent<Transform>();
		component.localScale = new Vector3(maxX - 2, maxY + 10, maxZ - 2);
	}
}
