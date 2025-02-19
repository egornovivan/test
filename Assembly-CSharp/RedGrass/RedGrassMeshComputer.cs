using System;
using System.Collections.Generic;
using UnityEngine;

namespace RedGrass;

public class RedGrassMeshComputer
{
	public class OutputStruct
	{
		public Vector3[] Verts;

		public Vector3[] Norms;

		public Vector2[] UVs;

		public Vector2[] UV2s;

		public Color32[] Color32s;

		public int[] Indices;

		public int BillboardCount;

		public int TriquadCount;

		public int TotalVertCount;

		public void Init()
		{
			Verts = new Vector3[262144];
			Norms = new Vector3[262144];
			UVs = new Vector2[262144];
			UV2s = new Vector2[262144];
			Color32s = new Color32[262144];
			Indices = new int[393216];
			Reset();
			Vector3 vector = new Vector3(-1f, -1f, 0f);
			Vector3 vector2 = new Vector3(-1f, 1f, 0f);
			Vector3 vector3 = new Vector3(1f, 1f, 0f);
			Vector3 vector4 = new Vector3(1f, -1f, 0f);
			for (int i = 0; i < 65536; i++)
			{
				Verts[i * 4] = vector;
				Verts[i * 4 + 1] = vector2;
				Verts[i * 4 + 2] = vector3;
				Verts[i * 4 + 3] = vector4;
			}
			for (int j = 0; j < 65536; j++)
			{
				Indices[j * 6] = j * 4;
				Indices[j * 6 + 1] = j * 4 + 1;
				Indices[j * 6 + 2] = j * 4 + 2;
				Indices[j * 6 + 3] = j * 4 + 2;
				Indices[j * 6 + 4] = j * 4 + 3;
				Indices[j * 6 + 5] = j * 4;
			}
		}

		public void Reset()
		{
			BillboardCount = 0;
			TriquadCount = 0;
			TotalVertCount = 0;
		}
	}

	public const int MAX_QUAD = 65536;

	public const int MAX_VERT = 262144;

	public const int MAX_IDX = 393216;

	public static float s_FullDensity = 2f;

	public static OutputStruct s_Output;

	public static void Init()
	{
		s_Output = new OutputStruct();
		s_Output.Init();
	}

	private static int RandomCount(float expectation, RedGrassInstance rgi)
	{
		float num = (int)expectation;
		float num2 = expectation - num;
		return (int)((num2 == 0f) ? num : ((!(rgi.RandAttr.x < num2)) ? num : (num + 1f)));
	}

