using UnityEngine;

public class GraphPlotter
{
	public enum GraphShapeType
	{
		TopAndBottom,
		Top,
		Grid
	}

	public Color BackgroundColor = new Color(0f, 0f, 0f, 0f);

	public Color32 TopColor;

	public Color32 BottomColor;

	public int TextureWidth = 16;

	public int TextureHeight = 16;

	public bool NormalizeGraph = true;

	public void PlotGraph(float[] data, float[] plotData, Color32[] colors)
	{
		int num = data.Length;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = Mathf.Max(1f, (float)num / (float)TextureWidth);
		float num5 = 0f;
		int num6;
		for (num6 = 0; num6 < num; num6 = (int)num5)
		{
			plotData[num6] = data[num6];
			num5 += num4;
		}
		if (NormalizeGraph)
		{
			num5 = 0f;
			for (num6 = 0; num6 < num; num6 = (int)num5)
			{
				float num7 = plotData[num6];
				if (num7 > 0f)
				{
					if (num7 > num3)
					{
						num3 = num7;
					}
				}
				else
				{
					num7 = 0f - num7;
					if (num7 > num2)
					{
						num2 = num7;
					}
				}
				num5 += num4;
			}
			num5 = 0f;
			for (num6 = 0; num6 < num; num6 = (int)num5)
			{
				float num8 = plotData[num6];
				if (num8 > 0f)
				{
					if (num3 != 0f)
					{
						num8 /= num3;
					}
				}
				else if (num2 != 0f)
				{
					num8 /= num2;
				}
				plotData[num6] = num8;
				num5 += num4;
			}
		}
		int num9 = TextureHeight / 2;
		int num10 = num9 - 1;
		int num11 = 0;
		num5 = 0f;
		num6 = 0;
		while (num6 < num && num11 < TextureWidth)
		{
			float num12 = plotData[num6];
			int num13 = (int)(num12 * (float)num9);
			for (int num14 = num9; num14 >= 0; num14--)
			{
				float t = (float)num14 / (float)num9;
				if (num13 > 0 && num14 <= num13)
				{
					SetColors(colors, num11, num9 + num14, Color.Lerp(BottomColor, TopColor, t));
					SetColors(colors, num11, num10 - num14, BackgroundColor);
				}
				else if (num13 < 0 && num14 <= -num13)
				{
					SetColors(colors, num11, num9 + num14, BackgroundColor);
					SetColors(colors, num11, num10 - num14, Color.Lerp(BottomColor, TopColor, t));
				}
				else
				{
					SetColors(colors, num11, num10 + num14, BackgroundColor);
					SetColors(colors, num11, num9 - num14, BackgroundColor);
				}
			}
			num5 += num4;
			num6 = (int)num5;
			num11++;
		}
	}

	public void PlotGraph2(float[] data, float[] plotData, Color32[] colors)
	{
		int num = data.Length;
		float num2 = (float)num / (float)TextureWidth;
		float num3 = 0f;
		int num4;
		for (num4 = 0; num4 < num; num4 = (int)num3)
		{
			plotData[num4] = Mathf.Abs(data[num4]);
			num3 += num2;
		}
		if (NormalizeGraph)
		{
			float num5 = 0f;
			num3 = 0f;
			for (num4 = 0; num4 < num; num4 = (int)num3)
			{
				float num6 = plotData[num4];
				if (num6 > num5)
				{
					num5 = num6;
				}
				num3 += num2;
			}
			num3 = 0f;
			for (num4 = 0; num4 < num; num4 = (int)num3)
			{
				float num7 = plotData[num4];
				if (num5 != 0f)
				{
					num7 /= num5;
				}
				plotData[num4] = num7;
				num3 += num2;
			}
		}
		int num8 = 0;
		num3 = 0f;
		num4 = 0;
		while (num4 < num && num8 < TextureWidth)
		{
			float num9 = plotData[num4];
			int num10 = (int)(num9 * (float)TextureHeight);
			for (int num11 = TextureHeight - 1; num11 >= 0; num11--)
			{
				float t = (float)num11 / (float)TextureHeight;
				if (num10 > 0 && num11 <= num10)
				{
					SetColors(colors, num8, num11, Color32.Lerp(BottomColor, TopColor, t));
				}
				else
				{
					SetColors(colors, num8, num11, BackgroundColor);
				}
			}
			num3 += num2;
			num4 = (int)num3;
			num8++;
		}
	}

