using System.Collections.Generic;
using UnityEngine;

public class MarchingCubesSWIndexed
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

	private List<int> _indexList = new List<int>();

	private byte[] chunkData;

	private ushort _baseIndex;

	private int tempIndexBufferPingpongIdx;

	private static int IndicesPerCell = 4;

	private ushort[] indexTempBuffer = new ushort[IndicesPerCell * 35 * 35 * 2];

	private int tvTempIndexBufferPingpongIdx;

	private VoxelInterpolationInfo[] val = new VoxelInterpolationInfo[8];

	private byte[] low8_array = new byte[3];

	private static byte[] regularCellClass = new byte[256]
	{
		0, 1, 1, 3, 1, 3, 2, 4, 1, 2,
		3, 4, 3, 4, 4, 3, 1, 3, 2, 4,
		2, 4, 6, 12, 2, 5, 5, 11, 5, 10,
		7, 4, 1, 2, 3, 4, 2, 5, 5, 10,
		2, 6, 4, 12, 5, 7, 11, 4, 3, 4,
		4, 3, 5, 11, 7, 4, 5, 7, 10, 4,
		8, 14, 14, 3, 1, 2, 2, 5, 3, 4,
		5, 11, 2, 6, 5, 7, 4, 12, 10, 4,
		3, 4, 5, 10, 4, 3, 7, 4, 5, 7,
		8, 14, 11, 4, 14, 3, 2, 6, 5, 7,
		5, 7, 8, 14, 6, 9, 7, 15, 7, 15,
		14, 13, 4, 12, 11, 4, 10, 4, 14, 3,
		7, 15, 14, 13, 14, 13, 2, 1, 1, 2,
		2, 5, 2, 5, 6, 7, 3, 5, 4, 10,
		4, 11, 12, 4, 2, 5, 6, 7, 6, 7,
		9, 15, 5, 8, 7, 14, 7, 14, 15, 13,
		3, 5, 4, 11, 5, 8, 7, 14, 4, 7,
		3, 4, 10, 14, 4, 3, 4, 10, 12, 4,
		7, 14, 15, 13, 11, 14, 4, 3, 14, 2,
		13, 1, 3, 5, 5, 8, 4, 10, 7, 14,
		4, 7, 11, 14, 3, 4, 4, 3, 4, 11,
		7, 14, 12, 4, 15, 13, 10, 14, 14, 2,
		4, 3, 13, 1, 4, 7, 10, 14, 11, 14,
		14, 2, 12, 15, 4, 13, 4, 13, 3, 1,
		3, 4, 4, 3, 4, 3, 13, 1, 4, 13,
		3, 1, 3, 1, 1, 0
	};

	private static byte[,] regularCellData = new byte[16, 16]
	{
		{
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0
		},
		{
			49, 0, 1, 2, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0
		},
		{
			98, 0, 1, 2, 3, 4, 5, 0, 0, 0,
			0, 0, 0, 0, 0, 0
		},
		{
			66, 0, 1, 2, 0, 2, 3, 0, 0, 0,
			0, 0, 0, 0, 0, 0
		},
		{
			83, 0, 1, 4, 1, 3, 4, 1, 2, 3,
			0, 0, 0, 0, 0, 0
		},
		{
			115, 0, 1, 2, 0, 2, 3, 4, 5, 6,
			0, 0, 0, 0, 0, 0
		},
		{
			147, 0, 1, 2, 3, 4, 5, 6, 7, 8,
			0, 0, 0, 0, 0, 0
		},
		{
			132, 0, 1, 4, 1, 3, 4, 1, 2, 3,
			5, 6, 7, 0, 0, 0
		},
		{
			132, 0, 1, 2, 0, 2, 3, 4, 5, 6,
			4, 6, 7, 0, 0, 0
		},
		{
			196, 0, 1, 2, 3, 4, 5, 6, 7, 8,
			9, 10, 11, 0, 0, 0
		},
		{
			100, 0, 4, 5, 0, 1, 4, 1, 3, 4,
			1, 2, 3, 0, 0, 0
		},
		{
			100, 0, 5, 4, 0, 4, 1, 1, 4, 3,
			1, 3, 2, 0, 0, 0
		},
		{
			100, 0, 4, 5, 0, 3, 4, 0, 1, 3,
			1, 2, 3, 0, 0, 0
		},
		{
			100, 0, 1, 2, 0, 2, 3, 0, 3, 4,
			0, 4, 5, 0, 0, 0
		},
		{
			117, 0, 1, 2, 0, 2, 3, 0, 3, 4,
			0, 4, 5, 0, 5, 6
		},
		{
			149, 0, 4, 5, 0, 3, 4, 0, 1, 3,
			1, 2, 3, 6, 7, 8
		}
	};

	private static ushort[,] regularVertexData = new ushort[256, 12]
	{
		{
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 20738, 13060, 0, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 8981, 16659, 0, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			20738, 13060, 8981, 16659, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			20738, 16931, 4902, 0, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			13060, 25089, 16931, 4902, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 8981, 16659, 20738, 16931, 4902, 0, 0, 0, 0,
			0, 0
		},
		{
			16931, 4902, 13060, 8981, 16659, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			16659, 33591, 16931, 0, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 20738, 13060, 16931, 16659, 33591, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 8981, 33591, 16931, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			20738, 13060, 8981, 33591, 16931, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			20738, 16659, 33591, 4902, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			16659, 33591, 4902, 13060, 25089, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 8981, 33591, 4902, 20738, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			13060, 8981, 33591, 4902, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			13060, 4422, 8773, 0, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 20738, 4422, 8773, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 8981, 16659, 13060, 4422, 8773, 0, 0, 0, 0,
			0, 0
		},
		{
			8981, 16659, 20738, 4422, 8773, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			20738, 16931, 4902, 13060, 4422, 8773, 0, 0, 0, 0,
			0, 0
		},
		{
			4422, 8773, 25089, 16931, 4902, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			13060, 4422, 8773, 25089, 8981, 16659, 20738, 16931, 4902, 0,
			0, 0
		},
		{
			16931, 4902, 4422, 8773, 8981, 16659, 0, 0, 0, 0,
			0, 0
		},
		{
			16931, 16659, 33591, 13060, 4422, 8773, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 20738, 4422, 8773, 16931, 16659, 33591, 0, 0, 0,
			0, 0
		},
		{
			16931, 25089, 8981, 33591, 13060, 4422, 8773, 0, 0, 0,
			0, 0
		},
		{
			16931, 33591, 8981, 8773, 4422, 20738, 0, 0, 0, 0,
			0, 0
		},
		{
			20738, 16659, 33591, 4902, 13060, 4422, 8773, 0, 0, 0,
			0, 0
		},
		{
			16659, 33591, 4902, 4422, 8773, 25089, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 8981, 33591, 4902, 20738, 13060, 4422, 8773, 0, 0,
			0, 0
		},
		{
			8773, 8981, 33591, 4902, 4422, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			8981, 8773, 33111, 0, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 20738, 13060, 8981, 8773, 33111, 0, 0, 0, 0,
			0, 0
		},
		{
			16659, 25089, 8773, 33111, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			8773, 33111, 16659, 20738, 13060, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			20738, 16931, 4902, 8981, 8773, 33111, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 16931, 4902, 13060, 8981, 8773, 33111, 0, 0, 0,
			0, 0
		},
		{
			25089, 8773, 33111, 16659, 20738, 16931, 4902, 0, 0, 0,
			0, 0
		},
		{
			16931, 4902, 13060, 8773, 33111, 16659, 0, 0, 0, 0,
			0, 0
		},
		{
			16931, 16659, 33591, 8981, 8773, 33111, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 20738, 13060, 16931, 16659, 33591, 8981, 8773, 33111, 0,
			0, 0
		},
		{
			33591, 16931, 25089, 8773, 33111, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			20738, 13060, 8773, 33111, 33591, 16931, 0, 0, 0, 0,
			0, 0
		},
		{
			20738, 16659, 33591, 4902, 8981, 8773, 33111, 0, 0, 0,
			0, 0
		},
		{
			16659, 33591, 4902, 13060, 25089, 8981, 8773, 33111, 0, 0,
			0, 0
		},
		{
			20738, 4902, 33591, 33111, 8773, 25089, 0, 0, 0, 0,
			0, 0
		},
		{
			33111, 33591, 4902, 13060, 8773, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			8981, 13060, 4422, 33111, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 20738, 4422, 33111, 8981, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			13060, 4422, 33111, 16659, 25089, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			16659, 20738, 4422, 33111, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			8981, 13060, 4422, 33111, 20738, 16931, 4902, 0, 0, 0,
			0, 0
		},
		{
			4902, 16931, 25089, 8981, 33111, 4422, 0, 0, 0, 0,
			0, 0
		},
		{
			13060, 4422, 33111, 16659, 25089, 20738, 16931, 4902, 0, 0,
			0, 0
		},
		{
			4902, 4422, 33111, 16659, 16931, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			8981, 13060, 4422, 33111, 16931, 16659, 33591, 0, 0, 0,
			0, 0
		},
		{
			25089, 20738, 4422, 33111, 8981, 16931, 16659, 33591, 0, 0,
			0, 0
		},
		{
			13060, 4422, 33111, 33591, 16931, 25089, 0, 0, 0, 0,
			0, 0
		},
		{
			16931, 20738, 4422, 33111, 33591, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			8981, 13060, 4422, 33111, 20738, 16659, 33591, 4902, 0, 0,
			0, 0
		},
		{
			25089, 16659, 33591, 4902, 4422, 33111, 8981, 0, 0, 0,
			0, 0
		},
		{
			25089, 13060, 4422, 33111, 33591, 4902, 20738, 0, 0, 0,
			0, 0
		},
		{
			4902, 4422, 33111, 33591, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			4902, 33383, 4422, 0, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 20738, 13060, 4902, 33383, 4422, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 8981, 16659, 4902, 33383, 4422, 0, 0, 0, 0,
			0, 0
		},
		{
			20738, 13060, 8981, 16659, 4902, 33383, 4422, 0, 0, 0,
			0, 0
		},
		{
			20738, 16931, 33383, 4422, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			13060, 25089, 16931, 33383, 4422, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			20738, 16931, 33383, 4422, 25089, 8981, 16659, 0, 0, 0,
			0, 0
		},
		{
			4422, 33383, 16931, 16659, 8981, 13060, 0, 0, 0, 0,
			0, 0
		},
		{
			16659, 33591, 16931, 4902, 33383, 4422, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 20738, 13060, 16931, 16659, 33591, 4902, 33383, 4422, 0,
			0, 0
		},
		{
			25089, 8981, 33591, 16931, 4902, 33383, 4422, 0, 0, 0,
			0, 0
		},
		{
			20738, 13060, 8981, 33591, 16931, 4902, 33383, 4422, 0, 0,
			0, 0
		},
		{
			33383, 4422, 20738, 16659, 33591, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 16659, 33591, 33383, 4422, 13060, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 8981, 33591, 33383, 4422, 20738, 0, 0, 0, 0,
			0, 0
		},
		{
			4422, 13060, 8981, 33591, 33383, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			13060, 4902, 33383, 8773, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			4902, 33383, 8773, 25089, 20738, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			13060, 4902, 33383, 8773, 25089, 8981, 16659, 0, 0, 0,
			0, 0
		},
		{
			4902, 33383, 8773, 8981, 16659, 20738, 0, 0, 0, 0,
			0, 0
		},
		{
			20738, 16931, 33383, 8773, 13060, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 16931, 33383, 8773, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			20738, 16931, 33383, 8773, 13060, 25089, 8981, 16659, 0, 0,
			0, 0
		},
		{
			16659, 16931, 33383, 8773, 8981, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			13060, 4902, 33383, 8773, 16931, 16659, 33591, 0, 0, 0,
			0, 0
		},
		{
			4902, 33383, 8773, 25089, 20738, 16931, 16659, 33591, 0, 0,
			0, 0
		},
		{
			13060, 4902, 33383, 8773, 16931, 25089, 8981, 33591, 0, 0,
			0, 0
		},
		{
			20738, 4902, 33383, 8773, 8981, 33591, 16931, 0, 0, 0,
			0, 0
		},
		{
			13060, 8773, 33383, 33591, 16659, 20738, 0, 0, 0, 0,
			0, 0
		},
		{
			33591, 33383, 8773, 25089, 16659, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			20738, 25089, 8981, 33591, 33383, 8773, 13060, 0, 0, 0,
			0, 0
		},
		{
			8981, 33591, 33383, 8773, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			8981, 8773, 33111, 4902, 33383, 4422, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 20738, 13060, 8981, 8773, 33111, 4902, 33383, 4422, 0,
			0, 0
		},
		{
			25089, 8773, 33111, 16659, 4902, 33383, 4422, 0, 0, 0,
			0, 0
		},
		{
			8773, 33111, 16659, 20738, 13060, 4902, 33383, 4422, 0, 0,
			0, 0
		},
		{
			16931, 33383, 4422, 20738, 8981, 8773, 33111, 0, 0, 0,
			0, 0
		},
		{
			13060, 25089, 16931, 33383, 4422, 8981, 8773, 33111, 0, 0,
			0, 0
		},
		{
			16931, 33383, 4422, 20738, 25089, 8773, 33111, 16659, 0, 0,
			0, 0
		},
		{
			13060, 8773, 33111, 16659, 16931, 33383, 4422, 0, 0, 0,
			0, 0
		},
		{
			16931, 16659, 33591, 8981, 8773, 33111, 4902, 33383, 4422, 0,
			0, 0
		},
		{
			25089, 20738, 13060, 16931, 16659, 33591, 8981, 8773, 33111, 4902,
			33383, 4422
		},
		{
			33591, 16931, 25089, 8773, 33111, 4902, 33383, 4422, 0, 0,
			0, 0
		},
		{
			16931, 20738, 13060, 8773, 33111, 33591, 4902, 33383, 4422, 0,
			0, 0
		},
		{
			33383, 4422, 20738, 16659, 33591, 8981, 8773, 33111, 0, 0,
			0, 0
		},
		{
			25089, 16659, 33591, 33383, 4422, 13060, 8981, 8773, 33111, 0,
			0, 0
		},
		{
			33591, 33383, 4422, 20738, 25089, 8773, 33111, 0, 0, 0,
			0, 0
		},
		{
			13060, 8773, 33111, 33591, 33383, 4422, 0, 0, 0, 0,
			0, 0
		},
		{
			33111, 8981, 13060, 4902, 33383, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			33383, 33111, 8981, 25089, 20738, 4902, 0, 0, 0, 0,
			0, 0
		},
		{
			33383, 4902, 13060, 25089, 16659, 33111, 0, 0, 0, 0,
			0, 0
		},
		{
			33383, 33111, 16659, 20738, 4902, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			20738, 16931, 33383, 33111, 8981, 13060, 0, 0, 0, 0,
			0, 0
		},
		{
			8981, 25089, 16931, 33383, 33111, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			13060, 20738, 16931, 33383, 33111, 16659, 25089, 0, 0, 0,
			0, 0
		},
		{
			16659, 16931, 33383, 33111, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			33111, 8981, 13060, 4902, 33383, 16931, 16659, 33591, 0, 0,
			0, 0
		},
		{
			33111, 8981, 25089, 20738, 4902, 33383, 16931, 16659, 33591, 0,
			0, 0
		},
		{
			33111, 33591, 16931, 25089, 13060, 4902, 33383, 0, 0, 0,
			0, 0
		},
		{
			20738, 4902, 33383, 33111, 33591, 16931, 0, 0, 0, 0,
			0, 0
		},
		{
			33383, 33111, 8981, 13060, 20738, 16659, 33591, 0, 0, 0,
			0, 0
		},
		{
			25089, 16659, 33591, 33383, 33111, 8981, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 13060, 20738, 33591, 33383, 33111, 0, 0, 0, 0,
			0, 0
		},
		{
			33591, 33383, 33111, 0, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			33591, 33111, 33383, 0, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 20738, 13060, 33591, 33111, 33383, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 8981, 16659, 33591, 33111, 33383, 0, 0, 0, 0,
			0, 0
		},
		{
			20738, 13060, 8981, 16659, 33591, 33111, 33383, 0, 0, 0,
			0, 0
		},
		{
			20738, 16931, 4902, 33591, 33111, 33383, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 16931, 4902, 13060, 33591, 33111, 33383, 0, 0, 0,
			0, 0
		},
		{
			25089, 8981, 16659, 20738, 16931, 4902, 33591, 33111, 33383, 0,
			0, 0
		},
		{
			16931, 4902, 13060, 8981, 16659, 33591, 33111, 33383, 0, 0,
			0, 0
		},
		{
			16659, 33111, 33383, 16931, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			16931, 16659, 33111, 33383, 25089, 20738, 13060, 0, 0, 0,
			0, 0
		},
		{
			33111, 33383, 16931, 25089, 8981, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			13060, 8981, 33111, 33383, 16931, 20738, 0, 0, 0, 0,
			0, 0
		},
		{
			4902, 20738, 16659, 33111, 33383, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			33111, 16659, 25089, 13060, 4902, 33383, 0, 0, 0, 0,
			0, 0
		},
		{
			4902, 20738, 25089, 8981, 33111, 33383, 0, 0, 0, 0,
			0, 0
		},
		{
			33383, 4902, 13060, 8981, 33111, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			13060, 4422, 8773, 33591, 33111, 33383, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 20738, 4422, 8773, 33591, 33111, 33383, 0, 0, 0,
			0, 0
		},
		{
			25089, 8981, 16659, 13060, 4422, 8773, 33591, 33111, 33383, 0,
			0, 0
		},
		{
			8981, 16659, 20738, 4422, 8773, 33591, 33111, 33383, 0, 0,
			0, 0
		},
		{
			20738, 16931, 4902, 13060, 4422, 8773, 33591, 33111, 33383, 0,
			0, 0
		},
		{
			4422, 8773, 25089, 16931, 4902, 33591, 33111, 33383, 0, 0,
			0, 0
		},
		{
			25089, 8981, 16659, 20738, 16931, 4902, 13060, 4422, 8773, 33591,
			33111, 33383
		},
		{
			16659, 16931, 4902, 4422, 8773, 8981, 33591, 33111, 33383, 0,
			0, 0
		},
		{
			16931, 16659, 33111, 33383, 13060, 4422, 8773, 0, 0, 0,
			0, 0
		},
		{
			25089, 20738, 4422, 8773, 16931, 16659, 33111, 33383, 0, 0,
			0, 0
		},
		{
			33111, 33383, 16931, 25089, 8981, 13060, 4422, 8773, 0, 0,
			0, 0
		},
		{
			8981, 33111, 33383, 16931, 20738, 4422, 8773, 0, 0, 0,
			0, 0
		},
		{
			4902, 20738, 16659, 33111, 33383, 13060, 4422, 8773, 0, 0,
			0, 0
		},
		{
			4902, 4422, 8773, 25089, 16659, 33111, 33383, 0, 0, 0,
			0, 0
		},
		{
			20738, 25089, 8981, 33111, 33383, 4902, 13060, 4422, 8773, 0,
			0, 0
		},
		{
			4902, 4422, 8773, 8981, 33111, 33383, 0, 0, 0, 0,
			0, 0
		},
		{
			8981, 8773, 33383, 33591, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			8981, 8773, 33383, 33591, 25089, 20738, 13060, 0, 0, 0,
			0, 0
		},
		{
			16659, 25089, 8773, 33383, 33591, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			20738, 16659, 33591, 33383, 8773, 13060, 0, 0, 0, 0,
			0, 0
		},
		{
			8981, 8773, 33383, 33591, 20738, 16931, 4902, 0, 0, 0,
			0, 0
		},
		{
			25089, 16931, 4902, 13060, 33591, 8981, 8773, 33383, 0, 0,
			0, 0
		},
		{
			16659, 25089, 8773, 33383, 33591, 20738, 16931, 4902, 0, 0,
			0, 0
		},
		{
			16659, 16931, 4902, 13060, 8773, 33383, 33591, 0, 0, 0,
			0, 0
		},
		{
			8981, 8773, 33383, 16931, 16659, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			8981, 8773, 33383, 16931, 16659, 25089, 20738, 13060, 0, 0,
			0, 0
		},
		{
			25089, 8773, 33383, 16931, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			13060, 8773, 33383, 16931, 20738, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			20738, 16659, 8981, 8773, 33383, 4902, 0, 0, 0, 0,
			0, 0
		},
		{
			16659, 8981, 8773, 33383, 4902, 13060, 25089, 0, 0, 0,
			0, 0
		},
		{
			20738, 25089, 8773, 33383, 4902, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			13060, 8773, 33383, 4902, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			33383, 33591, 8981, 13060, 4422, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			20738, 4422, 33383, 33591, 8981, 25089, 0, 0, 0, 0,
			0, 0
		},
		{
			13060, 4422, 33383, 33591, 16659, 25089, 0, 0, 0, 0,
			0, 0
		},
		{
			33591, 16659, 20738, 4422, 33383, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			33383, 33591, 8981, 13060, 4422, 20738, 16931, 4902, 0, 0,
			0, 0
		},
		{
			4422, 33383, 33591, 8981, 25089, 16931, 4902, 0, 0, 0,
			0, 0
		},
		{
			33383, 33591, 16659, 25089, 13060, 4422, 20738, 16931, 4902, 0,
			0, 0
		},
		{
			16659, 16931, 4902, 4422, 33383, 33591, 0, 0, 0, 0,
			0, 0
		},
		{
			13060, 8981, 16659, 16931, 33383, 4422, 0, 0, 0, 0,
			0, 0
		},
		{
			8981, 25089, 20738, 4422, 33383, 16931, 16659, 0, 0, 0,
			0, 0
		},
		{
			4422, 33383, 16931, 25089, 13060, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			20738, 4422, 33383, 16931, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			33383, 4902, 20738, 16659, 8981, 13060, 4422, 0, 0, 0,
			0, 0
		},
		{
			25089, 16659, 8981, 4902, 4422, 33383, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 13060, 4422, 33383, 4902, 20738, 0, 0, 0, 0,
			0, 0
		},
		{
			4902, 4422, 33383, 0, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			4902, 33591, 33111, 4422, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			33591, 33111, 4422, 4902, 25089, 20738, 13060, 0, 0, 0,
			0, 0
		},
		{
			33591, 33111, 4422, 4902, 25089, 8981, 16659, 0, 0, 0,
			0, 0
		},
		{
			16659, 20738, 13060, 8981, 4902, 33591, 33111, 4422, 0, 0,
			0, 0
		},
		{
			33591, 33111, 4422, 20738, 16931, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 16931, 33591, 33111, 4422, 13060, 0, 0, 0, 0,
			0, 0
		},
		{
			33591, 33111, 4422, 20738, 16931, 25089, 8981, 16659, 0, 0,
			0, 0
		},
		{
			16931, 33591, 33111, 4422, 13060, 8981, 16659, 0, 0, 0,
			0, 0
		},
		{
			16931, 16659, 33111, 4422, 4902, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			16931, 16659, 33111, 4422, 4902, 25089, 20738, 13060, 0, 0,
			0, 0
		},
		{
			4422, 33111, 8981, 25089, 16931, 4902, 0, 0, 0, 0,
			0, 0
		},
		{
			16931, 20738, 13060, 8981, 33111, 4422, 4902, 0, 0, 0,
			0, 0
		},
		{
			16659, 33111, 4422, 20738, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 16659, 33111, 4422, 13060, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			8981, 33111, 4422, 20738, 25089, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			8981, 33111, 4422, 13060, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			8773, 13060, 4902, 33591, 33111, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 8773, 33111, 33591, 4902, 20738, 0, 0, 0, 0,
			0, 0
		},
		{
			8773, 13060, 4902, 33591, 33111, 25089, 8981, 16659, 0, 0,
			0, 0
		},
		{
			8773, 8981, 16659, 20738, 4902, 33591, 33111, 0, 0, 0,
			0, 0
		},
		{
			16931, 33591, 33111, 8773, 13060, 20738, 0, 0, 0, 0,
			0, 0
		},
		{
			33111, 8773, 25089, 16931, 33591, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			8773, 13060, 20738, 16931, 33591, 33111, 16659, 25089, 8981, 0,
			0, 0
		},
		{
			16931, 33591, 33111, 8773, 8981, 16659, 0, 0, 0, 0,
			0, 0
		},
		{
			16659, 33111, 8773, 13060, 4902, 16931, 0, 0, 0, 0,
			0, 0
		},
		{
			4902, 16931, 16659, 33111, 8773, 25089, 20738, 0, 0, 0,
			0, 0
		},
		{
			33111, 8773, 13060, 4902, 16931, 25089, 8981, 0, 0, 0,
			0, 0
		},
		{
			20738, 4902, 16931, 8981, 33111, 8773, 0, 0, 0, 0,
			0, 0
		},
		{
			13060, 20738, 16659, 33111, 8773, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			16659, 33111, 8773, 25089, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			20738, 25089, 8981, 33111, 8773, 13060, 0, 0, 0, 0,
			0, 0
		},
		{
			8981, 33111, 8773, 0, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			4422, 4902, 33591, 8981, 8773, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			4422, 4902, 33591, 8981, 8773, 25089, 20738, 13060, 0, 0,
			0, 0
		},
		{
			25089, 8773, 4422, 4902, 33591, 16659, 0, 0, 0, 0,
			0, 0
		},
		{
			8773, 4422, 4902, 33591, 16659, 20738, 13060, 0, 0, 0,
			0, 0
		},
		{
			20738, 4422, 8773, 8981, 33591, 16931, 0, 0, 0, 0,
			0, 0
		},
		{
			4422, 13060, 25089, 16931, 33591, 8981, 8773, 0, 0, 0,
			0, 0
		},
		{
			33591, 16659, 25089, 8773, 4422, 20738, 16931, 0, 0, 0,
			0, 0
		},
		{
			16931, 33591, 16659, 13060, 8773, 4422, 0, 0, 0, 0,
			0, 0
		},
		{
			16659, 8981, 8773, 4422, 4902, 16931, 0, 0, 0, 0,
			0, 0
		},
		{
			4422, 4902, 16931, 16659, 8981, 8773, 25089, 20738, 13060, 0,
			0, 0
		},
		{
			4902, 16931, 25089, 8773, 4422, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			16931, 20738, 13060, 8773, 4422, 4902, 0, 0, 0, 0,
			0, 0
		},
		{
			8773, 4422, 20738, 16659, 8981, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			16659, 8981, 8773, 4422, 13060, 25089, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 8773, 4422, 20738, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			13060, 8773, 4422, 0, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			13060, 4902, 33591, 8981, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			20738, 4902, 33591, 8981, 25089, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 13060, 4902, 33591, 16659, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			20738, 4902, 33591, 16659, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			16931, 33591, 8981, 13060, 20738, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 16931, 33591, 8981, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			13060, 20738, 16931, 33591, 16659, 25089, 0, 0, 0, 0,
			0, 0
		},
		{
			16659, 16931, 33591, 0, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			16659, 8981, 13060, 4902, 16931, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			4902, 16931, 16659, 8981, 25089, 20738, 0, 0, 0, 0,
			0, 0
		},
		{
			13060, 4902, 16931, 25089, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			20738, 4902, 16931, 0, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			20738, 16659, 8981, 13060, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 16659, 8981, 0, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			25089, 13060, 20738, 0, 0, 0, 0, 0, 0, 0,
			0, 0
		},
		{
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0
		}
	};

	private static int[] indexConvert = new int[8] { 0, 1, 2, 3, 4, 5, 6, 7 };

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

	public Mesh RebuildMesh()
	{
		Mesh mesh = new Mesh();
		int subMeshCount = 1;
		mesh.Clear();
		_vertexList.Clear();
		_norm01.Clear();
		_norm2t.Clear();
		_baseIndex = 0;
		tempIndexBufferPingpongIdx = 0;
		List<int[]> list = new List<int[]>();
		for (int i = 0; i < 34; i++)
		{
			for (int j = 0; j < 34; j++)
			{
				for (int k = 0; k < 34; k++)
				{
					BuildRegularVoxelTriangles(k, j, i);
				}
			}
			tempIndexBufferPingpongIdx = (tempIndexBufferPingpongIdx + 1) % 2;
		}
		list.Add(_indexList.ToArray());
		mesh.vertices = _vertexList.ToArray();
		mesh.uv = _norm01.ToArray();
		mesh.uv2 = _norm2t.ToArray();
		mesh.subMeshCount = subMeshCount;
		mesh.SetTriangles(_indexList.ToArray(), 0);
		_indexList.Clear();
		return mesh;
	}

	private Vector3 VertexInterp(Vector3 p1, Vector3 p2, VoxelInterpolationInfo val1, VoxelInterpolationInfo val2, int index)
	{
		float t = (float)(128 - val1.Volume) / (float)(val2.Volume - val1.Volume);
		return Vector3.Lerp(p1, p2, t);
	}

	private void BuildRegularVoxelTriangles(int px, int py, int pz)
	{
		val[0].XYZ.x = px;
		val[0].XYZ.y = py;
		val[0].XYZ.z = pz;
		val[0].Volume = chunkData[OneIndex(px, py, pz) << 1];
		val[0].VType = chunkData[(OneIndex(px, py, pz) << 1) + 1];
		val[1].XYZ.x = px + 1;
		val[1].XYZ.y = py;
		val[1].XYZ.z = pz;
		val[1].Volume = chunkData[OneIndex(px + 1, py, pz) << 1];
		val[1].VType = chunkData[(OneIndex(px + 1, py, pz) << 1) + 1];
		val[2].XYZ.x = px;
		val[2].XYZ.y = py + 1;
		val[2].XYZ.z = pz;
		val[2].Volume = chunkData[OneIndex(px, py + 1, pz) << 1];
		val[2].VType = chunkData[(OneIndex(px, py + 1, pz) << 1) + 1];
		val[3].XYZ.x = px + 1;
		val[3].XYZ.y = py + 1;
		val[3].XYZ.z = pz;
		val[3].Volume = chunkData[OneIndex(px + 1, py + 1, pz) << 1];
		val[3].VType = chunkData[(OneIndex(px + 1, py + 1, pz) << 1) + 1];
		val[4].XYZ.x = px;
		val[4].XYZ.y = py;
		val[4].XYZ.z = pz + 1;
		val[4].Volume = chunkData[OneIndex(px, py, pz + 1) << 1];
		val[4].VType = chunkData[(OneIndex(px, py, pz + 1) << 1) + 1];
		val[5].XYZ.x = px + 1;
		val[5].XYZ.y = py;
		val[5].XYZ.z = pz + 1;
		val[5].Volume = chunkData[OneIndex(px + 1, py, pz + 1) << 1];
		val[5].VType = chunkData[(OneIndex(px + 1, py, pz + 1) << 1) + 1];
		val[6].XYZ.x = px;
		val[6].XYZ.y = py + 1;
		val[6].XYZ.z = pz + 1;
		val[6].Volume = chunkData[OneIndex(px, py + 1, pz + 1) << 1];
		val[6].VType = chunkData[(OneIndex(px, py + 1, pz + 1) << 1) + 1];
		val[7].XYZ.x = px + 1;
		val[7].XYZ.y = py + 1;
		val[7].XYZ.z = pz + 1;
		val[7].Volume = chunkData[OneIndex(px + 1, py + 1, pz + 1) << 1];
		val[7].VType = chunkData[(OneIndex(px + 1, py + 1, pz + 1) << 1) + 1];
		byte b = 0;
		if (val[0].Volume > 127)
		{
			b |= 1;
		}
		if (val[1].Volume > 127)
		{
			b |= 2;
		}
		if (val[2].Volume > 127)
		{
			b |= 4;
		}
		if (val[3].Volume > 127)
		{
			b |= 8;
		}
		if (val[4].Volume > 127)
		{
			b |= 0x10;
		}
		if (val[5].Volume > 127)
		{
			b |= 0x20;
		}
		if (val[6].Volume > 127)
		{
			b |= 0x40;
		}
		if (val[7].Volume > 127)
		{
			b |= 0x80;
		}
		if (b == 0)
		{
			return;
		}
		int num = 0;
		num = ((px == 0) ? 1 : 0);
		num |= (int)(((py == 0) ? 1u : 0u) << 1);
		num |= (int)(((pz == 0) ? 1u : 0u) << 2);
		int num2 = regularCellClass[b];
		int num3 = regularCellData[num2, 0] & 0xF;
		int num4 = num3 * 3;
		for (int i = 0; i < num4; i++)
		{
			int num5 = regularCellData[num2, i + 1];
			ushort num6 = regularVertexData[b, num5];
			byte b2 = (byte)(num6 >> 12);
			byte b3 = (byte)((num6 >> 8) & 0xF);
			int num7 = num6 & 0xF;
			int num8 = (num6 >> 4) & 0xF;
			IntVector3 intVector = new IntVector3();
			intVector.x = px - (b2 & 1);
			intVector.y = py - ((b2 >> 1) & 1);
			intVector.z = (b2 >> 2) & 1;
			int num9 = Mathf.Abs(tempIndexBufferPingpongIdx - intVector.z);
			int num10 = IndicesPerCell * (intVector.x + intVector.y * 35 + num9 * 1225) + b3;
			byte b4 = (byte)(num6 & 0xFF);
			if ((b2 & 8) > 0)
			{
				Vector3 item = VertexInterp(val[num7].XYZ, val[num8].XYZ, val[num7], val[num8], 0);
				if (num == 0)
				{
					_vertexList.Add(item);
					low8_array[0] = b4;
					addMatIndex(b);
					indexTempBuffer[num10] = _baseIndex;
					_indexList.Add(_baseIndex);
					_baseIndex++;
				}
				else if ((b3 == 1 && py > 0) || (b3 == 2 && px > 0) || (b3 == 3 && pz > 0))
				{
					_vertexList.Add(item);
					low8_array[0] = b4;
					addMatIndex(b);
					indexTempBuffer[num10] = _baseIndex;
					_baseIndex++;
				}
			}
			else if (num == 0)
			{
				ushort item2 = indexTempBuffer[num10];
				_indexList.Add(item2);
			}
		}
	}

	private void addMatIndex(byte cubeIndex)
	{
		byte[] array = new byte[3];
		int num = low8_array[0] & 0xF;
		int num2 = (low8_array[0] >> 4) & 0xF;
		array[0] = val[indexConvert[((cubeIndex & (1 << num)) <= 0) ? num2 : num]].VType;
		uint num3 = (uint)(array[0] * 4 + 2);
		uint num4 = (uint)(array[0] * 256 + array[0]);
		_norm2t.Add(new Vector2(num3, num4));
		_norm01.Add(new Vector2(0.1f, 0.2f));
	}
}
