using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AttachedRiverScript : MonoBehaviour
{
	public class Cubic
	{
		private float a;

		private float b;

		private float c;

		private float d;

		public Cubic(float a, float b, float c, float d)
		{
			this.a = a;
			this.b = b;
			this.c = c;
			this.d = d;
		}

		public float eval(float u)
		{
			return ((d * u + c) * u + b) * u + a;
		}
	}

	public GameObject parentTerrain;

	public GameObject riverObject;

	public LayerMask terrainLayer;

	public float defRiverWidth;

	public float defRiverDepth;

	public int riverSmooth;

	public int curRiverNodeToPosite;

	public bool showHandles;

	public bool finalized;

	public int seaHeight;

	public List<RiverNodeObject> nodeObjects;

	public Vector3[] nodeObjectVerts;

	public float lowestHeight;

	public ArrayList riverCells;

	public AttachedRiverScript()
	{
		defRiverWidth = 4f;
		defRiverDepth = 3f;
		riverSmooth = 15;
		seaHeight = 0;
	}

	public void Start()
	{
		curRiverNodeToPosite = -1;
		showHandles = true;
		finalized = false;
		CreateMesh(riverSmooth);
	}

	public void CreateMesh(int smoothLevel)
	{
		lowestHeight = 9999999f;
		MeshFilter meshFilter = (MeshFilter)riverObject.GetComponent(typeof(MeshFilter));
		Mesh mesh = meshFilter.sharedMesh;
		if (mesh == null)
		{
			mesh = new Mesh();
			mesh.name = "Generated River Mesh";
			meshFilter.sharedMesh = mesh;
		}
		else
		{
			mesh.Clear();
		}
		if (nodeObjects == null || nodeObjects.Count < 2)
		{
			return;
		}
		int count = nodeObjects.Count;
		int num = 2 * (smoothLevel + 1) * 2;
		int num2 = 6 * (smoothLevel + 1);
		num *= 2;
		num2 *= 3;
		int[] array = new int[num2 * (count - 1)];
		Vector3[] array2 = new Vector3[num * (count - 1)];
		Vector2[] array3 = new Vector2[num * (count - 1)];
		nodeObjectVerts = new Vector3[num * (count - 1)];
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		float[] array4 = new float[count];
		float[] array5 = new float[count];
		float[] array6 = new float[count];
		float[] array7 = new float[count];
		Vector3 vector = default(Vector3);
		Vector3[] array8 = new Vector3[smoothLevel + 1];
		Vector3[] array9 = new Vector3[smoothLevel + 1];
		Vector3[] array10 = new Vector3[smoothLevel + 1];
		Vector3 vector2 = default(Vector3);
		Vector3 vector3 = default(Vector3);
		Vector3 vector4 = default(Vector3);
		for (int i = 0; i < count; i++)
		{
			array4[i] = nodeObjects[i].position.x;
			array5[i] = nodeObjects[i].position.y;
			array6[i] = nodeObjects[i].position.z;
			array7[i] = nodeObjects[i].width;
		}
		Cubic[] array11 = calcNaturalCubic(count - 1, array4);
		Cubic[] array12 = calcNaturalCubic(count - 1, array5);
		Cubic[] array13 = calcNaturalCubic(count - 1, array6);
		Cubic[] array14 = calcNaturalCubic(count - 1, array7);
		for (int j = 0; j < count; j++)
		{
			array8 = new Vector3[smoothLevel + 1];
			array9 = new Vector3[smoothLevel + 1];
			array10 = new Vector3[smoothLevel + 1];
			vector3 = default(Vector3);
			vector4 = default(Vector3);
			if (j == 0)
			{
				ref Vector3 reference = ref array2[num3];
				reference = nodeObjects[0].position;
				num3++;
				ref Vector3 reference2 = ref array2[num3];
				reference2 = nodeObjects[0].position;
				num3++;
				ref Vector3 reference3 = ref array2[num3];
				reference3 = nodeObjects[0].position;
				num3++;
				ref Vector3 reference4 = ref array2[num3];
				reference4 = nodeObjects[0].position;
				num3++;
				ref Vector2 reference5 = ref array3[num5];
				reference5 = new Vector2(1f, 1f);
				num5++;
				ref Vector2 reference6 = ref array3[num5];
				reference6 = new Vector2(0f, 1f);
				num5++;
				ref Vector2 reference7 = ref array3[num5];
				reference7 = new Vector2(1f, 1f);
				num5++;
				ref Vector2 reference8 = ref array3[num5];
				reference8 = new Vector2(0f, 1f);
				num5++;
				continue;
			}
			for (int k = 0; k < smoothLevel + 1; k++)
			{
				if (j == 1 && k == 0)
				{
					vector2 = nodeObjects[0].position;
				}
				else
				{
					ref Vector3 reference9 = ref array2[num3];
					reference9 = array2[num3 - 4];
					num3++;
					ref Vector3 reference10 = ref array2[num3];
					reference10 = array2[num3 - 4];
					num3++;
					ref Vector3 reference11 = ref array2[num3];
					reference11 = array2[num3 - 4];
					num3++;
					ref Vector3 reference12 = ref array2[num3];
					reference12 = array2[num3 - 4];
					num3++;
					ref Vector2 reference13 = ref array3[num5];
					reference13 = new Vector2(1f, 1f);
					num5++;
					ref Vector2 reference14 = ref array3[num5];
					reference14 = new Vector2(0f, 1f);
					num5++;
					ref Vector2 reference15 = ref array3[num5];
					reference15 = new Vector2(1f, 1f);
					num5++;
					ref Vector2 reference16 = ref array3[num5];
					reference16 = new Vector2(0f, 1f);
					num5++;
				}
				float u = (float)(k + 1) / ((float)smoothLevel + 1f);
				Vector3 vector5 = new Vector3(array11[j - 1].eval(u), array12[j - 1].eval(u), array13[j - 1].eval(u));
				float num6 = array14[j - 1].eval(u);
				array9[k] = vector5;
				array8[k] = vector2;
				ref Vector3 reference17 = ref array10[k];
				reference17 = array9[k] - array8[k];
				vector2 = array9[k];
				vector3 = new Vector3(0f - array10[k].z, 0f, array10[k].x);
				vector4 = new Vector3(array10[k].z, 0f, 0f - array10[k].x);
				vector3.Normalize();
				vector4.Normalize();
				if (j == 1 && k == 0)
				{
					vector = vector5;
				}
				float num7 = 2f * num6;
				int num8 = num3 + 1;
				ref Vector3 reference18 = ref array2[num3];
				reference18 = vector5 + vector4 * num7;
				array2[num3].y = 0f;
				num3++;
				ref Vector3 reference19 = ref array2[num3];
				reference19 = vector5 + vector4 * num6;
				array2[num3].y = vector5.y;
				num3++;
				ref Vector3 reference20 = ref array2[num3];
				reference20 = vector5 + vector3 * num6;
				array2[num3].y = vector5.y;
				num3++;
				ref Vector3 reference21 = ref array2[num3];
				reference21 = vector5 + vector3 * num7;
				array2[num3].y = 0f;
				num3++;
				ref Vector2 reference22 = ref array3[num5];
				reference22 = new Vector2(1f, 0f);
				num5++;
				ref Vector2 reference23 = ref array3[num5];
				reference23 = new Vector2(0f, 0f);
				num5++;
				ref Vector2 reference24 = ref array3[num5];
				reference24 = new Vector2(1f, 0f);
				num5++;
				ref Vector2 reference25 = ref array3[num5];
				reference25 = new Vector2(0f, 0f);
				num5++;
				float y = ((!(array2[num8 + 1].y > array2[num8].y - 0.2f)) ? array2[num8 + 1].y : array2[num8].y);
				array2[num8 + 1].y = (array2[num8].y = y);
				if (!(array2[num8 + 1].y > lowestHeight))
				{
					lowestHeight = array2[num8 + 1].y;
				}
				array[num4] = num * (j - 1) + 8 * k;
				num4++;
				array[num4] = num * (j - 1) + 8 * k + 1;
				num4++;
				array[num4] = num * (j - 1) + 8 * k + 4;
				num4++;
				array[num4] = num * (j - 1) + 8 * k + 1;
				num4++;
				array[num4] = num * (j - 1) + 8 * k + 5;
				num4++;
				array[num4] = num * (j - 1) + 8 * k + 4;
				num4++;
				array[num4] = num * (j - 1) + 8 * k + 1;
				num4++;
				array[num4] = num * (j - 1) + 8 * k + 2;
				num4++;
				array[num4] = num * (j - 1) + 8 * k + 5;
				num4++;
				array[num4] = num * (j - 1) + 8 * k + 2;
				num4++;
				array[num4] = num * (j - 1) + 8 * k + 6;
				num4++;
				array[num4] = num * (j - 1) + 8 * k + 5;
				num4++;
				array[num4] = num * (j - 1) + 8 * k + 2;
				num4++;
				array[num4] = num * (j - 1) + 8 * k + 3;
				num4++;
				array[num4] = num * (j - 1) + 8 * k + 6;
				num4++;
				array[num4] = num * (j - 1) + 8 * k + 3;
				num4++;
				array[num4] = num * (j - 1) + 8 * k + 7;
				num4++;
				array[num4] = num * (j - 1) + 8 * k + 6;
				num4++;
			}
		}
		array9[0] = vector;
		ref Vector3 reference26 = ref array8[0];
		reference26 = nodeObjects[0].position;
		ref Vector3 reference27 = ref array10[0];
		reference27 = array9[0] - array8[0];
		vector3 = new Vector3(0f - array10[0].z, 0f, array10[0].x);
		vector4 = new Vector3(array10[0].z, 0f, 0f - array10[0].x);
		vector3.Normalize();
		vector4.Normalize();
		array2[0].y = array2[4].y;
		array2[1].y = array2[5].y;
		array2[2].y = array2[6].y;
		array2[3].y = array2[7].y;
		for (int l = 0; l < array2.Length; l++)
		{
			ref Vector3 reference28 = ref nodeObjectVerts[l];
			reference28 = array2[l];
		}
		mesh.vertices = array2;
		mesh.triangles = array;
		mesh.uv = array3;
		Vector3[] array15 = new Vector3[mesh.vertexCount];
		for (int m = 0; m < mesh.vertexCount; m++)
		{
			ref Vector3 reference29 = ref array15[m];
			reference29 = Vector3.up;
		}
		mesh.normals = array15;
		TangentSolver(mesh);
		mesh.Optimize();
		MeshCollider component = GetComponent<MeshCollider>();
		if (component != null)
		{
			component.sharedMesh = mesh;
		}
	}

	public void OnDrawGizmos()
	{
		if (showHandles && nodeObjectVerts != null && nodeObjectVerts.Length > 0)
		{
			int num = nodeObjectVerts.Length;
			for (int i = 0; i < num; i++)
			{
				Gizmos.color = Color.white;
				Gizmos.DrawLine(base.transform.TransformPoint(nodeObjectVerts[i] + new Vector3(-0.5f, 0f, 0f)), base.transform.TransformPoint(nodeObjectVerts[i] + new Vector3(0.5f, 0f, 0f)));
				Gizmos.DrawLine(base.transform.TransformPoint(nodeObjectVerts[i] + new Vector3(0f, -0.5f, 0f)), base.transform.TransformPoint(nodeObjectVerts[i] + new Vector3(0f, 0.5f, 0f)));
				Gizmos.DrawLine(base.transform.TransformPoint(nodeObjectVerts[i] + new Vector3(0f, 0f, -0.5f)), base.transform.TransformPoint(nodeObjectVerts[i] + new Vector3(0f, 0f, 0.5f)));
			}
		}
	}

	public void TangentSolver(Mesh theMesh)
	{
		int vertexCount = theMesh.vertexCount;
		Vector3[] vertices = theMesh.vertices;
		Vector3[] normals = theMesh.normals;
		Vector2[] uv = theMesh.uv;
		int[] triangles = theMesh.triangles;
		int num = triangles.Length / 3;
		Vector4[] array = new Vector4[vertexCount];
		Vector3[] array2 = new Vector3[vertexCount];
		Vector3[] array3 = new Vector3[vertexCount];
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			int num3 = triangles[num2];
			int num4 = triangles[num2 + 1];
			int num5 = triangles[num2 + 2];
			Vector3 vector = vertices[num3];
			Vector3 vector2 = vertices[num4];
			Vector3 vector3 = vertices[num5];
			Vector3 vector4 = uv[num3];
			Vector3 vector5 = uv[num4];
			Vector3 vector6 = uv[num5];
			float num6 = vector2.x - vector.x;
			float num7 = vector3.x - vector.x;
			float num8 = vector2.y - vector.y;
			float num9 = vector3.y - vector.y;
			float num10 = vector2.z - vector.z;
			float num11 = vector3.z - vector.z;
			float num12 = vector5.x - vector4.x;
			float num13 = vector6.x - vector4.x;
			float num14 = vector5.y - vector4.y;
			float num15 = vector6.y - vector4.y;
			float num16 = 1f / (num12 * num15 - num13 * num14);
			Vector3 vector7 = new Vector3((num15 * num6 - num14 * num7) * num16, (num15 * num8 - num14 * num9) * num16, (num15 * num10 - num14 * num11) * num16);
			Vector3 vector8 = new Vector3((num12 * num7 - num13 * num6) * num16, (num12 * num9 - num13 * num8) * num16, (num12 * num11 - num13 * num10) * num16);
			array2[num3] += vector7;
			array2[num4] += vector7;
			array2[num5] += vector7;
			array3[num3] += vector8;
			array3[num4] += vector8;
			array3[num5] += vector8;
			num2 += 3;
		}
		for (int j = 0; j < vertexCount; j++)
		{
			Vector3 normal = normals[j];
			Vector3 tangent = array2[j];
			Vector3.OrthoNormalize(ref normal, ref tangent);
			array[j].x = tangent.x;
			array[j].y = tangent.y;
			array[j].z = tangent.z;
			array[j].w = ((!(Vector3.Dot(Vector3.Cross(normal, tangent), array3[j]) < 0f)) ? 1f : (-1f));
		}
		theMesh.tangents = array;
	}

	public Cubic[] calcNaturalCubic(int n, float[] x)
	{
		float[] array = new float[n + 1];
		float[] array2 = new float[n + 1];
		float[] array3 = new float[n + 1];
		array[0] = 0.5f;
		for (int i = 1; i < n; i++)
		{
			array[i] = 1f / (4f - array[i - 1]);
		}
		array[n] = 1f / (2f - array[n - 1]);
		array2[0] = 3f * (x[1] - x[0]) * array[0];
		for (int i = 1; i < n; i++)
		{
			array2[i] = (3f * (x[i + 1] - x[i - 1]) - array2[i - 1]) * array[i];
		}
		array2[n] = (3f * (x[n] - x[n - 1]) - array2[n - 1]) * array[n];
		array3[n] = array2[n];
		for (int i = n - 1; i >= 0; i--)
		{
			array3[i] = array2[i] - array[i] * array3[i + 1];
		}
		Cubic[] array4 = new Cubic[n + 1];
		for (int i = 0; i < n; i++)
		{
			array4[i] = new Cubic(x[i], array3[i], 3f * (x[i + 1] - x[i]) - 2f * array3[i] - array3[i + 1], 2f * (x[i] - x[i + 1]) + array3[i] + array3[i + 1]);
		}
		return array4;
	}
}