	public void PlotGraph3(float[] data, float[] plotData, int xGridCount, int yGridCount, int gridBorderX, int gridBorderY, Color32[] colors)
	{
		int num = data.Length;
		plotData = new float[xGridCount];
		int num2 = num / xGridCount;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		for (int i = 0; i < xGridCount; i++)
		{
			num3 = 0f;
			for (int j = i * num2; j < (i + 1) * num2; j++)
			{
				num3 += Mathf.Abs(data[j]);
			}
			num4 = (plotData[i] = num3 / (float)num2);
			if (NormalizeGraph && num4 > num5)
			{
				num5 = num4;
			}
		}
		if (NormalizeGraph)
		{
			for (int k = 0; k < xGridCount; k++)
			{
				float num6 = plotData[k];
				if (num5 != 0f)
				{
					num6 /= num5;
				}
				plotData[k] = num6;
			}
		}
		int num7 = (TextureWidth - gridBorderX * (xGridCount - 1)) / xGridCount;
		int num8 = (TextureHeight - gridBorderY * (yGridCount - 1)) / yGridCount;
		int num9 = 0;
		int num10 = 0;
		int num11 = 0;
		bool flag = false;
		bool flag2 = false;
		float num12 = 0f;
		int num13 = yGridCount * num8;
		int num14 = 0;
		int num15 = 0;
		int num16 = 0;
		for (int l = 0; l < xGridCount * 2 - 1; l++)
		{
			flag = l % 2 == 0;
			num14 = ((!flag) ? gridBorderX : num7);
			int num17 = 0;
			if (flag)
			{
				num12 = plotData[num11];
				num17 = (int)(num12 * (float)num13);
				num16 = num13;
				num11++;
			}
			num10 = TextureHeight - 1;
			for (int m = 0; m < yGridCount * 2 - 1; m++)
			{
				flag2 = m % 2 == 0;
				num15 = ((!flag2) ? gridBorderY : num8);
				if (flag2 && flag)
				{
					for (int n = 0; n < num15; n++)
					{
						for (int num18 = 0; num18 < num14; num18++)
						{
							float t = (float)num16 / (float)num13;
							if (num17 > 0 && num16 <= num17)
							{
								SetColors(colors, num9 + num18, num10, Color32.Lerp(BottomColor, TopColor, t));
							}
							else
							{
								SetColors(colors, num9 + num18, num10, BackgroundColor);
							}
						}
						num10--;
						num16--;
					}
					continue;
				}
				for (int num19 = 0; num19 < num15; num19++)
				{
					for (int num20 = 0; num20 < num14; num20++)
					{
						SetColors(colors, num9 + num20, num10, BackgroundColor);
					}
					num10--;
				}
			}
			while (num10 >= 0)
			{
				for (int num21 = 0; num21 < num14; num21++)
				{
					SetColors(colors, num9 + num21, num10, BackgroundColor);
				}
				num10--;
			}
			num9 += num14;
		}
		if (num9 >= TextureWidth)
		{
			return;
		}
		for (num10 = TextureHeight - 1; num10 >= 0; num10--)
		{
			for (int num22 = num9; num22 < TextureWidth; num22++)
			{
				SetColors(colors, num22, num10, BackgroundColor);
			}
		}
	}

	private void SetColors(Color32[] colors, int x, int y, Color32 color)
	{
		if (colors != null)
		{
			int num = x + y * TextureWidth;
			if (num >= 0 && num < TextureHeight * TextureWidth)
			{
				colors[num] = color;
			}
		}
	}
}
