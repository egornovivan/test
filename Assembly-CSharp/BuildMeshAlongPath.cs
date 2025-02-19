using System.Collections.Generic;
using UnityEngine;
using WhiteCat;

[RequireComponent(typeof(MeshFilter))]
public class BuildMeshAlongPath : MonoBehaviour
{
	public Path path;

	[Space(8f)]
	[Range(0.1f, 100f)]
	public float width = 6f;

	[Range(1f, 30f)]
	public float deltaAngle = 3f;

	[Range(0.01f, 1f)]
	public float angleError = 0.1f;

	[Range(0.01f, 1f)]
	public float uvxPerUnit = 0.1f;

	[Space(8f)]
	public string directory = "Assets/WhiteCat/Examples/Res";

	public string fileName = "mesh.asset";

	public void Bulid()
	{
		List<Vector3> list = new List<Vector3>();
		List<Vector3> list2 = new List<Vector3>();
		List<Vector2> list3 = new List<Vector2>();
		List<int> list4 = new List<int>();
		int index = 0;
		float time = 0f;
		Vector3 splinePoint = path.GetSplinePoint(0, 0f);
		Quaternion splineRotation = path.GetSplineRotation(0, 0f);
		Vector3 vector;
		list.Add(splinePoint + (vector = splineRotation * Vector3.left * (width * 0.5f)));
		list.Add(splinePoint - vector);
		list2.Add(vector = splineRotation * Vector3.up);
		list2.Add(vector);
		list3.Add(new Vector2(0f, 1f));
		list3.Add(new Vector2(0f, 0f));
		bool flag = true;
		while (flag)
		{
			flag = !path.GetNextSplinePosition(ref index, ref time, deltaAngle - angleError, deltaAngle + angleError);
			splinePoint = path.GetSplinePoint(index, time);
			splineRotation = path.GetSplineRotation(index, time);
			float x = path.GetPathLength(index, time) * uvxPerUnit;
			list.Add(splinePoint + (vector = splineRotation * Vector3.left * (width * 0.5f)));
			list.Add(splinePoint - vector);
			list2.Add(vector = splineRotation * Vector3.up);
			list2.Add(vector);
			list3.Add(new Vector2(x, 1f));
			list3.Add(new Vector2(x, 0f));
			list4.Add(list.Count - 4);
			list4.Add(list.Count - 2);
			list4.Add(list.Count - 3);
			list4.Add(list.Count - 3);
			list4.Add(list.Count - 2);
			list4.Add(list.Count - 1);
		}
		Mesh mesh = new Mesh();
		mesh.vertices = list.ToArray();
		mesh.normals = list2.ToArray();
		mesh.uv = list3.ToArray();
		mesh.triangles = list4.ToArray();
		mesh.RecalculateBounds();
		GetComponent<MeshFilter>().sharedMesh = mesh;
	}
}
