using System;
using UnityEngine;

public class SimplexNoise
{
	public class Grad
	{
		public double x;

		public double y;

		public double z;

		public double w;

		public Grad(double x, double y, double z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public Grad(double x, double y, double z, double w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}
	}

	private static Grad[] grad3 = new Grad[12]
	{
		new Grad(1.0, 1.0, 0.0),
		new Grad(-1.0, 1.0, 0.0),
		new Grad(1.0, -1.0, 0.0),
		new Grad(-1.0, -1.0, 0.0),
		new Grad(1.0, 0.0, 1.0),
		new Grad(-1.0, 0.0, 1.0),
		new Grad(1.0, 0.0, -1.0),
		new Grad(-1.0, 0.0, -1.0),
		new Grad(0.0, 1.0, 1.0),
		new Grad(0.0, -1.0, 1.0),
		new Grad(0.0, 1.0, -1.0),
		new Grad(0.0, -1.0, -1.0)
	};

	private static Grad[] grad4 = new Grad[32]
	{
		new Grad(0.0, 1.0, 1.0, 1.0),
		new Grad(0.0, 1.0, 1.0, -1.0),
		new Grad(0.0, 1.0, -1.0, 1.0),
		new Grad(0.0, 1.0, -1.0, -1.0),
		new Grad(0.0, -1.0, 1.0, 1.0),
		new Grad(0.0, -1.0, 1.0, -1.0),
		new Grad(0.0, -1.0, -1.0, 1.0),
		new Grad(0.0, -1.0, -1.0, -1.0),
		new Grad(1.0, 0.0, 1.0, 1.0),
		new Grad(1.0, 0.0, 1.0, -1.0),
		new Grad(1.0, 0.0, -1.0, 1.0),
		new Grad(1.0, 0.0, -1.0, -1.0),
		new Grad(-1.0, 0.0, 1.0, 1.0),
		new Grad(-1.0, 0.0, 1.0, -1.0),
		new Grad(-1.0, 0.0, -1.0, 1.0),
		new Grad(-1.0, 0.0, -1.0, -1.0),
		new Grad(1.0, 1.0, 0.0, 1.0),
		new Grad(1.0, 1.0, 0.0, -1.0),
		new Grad(1.0, -1.0, 0.0, 1.0),
		new Grad(1.0, -1.0, 0.0, -1.0),
		new Grad(-1.0, 1.0, 0.0, 1.0),
		new Grad(-1.0, 1.0, 0.0, -1.0),
		new Grad(-1.0, -1.0, 0.0, 1.0),
		new Grad(-1.0, -1.0, 0.0, -1.0),
		new Grad(1.0, 1.0, 1.0, 0.0),
		new Grad(1.0, 1.0, -1.0, 0.0),
		new Grad(1.0, -1.0, 1.0, 0.0),
		new Grad(1.0, -1.0, -1.0, 0.0),
		new Grad(-1.0, 1.0, 1.0, 0.0),
		new Grad(-1.0, 1.0, -1.0, 0.0),
		new Grad(-1.0, -1.0, 1.0, 0.0),
		new Grad(-1.0, -1.0, -1.0, 0.0)
	};

