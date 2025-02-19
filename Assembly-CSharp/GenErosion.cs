using UnityEngine;

public class GenErosion
{
	private float[,] hmAverage;

	private float[,] heightmap;

	public int HMWidth;

	public int HMHeight;

	public int NumDrops = 1;

	private int DropletX;

	private int DropletY;

	private float velocity;

	private float carriedSoil;

	private float maxCapacity;

	private int[] GradientDirection = new int[8] { 1, 0, -1, 0, 0, 1, 0, -1 };

	public int avgfilterSize = 2;

	private float[,] GaussianCoeffs = new float[5, 5]
	{
		{ 0.00079f, 0.0066f, 0.0133f, 0.0066f, 0.00079f },
		{ 0.0066f, 0.055f, 0.111f, 0.055f, 0.0066f },
		{ 0.0133f, 0.111f, 0.2251f, 0.111f, 0.0133f },
		{ 0.0066f, 0.055f, 0.111f, 0.055f, 0.0066f },
		{ 0.00079f, 0.0066f, 0.0133f, 0.0066f, 0.00079f }
	};

	public GenErosion(int w, int h, float[,] _heightmap)
	{
		HMWidth = w;
		HMHeight = h;
		heightmap = _heightmap;
	}

	public void ApplyErosion()
	{
		int num = HMWidth - avgfilterSize * 2;
		int num2 = 1000;
		int num3 = Mathf.RoundToInt(Mathf.Sqrt(num2));
		int num4 = Mathf.FloorToInt(num / num3);
		for (int i = 0; i < num2; i++)
		{
			int num5 = Mathf.FloorToInt(Random.value * (float)num - 20f) + 10;
			int num6 = Mathf.FloorToInt(Random.value * (float)num - 20f) + 10;
			num5 = i % num3 * num4;
			num6 = i / num3 * num4;
			num5 += 10;
			num6 += 10;
			DropletX = num5;
			DropletY = num6;
			velocity = 1f;
			int num7 = 0;
			while (velocity > 0.001f)
			{
				FlowDroplet();
				num7++;
			}
		}
	}

	private float sampleAverageHeight(int x, int y)
	{
		float num = 0f;
		for (int i = 1; i < avgfilterSize; i++)
		{
			for (int j = 1; j < avgfilterSize; j++)
			{
				num += heightmap[x + j, y + i];
				num += heightmap[x + j, y - i];
				num += heightmap[x - j, y + i];
				num += heightmap[x - j, y - i];
			}
		}
		for (int k = 0; k < avgfilterSize; k++)
		{
			num += heightmap[x + k, y];
			num += heightmap[x - k, y];
			num += heightmap[x, y + k];
			num += heightmap[x, y - k];
		}
		num -= heightmap[x, y] * 3f;
		return num / Mathf.Pow(avgfilterSize * 2 - 1, 2f);
	}

	public void initAverageMap()
	{
		hmAverage = new float[HMWidth, HMHeight];
		for (int i = avgfilterSize; i < HMHeight - avgfilterSize; i++)
		{
			for (int j = avgfilterSize; j < HMWidth - avgfilterSize; j++)
			{
				float num = 0f;
				for (int k = 1; k < avgfilterSize; k++)
				{
					for (int l = 1; l < avgfilterSize; l++)
					{
						num += heightmap[j + l, i + k];
						num += heightmap[j + l, i - k];
						num += heightmap[j - l, i + k];
						num += heightmap[j - l, i - k];
					}
				}
				for (int m = 0; m < avgfilterSize; m++)
				{
					num += heightmap[j + m, i];
					num += heightmap[j - m, i];
					num += heightmap[j, i + m];
					num += heightmap[j, i - m];
				}
				num -= heightmap[j, i] * 3f;
				num /= Mathf.Pow(avgfilterSize * 2 - 1, 2f);
				hmAverage[j, i] = num;
			}
		}
	}

	private void ApplyFilter(int x, int y, float strength)
	{
		for (int i = -2; i < 3; i++)
		{
			for (int j = -2; j < 3; j++)
			{
				heightmap[x + j, y + i] += strength * GaussianCoeffs[j + 2, i + 2];
			}
		}
	}

	private void FlowDroplet()
	{
		int num = 1;
		if (DropletX < 2 + num || DropletX >= HMWidth - 2 - num || DropletY < 2 + num || DropletY >= HMHeight - 2 - num)
		{
			velocity = 0f;
			return;
		}
		float[] array = new float[4];
		int num2 = 0;
		int num3 = 0;
		int num4 = -1;
		float num5 = 0f;
		float num6 = sampleAverageHeight(DropletX, DropletY);
		for (int i = 0; i < 4; i++)
		{
			array[i] = num6 - sampleAverageHeight(DropletX + GradientDirection[i * 2] * num, DropletY + GradientDirection[i * 2 + 1] * num);
			if (array[i] > num5 && array[i] > 0f)
			{
				num5 = array[i];
				num4 = i;
				num2 = GradientDirection[i * 2];
				num3 = GradientDirection[i * 2 + 1];
			}
		}
		if (num4 < 0)
		{
			velocity = 0f;
		}
		else
		{
			float num7 = num6;
			float num8 = array[num4];
			if (num8 < num7 - 0.1f)
			{
				float num9 = -0.3f;
				if (num7 + num9 * 2f > num8)
				{
					ApplyFilter(DropletX, DropletY, num9);
					carriedSoil += num9;
				}
			}
		}
		DropletX += num2;
		DropletY += num3;
	}
}
