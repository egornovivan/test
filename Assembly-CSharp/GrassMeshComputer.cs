using System.Collections.Generic;
using UnityEngine;

public static class GrassMeshComputer
{
	public static int s_GrassCountPerMesh = 15000;

	public static int ComputeMesh(List<GrassInstance> grass_list, int offset, MeshFilter mf_inout)
	{
		if (grass_list == null)
		{
			return 0;
		}
		offset = Mathf.Clamp(offset, 0, grass_list.Count);
		int a = Mathf.Clamp(s_GrassCountPerMesh, 128, 16240);
		int num = Mathf.Min(a, grass_list.Count - offset);
		if (num < 1)
		{
			return grass_list.Count;
		}
		if (mf_inout == null)
		{
			return grass_list.Count;
		}
		Vector3[] array = new Vector3[num << 2];
		Vector3[] array2 = new Vector3[num << 2];
		Vector2[] array3 = new Vector2[num << 2];
		Vector2[] array4 = new Vector2[num << 2];
		Color32[] array5 = new Color32[num << 2];
		int[] array6 = new int[num * 6];
		Vector3 vector = new Vector3(-1f, -1f, 0f);
		Vector3 vector2 = new Vector3(-1f, 1f, 0f);
		Vector3 vector3 = new Vector3(1f, 1f, 0f);
		Vector3 vector4 = new Vector3(1f, -1f, 0f);
		Vector3 up = Vector3.up;
		for (int i = 0; i < num; i++)
		{
			int num2 = i * 4;
			int num3 = num2 + 1;
			int num4 = num2 + 2;
			int num5 = num2 + 3;
			array6[i * 6] = num2;
			array6[i * 6 + 1] = num3;
			array6[i * 6 + 2] = num4;
			array6[i * 6 + 3] = num4;
			array6[i * 6 + 4] = num5;
			array6[i * 6 + 5] = num2;
			array[num2] = vector;
			array[num3] = vector2;
			array[num4] = vector3;
			array[num5] = vector4;
			GrassInstance grassInstance = grass_list[offset + i];
			ref Vector3 reference = ref array2[num5];
			ref Vector3 reference2 = ref array2[num4];
			ref Vector3 reference3 = ref array2[num3];
			ref Vector3 reference4 = ref array2[num2];
			reference = (reference2 = (reference3 = (reference4 = grassInstance.Position)));
			Vector3 normal = grassInstance.Normal;
			Vector3 normalized = (normal * 1.25f - up * 0.25f).normalized;
			Vector3 normalized2 = (normal * 0.5f + up * 0.5f).normalized;
			ref Vector2 reference5 = ref array3[num2];
			reference5 = new Vector2(normalized.x, normalized.z);
			ref Vector2 reference6 = ref array3[num4];
			ref Vector2 reference7 = ref array3[num3];
			reference6 = (reference7 = new Vector2(normalized2.x, normalized2.z));
			ref Vector2 reference8 = ref array3[num5];
			reference8 = array3[num2];
			ref Vector2 reference9 = ref array4[num5];
			ref Vector2 reference10 = ref array4[num4];
			ref Vector2 reference11 = ref array4[num3];
			ref Vector2 reference12 = ref array4[num2];
			reference9 = (reference10 = (reference11 = (reference12 = new Vector2((float)grassInstance.Prototype / 64f, 0f))));
			ref Color32 reference13 = ref array5[num5];
			ref Color32 reference14 = ref array5[num4];
			ref Color32 reference15 = ref array5[num3];
			ref Color32 reference16 = ref array5[num2];
			reference13 = (reference14 = (reference15 = (reference16 = grassInstance.ColorDw)));
		}
		Mesh mesh = mf_inout.mesh;
		mesh.Clear();
		mesh.vertices = array;
		mesh.triangles = array6;
		mesh.normals = array2;
		mesh.uv = array3;
		mesh.uv2 = array4;
		mesh.colors32 = array5;
		return offset + num;
	}
}