	private static short[] p = new short[256]
	{
		151, 160, 137, 91, 90, 15, 131, 13, 201, 95,
		96, 53, 194, 233, 7, 225, 140, 36, 103, 30,
		69, 142, 8, 99, 37, 240, 21, 10, 23, 190,
		6, 148, 247, 120, 234, 75, 0, 26, 197, 62,
		94, 252, 219, 203, 117, 35, 11, 32, 57, 177,
		33, 88, 237, 149, 56, 87, 174, 20, 125, 136,
		171, 168, 68, 175, 74, 165, 71, 134, 139, 48,
		27, 166, 77, 146, 158, 231, 83, 111, 229, 122,
		60, 211, 133, 230, 220, 105, 92, 41, 55, 46,
		245, 40, 244, 102, 143, 54, 65, 25, 63, 161,
		1, 216, 80, 73, 209, 76, 132, 187, 208, 89,
		18, 169, 200, 196, 135, 130, 116, 188, 159, 86,
		164, 100, 109, 198, 173, 186, 3, 64, 52, 217,
		226, 250, 124, 123, 5, 202, 38, 147, 118, 126,
		255, 82, 85, 212, 207, 206, 59, 227, 47, 16,
		58, 17, 182, 189, 28, 42, 223, 183, 170, 213,
		119, 248, 152, 2, 44, 154, 163, 70, 221, 153,
		101, 155, 167, 43, 172, 9, 129, 22, 39, 253,
		19, 98, 108, 110, 79, 113, 224, 232, 178, 185,
		112, 104, 218, 246, 97, 228, 251, 34, 242, 193,
		238, 210, 144, 12, 191, 179, 162, 241, 81, 51,
		145, 235, 249, 14, 239, 107, 49, 192, 214, 31,
		181, 199, 106, 157, 184, 84, 204, 176, 115, 121,
		50, 45, 127, 4, 150, 254, 138, 236, 205, 93,
		222, 114, 67, 29, 24, 72, 243, 141, 128, 195,
		78, 66, 215, 61, 156, 180
	};

	private static double F2 = 0.5 * ((double)Mathf.Sqrt(3f) - 1.0);

	private static double G2 = (3.0 - (double)Mathf.Sqrt(3f)) / 6.0;

	private static double F3 = 1.0 / 3.0;

	private static double G3 = 1.0 / 6.0;

	private static double F4 = ((double)Mathf.Sqrt(5f) - 1.0) / 4.0;

	private static double G4 = (5.0 - (double)Mathf.Sqrt(5f)) / 20.0;

	private short[] perm = new short[512];

	private short[] permMod12 = new short[512];

	public SimplexNoise()
	{
		InitMember();
	}

	public SimplexNoise(long seed)
	{
		InitMember(seed);
	}

	private static int fastfloor(double x)
	{
		int num = (int)x;
		return (!(x < (double)num)) ? num : (num - 1);
	}

	private static double dot(Grad g, double x, double y)
	{
		return g.x * x + g.y * y;
	}

	private static double dot(Grad g, double x, double y, double z)
	{
		return g.x * x + g.y * y + g.z * z;
	}

	private static double dot(Grad g, double x, double y, double z, double w)
	{
		return g.x * x + g.y * y + g.z * z + g.w * w;
	}

	private void InitMember()
	{
		for (int i = 0; i < 512; i++)
		{
			perm[i] = p[i & 0xFF];
			permMod12[i] = (short)(perm[i] % 12);
		}
	}

	private void InitMember(long seed)
	{
		System.Random random = new System.Random((int)seed);
		for (int i = 0; i < 256; i++)
		{
			p[i] = (short)i;
		}
		for (int j = 0; j < 256; j++)
		{
			int num = random.Next(0, 255);
			int num2 = p[j];
			p[j] = p[num];
			p[num] = (short)num2;
		}
		for (int k = 0; k < 512; k++)
		{
			perm[k] = p[k & 0xFF];
			permMod12[k] = (short)(perm[k] % 12);
		}
	}

	public double Noise(double xin)
	{
		int num = fastfloor(xin);
		int num2 = num + 1;
		double num3 = xin - (double)num;
		double num4 = num3 - 1.0;
		double num5 = 1.0 - num3 * num3;
		num5 *= num5;
		double num6 = num5 * num5 * grad3[perm[num & 0xFF] & 7].x * num3;
		double num7 = 1.0 - num4 * num4;
		num7 *= num7;
		double num8 = num7 * num7 * grad3[perm[num2 & 0xFF] & 7].x * num4;
		return (num6 + num8 + 0.076368899) * 0.4073517;
	}