	public static void ComputeMesh(List<RedGrassInstance> grass_list, List<RedGrassInstance> tri_grass_list, float density)
	{
		try
		{
			s_Output.Reset();
			int num = 0;
			int num2 = 0;
			if (grass_list != null)
			{
				num = grass_list.Count;
			}
			if (tri_grass_list != null)
			{
				num2 = tri_grass_list.Count;
			}
			Vector3 up = Vector3.up;
			int num3 = 0;
			float num4 = (float)Math.PI * 2f;
			float num5 = num4 / 3f;
			float num6 = 0.3f;
			for (int i = 0; i < num; i++)
			{
				RedGrassInstance rgi = grass_list[i];
				int num7 = RandomCount(s_FullDensity * rgi.Density * density, rgi);
				for (int j = 0; j < num7; j++)
				{
					int num8 = num3 * 4;
					int num9 = num8 + 1;
					int num10 = num8 + 2;
					int num11 = num8 + 3;
					ref Vector3 reference = ref s_Output.Norms[num11];
					ref Vector3 reference2 = ref s_Output.Norms[num10];
					ref Vector3 reference3 = ref s_Output.Norms[num9];
					ref Vector3 reference4 = ref s_Output.Norms[num8];
					reference = (reference2 = (reference3 = (reference4 = rgi.RandPos(j))));
					Vector3 normal = rgi.Normal;
					Vector3 normalized = (normal * 1.25f - up * 0.25f).normalized;
					Vector3 normalized2 = (normal * 0.5f + up * 0.5f).normalized;
					ref Vector2 reference5 = ref s_Output.UVs[num8];
					reference5 = new Vector2(normalized.x, normalized.z);
					ref Vector2 reference6 = ref s_Output.UVs[num10];
					ref Vector2 reference7 = ref s_Output.UVs[num9];
					reference6 = (reference7 = new Vector2(normalized2.x, normalized2.z));
					ref Vector2 reference8 = ref s_Output.UVs[num11];
					reference8 = s_Output.UVs[num8];
					ref Vector2 reference9 = ref s_Output.UV2s[num11];
					ref Vector2 reference10 = ref s_Output.UV2s[num10];
					ref Vector2 reference11 = ref s_Output.UV2s[num9];
					ref Vector2 reference12 = ref s_Output.UV2s[num8];
					reference9 = (reference10 = (reference11 = (reference12 = new Vector2((float)rgi.Prototype / 64f, 0f))));
					ref Color32 reference13 = ref s_Output.Color32s[num11];
					ref Color32 reference14 = ref s_Output.Color32s[num10];
					ref Color32 reference15 = ref s_Output.Color32s[num9];
					ref Color32 reference16 = ref s_Output.Color32s[num8];
					reference13 = (reference14 = (reference15 = (reference16 = rgi.ColorDw)));
					num3++;
				}
			}
			s_Output.BillboardCount = num3;
			for (int k = 0; k < num2; k++)
			{
				RedGrassInstance rgi2 = tri_grass_list[k];
				int num12 = RandomCount(s_FullDensity * rgi2.Density * density * 0.333f, rgi2);
				for (int l = 0; l < num12; l++)
				{
					float num13 = rgi2.RandAttr.x * num4;
					Vector3 vector = rgi2.RandPos(l);
					for (int m = 0; m < 3; m++)
					{
						int num14 = num3 * 4;
						int num15 = num14 + 1;
						int num16 = num14 + 2;
						int num17 = num14 + 3;
						ref Vector3 reference17 = ref s_Output.Norms[num17];
						ref Vector3 reference18 = ref s_Output.Norms[num16];
						ref Vector3 reference19 = ref s_Output.Norms[num15];
						reference17 = (reference18 = (reference19 = (s_Output.Norms[num14] = vector)));
						Vector3 normal2 = rgi2.Normal;
						Vector3 normalized3 = (normal2 * 1.1f - up * 0.1f).normalized;
						Vector3 normalized4 = (normal2 * 0.5f + up * 0.5f).normalized;
						ref Vector2 reference20 = ref s_Output.UVs[num14];
						reference20 = new Vector2(normalized3.x, normalized3.z);
						ref Vector2 reference21 = ref s_Output.UVs[num16];
						ref Vector2 reference22 = ref s_Output.UVs[num15];
						reference21 = (reference22 = new Vector2(normalized4.x, normalized4.z));
						ref Vector2 reference23 = ref s_Output.UVs[num17];
						reference23 = s_Output.UVs[num14];
						ref Vector2 reference24 = ref s_Output.UV2s[num17];
						ref Vector2 reference25 = ref s_Output.UV2s[num16];
						ref Vector2 reference26 = ref s_Output.UV2s[num15];
						ref Vector2 reference27 = ref s_Output.UV2s[num14];
						reference24 = (reference25 = (reference26 = (reference27 = new Vector2((float)(rgi2.Prototype - 64) / 64f, (float)m * num5 + num13 + (rgi2.RandAttrs(m + 1).x - 0.5f) * num6))));
						ref Color32 reference28 = ref s_Output.Color32s[num17];
						ref Color32 reference29 = ref s_Output.Color32s[num16];
						ref Color32 reference30 = ref s_Output.Color32s[num15];
						ref Color32 reference31 = ref s_Output.Color32s[num14];
						reference28 = (reference29 = (reference30 = (reference31 = rgi2.ColorDw)));
						num3++;
					}
				}
			}
			s_Output.TriquadCount = num3 - s_Output.BillboardCount;
			s_Output.TotalVertCount = num3 * 4;
		}
		catch
		{
			Debug.Log("-----------------------------Grass Thread error");
		}
	}

	public static void ComputeParticleMesh(RedGrassInstance[] grass_array, int count)
	{
		s_Output.Reset();
		for (int i = 0; i < count; i++)
		{
			RedGrassInstance redGrassInstance = grass_array[i];
			int num = i * 4;
			int num2 = num + 1;
			int num3 = num + 2;
			int num4 = num + 3;
			ref Vector3 reference = ref s_Output.Norms[num4];
			ref Vector3 reference2 = ref s_Output.Norms[num3];
			ref Vector3 reference3 = ref s_Output.Norms[num2];
			ref Vector3 reference4 = ref s_Output.Norms[num];
			reference = (reference2 = (reference3 = (reference4 = redGrassInstance.RandPos(0))));
			Vector3 normal = redGrassInstance.Normal;
			ref Vector2 reference5 = ref s_Output.UVs[num4];
			ref Vector2 reference6 = ref s_Output.UVs[num3];
			ref Vector2 reference7 = ref s_Output.UVs[num2];
			ref Vector2 reference8 = ref s_Output.UVs[num];
			reference5 = (reference6 = (reference7 = (reference8 = new Vector2(normal.x, normal.z))));
			ref Vector2 reference9 = ref s_Output.UV2s[num4];
			ref Vector2 reference10 = ref s_Output.UV2s[num3];
			ref Vector2 reference11 = ref s_Output.UV2s[num2];
			ref Vector2 reference12 = ref s_Output.UV2s[num];
			reference9 = (reference10 = (reference11 = (reference12 = new Vector2((float)redGrassInstance.Prototype / 64f, 0f))));
			ref Color32 reference13 = ref s_Output.Color32s[num4];
			ref Color32 reference14 = ref s_Output.Color32s[num3];
			ref Color32 reference15 = ref s_Output.Color32s[num2];
			ref Color32 reference16 = ref s_Output.Color32s[num];
			reference13 = (reference14 = (reference15 = (reference16 = Color.white)));
		}
		s_Output.BillboardCount = count;
		s_Output.TriquadCount = 0;
		s_Output.TotalVertCount = count * 4;
	}
}
