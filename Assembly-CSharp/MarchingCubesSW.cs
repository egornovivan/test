using System;
using System.Collections.Generic;
using UnityEngine;

public class MarchingCubesSW
{
	private struct VoxelInterpolationInfo
	{
		public Vector3 XYZ;

		public byte Volume;

		public byte VType;

		public VoxelInterpolationInfo(float x, float y, float z, byte volume, byte vtype)
		{
			XYZ.x = x;
			XYZ.y = y;
			XYZ.z = z;
			Volume = volume;
			VType = vtype;
		}

		public VoxelInterpolationInfo(Vector3 xyz, byte volume, byte vtype)
		{
			XYZ = xyz;
			Volume = volume;
			VType = vtype;
		}
	}

	private List<Vector3> _vertexList = new List<Vector3>();

	private List<Vector2> _norm01 = new List<Vector2>();

	private List<Vector2> _norm2t = new List<Vector2>();

	private byte[] chunkData;

	private int[] indicesConst;

	private VoxelInterpolationInfo[] val = new VoxelInterpolationInfo[32];

	private Vector3[] vertlist = new Vector3[12];

	private Vector3[] tmpN = new Vector3[8];

	private Vector3[] normlist = new Vector3[12];

	private int[] matIdx = new int[3];

	private static int[] edgeInfo = new int[24]
	{
		0, 1, 1, 2, 2, 3, 3, 0, 4, 5,
		5, 6, 6, 7, 7, 4, 0, 4, 1, 5,
		2, 6, 3, 7
	};

	private static int[] edgeTable = new int[256]
	{
		0, 265, 515, 778, 1030, 1295, 1541, 1804, 2060, 2309,
		2575, 2822, 3082, 3331, 3593, 3840, 400, 153, 915, 666,
		1430, 1183, 1941, 1692, 2460, 2197, 2975, 2710, 3482, 3219,
		3993, 3728, 560, 825, 51, 314, 1590, 1855, 1077, 1340,
		2620, 2869, 2111, 2358, 3642, 3891, 3129, 3376, 928, 681,
		419, 170, 1958, 1711, 1445, 1196, 2988, 2725, 2479, 2214,
		4010, 3747, 3497, 3232, 1120, 1385, 1635, 1898, 102, 367,
		613, 876, 3180, 3429, 3695, 3942, 2154, 2403, 2665, 2912,
		1520, 1273, 2035, 1786, 502, 255, 1013, 764, 3580, 3317,
		4095, 3830, 2554, 2291, 3065, 2800, 1616, 1881, 1107, 1370,
		598, 863, 85, 348, 3676, 3925, 3167, 3414, 2650, 2899,
		2137, 2384, 1984, 1737, 1475, 1226, 966, 719, 453, 204,
		4044, 3781, 3535, 3270, 3018, 2755, 2505, 2240, 2240, 2505,
		2755, 3018, 3270, 3535, 3781, 4044, 204, 453, 719, 966,
		1226, 1475, 1737, 1984, 2384, 2137, 2899, 2650, 3414, 3167,
		3925, 3676, 348, 85, 863, 598, 1370, 1107, 1881, 1616,
		2800, 3065, 2291, 2554, 3830, 4095, 3317, 3580, 764, 1013,
		255, 502, 1786, 2035, 1273, 1520, 2912, 2665, 2403, 2154,
		3942, 3695, 3429, 3180, 876, 613, 367, 102, 1898, 1635,
		1385, 1120, 3232, 3497, 3747, 4010, 2214, 2479, 2725, 2988,
		1196, 1445, 1711, 1958, 170, 419, 681, 928, 3376, 3129,
		3891, 3642, 2358, 2111, 2869, 2620, 1340, 1077, 1855, 1590,
		314, 51, 825, 560, 3728, 3993, 3219, 3482, 2710, 2975,
		2197, 2460, 1692, 1941, 1183, 1430, 666, 915, 153, 400,
		3840, 3593, 3331, 3082, 2822, 2575, 2309, 2060, 1804, 1541,
		1295, 1030, 778, 515, 265, 0
	};

