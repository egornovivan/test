using System;
using System.Collections.Generic;
using UnityEngine;

public class GenCaveHM
{
	private const float trimLevel = 0.4f;

	private const float reverseTrimLevel = 1.6666666f;

	private CustomRandom myRand;

	private SimplexNoise myNoise;

	public int NumSmoothOps;

	private float[,] caveHeightmapSrc;

	public float[,] caveHeightmapFloor;

	public float[,] caveHeightmapCeiling;

	private int[] fourDirections = new int[8] { 1, 0, -1, 0, 0, 1, 0, -1 };

	private int caveShrinkFactor = 4;

	private int caveHMWidthX;

	private int caveHMWidthZ;

	private int cavePadding;

	public void init(CustomRandom _myrand, SimplexNoise _myNoise, int _caveWidthX, int _caveWidthZ, int _shrinkFactor, int _padding)
	{
		myRand = _myrand;
		myNoise = _myNoise;
		caveHMWidthX = _caveWidthX;
		caveHMWidthZ = _caveWidthZ;
		caveShrinkFactor = _shrinkFactor;
		cavePadding = _padding;
		if (_shrinkFactor == 0)
		{
			_shrinkFactor = 4;
		}
	}

	public int getCaveWidthX()
	{
		return caveHMWidthX;
	}

	public int getCaveWidthZ()
	{
		return caveHMWidthZ;
	}

	public void GenCave(int stopAtMiners, float MinerSpawnRate)
	{
		int num = caveHMWidthX / caveShrinkFactor;
		int num2 = caveHMWidthZ / caveShrinkFactor;
		List<IntVector2> list = new List<IntVector2>();
		list.Add(new IntVector2(num / 2, num2 / 2));
		byte[,] array = new byte[num, num2];
		int[] array2 = new int[4];
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		while (list.Count != 0 && num5 < stopAtMiners)
		{
			for (int num7 = list.Count - 1; num7 >= 0; num7--)
			{
				int num8 = 0;
				for (int i = 0; i < 4; i++)
				{
					num3 = list[num7].x + fourDirections[i << 1];
					num4 = list[num7].y + fourDirections[(i << 1) + 1];
					if (num3 >= cavePadding && num4 >= cavePadding && num3 < num - cavePadding && num4 < num2 - cavePadding && array[num3, num4] == 0)
					{
						array2[num8] = i;
						num8++;
					}
				}
				if (num8 == 0)
				{
					list.RemoveAt(num7);
				}
				else
				{
					int num9 = array2[Mathf.FloorToInt(myRand.Value * 0.999f * (float)num8)];
					num3 = list[num7].x + fourDirections[num9 << 1];
					num4 = list[num7].y + fourDirections[(num9 << 1) + 1];
					array[num3, num4] = 1;
					if (myRand.Value < MinerSpawnRate)
					{
						list.Add(new IntVector2(list[num7].x, list[num7].y));
						num5++;
					}
					list[num7].x = num3;
					list[num7].y = num4;
				}
			}
			num6++;
		}
		for (int j = 0; j < NumSmoothOps; j++)
		{
			smooth(array, num, num2);
		}
		Texture2D texture2D = new Texture2D(num, num2, TextureFormat.ARGB32, mipmap: false, linear: true);
		texture2D.filterMode = FilterMode.Bilinear;
		texture2D.wrapMode = TextureWrapMode.Clamp;
		Color[] array3 = new Color[num * num2];
		int num10 = 0;
		for (int k = 0; k < num2; k++)
		{
			for (int l = 0; l < num; l++)
			{
				float num11 = ((array[l, k] != 1) ? 0f : 1f);
				ref Color reference = ref array3[num10++];
				reference = new Color(num11, num11, num11, 1f);
			}
		}
		texture2D.SetPixels(array3);
		caveHeightmapSrc = new float[caveHMWidthX, caveHMWidthZ];
		for (int m = 0; m < caveHMWidthZ; m++)
		{
			for (int n = 0; n < caveHMWidthX; n++)
			{
				caveHeightmapSrc[n, m] = texture2D.GetPixelBilinear((float)n / (float)caveHMWidthX, (float)m / (float)caveHMWidthZ).r;
			}
		}
	}

	private void smooth(byte[,] map, int w, int h)
	{
		for (int i = 0; i < h; i++)
		{
			for (int j = 0; j < w; j++)
			{
				if (map[j, i] == 1)
				{
					continue;
				}
				int num = 0;
				for (int k = 0; k < 4; k++)
				{
					int num2 = j + fourDirections[k << 1];
					int num3 = i + fourDirections[(k << 1) + 1];
					if (num2 >= 0 && num3 >= 0 && num2 < w && num3 < h && map[num2, num3] == 1)
					{
						num++;
					}
				}
				if (num > 2)
				{
					map[j, i] = 1;
				}
			}
		}
	}

	public void ApplyNoise(int bl_max_iterations, float bl_length, float bl_power, int bv_max_iterations, float bv_length, float bv_power)
	{
		caveHeightmapCeiling = new float[caveHMWidthX, caveHMWidthZ];
		caveHeightmapFloor = new float[caveHMWidthX, caveHMWidthZ];
		Array.Copy(caveHeightmapSrc, caveHeightmapCeiling, caveHMWidthX * caveHMWidthZ);
		Array.Copy(caveHeightmapSrc, caveHeightmapFloor, caveHMWidthX * caveHMWidthZ);
		ApplyNoiseToHeightmap(bl_max_iterations, bl_length, bl_power, caveHeightmapFloor, 34.56f);
		ApplyNoiseToHeightmap(bv_max_iterations, bv_length, bv_power, caveHeightmapCeiling, 45.321f);
		NormalizeHM(caveHeightmapFloor, caveHMWidthX, caveHMWidthZ);
		NormalizeHM(caveHeightmapCeiling, caveHMWidthX, caveHMWidthZ);
	}

	private void ApplyNoiseToHeightmap(int max_iterations, float _length, float _power, float[,] hmHandle, float offset = 0f)
	{
		float num = _power;
		float num2 = _length;
		for (int i = 0; i < max_iterations; i++)
		{
			for (int j = 0; j < caveHMWidthZ; j++)
			{
				for (int k = 0; k < caveHMWidthX; k++)
				{
					float num3 = (float)k / (float)caveHMWidthX;
					float num4 = (float)j / (float)caveHMWidthZ;
					float num5 = (float)myNoise.Noise((num3 + offset) * num2, (num4 + offset) * num2);
					num5 *= caveHeightmapSrc[k, j];
					hmHandle[k, j] += num5 * num;
				}
			}
			num /= 2f;
			num2 *= 2f;
		}
	}

	private void NormalizeHM(float[,] hmHandle, int w, int h)
	{
		float num = 65535f;
		float num2 = -65535f;
		for (int i = 0; i < h; i++)
		{
			for (int j = 0; j < w; j++)
			{
				if (hmHandle[j, i] < num)
				{
					num = hmHandle[j, i];
				}
				else if (hmHandle[j, i] > num2)
				{
					num2 = hmHandle[j, i];
				}
			}
		}
		float num3 = num2 - num;
		for (int k = 0; k < h; k++)
		{
			for (int l = 0; l < w; l++)
			{
				hmHandle[l, k] = (hmHandle[l, k] - num) / num3;
			}
		}
	}
}
