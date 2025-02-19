using System;
using System.Collections.Generic;
using UnityEngine;

public static class TrigrassMeshComputer
{
	public static int s_GrassCountPerMesh = 3000;

	public static int ComputeMesh(List<GrassInstance> grass_list, int offset, MeshFilter mf_inout)
	{
		if (grass_list == null)
		{
			return 0;
		}
		offset = Mathf.Clamp(offset, 0, grass_list.Count);
		int a = Mathf.Clamp(s_GrassCountPerMesh, 128, 5400);
		int num = Mathf.Min(a, grass_list.Count - offset);
		if (num < 1)
		{
			return grass_list.Count;
		}
		if (mf_inout == null)
		{
			return grass_list.Count;
		}
		Vector3[] array = new Vector3[num * 12];
		Vector3[] array2 = new Vector3[num * 12];
		Vector2[] array3 = new Vector2[num * 12];
		Vector2[] array4 = new Vector2[num * 12];
		Color32[] array5 = new Color32[num * 12];
		int[] array6 = new int[num * 18];
		Vector3 vector = new Vector3(-1f, -1f, 0f);
		Vector3 vector2 = new Vector3(-1f, 1f, 0f);
		Vector3 vector3 = new Vector3(1f, 1f, 0f);
		Vector3 vector4 = new Vector3(1f, -1f, 0f);
		Vector3 up = Vector3.up;
		float num2 = (float)Math.PI * 2f;
		float num3 = num2 / 3f;
		float num4 = 0.3f;
		for (int i = 0; i < num; i++)
		{
			float num5 = UnityEngine.Random.value * num2;
			for (int j = 0; j < 3; j++)
			{
				int num6 = i * 12 + j * 4;
				int num7 = num6 + 1;
				int num8 = num6 + 2;
				int num9 = num6 + 3;
				int num10 = i * 18 + j * 6;
				array6[num10] = num6;
				array6[num10 + 1] = num7;
				array6[num10 + 2] = num8;
				array6[num10 + 3] = num8;
				array6[num10 + 4] = num9;
				array6[num10 + 5] = num6;
				array[num6] = vector;
				array[num7] = vector2;
				array[num8] = vector3;
				array[num9] = vector4;
				GrassInstance grassInstance = grass_list[offset + i];
				ref Vector3 reference = ref array2[num9];
				ref Vector3 reference2 = ref array2[num8];
				ref Vector3 reference3 = ref array2[num7];
				ref Vector3 reference4 = ref array2[num6];
				reference = (reference2 = (reference3 = (reference4 = grassInstance.Position)));
				Vector3 normal = grassInstance.Normal;
				Vector3 normalized = (normal * 1.1f - up * 0.1f).normalized;
				Vector3 normalized2 = (normal * 0.5f + up * 0.5f).normalized;
				ref Vector2 reference5 = ref array3[num6];
				reference5 = new Vector2(normalized.x, normalized.z);
				ref Vector2 reference6 = ref array3[num8];
				ref Vector2 reference7 = ref array3[num7];
				reference6 = (reference7 = new Vector2(normalized2.x, normalized2.z));
				ref Vector2 reference8 = ref array3[num9];
				reference8 = array3[num6];
				ref Vector2 reference9 = ref array4[num9];
				ref Vector2 reference10 = ref array4[num8];
				ref Vector2 reference11 = ref array4[num7];
				ref Vector2 reference12 = ref array4[num6];
				reference9 = (reference10 = (reference11 = (reference12 = new Vector2((float)grassInstance.Prototype / 64f, (float)j * num3 + num5 + (UnityEngine.Random.value - 0.5f) * num4))));
				ref Color32 reference13 = ref array5[num9];
				ref Color32 reference14 = ref array5[num8];
				ref Color32 reference15 = ref array5[num7];
				ref Color32 reference16 = ref array5[num6];
				reference13 = (reference14 = (reference15 = (reference16 = grassInstance.ColorDw)));
			}
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