	public static int[,] triTable = new int[256, 16]
	{
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 8, 3, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 1, 9, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 8, 3, 9, 8, 1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 2, 10, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 8, 3, 1, 2, 10, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 2, 10, 0, 2, 9, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			2, 8, 3, 2, 10, 8, 10, 9, 8, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 11, 2, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 11, 2, 8, 11, 0, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 9, 0, 2, 3, 11, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 11, 2, 1, 9, 11, 9, 8, 11, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 10, 1, 11, 10, 3, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 10, 1, 0, 8, 10, 8, 11, 10, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 9, 0, 3, 11, 9, 11, 10, 9, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 8, 10, 10, 8, 11, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 7, 8, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 3, 0, 7, 3, 4, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 1, 9, 8, 4, 7, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 1, 9, 4, 7, 1, 7, 3, 1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 2, 10, 8, 4, 7, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 4, 7, 3, 0, 4, 1, 2, 10, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 2, 10, 9, 0, 2, 8, 4, 7, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			2, 10, 9, 2, 9, 7, 2, 7, 3, 7,
			9, 4, -1, -1, -1, -1
		},
		{
			8, 4, 7, 3, 11, 2, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			11, 4, 7, 11, 2, 4, 2, 0, 4, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 0, 1, 8, 4, 7, 2, 3, 11, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 7, 11, 9, 4, 11, 9, 11, 2, 9,
			2, 1, -1, -1, -1, -1
		},
		{
			3, 10, 1, 3, 11, 10, 7, 8, 4, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 11, 10, 1, 4, 11, 1, 0, 4, 7,
			11, 4, -1, -1, -1, -1
		},
		{
			4, 7, 8, 9, 0, 11, 9, 11, 10, 11,
			0, 3, -1, -1, -1, -1
		},
		{
			4, 7, 11, 4, 11, 9, 9, 11, 10, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 5, 4, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 5, 4, 0, 8, 3, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 5, 4, 1, 5, 0, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			8, 5, 4, 8, 3, 5, 3, 1, 5, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 2, 10, 9, 5, 4, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 0, 8, 1, 2, 10, 4, 9, 5, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			5, 2, 10, 5, 4, 2, 4, 0, 2, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			2, 10, 5, 3, 2, 5, 3, 5, 4, 3,
			4, 8, -1, -1, -1, -1
		},
		{
			9, 5, 4, 2, 3, 11, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 11, 2, 0, 8, 11, 4, 9, 5, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 5, 4, 0, 1, 5, 2, 3, 11, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			2, 1, 5, 2, 5, 8, 2, 8, 11, 4,
			8, 5, -1, -1, -1, -1
		},
		{
			10, 3, 11, 10, 1, 3, 9, 5, 4, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 9, 5, 0, 8, 1, 8, 10, 1, 8,
			11, 10, -1, -1, -1, -1
		},
		{
			5, 4, 0, 5, 0, 11, 5, 11, 10, 11,
			0, 3, -1, -1, -1, -1
		},
		{
			5, 4, 8, 5, 8, 10, 10, 8, 11, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 7, 8, 5, 7, 9, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 3, 0, 9, 5, 3, 5, 7, 3, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 7, 8, 0, 1, 7, 1, 5, 7, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 5, 3, 3, 5, 7, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 7, 8, 9, 5, 7, 10, 1, 2, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			10, 1, 2, 9, 5, 0, 5, 3, 0, 5,
			7, 3, -1, -1, -1, -1
		},
		{
			8, 0, 2, 8, 2, 5, 8, 5, 7, 10,
			5, 2, -1, -1, -1, -1
		},
		{
			2, 10, 5, 2, 5, 3, 3, 5, 7, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			7, 9, 5, 7, 8, 9, 3, 11, 2, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 5, 7, 9, 7, 2, 9, 2, 0, 2,
			7, 11, -1, -1, -1, -1
		},
		{
			2, 3, 11, 0, 1, 8, 1, 7, 8, 1,
			5, 7, -1, -1, -1, -1
		},
		{
			11, 2, 1, 11, 1, 7, 7, 1, 5, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 5, 8, 8, 5, 7, 10, 1, 3, 10,
			3, 11, -1, -1, -1, -1
		},
		{
			5, 7, 0, 5, 0, 9, 7, 11, 0, 1,
			0, 10, 11, 10, 0, -1
		},
		{
			11, 10, 0, 11, 0, 3, 10, 5, 0, 8,
			0, 7, 5, 7, 0, -1
		},
		{
			11, 10, 5, 7, 11, 5, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			10, 6, 5, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 8, 3, 5, 10, 6, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 0, 1, 5, 10, 6, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 8, 3, 1, 9, 8, 5, 10, 6, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 6, 5, 2, 6, 1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 6, 5, 1, 2, 6, 3, 0, 8, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 6, 5, 9, 0, 6, 0, 2, 6, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			5, 9, 8, 5, 8, 2, 5, 2, 6, 3,
			2, 8, -1, -1, -1, -1
		},
		{
			2, 3, 11, 10, 6, 5, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			11, 0, 8, 11, 2, 0, 10, 6, 5, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 1, 9, 2, 3, 11, 5, 10, 6, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			5, 10, 6, 1, 9, 2, 9, 11, 2, 9,
			8, 11, -1, -1, -1, -1
		},
		{
			6, 3, 11, 6, 5, 3, 5, 1, 3, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 8, 11, 0, 11, 5, 0, 5, 1, 5,
			11, 6, -1, -1, -1, -1
		},
		{
			3, 11, 6, 0, 3, 6, 0, 6, 5, 0,
			5, 9, -1, -1, -1, -1
		},
		{
			6, 5, 9, 6, 9, 11, 11, 9, 8, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			5, 10, 6, 4, 7, 8, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 3, 0, 4, 7, 3, 6, 5, 10, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 9, 0, 5, 10, 6, 8, 4, 7, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			10, 6, 5, 1, 9, 7, 1, 7, 3, 7,
			9, 4, -1, -1, -1, -1
		},
		{
			6, 1, 2, 6, 5, 1, 4, 7, 8, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 2, 5, 5, 2, 6, 3, 0, 4, 3,
			4, 7, -1, -1, -1, -1
		},
		{
			8, 4, 7, 9, 0, 5, 0, 6, 5, 0,
			2, 6, -1, -1, -1, -1
		},
		{
			7, 3, 9, 7, 9, 4, 3, 2, 9, 5,
			9, 6, 2, 6, 9, -1
		},
		{
			3, 11, 2, 7, 8, 4, 10, 6, 5, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			5, 10, 6, 4, 7, 2, 4, 2, 0, 2,
			7, 11, -1, -1, -1, -1
		},
		{
			0, 1, 9, 4, 7, 8, 2, 3, 11, 5,
			10, 6, -1, -1, -1, -1
		},
		{
			9, 2, 1, 9, 11, 2, 9, 4, 11, 7,
			11, 4, 5, 10, 6, -1
		},
		{
			8, 4, 7, 3, 11, 5, 3, 5, 1, 5,
			11, 6, -1, -1, -1, -1
		},
		{
			5, 1, 11, 5, 11, 6, 1, 0, 11, 7,
			11, 4, 0, 4, 11, -1
		},
		{
			0, 5, 9, 0, 6, 5, 0, 3, 6, 11,
			6, 3, 8, 4, 7, -1
		},
		{
			6, 5, 9, 6, 9, 11, 4, 7, 9, 7,
			11, 9, -1, -1, -1, -1
		},
		{
			10, 4, 9, 6, 4, 10, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 10, 6, 4, 9, 10, 0, 8, 3, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			10, 0, 1, 10, 6, 0, 6, 4, 0, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			8, 3, 1, 8, 1, 6, 8, 6, 4, 6,
			1, 10, -1, -1, -1, -1
		},
		{
			1, 4, 9, 1, 2, 4, 2, 6, 4, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 0, 8, 1, 2, 9, 2, 4, 9, 2,
			6, 4, -1, -1, -1, -1
		},
		{
			0, 2, 4, 4, 2, 6, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			8, 3, 2, 8, 2, 4, 4, 2, 6, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			10, 4, 9, 10, 6, 4, 11, 2, 3, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 8, 2, 2, 8, 11, 4, 9, 10, 4,
			10, 6, -1, -1, -1, -1
		},
		{
			3, 11, 2, 0, 1, 6, 0, 6, 4, 6,
			1, 10, -1, -1, -1, -1
		},
		{
			6, 4, 1, 6, 1, 10, 4, 8, 1, 2,
			1, 11, 8, 11, 1, -1
		},
		{
			9, 6, 4, 9, 3, 6, 9, 1, 3, 11,
			6, 3, -1, -1, -1, -1
		},
		{
			8, 11, 1, 8, 1, 0, 11, 6, 1, 9,
			1, 4, 6, 4, 1, -1
		},
		{
			3, 11, 6, 3, 6, 0, 0, 6, 4, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			6, 4, 8, 11, 6, 8, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			7, 10, 6, 7, 8, 10, 8, 9, 10, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 7, 3, 0, 10, 7, 0, 9, 10, 6,
			7, 10, -1, -1, -1, -1
		},
		{
			10, 6, 7, 1, 10, 7, 1, 7, 8, 1,
			8, 0, -1, -1, -1, -1
		},
		{
			10, 6, 7, 10, 7, 1, 1, 7, 3, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 2, 6, 1, 6, 8, 1, 8, 9, 8,
			6, 7, -1, -1, -1, -1
		},
		{
			2, 6, 9, 2, 9, 1, 6, 7, 9, 0,
			9, 3, 7, 3, 9, -1
		},
		{
			7, 8, 0, 7, 0, 6, 6, 0, 2, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			7, 3, 2, 6, 7, 2, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			2, 3, 11, 10, 6, 8, 10, 8, 9, 8,
			6, 7, -1, -1, -1, -1
		},
		{
			2, 0, 7, 2, 7, 11, 0, 9, 7, 6,
			7, 10, 9, 10, 7, -1
		},
		{
			1, 8, 0, 1, 7, 8, 1, 10, 7, 6,
			7, 10, 2, 3, 11, -1
		},
		{
			11, 2, 1, 11, 1, 7, 10, 6, 1, 6,
			7, 1, -1, -1, -1, -1
		},
		{
			8, 9, 6, 8, 6, 7, 9, 1, 6, 11,
			6, 3, 1, 3, 6, -1
		},
		{
			0, 9, 1, 11, 6, 7, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			7, 8, 0, 7, 0, 6, 3, 11, 0, 11,
			6, 0, -1, -1, -1, -1
		},
		{
			7, 11, 6, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			7, 6, 11, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 0, 8, 11, 7, 6, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 1, 9, 11, 7, 6, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			8, 1, 9, 8, 3, 1, 11, 7, 6, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			10, 1, 2, 6, 11, 7, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 2, 10, 3, 0, 8, 6, 11, 7, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			2, 9, 0, 2, 10, 9, 6, 11, 7, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			6, 11, 7, 2, 10, 3, 10, 8, 3, 10,
			9, 8, -1, -1, -1, -1
		},
		{
			7, 2, 3, 6, 2, 7, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			7, 0, 8, 7, 6, 0, 6, 2, 0, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			2, 7, 6, 2, 3, 7, 0, 1, 9, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 6, 2, 1, 8, 6, 1, 9, 8, 8,
			7, 6, -1, -1, -1, -1
		},
		{
			10, 7, 6, 10, 1, 7, 1, 3, 7, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			10, 7, 6, 1, 7, 10, 1, 8, 7, 1,
			0, 8, -1, -1, -1, -1
		},
		{
			0, 3, 7, 0, 7, 10, 0, 10, 9, 6,
			10, 7, -1, -1, -1, -1
		},
		{
			7, 6, 10, 7, 10, 8, 8, 10, 9, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			6, 8, 4, 11, 8, 6, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 6, 11, 3, 0, 6, 0, 4, 6, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			8, 6, 11, 8, 4, 6, 9, 0, 1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 4, 6, 9, 6, 3, 9, 3, 1, 11,
			3, 6, -1, -1, -1, -1
		},
		{
			6, 8, 4, 6, 11, 8, 2, 10, 1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 2, 10, 3, 0, 11, 0, 6, 11, 0,
			4, 6, -1, -1, -1, -1
		},
		{
			4, 11, 8, 4, 6, 11, 0, 2, 9, 2,
			10, 9, -1, -1, -1, -1
		},
		{
			10, 9, 3, 10, 3, 2, 9, 4, 3, 11,
			3, 6, 4, 6, 3, -1
		},
		{
			8, 2, 3, 8, 4, 2, 4, 6, 2, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 4, 2, 4, 6, 2, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 9, 0, 2, 3, 4, 2, 4, 6, 4,
			3, 8, -1, -1, -1, -1
		},
		{
			1, 9, 4, 1, 4, 2, 2, 4, 6, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			8, 1, 3, 8, 6, 1, 8, 4, 6, 6,
			10, 1, -1, -1, -1, -1
		},
		{
			10, 1, 0, 10, 0, 6, 6, 0, 4, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 6, 3, 4, 3, 8, 6, 10, 3, 0,
			3, 9, 10, 9, 3, -1
		},
		{
			10, 9, 4, 6, 10, 4, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 9, 5, 7, 6, 11, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 8, 3, 4, 9, 5, 11, 7, 6, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			5, 0, 1, 5, 4, 0, 7, 6, 11, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			11, 7, 6, 8, 3, 4, 3, 5, 4, 3,
			1, 5, -1, -1, -1, -1
		},
		{
			9, 5, 4, 10, 1, 2, 7, 6, 11, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			6, 11, 7, 1, 2, 10, 0, 8, 3, 4,
			9, 5, -1, -1, -1, -1
		},
		{
			7, 6, 11, 5, 4, 10, 4, 2, 10, 4,
			0, 2, -1, -1, -1, -1
		},
		{
			3, 4, 8, 3, 5, 4, 3, 2, 5, 10,
			5, 2, 11, 7, 6, -1
		},
		{
			7, 2, 3, 7, 6, 2, 5, 4, 9, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 5, 4, 0, 8, 6, 0, 6, 2, 6,
			8, 7, -1, -1, -1, -1
		},
		{
			3, 6, 2, 3, 7, 6, 1, 5, 0, 5,
			4, 0, -1, -1, -1, -1
		},
		{
			6, 2, 8, 6, 8, 7, 2, 1, 8, 4,
			8, 5, 1, 5, 8, -1
		},
		{
			9, 5, 4, 10, 1, 6, 1, 7, 6, 1,
			3, 7, -1, -1, -1, -1
		},
		{
			1, 6, 10, 1, 7, 6, 1, 0, 7, 8,
			7, 0, 9, 5, 4, -1
		},
		{
			4, 0, 10, 4, 10, 5, 0, 3, 10, 6,
			10, 7, 3, 7, 10, -1
		},
		{
			7, 6, 10, 7, 10, 8, 5, 4, 10, 4,
			8, 10, -1, -1, -1, -1
		},
		{
			6, 9, 5, 6, 11, 9, 11, 8, 9, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 6, 11, 0, 6, 3, 0, 5, 6, 0,
			9, 5, -1, -1, -1, -1
		},
		{
			0, 11, 8, 0, 5, 11, 0, 1, 5, 5,
			6, 11, -1, -1, -1, -1
		},
		{
			6, 11, 3, 6, 3, 5, 5, 3, 1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 2, 10, 9, 5, 11, 9, 11, 8, 11,
			5, 6, -1, -1, -1, -1
		},
		{
			0, 11, 3, 0, 6, 11, 0, 9, 6, 5,
			6, 9, 1, 2, 10, -1
		},
		{
			11, 8, 5, 11, 5, 6, 8, 0, 5, 10,
			5, 2, 0, 2, 5, -1
		},
		{
			6, 11, 3, 6, 3, 5, 2, 10, 3, 10,
			5, 3, -1, -1, -1, -1
		},
		{
			5, 8, 9, 5, 2, 8, 5, 6, 2, 3,
			8, 2, -1, -1, -1, -1
		},
		{
			9, 5, 6, 9, 6, 0, 0, 6, 2, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 5, 8, 1, 8, 0, 5, 6, 8, 3,
			8, 2, 6, 2, 8, -1
		},
		{
			1, 5, 6, 2, 1, 6, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 3, 6, 1, 6, 10, 3, 8, 6, 5,
			6, 9, 8, 9, 6, -1
		},
		{
			10, 1, 0, 10, 0, 6, 9, 5, 0, 5,
			6, 0, -1, -1, -1, -1
		},
		{
			0, 3, 8, 5, 6, 10, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			10, 5, 6, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			11, 5, 10, 7, 5, 11, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			11, 5, 10, 11, 7, 5, 8, 3, 0, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			5, 11, 7, 5, 10, 11, 1, 9, 0, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			10, 7, 5, 10, 11, 7, 9, 8, 1, 8,
			3, 1, -1, -1, -1, -1
		},
		{
			11, 1, 2, 11, 7, 1, 7, 5, 1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 8, 3, 1, 2, 7, 1, 7, 5, 7,
			2, 11, -1, -1, -1, -1
		},
		{
			9, 7, 5, 9, 2, 7, 9, 0, 2, 2,
			11, 7, -1, -1, -1, -1
		},
		{
			7, 5, 2, 7, 2, 11, 5, 9, 2, 3,
			2, 8, 9, 8, 2, -1
		},
		{
			2, 5, 10, 2, 3, 5, 3, 7, 5, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			8, 2, 0, 8, 5, 2, 8, 7, 5, 10,
			2, 5, -1, -1, -1, -1
		},
		{
			9, 0, 1, 5, 10, 3, 5, 3, 7, 3,
			10, 2, -1, -1, -1, -1
		},
		{
			9, 8, 2, 9, 2, 1, 8, 7, 2, 10,
			2, 5, 7, 5, 2, -1
		},
		{
			1, 3, 5, 3, 7, 5, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 8, 7, 0, 7, 1, 1, 7, 5, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 0, 3, 9, 3, 5, 5, 3, 7, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 8, 7, 5, 9, 7, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			5, 8, 4, 5, 10, 8, 10, 11, 8, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			5, 0, 4, 5, 11, 0, 5, 10, 11, 11,
			3, 0, -1, -1, -1, -1
		},
		{
			0, 1, 9, 8, 4, 10, 8, 10, 11, 10,
			4, 5, -1, -1, -1, -1
		},
		{
			10, 11, 4, 10, 4, 5, 11, 3, 4, 9,
			4, 1, 3, 1, 4, -1
		},
		{
			2, 5, 1, 2, 8, 5, 2, 11, 8, 4,
			5, 8, -1, -1, -1, -1
		},
		{
			0, 4, 11, 0, 11, 3, 4, 5, 11, 2,
			11, 1, 5, 1, 11, -1
		},
		{
			0, 2, 5, 0, 5, 9, 2, 11, 5, 4,
			5, 8, 11, 8, 5, -1
		},
		{
			9, 4, 5, 2, 11, 3, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			2, 5, 10, 3, 5, 2, 3, 4, 5, 3,
			8, 4, -1, -1, -1, -1
		},
		{
			5, 10, 2, 5, 2, 4, 4, 2, 0, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 10, 2, 3, 5, 10, 3, 8, 5, 4,
			5, 8, 0, 1, 9, -1
		},
		{
			5, 10, 2, 5, 2, 4, 1, 9, 2, 9,
			4, 2, -1, -1, -1, -1
		},
		{
			8, 4, 5, 8, 5, 3, 3, 5, 1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 4, 5, 1, 0, 5, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			8, 4, 5, 8, 5, 3, 9, 0, 5, 0,
			3, 5, -1, -1, -1, -1
		},
		{
			9, 4, 5, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 11, 7, 4, 9, 11, 9, 10, 11, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 8, 3, 4, 9, 7, 9, 11, 7, 9,
			10, 11, -1, -1, -1, -1
		},
		{
			1, 10, 11, 1, 11, 4, 1, 4, 0, 7,
			4, 11, -1, -1, -1, -1
		},
		{
			3, 1, 4, 3, 4, 8, 1, 10, 4, 7,
			4, 11, 10, 11, 4, -1
		},
		{
			4, 11, 7, 9, 11, 4, 9, 2, 11, 9,
			1, 2, -1, -1, -1, -1
		},
		{
			9, 7, 4, 9, 11, 7, 9, 1, 11, 2,
			11, 1, 0, 8, 3, -1
		},
		{
			11, 7, 4, 11, 4, 2, 2, 4, 0, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			11, 7, 4, 11, 4, 2, 8, 3, 4, 3,
			2, 4, -1, -1, -1, -1
		},
		{
			2, 9, 10, 2, 7, 9, 2, 3, 7, 7,
			4, 9, -1, -1, -1, -1
		},
		{
			9, 10, 7, 9, 7, 4, 10, 2, 7, 8,
			7, 0, 2, 0, 7, -1
		},
		{
			3, 7, 10, 3, 10, 2, 7, 4, 10, 1,
			10, 0, 4, 0, 10, -1
		},
		{
			1, 10, 2, 8, 7, 4, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 9, 1, 4, 1, 7, 7, 1, 3, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 9, 1, 4, 1, 7, 0, 8, 1, 8,
			7, 1, -1, -1, -1, -1
		},
		{
			4, 0, 3, 7, 4, 3, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			4, 8, 7, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 10, 8, 10, 11, 8, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 0, 9, 3, 9, 11, 11, 9, 10, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 1, 10, 0, 10, 8, 8, 10, 11, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 1, 10, 11, 3, 10, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 2, 11, 1, 11, 9, 9, 11, 8, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 0, 9, 3, 9, 11, 1, 2, 9, 2,
			11, 9, -1, -1, -1, -1
		},
		{
			0, 2, 11, 8, 0, 11, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			3, 2, 11, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			2, 3, 8, 2, 8, 10, 10, 8, 9, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			9, 10, 2, 0, 9, 2, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			2, 3, 8, 2, 8, 10, 0, 1, 8, 1,
			10, 8, -1, -1, -1, -1
		},
		{
			1, 10, 2, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			1, 3, 8, 9, 1, 8, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 9, 1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			0, 3, 8, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		},
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		}
	};

	public List<Vector3> VertexList => _vertexList;

	public List<Vector2> Norm01List => _norm01;

	public List<Vector2> Norm2tList => _norm2t;

	public MarchingCubesSW()
	{
		indicesConst = new int[64999];
		for (int i = 0; i < indicesConst.Length; i++)
		{
			indicesConst[i] = i;
		}
	}

	public static int OneIndex(int x, int y, int z)
	{
		return z * 1225 + y * 35 + x;
	}

	public static int OneIndexPrefixed(int x, int y, int z)
	{
		return (z + 1) * 1225 + (y + 1) * 35 + (x + 1);
	}

	public void setInputChunkData(byte[] _input)
	{
		chunkData = _input;
	}

	public void Rebuild()
	{
		_vertexList.Clear();
		_norm01.Clear();
		_norm2t.Clear();
		for (int i = 1; i < 33; i++)
		{
			for (int j = 1; j < 33; j++)
			{
				for (int k = 1; k < 33; k++)
				{
					BuildRegularVoxelTriangles(k, j, i);
				}
			}
		}
	}

	public Mesh RebuildMesh()
	{
		Rebuild();
		Mesh mesh = new Mesh();
		mesh.Clear();
		mesh.vertices = _vertexList.ToArray();
		mesh.uv = _norm01.ToArray();
		mesh.uv2 = _norm2t.ToArray();
		mesh.subMeshCount = 1;
		int[] array = new int[_vertexList.Count];
		Array.Copy(indicesConst, array, _vertexList.Count);
		mesh.SetTriangles(array, 0);
		return mesh;
	}

	private Vector3 VertexInterp(VoxelInterpolationInfo val1, VoxelInterpolationInfo val2)
	{
		float t = (float)(128 - val1.Volume) / (float)(val2.Volume - val1.Volume);
		return Vector3.Lerp(val1.XYZ, val2.XYZ, t);
	}

	private Vector3 normalInterp(Vector3 n1, Vector3 n2, VoxelInterpolationInfo val1, VoxelInterpolationInfo val2)
	{
		float t = (float)(128 - val1.Volume) / (float)(val2.Volume - val1.Volume);
		return Vector3.Lerp(n1, n2, t);
	}

	private void BuildRegularVoxelTriangles(int vx, int vy, int vz)
	{
		int num = vx - 1;
		int num2 = vy - 1;
		int num3 = vz - 1;
		val[0].Volume = chunkData[OneIndex(vx, vy, vz) << 1];
		val[1].Volume = chunkData[OneIndex(vx + 1, vy, vz) << 1];
		val[2].Volume = chunkData[OneIndex(vx + 1, vy + 1, vz) << 1];
		val[3].Volume = chunkData[OneIndex(vx, vy + 1, vz) << 1];
		val[4].Volume = chunkData[OneIndex(vx, vy, vz + 1) << 1];
		val[5].Volume = chunkData[OneIndex(vx + 1, vy, vz + 1) << 1];
		val[6].Volume = chunkData[OneIndex(vx + 1, vy + 1, vz + 1) << 1];
		val[7].Volume = chunkData[OneIndex(vx, vy + 1, vz + 1) << 1];
		int num4 = 0;
		if (val[0].Volume < 128)
		{
			num4 |= 1;
		}
		if (val[1].Volume < 128)
		{
			num4 |= 2;
		}
		if (val[2].Volume < 128)
		{
			num4 |= 4;
		}
		if (val[3].Volume < 128)
		{
			num4 |= 8;
		}
		if (val[4].Volume < 128)
		{
			num4 |= 0x10;
		}
		if (val[5].Volume < 128)
		{
			num4 |= 0x20;
		}
		if (val[6].Volume < 128)
		{
			num4 |= 0x40;
		}
		if (val[7].Volume < 128)
		{
			num4 |= 0x80;
		}
		if (edgeTable[num4] != 0)
		{
			val[0].XYZ.x = num;
			val[0].XYZ.y = num2;
			val[0].XYZ.z = num3;
			val[0].VType = chunkData[(OneIndex(vx, vy, vz) << 1) + 1];
			val[1].XYZ.x = num + 1;
			val[1].XYZ.y = num2;
			val[1].XYZ.z = num3;
			val[1].VType = chunkData[(OneIndex(vx + 1, vy, vz) << 1) + 1];
			val[2].XYZ.x = num + 1;
			val[2].XYZ.y = num2 + 1;
			val[2].XYZ.z = num3;
			val[2].VType = chunkData[(OneIndex(vx + 1, vy + 1, vz) << 1) + 1];
			val[3].XYZ.x = num;
			val[3].XYZ.y = num2 + 1;
			val[3].XYZ.z = num3;
			val[3].VType = chunkData[(OneIndex(vx, vy + 1, vz) << 1) + 1];
			val[4].XYZ.x = num;
			val[4].XYZ.y = num2;
			val[4].XYZ.z = num3 + 1;
			val[4].VType = chunkData[(OneIndex(vx, vy, vz + 1) << 1) + 1];
			val[5].XYZ.x = num + 1;
			val[5].XYZ.y = num2;
			val[5].XYZ.z = num3 + 1;
			val[5].VType = chunkData[(OneIndex(vx + 1, vy, vz + 1) << 1) + 1];
			val[6].XYZ.x = num + 1;
			val[6].XYZ.y = num2 + 1;
			val[6].XYZ.z = num3 + 1;
			val[6].VType = chunkData[(OneIndex(vx + 1, vy + 1, vz + 1) << 1) + 1];
			val[7].XYZ.x = num;
			val[7].XYZ.y = num2 + 1;
			val[7].XYZ.z = num3 + 1;
			val[7].VType = chunkData[(OneIndex(vx, vy + 1, vz + 1) << 1) + 1];
			val[8].Volume = chunkData[OneIndex(vx - 1, vy, vz) << 1];
			val[9].Volume = chunkData[OneIndex(vx - 1, vy + 1, vz) << 1];
			val[10].Volume = chunkData[OneIndex(vx - 1, vy, vz + 1) << 1];
			val[11].Volume = chunkData[OneIndex(vx, vy - 1, vz) << 1];
			val[12].Volume = chunkData[OneIndex(vx + 1, vy - 1, vz) << 1];
			val[13].Volume = chunkData[OneIndex(vx, vy - 1, vz + 1) << 1];
			val[14].Volume = chunkData[OneIndex(vx, vy, vz - 1) << 1];
			val[15].Volume = chunkData[OneIndex(vx + 1, vy, vz - 1) << 1];
			val[16].Volume = chunkData[OneIndex(vx, vy + 1, vz - 1) << 1];
			val[17].Volume = chunkData[OneIndex(vx - 1, vy + 1, vz + 1) << 1];
			val[18].Volume = chunkData[OneIndex(vx + 1, vy - 1, vz + 1) << 1];
			val[19].Volume = chunkData[OneIndex(vx + 1, vy + 1, vz - 1) << 1];
			val[20].Volume = chunkData[OneIndex(vx + 2, vy, vz) << 1];
			val[21].Volume = chunkData[OneIndex(vx + 2, vy + 1, vz) << 1];
			val[22].Volume = chunkData[OneIndex(vx, vy + 2, vz) << 1];
			val[23].Volume = chunkData[OneIndex(vx, vy, vz + 2) << 1];
			val[24].Volume = chunkData[OneIndex(vx + 2, vy, vz + 1) << 1];
			val[25].Volume = chunkData[OneIndex(vx + 2, vy + 1, vz + 1) << 1];
			val[26].Volume = chunkData[OneIndex(vx, vy + 2, vz + 1) << 1];
			val[27].Volume = chunkData[OneIndex(vx + 1, vy + 2, vz) << 1];
			val[28].Volume = chunkData[OneIndex(vx + 1, vy, vz + 2) << 1];
			val[29].Volume = chunkData[OneIndex(vx + 1, vy + 2, vz + 1) << 1];
			val[30].Volume = chunkData[OneIndex(vx + 1, vy + 1, vz + 2) << 1];
			val[31].Volume = chunkData[OneIndex(vx, vy + 1, vz + 2) << 1];
			ref Vector3 reference = ref tmpN[0];
			reference = new Vector3((float)(val[8].Volume - val[1].Volume) / 255f, (float)(val[11].Volume - val[3].Volume) / 255f, (float)(val[14].Volume - val[4].Volume) / 255f);
			ref Vector3 reference2 = ref tmpN[1];
			reference2 = new Vector3((float)(val[0].Volume - val[20].Volume) / 255f, (float)(val[12].Volume - val[2].Volume) / 255f, (float)(val[15].Volume - val[5].Volume) / 255f);
			ref Vector3 reference3 = ref tmpN[3];
			reference3 = new Vector3((float)(val[9].Volume - val[2].Volume) / 255f, (float)(val[0].Volume - val[22].Volume) / 255f, (float)(val[16].Volume - val[7].Volume) / 255f);
			ref Vector3 reference4 = ref tmpN[4];
			reference4 = new Vector3((float)(val[10].Volume - val[5].Volume) / 255f, (float)(val[13].Volume - val[7].Volume) / 255f, (float)(val[0].Volume - val[23].Volume) / 255f);
			ref Vector3 reference5 = ref tmpN[2];
			reference5 = new Vector3((float)(val[3].Volume - val[21].Volume) / 255f, (float)(val[1].Volume - val[27].Volume) / 255f, (float)(val[19].Volume - val[6].Volume) / 255f);
			ref Vector3 reference6 = ref tmpN[5];
			reference6 = new Vector3((float)(val[4].Volume - val[24].Volume) / 255f, (float)(val[18].Volume - val[6].Volume) / 255f, (float)(val[1].Volume - val[28].Volume) / 255f);
			ref Vector3 reference7 = ref tmpN[6];
			reference7 = new Vector3((float)(val[7].Volume - val[25].Volume) / 255f, (float)(val[5].Volume - val[29].Volume) / 255f, (float)(val[2].Volume - val[30].Volume) / 255f);
			ref Vector3 reference8 = ref tmpN[7];
			reference8 = new Vector3((float)(val[17].Volume - val[6].Volume) / 255f, (float)(val[4].Volume - val[26].Volume) / 255f, (float)(val[3].Volume - val[31].Volume) / 255f);
			if ((edgeTable[num4] & 1) > 0)
			{
				ref Vector3 reference9 = ref vertlist[0];
				reference9 = VertexInterp(val[0], val[1]);
				ref Vector3 reference10 = ref normlist[0];
				reference10 = normalInterp(tmpN[0], tmpN[1], val[0], val[1]);
			}
			if ((edgeTable[num4] & 2) > 0)
			{
				ref Vector3 reference11 = ref vertlist[1];
				reference11 = VertexInterp(val[1], val[2]);
				ref Vector3 reference12 = ref normlist[1];
				reference12 = normalInterp(tmpN[1], tmpN[2], val[1], val[2]);
			}
			if ((edgeTable[num4] & 4) > 0)
			{
				ref Vector3 reference13 = ref vertlist[2];
				reference13 = VertexInterp(val[2], val[3]);
				ref Vector3 reference14 = ref normlist[2];
				reference14 = normalInterp(tmpN[2], tmpN[3], val[2], val[3]);
			}
			if ((edgeTable[num4] & 8) > 0)
			{
				ref Vector3 reference15 = ref vertlist[3];
				reference15 = VertexInterp(val[3], val[0]);
				ref Vector3 reference16 = ref normlist[3];
				reference16 = normalInterp(tmpN[3], tmpN[0], val[3], val[0]);
			}
			if ((edgeTable[num4] & 0x10) > 0)
			{
				ref Vector3 reference17 = ref vertlist[4];
				reference17 = VertexInterp(val[4], val[5]);
				ref Vector3 reference18 = ref normlist[4];
				reference18 = normalInterp(tmpN[4], tmpN[5], val[4], val[5]);
			}
			if ((edgeTable[num4] & 0x20) > 0)
			{
				ref Vector3 reference19 = ref vertlist[5];
				reference19 = VertexInterp(val[5], val[6]);
				ref Vector3 reference20 = ref normlist[5];
				reference20 = normalInterp(tmpN[5], tmpN[6], val[5], val[6]);
			}
			if ((edgeTable[num4] & 0x40) > 0)
			{
				ref Vector3 reference21 = ref vertlist[6];
				reference21 = VertexInterp(val[6], val[7]);
				ref Vector3 reference22 = ref normlist[6];
				reference22 = normalInterp(tmpN[6], tmpN[7], val[6], val[7]);
			}
			if ((edgeTable[num4] & 0x80) > 0)
			{
				ref Vector3 reference23 = ref vertlist[7];
				reference23 = VertexInterp(val[7], val[4]);
				ref Vector3 reference24 = ref normlist[7];
				reference24 = normalInterp(tmpN[7], tmpN[4], val[7], val[4]);
			}
			if ((edgeTable[num4] & 0x100) > 0)
			{
				ref Vector3 reference25 = ref vertlist[8];
				reference25 = VertexInterp(val[0], val[4]);
				ref Vector3 reference26 = ref normlist[8];
				reference26 = normalInterp(tmpN[0], tmpN[4], val[0], val[4]);
			}
			if ((edgeTable[num4] & 0x200) > 0)
			{
				ref Vector3 reference27 = ref vertlist[9];
				reference27 = VertexInterp(val[1], val[5]);
				ref Vector3 reference28 = ref normlist[9];
				reference28 = normalInterp(tmpN[1], tmpN[5], val[1], val[5]);
			}
			if ((edgeTable[num4] & 0x400) > 0)
			{
				ref Vector3 reference29 = ref vertlist[10];
				reference29 = VertexInterp(val[2], val[6]);
				ref Vector3 reference30 = ref normlist[10];
				reference30 = normalInterp(tmpN[2], tmpN[6], val[2], val[6]);
			}
			if ((edgeTable[num4] & 0x800) > 0)
			{
				ref Vector3 reference31 = ref vertlist[11];
				reference31 = VertexInterp(val[3], val[7]);
				ref Vector3 reference32 = ref normlist[11];
				reference32 = normalInterp(tmpN[3], tmpN[7], val[3], val[7]);
			}
			for (int i = 0; triTable[num4, i] != -1; i += 3)
			{
				int num5 = triTable[num4, i];
				int num6 = triTable[num4, i + 1];
				int num7 = triTable[num4, i + 2];
				Vector3 item = vertlist[num5];
				Vector3 item2 = vertlist[num6];
				Vector3 item3 = vertlist[num7];
				Vector3 vector = normlist[num5];
				Vector3 vector2 = normlist[num6];
				Vector3 vector3 = normlist[num7];
				int num8 = edgeInfo[num5 * 2];
				int num9 = edgeInfo[num5 * 2 + 1];
				matIdx[0] = val[((num4 & (1 << num8)) != 0) ? num9 : num8].VType;
				num8 = edgeInfo[num6 * 2];
				num9 = edgeInfo[num6 * 2 + 1];
				matIdx[1] = val[((num4 & (1 << num8)) != 0) ? num9 : num8].VType;
				num8 = edgeInfo[num7 * 2];
				num9 = edgeInfo[num7 * 2 + 1];
				matIdx[2] = val[((num4 & (1 << num8)) != 0) ? num9 : num8].VType;
				_vertexList.Add(item);
				_vertexList.Add(item2);
				_vertexList.Add(item3);
				int num10 = matIdx[0] * 4 + 2;
				int num11 = matIdx[1] * 256 + matIdx[2];
				_norm2t.Add(new Vector2((float)num10 + vector.z, num11));
				_norm2t.Add(new Vector2((float)num10 + vector2.z, (float)num11 + 0.1f));
				_norm2t.Add(new Vector2((float)num10 + vector3.z, (float)num11 + 0.2f));
				_norm01.Add(new Vector2(vector.x, vector.y));
				_norm01.Add(new Vector2(vector2.x, vector2.y));
				_norm01.Add(new Vector2(vector3.x, vector3.y));
			}
		}
	}
}