	public double Noise(double xin, double yin)
	{
		double num = (xin + yin) * F2;
		int num2 = fastfloor(xin + num);
		int num3 = fastfloor(yin + num);
		double num4 = (double)(num2 + num3) * G2;
		double num5 = (double)num2 - num4;
		double num6 = (double)num3 - num4;
		double num7 = xin - num5;
		double num8 = yin - num6;
		int num9;
		int num10;
		if (num7 > num8)
		{
			num9 = 1;
			num10 = 0;
		}
		else
		{
			num9 = 0;
			num10 = 1;
		}
		double num11 = num7 - (double)num9 + G2;
		double num12 = num8 - (double)num10 + G2;
		double num13 = num7 - 1.0 + 2.0 * G2;
		double num14 = num8 - 1.0 + 2.0 * G2;
		int num15 = num2 & 0xFF;
		int num16 = num3 & 0xFF;
		int num17 = permMod12[num15 + perm[num16]];
		int num18 = permMod12[num15 + num9 + perm[num16 + num10]];
		int num19 = permMod12[num15 + 1 + perm[num16 + 1]];
		double num20 = 0.5 - num7 * num7 - num8 * num8;
		double num21;
		if (num20 < 0.0)
		{
			num21 = 0.0;
		}
		else
		{
			num20 *= num20;
			num21 = num20 * num20 * dot(grad3[num17], num7, num8);
		}
		double num22 = 0.5 - num11 * num11 - num12 * num12;
		double num23;
		if (num22 < 0.0)
		{
			num23 = 0.0;
		}
		else
		{
			num22 *= num22;
			num23 = num22 * num22 * dot(grad3[num18], num11, num12);
		}
		double num24 = 0.5 - num13 * num13 - num14 * num14;
		double num25;
		if (num24 < 0.0)
		{
			num25 = 0.0;
		}
		else
		{
			num24 *= num24;
			num25 = num24 * num24 * dot(grad3[num19], num13, num14);
		}
		return 70.0 * (num21 + num23 + num25);
	}

	public double Noise(double xin, double yin, double zin)
	{
		double num = (xin + yin + zin) * F3;
		int num2 = fastfloor(xin + num);
		int num3 = fastfloor(yin + num);
		int num4 = fastfloor(zin + num);
		double num5 = (double)(num2 + num3 + num4) * G3;
		double num6 = (double)num2 - num5;
		double num7 = (double)num3 - num5;
		double num8 = (double)num4 - num5;
		double num9 = xin - num6;
		double num10 = yin - num7;
		double num11 = zin - num8;
		int num12;
		int num13;
		int num14;
		int num15;
		int num16;
		int num17;
		if (num9 >= num10)
		{
			if (num10 >= num11)
			{
				num12 = 1;
				num13 = 0;
				num14 = 0;
				num15 = 1;
				num16 = 1;
				num17 = 0;
			}
			else if (num9 >= num11)
			{
				num12 = 1;
				num13 = 0;
				num14 = 0;
				num15 = 1;
				num16 = 0;
				num17 = 1;
			}
			else
			{
				num12 = 0;
				num13 = 0;
				num14 = 1;
				num15 = 1;
				num16 = 0;
				num17 = 1;
			}
		}
		else if (num10 < num11)
		{
			num12 = 0;
			num13 = 0;
			num14 = 1;
			num15 = 0;
			num16 = 1;
			num17 = 1;
		}
		else if (num9 < num11)
		{
			num12 = 0;
			num13 = 1;
			num14 = 0;
			num15 = 0;
			num16 = 1;
			num17 = 1;
		}
		else
		{
			num12 = 0;
			num13 = 1;
			num14 = 0;
			num15 = 1;
			num16 = 1;
			num17 = 0;
		}
		double num18 = num9 - (double)num12 + G3;
		double num19 = num10 - (double)num13 + G3;
		double num20 = num11 - (double)num14 + G3;
		double num21 = num9 - (double)num15 + 2.0 * G3;
		double num22 = num10 - (double)num16 + 2.0 * G3;
		double num23 = num11 - (double)num17 + 2.0 * G3;
		double num24 = num9 - 1.0 + 3.0 * G3;
		double num25 = num10 - 1.0 + 3.0 * G3;
		double num26 = num11 - 1.0 + 3.0 * G3;
		int num27 = num2 & 0xFF;
		int num28 = num3 & 0xFF;
		int num29 = num4 & 0xFF;
		int num30 = permMod12[num27 + perm[num28 + perm[num29]]];
		int num31 = permMod12[num27 + num12 + perm[num28 + num13 + perm[num29 + num14]]];
		int num32 = permMod12[num27 + num15 + perm[num28 + num16 + perm[num29 + num17]]];
		int num33 = permMod12[num27 + 1 + perm[num28 + 1 + perm[num29 + 1]]];
		double num34 = 0.6 - num9 * num9 - num10 * num10 - num11 * num11;
		double num35;
		if (num34 < 0.0)
		{
			num35 = 0.0;
		}
		else
		{
			num34 *= num34;
			num35 = num34 * num34 * dot(grad3[num30], num9, num10, num11);
		}
		double num36 = 0.6 - num18 * num18 - num19 * num19 - num20 * num20;
		double num37;
		if (num36 < 0.0)
		{
			num37 = 0.0;
		}
		else
		{
			num36 *= num36;
			num37 = num36 * num36 * dot(grad3[num31], num18, num19, num20);
		}
		double num38 = 0.6 - num21 * num21 - num22 * num22 - num23 * num23;
		double num39;
		if (num38 < 0.0)
		{
			num39 = 0.0;
		}
		else
		{
			num38 *= num38;
			num39 = num38 * num38 * dot(grad3[num32], num21, num22, num23);
		}
		double num40 = 0.6 - num24 * num24 - num25 * num25 - num26 * num26;
		double num41;
		if (num40 < 0.0)
		{
			num41 = 0.0;
		}
		else
		{
			num40 *= num40;
			num41 = num40 * num40 * dot(grad3[num33], num24, num25, num26);
		}
		return 32.0 * (num35 + num37 + num39 + num41);
	}

