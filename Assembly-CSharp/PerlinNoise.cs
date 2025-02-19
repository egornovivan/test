using System;
using System.Runtime.CompilerServices;

public class PerlinNoise
{
	private const int YSTEP = 16;

	private const int YSTEP2 = 32;

	private const int YSTEP3 = 48;

	private const int YSHIFT = 4;

	private const int ZSTEP = 256;

	private const int ZSTEP2 = 512;

	private const int ZSTEP3 = 768;

	private const int ZSHIFT = 8;

	private float[] Randoms = new float[65536];

	public PerlinNoise()
	{
		Init();
	}

	public PerlinNoise(int seed)
	{
		Init(seed);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static float Lerp(float Y0, float Y1, float Y2, float Y3, float Blend)
	{
		float num = Y3 - Y2 - Y0 + Y1;
		float num2 = Y0 - Y1 - num;
		float num3 = Y2 - Y0;
		float num4 = Blend * Blend;
		return num * Blend * num4 + num2 * num4 + num3 * Blend + Y1;
	}

	private void Init()
	{
		Random random = new Random();
		for (int i = 0; i < 65536; i++)
		{
			Randoms[i] = (float)(random.NextDouble() * 2.0 - 1.0);
		}
	}

	private void Init(int seed)
	{
		Random random = new Random(seed);
		for (int i = 0; i < 65536; i++)
		{
			Randoms[i] = (float)(random.NextDouble() * 2.0 - 1.0);
		}
	}

	public float Noise1DFBM(float X, int Octaves)
	{
		float num = 0f;
		float num2 = 0.5f;
		if (X < 0f)
		{
			X = 0f - X;
		}
		int num3 = (int)X;
		float num4 = X - (float)num3;
		for (int i = 0; i < Octaves; i++)
		{
			float y = Randoms[num3 & 0xFFFF];
			float y2 = Randoms[(num3 + 1) & 0xFFFF];
			float y3 = Randoms[(num3 + 2) & 0xFFFF];
			float y4 = Randoms[(num3 + 3) & 0xFFFF];
			num += Lerp(y, y2, y3, y4, num4) * num2;
			num2 *= 0.5f;
			num3 <<= 1;
			num4 *= 2f;
			if (num4 > 1f)
			{
				num3++;
				num4 -= 1f;
			}
		}
		return num;
	}

	public float Noise2DFBM(float X, float Y, int Octaves)
	{
		float num = 0f;
		float num2 = 0.5f;
		int num3 = (int)X;
		int num4 = (int)Y;
		if (X < 0f)
		{
			num3--;
		}
		if (Y < 0f)
		{
			num4--;
		}
		float num5 = X - (float)num3;
		float num6 = Y - (float)num4;
		for (int i = 0; i < Octaves; i++)
		{
			int num7 = num3 + (num4 << 4);
			float y = Lerp(Randoms[num7 & 0xFFFF], Randoms[(num7 + 1) & 0xFFFF], Randoms[(num7 + 2) & 0xFFFF], Randoms[(num7 + 3) & 0xFFFF], num5);
			float y2 = Lerp(Randoms[(num7 + 16) & 0xFFFF], Randoms[(num7 + 1 + 16) & 0xFFFF], Randoms[(num7 + 2 + 16) & 0xFFFF], Randoms[(num7 + 3 + 16) & 0xFFFF], num5);
			float y3 = Lerp(Randoms[(num7 + 32) & 0xFFFF], Randoms[(num7 + 1 + 32) & 0xFFFF], Randoms[(num7 + 2 + 32) & 0xFFFF], Randoms[(num7 + 3 + 32) & 0xFFFF], num5);
			float y4 = Lerp(Randoms[(num7 + 48) & 0xFFFF], Randoms[(num7 + 1 + 48) & 0xFFFF], Randoms[(num7 + 2 + 48) & 0xFFFF], Randoms[(num7 + 3 + 48) & 0xFFFF], num5);
			num += Lerp(y, y2, y3, y4, num6) * num2;
			num2 *= 0.5f;
			num3 <<= 1;
			num4 <<= 1;
			num5 *= 2f;
			num6 *= 2f;
			if (num5 > 1f)
			{
				num3++;
				num5 -= 1f;
			}
			if (num6 > 1f)
			{
				num4++;
				num6 -= 1f;
			}
		}
		return num;
	}

	public float Noise3DFBM(float X, float Y, float Z, int Octaves)
	{
		float num = 0f;
		float num2 = 0.5f;
		int num3 = (int)X;
		int num4 = (int)Y;
		int num5 = (int)Z;
		if (X < 0f)
		{
			num3--;
		}
		if (Y < 0f)
		{
			num4--;
		}
		if (Z < 0f)
		{
			num5--;
		}
		float num6 = X - (float)num3;
		float num7 = Y - (float)num4;
		float num8 = Z - (float)num5;
		for (int i = 0; i < Octaves; i++)
		{
			int num9 = num3 + (num4 << 4) + (num5 << 8);
			float y = Lerp(Randoms[num9 & 0xFFFF], Randoms[(num9 + 1) & 0xFFFF], Randoms[(num9 + 2) & 0xFFFF], Randoms[(num9 + 3) & 0xFFFF], num6);
			float y2 = Lerp(Randoms[(num9 + 16) & 0xFFFF], Randoms[(num9 + 1 + 16) & 0xFFFF], Randoms[(num9 + 2 + 16) & 0xFFFF], Randoms[(num9 + 3 + 16) & 0xFFFF], num6);
			float y3 = Lerp(Randoms[(num9 + 32) & 0xFFFF], Randoms[(num9 + 1 + 32) & 0xFFFF], Randoms[(num9 + 2 + 32) & 0xFFFF], Randoms[(num9 + 3 + 32) & 0xFFFF], num6);
			float y4 = Lerp(Randoms[(num9 + 48) & 0xFFFF], Randoms[(num9 + 1 + 48) & 0xFFFF], Randoms[(num9 + 2 + 48) & 0xFFFF], Randoms[(num9 + 3 + 48) & 0xFFFF], num6);
			num9 += 256;
			float y5 = Lerp(Randoms[num9 & 0xFFFF], Randoms[(num9 + 1) & 0xFFFF], Randoms[(num9 + 2) & 0xFFFF], Randoms[(num9 + 3) & 0xFFFF], num6);
			float y6 = Lerp(Randoms[(num9 + 16) & 0xFFFF], Randoms[(num9 + 1 + 16) & 0xFFFF], Randoms[(num9 + 2 + 16) & 0xFFFF], Randoms[(num9 + 3 + 16) & 0xFFFF], num6);
			float y7 = Lerp(Randoms[(num9 + 32) & 0xFFFF], Randoms[(num9 + 1 + 32) & 0xFFFF], Randoms[(num9 + 2 + 32) & 0xFFFF], Randoms[(num9 + 3 + 32) & 0xFFFF], num6);
			float y8 = Lerp(Randoms[(num9 + 48) & 0xFFFF], Randoms[(num9 + 1 + 48) & 0xFFFF], Randoms[(num9 + 2 + 48) & 0xFFFF], Randoms[(num9 + 3 + 48) & 0xFFFF], num6);
			num9 += 256;
			float y9 = Lerp(Randoms[num9 & 0xFFFF], Randoms[(num9 + 1) & 0xFFFF], Randoms[(num9 + 2) & 0xFFFF], Randoms[(num9 + 3) & 0xFFFF], num6);
			float y10 = Lerp(Randoms[(num9 + 16) & 0xFFFF], Randoms[(num9 + 1 + 16) & 0xFFFF], Randoms[(num9 + 2 + 16) & 0xFFFF], Randoms[(num9 + 3 + 16) & 0xFFFF], num6);
			float y11 = Lerp(Randoms[(num9 + 32) & 0xFFFF], Randoms[(num9 + 1 + 32) & 0xFFFF], Randoms[(num9 + 2 + 32) & 0xFFFF], Randoms[(num9 + 3 + 32) & 0xFFFF], num6);
			float y12 = Lerp(Randoms[(num9 + 48) & 0xFFFF], Randoms[(num9 + 1 + 48) & 0xFFFF], Randoms[(num9 + 2 + 48) & 0xFFFF], Randoms[(num9 + 3 + 48) & 0xFFFF], num6);
			num9 += 256;
			float y13 = Lerp(Randoms[num9 & 0xFFFF], Randoms[(num9 + 1) & 0xFFFF], Randoms[(num9 + 2) & 0xFFFF], Randoms[(num9 + 3) & 0xFFFF], num6);
			float y14 = Lerp(Randoms[(num9 + 16) & 0xFFFF], Randoms[(num9 + 1 + 16) & 0xFFFF], Randoms[(num9 + 2 + 16) & 0xFFFF], Randoms[(num9 + 3 + 16) & 0xFFFF], num6);
			float y15 = Lerp(Randoms[(num9 + 32) & 0xFFFF], Randoms[(num9 + 1 + 32) & 0xFFFF], Randoms[(num9 + 2 + 32) & 0xFFFF], Randoms[(num9 + 3 + 32) & 0xFFFF], num6);
			float y16 = Lerp(Randoms[(num9 + 48) & 0xFFFF], Randoms[(num9 + 1 + 48) & 0xFFFF], Randoms[(num9 + 2 + 48) & 0xFFFF], Randoms[(num9 + 3 + 48) & 0xFFFF], num6);
			float y17 = Lerp(y, y2, y3, y4, num7);
			float y18 = Lerp(y5, y6, y7, y8, num7);
			float y19 = Lerp(y9, y10, y11, y12, num7);
			float y20 = Lerp(y13, y14, y15, y16, num7);
			num += Lerp(y17, y18, y19, y20, num8) * num2;
			num2 *= 0.5f;
			num3 <<= 1;
			num4 <<= 1;
			num5 <<= 1;
			num6 *= 2f;
			num7 *= 2f;
			num8 *= 2f;
			if (num6 > 1f)
			{
				num3++;
				num6 -= 1f;
			}
			if (num7 > 1f)
			{
				num4++;
				num7 -= 1f;
			}
			if (num8 > 1f)
			{
				num5++;
				num8 -= 1f;
			}
		}
		return num;
	}
}