	public double Noise(double x, double y, double z, double w)
	{
		double num = (x + y + z + w) * F4;
		int num2 = fastfloor(x + num);
		int num3 = fastfloor(y + num);
		int num4 = fastfloor(z + num);
		int num5 = fastfloor(w + num);
		double num6 = (double)(num2 + num3 + num4 + num5) * G4;
		double num7 = (double)num2 - num6;
		double num8 = (double)num3 - num6;
		double num9 = (double)num4 - num6;
		double num10 = (double)num5 - num6;
		double num11 = x - num7;
		double num12 = y - num8;
		double num13 = z - num9;
		double num14 = w - num10;
		int num15 = 0;
		int num16 = 0;
		int num17 = 0;
		int num18 = 0;
		if (num11 > num12)
		{
			num15++;
		}
		else
		{
			num16++;
		}
		if (num11 > num13)
		{
			num15++;
		}
		else
		{
			num17++;
		}
		if (num11 > num14)
		{
			num15++;
		}
		else
		{
			num18++;
		}
		if (num12 > num13)
		{
			num16++;
		}
		else
		{
			num17++;
		}
		if (num12 > num14)
		{
			num16++;
		}
		else
		{
			num18++;
		}
		if (num13 > num14)
		{
			num17++;
		}
		else
		{
			num18++;
		}
		int num19 = ((num15 >= 3) ? 1 : 0);
		int num20 = ((num16 >= 3) ? 1 : 0);
		int num21 = ((num17 >= 3) ? 1 : 0);
		int num22 = ((num18 >= 3) ? 1 : 0);
		int num23 = ((num15 >= 2) ? 1 : 0);
		int num24 = ((num16 >= 2) ? 1 : 0);
		int num25 = ((num17 >= 2) ? 1 : 0);
		int num26 = ((num18 >= 2) ? 1 : 0);
		int num27 = ((num15 >= 1) ? 1 : 0);
		int num28 = ((num16 >= 1) ? 1 : 0);
		int num29 = ((num17 >= 1) ? 1 : 0);
		int num30 = ((num18 >= 1) ? 1 : 0);
		double num31 = num11 - (double)num19 + G4;
		double num32 = num12 - (double)num20 + G4;
		double num33 = num13 - (double)num21 + G4;
		double num34 = num14 - (double)num22 + G4;
		double num35 = num11 - (double)num23 + 2.0 * G4;
		double num36 = num12 - (double)num24 + 2.0 * G4;
		double num37 = num13 - (double)num25 + 2.0 * G4;
		double num38 = num14 - (double)num26 + 2.0 * G4;
		double num39 = num11 - (double)num27 + 3.0 * G4;
		double num40 = num12 - (double)num28 + 3.0 * G4;
		double num41 = num13 - (double)num29 + 3.0 * G4;
		double num42 = num14 - (double)num30 + 3.0 * G4;
		double num43 = num11 - 1.0 + 4.0 * G4;
		double num44 = num12 - 1.0 + 4.0 * G4;
		double num45 = num13 - 1.0 + 4.0 * G4;
		double num46 = num14 - 1.0 + 4.0 * G4;
		int num47 = num2 & 0xFF;
		int num48 = num3 & 0xFF;
		int num49 = num4 & 0xFF;
		int num50 = num5 & 0xFF;
		int num51 = perm[num47 + perm[num48 + perm[num49 + perm[num50]]]] % 32;
		int num52 = perm[num47 + num19 + perm[num48 + num20 + perm[num49 + num21 + perm[num50 + num22]]]] % 32;
		int num53 = perm[num47 + num23 + perm[num48 + num24 + perm[num49 + num25 + perm[num50 + num26]]]] % 32;
		int num54 = perm[num47 + num27 + perm[num48 + num28 + perm[num49 + num29 + perm[num50 + num30]]]] % 32;
		int num55 = perm[num47 + 1 + perm[num48 + 1 + perm[num49 + 1 + perm[num50 + 1]]]] % 32;
		double num56 = 0.6 - num11 * num11 - num12 * num12 - num13 * num13 - num14 * num14;
		double num57;
		if (num56 < 0.0)
		{
			num57 = 0.0;
		}
		else
		{
			num56 *= num56;
			num57 = num56 * num56 * dot(grad4[num51], num11, num12, num13, num14);
		}
		double num58 = 0.6 - num31 * num31 - num32 * num32 - num33 * num33 - num34 * num34;
		double num59;
		if (num58 < 0.0)
		{
			num59 = 0.0;
		}
		else
		{
			num58 *= num58;
			num59 = num58 * num58 * dot(grad4[num52], num31, num32, num33, num34);
		}
		double num60 = 0.6 - num35 * num35 - num36 * num36 - num37 * num37 - num38 * num38;
		double num61;
		if (num60 < 0.0)
		{
			num61 = 0.0;
		}
		else
		{
			num60 *= num60;
			num61 = num60 * num60 * dot(grad4[num53], num35, num36, num37, num38);
		}
		double num62 = 0.6 - num39 * num39 - num40 * num40 - num41 * num41 - num42 * num42;
		double num63;
		if (num62 < 0.0)
		{
			num63 = 0.0;
		}
		else
		{
			num62 *= num62;
			num63 = num62 * num62 * dot(grad4[num54], num39, num40, num41, num42);
		}
		double num64 = 0.6 - num43 * num43 - num44 * num44 - num45 * num45 - num46 * num46;
		double num65;
		if (num64 < 0.0)
		{
			num65 = 0.0;
		}
		else
		{
			num64 *= num64;
			num65 = num64 * num64 * dot(grad4[num55], num43, num44, num45, num46);
		}
		return 27.0 * (num57 + num59 + num61 + num63 + num65);
	}

	public double Noise2DFBM(double x, double y, int nOctaves, double deltaAmp = 0.5, double deltaWLen = 2.0)
	{
		double num = 0.0;
		double num2 = 0.5;
		for (int i = 0; i < nOctaves; i++)
		{
			num += Noise(x, y) * num2;
			num2 *= deltaAmp;
			x *= deltaWLen;
			y *= deltaWLen;
		}
		return num;
	}

	public double Noise3DFBM(double x, double y, double z, int nOctaves, double deltaAmp = 0.5, double deltaWLen = 2.0)
	{
		double num = 0.0;
		double num2 = 0.5;
		for (int i = 0; i < nOctaves; i++)
		{
			num += Noise(x, y, z) * num2;
			num2 *= deltaAmp;
			x *= deltaWLen;
			y *= deltaWLen;
			z *= deltaWLen;
		}
		return num;
	}
}
