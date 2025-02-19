using System;
using System.Collections.Generic;
using UnityEngine;

public class Block45Kernel
{
	private byte[] chunkData;

	public static int[] indicesConst;

	public List<Vector3> verts = new List<Vector3>();

	public List<Vector2> uvs = new List<Vector2>();

	public int matCnt;

	public int[] materialMap;

	public List<List<int>> subMeshIndices = new List<List<int>>();

	private int _baseIndex;

	private List<List<Vector3>> vertsByMaterial;

	private List<List<Vector2>> uvsByMaterial;

	public static int[] _2BitToExDir = new int[9] { 1, 0, 0, 0, 0, 1, 0, 1, 0 };

	private byte[] BlockExtendFlags = new byte[512];

	private byte[] BlockExtendFlagsResetBuffer = new byte[512];

	private byte[] t_neighbourPrimitiveTypes = new byte[6];

	private byte[] t_neighbourMaterialTypes = new byte[6];

	private int[] CornerCoordExtensionFlags = new int[24]
	{
		0, 0, 0, 0, 1, 0, 1, 1, 0, 1,
		0, 0, 0, 0, 1, 0, 1, 1, 1, 1,
		1, 1, 0, 1
	};

	private int[][] SpecialUVIndexTable = new int[23][]
	{
		null,
		null,
		new int[6] { 1, 2, 0, 1, 3, 2 },
		new int[6] { 1, 2, 0, 1, 3, 2 },
		new int[3] { 0, 2, 1 },
		new int[3] { 0, 2, 1 },
		null,
		null,
		new int[3] { 0, 2, 1 },
		new int[3] { 0, 2, 1 },
		new int[6] { 1, 2, 0, 1, 3, 2 },
		null,
		new int[36]
		{
			-1, 1, -1, -1, 1, -1, -1, 1, -1, -1,
			1, -1, -1, 1, -1, -1, 1, -1, -1, 0,
			-1, -1, 0, -1, -1, 0, -1, -1, 0, -1,
			-1, 0, -1, -1, 0, -1
		},
		new int[36]
		{
			-1, 3, -1, -1, 3, -1, -1, 3, -1, -1,
			3, -1, -1, 3, -1, -1, 3, -1, -1, -1,
			2, -1, -1, 2, -1, -1, 2, -1, -1, 2,
			-1, -1, 2, -1, -1, 2
		},
		new int[36]
		{
			-1, 1, -1, -1, 1, -1, -1, 1, -1, -1,
			1, -1, -1, 1, -1, -1, 1, -1, -1, -1,
			0, -1, -1, 0, -1, -1, 0, -1, -1, 0,
			-1, -1, 0, -1, -1, 0
		},
		null,
		null,
		new int[12]
		{
			2, -1, -1, 2, -1, -1, -1, 0, -1, -1,
			-1, 0
		},
		new int[30]
		{
			3, -1, -1, 3, -1, -1, 3, -1, -1, 3,
			-1, -1, 3, -1, 1, 1, -1, -1, 1, -1,
			-1, 1, -1, -1, 1, -1, -1, 1, 0, -1
		},
		new int[66]
		{
			2, -1, -1, 2, -1, -1, 2, -1, -1, 2,
			-1, -1, 2, -1, -1, 2, -1, -1, 2, -1,
			-1, 2, -1, -1, 2, -1, -1, 2, -1, -1,
			2, -1, -1, 0, -1, -1, 0, -1, -1, 0,
			-1, -1, 0, -1, -1, 0, -1, -1, 0, -1,
			-1, 0, -1, -1, 0, -1, -1, 0, -1, -1,
			0, -1, -1, 0, -1, -1
		},
		new int[66]
		{
			3, -1, -1, 3, -1, -1, 3, -1, -1, 3,
			-1, -1, 3, -1, -1, 3, -1, -1, 3, -1,
			-1, 3, -1, -1, 3, -1, -1, 3, -1, -1,
			3, -1, -1, 1, -1, -1, 1, -1, -1, 1,
			-1, -1, 1, -1, -1, 1, -1, -1, 1, -1,
			-1, 1, -1, -1, 1, -1, -1, 1, -1, -1,
			1, -1, -1, 1, -1, -1
		},
		null,
		null
	};

	private Vector2[] SpecialUVs = new Vector2[4]
	{
		new Vector2(0f, 0f),
		new Vector2(0f, 1f),
		new Vector2(1f, 0f),
		new Vector2(1f, 1f)
	};

	private byte[] MaterialGroups = new byte[16]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 1, 0
	};

	private int[] BlockTypeTable = new int[46]
	{
		0, -1, 0, -1, 0, 0, 0, 9, 0, 18,
		0, 24, 0, 30, 0, 45, 0, 60, 0, 66,
		0, 72, 0, 81, 0, 96, 0, 171, 0, 246,
		0, 321, 0, 390, 0, 459, 0, 528, 0, 597,
		0, 738, 0, 879, 0, 888
	};

	private short[] InteriorFaceVertTable = new short[897]
	{
		4, 2, 3, 4, 5, 2, -1, -1, -1, 7,
		1, 3, 7, 5, 1, -1, -1, -1, 4, 1,
		3, -1, -1, -1, 7, 5, 2, -1, -1, -1,
		0, 8, 3, 3, 8, 2, 2, 8, 1, 1,
		8, 0, -1, -1, -1, 4, 7, 8, 7, 6,
		8, 6, 5, 8, 5, 4, 8, -1, -1, -1,
		6, 1, 3, -1, -1, -1, 7, 5, 0, -1,
		-1, -1, 7, 1, 0, 7, 6, 1, -1, -1,
		-1, 4, 5, 8, 5, 1, 8, 1, 0, 8,
		4, 8, 0, -1, -1, -1, 9, 0, 3, 10,
		0, 9, 11, 0, 10, 12, 0, 11, 13, 0,
		12, 1, 0, 13, 7, 4, 14, 14, 4, 15,
		15, 4, 16, 16, 4, 17, 17, 4, 18, 18,
		4, 5, 7, 9, 3, 7, 14, 9, 14, 10,
		9, 14, 15, 10, 15, 11, 10, 15, 16, 11,
		16, 12, 11, 16, 17, 12, 17, 13, 12, 17,
		18, 13, 18, 1, 13, 18, 5, 1, -1, -1,
		-1, 3, 0, 19, 19, 0, 20, 20, 0, 21,
		21, 0, 22, 22, 0, 23, 23, 0, 4, 2,
		24, 1, 24, 25, 1, 25, 26, 1, 26, 27,
		1, 27, 28, 1, 28, 5, 1, 3, 24, 2,
		3, 19, 24, 19, 25, 24, 19, 20, 25, 20,
		26, 25, 20, 21, 26, 21, 27, 26, 21, 22,
		27, 22, 28, 27, 22, 23, 28, 23, 5, 28,
		23, 4, 5, -1, -1, -1, 0, 4, 29, 29,
		4, 30, 30, 4, 31, 31, 4, 32, 32, 4,
		33, 33, 4, 7, 1, 34, 5, 34, 35, 5,
		35, 36, 5, 36, 37, 5, 37, 38, 5, 38,
		6, 5, 0, 34, 1, 0, 29, 34, 29, 35,
		34, 29, 30, 35, 30, 36, 35, 30, 31, 36,
		31, 37, 36, 31, 32, 37, 32, 38, 37, 32,
		33, 38, 33, 6, 38, 33, 7, 6, -1, -1,
		-1, 7, 39, 40, 7, 40, 41, 7, 41, 42,
		7, 42, 43, 7, 43, 4, 6, 45, 44, 6,
		46, 45, 6, 47, 46, 6, 48, 47, 6, 5,
		48, 7, 6, 44, 7, 44, 39, 39, 44, 45,
		39, 45, 40, 40, 45, 46, 40, 46, 41, 41,
		46, 47, 41, 47, 42, 42, 47, 48, 42, 48,
		43, 43, 48, 5, 43, 5, 4, -1, -1, -1,
		3, 50, 49, 3, 51, 50, 3, 52, 51, 3,
		53, 52, 3, 0, 53, 2, 58, 1, 2, 57,
		58, 2, 56, 57, 2, 55, 56, 2, 54, 55,
		3, 54, 2, 3, 49, 54, 49, 55, 54, 49,
		50, 55, 50, 56, 55, 50, 51, 56, 51, 57,
		56, 51, 52, 57, 52, 58, 57, 52, 53, 58,
		53, 1, 58, 53, 0, 1, -1, -1, -1, 4,
		63, 0, 4, 68, 63, 64, 1, 59, 64, 5,
		1, 68, 62, 63, 68, 67, 62, 65, 59, 60,
		65, 64, 59, 67, 61, 62, 67, 66, 61, 66,
		60, 61, 66, 65, 60, 0, 63, 62, 0, 62,
		61, 0, 61, 60, 0, 60, 59, 0, 59, 1,
		4, 67, 68, 4, 66, 67, 4, 65, 66, 4,
		64, 65, 4, 5, 64, -1, -1, -1, 4, 69,
		70, 4, 70, 71, 4, 71, 72, 4, 72, 73,
		4, 73, 0, 5, 75, 74, 5, 76, 75, 5,
		77, 76, 5, 78, 77, 5, 1, 78, 4, 5,
		74, 4, 74, 69, 69, 74, 75, 69, 75, 70,
		70, 75, 76, 70, 76, 71, 71, 76, 77, 71,
		77, 72, 72, 77, 78, 72, 78, 73, 73, 78,
		1, 73, 1, 0, -1, -1, -1, 79, 80, 81,
		79, 81, 82, 79, 82, 83, 79, 83, 84, 79,
		84, 85, 79, 85, 86, 79, 86, 87, 79, 87,
		88, 79, 88, 89, 79, 89, 90, 79, 90, 91,
		92, 104, 103, 92, 103, 102, 92, 102, 101, 92,
		101, 100, 92, 100, 99, 92, 99, 98, 92, 98,
		97, 92, 97, 96, 92, 96, 95, 92, 95, 94,
		92, 94, 93, 92, 80, 79, 92, 93, 80, 93,
		81, 80, 93, 94, 81, 94, 82, 81, 94, 95,
		82, 95, 83, 82, 95, 96, 83, 96, 84, 83,
		96, 97, 84, 97, 85, 84, 97, 98, 85, 98,
		86, 85, 98, 99, 86, 99, 87, 86, 99, 100,
		87, 100, 88, 87, 100, 101, 88, 101, 89, 88,
		101, 102, 89, 102, 90, 89, 102, 103, 90, 103,
		91, 90, 103, 104, 91, -1, -1, -1, 105, 107,
		106, 105, 108, 107, 105, 109, 108, 105, 110, 109,
		105, 111, 110, 105, 112, 111, 105, 113, 112, 105,
		114, 113, 105, 115, 114, 105, 116, 115, 105, 117,
		116, 118, 129, 130, 118, 128, 129, 118, 127, 128,
		118, 126, 127, 118, 125, 126, 118, 124, 125, 118,
		123, 124, 118, 122, 123, 118, 121, 122, 118, 120,
		121, 118, 119, 120, 118, 105, 106, 118, 106, 119,
		119, 106, 107, 119, 107, 120, 120, 107, 108, 120,
		108, 121, 121, 108, 109, 121, 109, 122, 122, 109,
		110, 122, 110, 123, 123, 110, 111, 123, 111, 124,
		124, 111, 112, 124, 112, 125, 125, 112, 113, 125,
		113, 126, 126, 113, 114, 126, 114, 127, 127, 114,
		115, 127, 115, 128, 128, 115, 116, 128, 116, 129,
		129, 116, 117, 129, 117, 130, -1, -1, -1, 4,
		2, 3, 4, 1, 2, -1, -1, -1, 7, 6,
		0, 6, 5, 0, -1, -1, -1
	};

	private Vector2[] VertUVByIndexXZ = new Vector2[8]
	{
		new Vector2(0f, 0f),
		new Vector2(1f, 0f),
		new Vector2(1f, 1f),
		new Vector2(0f, 1f),
		new Vector2(0f, 0f),
		new Vector2(1f, 0f),
		new Vector2(1f, 1f),
		new Vector2(0f, 1f)
	};

	private Vector2[] InteriorVertUVByIndex = new Vector2[143]
	{
		new Vector2(0f, 0f),
		new Vector2(1f, 0f),
		new Vector2(0f, 0f),
		new Vector2(1f, 0f),
		new Vector2(0f, 1f),
		new Vector2(1f, 1f),
		new Vector2(0f, 1f),
		new Vector2(1f, 1f),
		new Vector2(0.5f, 0.5f),
		new Vector2(0.6666666f, 0f),
		new Vector2(0.3333333f, 0f),
		new Vector2(0f, 0f),
		new Vector2(0.3333333f, 0f),
		new Vector2(2f / 3f, 0f),
		new Vector2(0.6666666f, 1f),
		new Vector2(0.3333333f, 1f),
		new Vector2(0f, 1f),
		new Vector2(0.3333333f, 1f),
		new Vector2(2f / 3f, 1f),
		new Vector2(0.6666666f, 0f),
		new Vector2(0.3333333f, 0f),
		new Vector2(0f, 0f),
		new Vector2(0.3333333f, 0f),
		new Vector2(2f / 3f, 0f),
		new Vector2(0.6666666f, 1f),
		new Vector2(0.3333333f, 1f),
		new Vector2(0f, 1f),
		new Vector2(0.3333333f, 1f),
		new Vector2(2f / 3f, 1f),
		new Vector2(0.6666666f, 0f),
		new Vector2(0.3333333f, 0f),
		new Vector2(0f, 0f),
		new Vector2(0.3333333f, 0f),
		new Vector2(2f / 3f, 0f),
		new Vector2(0.6666666f, 1f),
		new Vector2(0.3333333f, 1f),
		new Vector2(0f, 1f),
		new Vector2(0.3333333f, 1f),
		new Vector2(2f / 3f, 1f),
		new Vector2(0f, 0.3333333f),
		new Vector2(0f, 2f / 3f),
		new Vector2(0f, 1f),
		new Vector2(0f, 0.6666666f),
		new Vector2(0f, 0.3333333f),
		new Vector2(1f, 0.3333333f),
		new Vector2(1f, 2f / 3f),
		new Vector2(1f, 1f),
		new Vector2(1f, 0.6666666f),
		new Vector2(1f, 0.3333333f),
		new Vector2(0f, 0.3333333f),
		new Vector2(0f, 2f / 3f),
		new Vector2(0f, 1f),
		new Vector2(0f, 0.6666666f),
		new Vector2(0f, 0.3333333f),
		new Vector2(1f, 0.3333333f),
		new Vector2(1f, 2f / 3f),
		new Vector2(1f, 1f),
		new Vector2(1f, 0.6666666f),
		new Vector2(1f, 0.3333333f),
		new Vector2(0f, 0.3333333f),
		new Vector2(0f, 2f / 3f),
		new Vector2(0f, 1f),
		new Vector2(0f, 0.6666666f),
		new Vector2(0f, 0.3333333f),
		new Vector2(1f, 0.3333333f),
		new Vector2(1f, 2f / 3f),
		new Vector2(1f, 1f),
		new Vector2(1f, 0.6666666f),
		new Vector2(1f, 0.3333333f),
		new Vector2(0f, 0.3333333f),
		new Vector2(0f, 2f / 3f),
		new Vector2(0f, 1f),
		new Vector2(0f, 0.6666666f),
		new Vector2(0f, 0.3333333f),
		new Vector2(1f, 0.3333333f),
		new Vector2(1f, 2f / 3f),
		new Vector2(1f, 1f),
		new Vector2(1f, 0.6666666f),
		new Vector2(1f, 0.3333333f),
		new Vector2(0f, 0f),
		new Vector2(0f, 0.1666667f),
		new Vector2(0f, 0.3333333f),
		new Vector2(0f, 0.5f),
		new Vector2(0f, 2f / 3f),
		new Vector2(0f, 0.8333334f),
		new Vector2(0f, 1f),
		new Vector2(0f, 5f / 6f),
		new Vector2(0f, 0.6666666f),
		new Vector2(0f, 0.5f),
		new Vector2(0f, 0.3333333f),
		new Vector2(0f, 0.1666666f),
		new Vector2(0f, 0f),
		new Vector2(1f, 0f),
		new Vector2(1f, 0.1666667f),
		new Vector2(1f, 0.3333333f),
		new Vector2(1f, 0.5f),
		new Vector2(1f, 2f / 3f),
		new Vector2(1f, 0.8333334f),
		new Vector2(1f, 1f),
		new Vector2(1f, 5f / 6f),
		new Vector2(1f, 0.6666666f),
		new Vector2(1f, 0.5f),
		new Vector2(1f, 0.3333333f),
		new Vector2(1f, 0.1666666f),
		new Vector2(1f, 0f),
		new Vector2(0f, 0f),
		new Vector2(0f, 0.1666667f),
		new Vector2(0f, 0.3333333f),
		new Vector2(0f, 0.5f),
		new Vector2(0f, 2f / 3f),
		new Vector2(0f, 0.8333334f),
		new Vector2(0f, 1f),
		new Vector2(0f, 5f / 6f),
		new Vector2(0f, 0.6666666f),
		new Vector2(0f, 0.5f),
		new Vector2(0f, 0.3333333f),
		new Vector2(0f, 0.1666666f),
		new Vector2(0f, 0f),
		new Vector2(1f, 0f),
		new Vector2(1f, 0.1666667f),
		new Vector2(1f, 0.3333333f),
		new Vector2(1f, 0.5f),
		new Vector2(1f, 2f / 3f),
		new Vector2(1f, 0.8333334f),
		new Vector2(1f, 1f),
		new Vector2(1f, 5f / 6f),
		new Vector2(1f, 0.6666666f),
		new Vector2(1f, 0.5f),
		new Vector2(1f, 0.3333333f),
		new Vector2(1f, 0.1666666f),
		new Vector2(1f, 0f),
		new Vector2(0f, 1f),
		new Vector2(1f, 0f),
		new Vector2(0f, 0f),
		new Vector2(1f, 1f),
		new Vector2(1f, 0f),
		new Vector2(0f, 0f),
		new Vector2(1f, 0f),
		new Vector2(1f, 1f),
		new Vector2(0f, 0f),
		new Vector2(1f, 0f),
		new Vector2(1f, 1f),
		new Vector2(0f, 0f)
	};

	private Vector2[] InteriorVertUV = new Vector2[102]
	{
		new Vector2(0f, 1f),
		new Vector2(1f, 0f),
		new Vector2(0f, 0f),
		new Vector2(0f, 1f),
		new Vector2(1f, 1f),
		new Vector2(1f, 0f),
		Vector2.zero,
		Vector2.zero,
		Vector2.zero,
		new Vector2(0f, 1f),
		new Vector2(1f, 0f),
		new Vector2(0f, 0f),
		new Vector2(0f, 1f),
		new Vector2(1f, 1f),
		new Vector2(1f, 0f),
		Vector2.zero,
		Vector2.zero,
		Vector2.zero,
		new Vector2(0f, 1f),
		new Vector2(1f, 0f),
		new Vector2(0f, 0f),
		Vector2.zero,
		Vector2.zero,
		Vector2.zero,
		new Vector2(0f, 1f),
		new Vector2(1f, 1f),
		new Vector2(1f, 0f),
		Vector2.zero,
		Vector2.zero,
		Vector2.zero,
		new Vector2(0f, 1f),
		new Vector2(0.5f, 0.5f),
		new Vector2(0f, 0f),
		new Vector2(0f, 0f),
		new Vector2(0.5f, 0.5f),
		new Vector2(1f, 0f),
		new Vector2(1f, 0f),
		new Vector2(0.5f, 0.5f),
		new Vector2(1f, 1f),
		new Vector2(1f, 1f),
		new Vector2(0.5f, 0.5f),
		new Vector2(0f, 1f),
		Vector2.zero,
		Vector2.zero,
		Vector2.zero,
		new Vector2(0f, 1f),
		new Vector2(0.5f, 0.5f),
		new Vector2(0f, 0f),
		new Vector2(0f, 0f),
		new Vector2(0.5f, 0.5f),
		new Vector2(1f, 0f),
		new Vector2(1f, 0f),
		new Vector2(0.5f, 0.5f),
		new Vector2(1f, 1f),
		new Vector2(1f, 1f),
		new Vector2(0.5f, 0.5f),
		new Vector2(0f, 1f),
		Vector2.zero,
		Vector2.zero,
		Vector2.zero,
		new Vector2(0f, 1f),
		new Vector2(1f, 1f),
		new Vector2(1f, 0f),
		Vector2.zero,
		Vector2.zero,
		Vector2.zero,
		new Vector2(1f, 1f),
		new Vector2(0f, 1f),
		new Vector2(1f, 0f),
		Vector2.zero,
		Vector2.zero,
		Vector2.zero,
		new Vector2(0f, 1f),
		new Vector2(1f, 0f),
		new Vector2(0f, 0f),
		new Vector2(0f, 1f),
		new Vector2(1f, 1f),
		new Vector2(1f, 0f),
		Vector2.zero,
		Vector2.zero,
		Vector2.zero,
		new Vector2(0f, 1f),
		new Vector2(0.5f, 0.5f),
		new Vector2(0f, 0f),
		new Vector2(0f, 0f),
		new Vector2(0.5f, 0.5f),
		new Vector2(1f, 0f),
		new Vector2(1f, 0f),
		new Vector2(0.5f, 0.5f),
		new Vector2(1f, 1f),
		new Vector2(1f, 1f),
		new Vector2(0.5f, 0.5f),
		new Vector2(0f, 1f),
		Vector2.zero,
		Vector2.zero,
		Vector2.zero,
		new Vector2(0f, 1f),
		new Vector2(0.16667f, 0f),
		new Vector2(0f, 0f),
		new Vector2(0f, 1f),
		new Vector2(0.16667f, 1f),
		new Vector2(0.16667f, 0f)
	};

	private Vector2[] EdgeVertUV = new Vector2[54]
	{
		new Vector2(1f, 0f),
		new Vector2(0f, 0f),
		Vector2.zero,
		Vector2.zero,
		new Vector2(0f, 0f),
		Vector2.zero,
		new Vector2(0f, 0f),
		Vector2.zero,
		Vector2.zero,
		new Vector2(1f, 0f),
		new Vector2(1f, 0f),
		Vector2.zero,
		Vector2.zero,
		Vector2.zero,
		new Vector2(1f, 0f),
		new Vector2(0f, 0f),
		new Vector2(1f, 1f),
		Vector2.zero,
		Vector2.zero,
		new Vector2(1f, 0f),
		new Vector2(0f, 0f),
		Vector2.zero,
		new Vector2(0f, 1f),
		Vector2.zero,
		new Vector2(1f, 1f),
		new Vector2(0f, 1f),
		Vector2.zero,
		Vector2.zero,
		Vector2.zero,
		new Vector2(0f, 1f),
		new Vector2(0f, 1f),
		Vector2.zero,
		Vector2.zero,
		new Vector2(1f, 1f),
		Vector2.zero,
		new Vector2(1f, 1f),
		Vector2.zero,
		Vector2.zero,
		new Vector2(1f, 1f),
		new Vector2(0f, 1f),
		Vector2.zero,
		new Vector2(1f, 0f),
		Vector2.zero,
		new Vector2(1f, 1f),
		new Vector2(0f, 1f),
		Vector2.zero,
		Vector2.zero,
		new Vector2(0f, 0f),
		new Vector2(0.5f, 0.5f),
		new Vector2(0.5f, 0.5f),
		new Vector2(0.5f, 0.5f),
		new Vector2(0.5f, 0.5f),
		Vector2.zero,
		Vector2.zero
	};

	private short[] EdgeFaceVertTable = new short[828]
	{
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, 5, 0, 1, 5,
		4, 0, 4, 3, 0, 4, 7, 3, 7, 2,
		3, 7, 6, 2, 6, 1, 2, 6, 5, 1,
		3, 1, 0, 3, 2, 1, 4, 6, 7, 4,
		5, 6, 5, 0, 1, 5, 4, 0, 4, 3,
		0, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		5, 1, 2, -1, -1, -1, 3, 1, 0, 3,
		2, 1, -1, -1, -1, -1, -1, -1, 5, 0,
		1, 5, 4, 0, 4, 3, 0, 4, 7, 3,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, 3, 1, 0, -1, -1, -1, 4, 5,
		7, -1, -1, -1, 4, 0, 1, -1, -1, -1,
		4, 3, 0, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, 3, 1,
		0, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		5, 0, 1, 5, 4, 0, 0, 4, 3, 4,
		7, 3, 7, 2, 3, -1, -1, -1, 5, 1,
		2, -1, -1, -1, 0, 3, 2, 0, 2, 1,
		4, 5, 7, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		0, 3, 2, 0, 2, 1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, 4, 5, 6, 4, 6, 7, 5, 0,
		1, 5, 4, 0, 0, 4, 3, 4, 7, 3,
		7, 6, 3, -1, -1, -1, 5, 1, 6, -1,
		-1, -1, 0, 3, 1, -1, -1, -1, 4, 5,
		7, 7, 5, 6, 4, 0, 5, -1, -1, -1,
		4, 7, 0, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, 4, 5, 7, -1, -1, -1,
		5, 0, 1, 5, 4, 0, 4, 7, 0, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, 6, 5,
		1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		7, 4, 5, 7, 5, 6, 5, 0, 1, 5,
		4, 0, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, 5, 4, 0, 5, 0, 1, 4, 3,
		0, 4, 7, 3, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, 5, 0,
		1, 5, 4, 0, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, 3, 1, 0, 3, 2, 1, -1, -1,
		-1, -1, -1, -1, 5, 0, 1, 5, 4, 0,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, 4, 6, 7, 4, 5, 6,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		4, 6, 7, 4, 5, 6, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		3, 1, 0, 3, 2, 1, -1, -1, -1, -1,
		-1, -1, 5, 0, 1, 5, 4, 0, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, 5, 0,
		1, 5, 4, 0, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, 4, 0, 1, -1,
		-1, -1, 4, 3, 0, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		0, 2, 1, 0, 3, 2, -1, -1, -1, -1,
		-1, -1, 5, 4, 0, -1, -1, -1, 4, 7,
		0, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, 4, 6, 7, 4, 5, 6
	};

	private byte[] faceTriMask = new byte[138]
	{
		0, 0, 0, 0, 0, 0, 15, 15, 15, 15,
		255, 255, 15, 12, 0, 9, 255, 0, 15, 15,
		0, 0, 204, 204, 9, 12, 0, 0, 204, 0,
		15, 15, 12, 9, 255, 102, 0, 0, 0, 0,
		255, 0, 0, 0, 0, 0, 0, 255, 15, 15,
		12, 9, 102, 255, 3, 6, 0, 0, 0, 102,
		15, 6, 0, 3, 0, 255, 15, 0, 0, 0,
		0, 0, 15, 15, 0, 0, 0, 0, 15, 0,
		0, 0, 255, 0, 15, 0, 0, 0, 0, 255,
		0, 0, 0, 0, 0, 255, 0, 0, 0, 0,
		255, 0, 15, 0, 0, 0, 0, 0, 15, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 9, 12, 0, 0,
		255, 0, 3, 6, 0, 0, 0, 255
	};

	public static Vector3[] indexedCoords = new Vector3[131]
	{
		new Vector3(0f, 0f, 0f),
		new Vector3(0f, 0f, 1f),
		new Vector3(1f, 0f, 1f),
		new Vector3(1f, 0f, 0f),
		new Vector3(0f, 1f, 0f),
		new Vector3(0f, 1f, 1f),
		new Vector3(1f, 1f, 1f),
		new Vector3(1f, 1f, 0f),
		new Vector3(0.5f, 0.5f, 0.5f),
		new Vector3(0.96593f, 0f, 0.25882f),
		new Vector3(0.86603f, 0f, 0.5f),
		new Vector3(0.70711f, 0f, 0.70711f),
		new Vector3(0.5f, 0f, 0.86603f),
		new Vector3(0.25882f, 0f, 0.96593f),
		new Vector3(0.96593f, 1f, 0.25882f),
		new Vector3(0.86603f, 1f, 0.5f),
		new Vector3(0.70711f, 1f, 0.70711f),
		new Vector3(0.5f, 1f, 0.86603f),
		new Vector3(0.25882f, 1f, 0.96593f),
		new Vector3(0.96593f, 0.25882f, 0f),
		new Vector3(0.86603f, 0.5f, 0f),
		new Vector3(0.70711f, 0.70711f, 0f),
		new Vector3(0.5f, 0.86603f, 0f),
		new Vector3(0.25882f, 0.96593f, 0f),
		new Vector3(0.96593f, 0.25882f, 1f),
		new Vector3(0.86603f, 0.5f, 1f),
		new Vector3(0.70711f, 0.70711f, 1f),
		new Vector3(0.5f, 0.86603f, 1f),
		new Vector3(0.25882f, 0.96593f, 1f),
		new Vector3(0.25882f, 0.034070015f, 0f),
		new Vector3(0.5f, 0.13397002f, 0f),
		new Vector3(0.70711f, 0.29289f, 0f),
		new Vector3(0.86603f, 0.5f, 0f),
		new Vector3(0.96593f, 0.74118f, 0f),
		new Vector3(0.25882f, 0.034070015f, 1f),
		new Vector3(0.5f, 0.13397002f, 1f),
		new Vector3(0.70711f, 0.29289f, 1f),
		new Vector3(0.86603f, 0.5f, 1f),
		new Vector3(0.96593f, 0.74118f, 1f),
		new Vector3(0.93301f, 0.75f, 0f),
		new Vector3(0.75f, 0.56699f, 0f),
		new Vector3(0.5f, 0.5f, 0f),
		new Vector3(0.25f, 0.56699f, 0f),
		new Vector3(0.06699f, 0.75f, 0f),
		new Vector3(0.93301f, 0.75f, 1f),
		new Vector3(0.75f, 0.56699f, 1f),
		new Vector3(0.5f, 0.5f, 1f),
		new Vector3(0.25f, 0.56699f, 1f),
		new Vector3(0.06699f, 0.75f, 1f),
		new Vector3(0.93301f, 0.25f, 0f),
		new Vector3(0.75f, 0.43301f, 0f),
		new Vector3(0.5f, 0.5f, 0f),
		new Vector3(0.25f, 0.43301f, 0f),
		new Vector3(0.06699f, 0.25f, 0f),
		new Vector3(0.93301f, 0.25f, 1f),
		new Vector3(0.75f, 0.43301f, 1f),
		new Vector3(0.5f, 0.5f, 1f),
		new Vector3(0.25f, 0.43301f, 1f),
		new Vector3(0.06699f, 0.25f, 1f),
		new Vector3(0.25f, 0f, 0.93301f),
		new Vector3(0.43301f, 0f, 0.75f),
		new Vector3(0.5f, 0f, 0.5f),
		new Vector3(0.43301f, 0f, 0.25f),
		new Vector3(0.25f, 0f, 0.06699f),
		new Vector3(0.25f, 1f, 0.93301f),
		new Vector3(0.43301f, 1f, 0.75f),
		new Vector3(0.5f, 1f, 0.5f),
		new Vector3(0.43301f, 1f, 0.25f),
		new Vector3(0.25f, 1f, 0.06699f),
		new Vector3(0.25f, 0.93301f, 0f),
		new Vector3(0.43301f, 0.75f, 0f),
		new Vector3(0.5f, 0.5f, 0f),
		new Vector3(0.43301f, 0.25f, 0f),
		new Vector3(0.25f, 0.06699f, 0f),
		new Vector3(0.25f, 0.93301f, 1f),
		new Vector3(0.43301f, 0.75f, 1f),
		new Vector3(0.5f, 0.5f, 1f),
		new Vector3(0.43301f, 0.25f, 1f),
		new Vector3(0.25f, 0.06699f, 1f),
		new Vector3(0.93301f, 0f, 0.75f),
		new Vector3(0.75f, 0f, 0.93301f),
		new Vector3(0.5f, 0f, 1f),
		new Vector3(0.25f, 0f, 0.93301f),
		new Vector3(0.06699f, 0f, 0.75f),
		new Vector3(0f, 0f, 0.5f),
		new Vector3(0.06699f, 0f, 0.25f),
		new Vector3(0.25f, 0f, 0.06699f),
		new Vector3(0.5f, 0f, 0f),
		new Vector3(0.75f, 0f, 0.06699f),
		new Vector3(0.93301f, 0f, 0.25f),
		new Vector3(1f, 0f, 0.5f),
		new Vector3(0.93301f, 0f, 0.75f),
		new Vector3(0.93301f, 1f, 0.75f),
		new Vector3(0.75f, 1f, 0.93301f),
		new Vector3(0.5f, 1f, 1f),
		new Vector3(0.25f, 1f, 0.93301f),
		new Vector3(0.06699f, 1f, 0.75f),
		new Vector3(0f, 1f, 0.5f),
		new Vector3(0.06699f, 1f, 0.25f),
		new Vector3(0.25f, 1f, 0.06699f),
		new Vector3(0.5f, 1f, 0f),
		new Vector3(0.75f, 1f, 0.06699f),
		new Vector3(0.93301f, 1f, 0.25f),
		new Vector3(1f, 1f, 0.5f),
		new Vector3(0.93301f, 1f, 0.75f),
		new Vector3(0.93301f, 0.75f, 0f),
		new Vector3(0.75f, 0.93301f, 0f),
		new Vector3(0.5f, 1f, 0f),
		new Vector3(0.25f, 0.93301f, 0f),
		new Vector3(0.06699f, 0.75f, 0f),
		new Vector3(0f, 0.5f, 0f),
		new Vector3(0.06699f, 0.25f, 0f),
		new Vector3(0.25f, 0.06699f, 0f),
		new Vector3(0.5f, 0f, 0f),
		new Vector3(0.75f, 0.06699f, 0f),
		new Vector3(0.93301f, 0.25f, 0f),
		new Vector3(1f, 0.5f, 0f),
		new Vector3(0.93301f, 0.75f, 0f),
		new Vector3(0.93301f, 0.75f, 1f),
		new Vector3(0.75f, 0.93301f, 1f),
		new Vector3(0.5f, 1f, 1f),
		new Vector3(0.25f, 0.93301f, 1f),
		new Vector3(0.06699f, 0.75f, 1f),
		new Vector3(0f, 0.5f, 1f),
		new Vector3(0.06699f, 0.25f, 1f),
		new Vector3(0.25f, 0.06699f, 1f),
		new Vector3(0.5f, 0f, 1f),
		new Vector3(0.75f, 0.06699f, 1f),
		new Vector3(0.93301f, 0.25f, 1f),
		new Vector3(1f, 0.5f, 1f),
		new Vector3(0.93301f, 0.75f, 1f)
	};

	private short[] faceNeighbourFace = new short[6] { 2, 3, 0, 1, 5, 4 };

	public Block45Kernel()
	{
		indicesConst = new int[64998];
		for (int i = 0; i < indicesConst.Length; i++)
		{
			indicesConst[i] = i;
		}
		int num = 512;
		for (int j = 0; j < num; j++)
		{
			BlockExtendFlagsResetBuffer[j] = 0;
		}
	}

	public static int OneIndexNoPrefix(int x, int y, int z)
	{
		return (x + y * 10 + z * 100) * 2;
	}

	public static int OneIndex4Flags(int x, int y, int z)
	{
		return x + (y << 3) + (z << 6);
	}

	public static int OneIndex(int x, int y, int z)
	{
		return (x + 1 + (y + 1) * 10 + (z + 1) * 100) * 2;
	}

	public void setInputChunkData(byte[] _input)
	{
		chunkData = _input;
	}

	public int[] getMaterialMap()
	{
		return materialMap;
	}

	public void Rebuild()
	{
		verts.Clear();
		uvs.Clear();
		subMeshIndices.Clear();
		for (int i = 0; i < 256; i++)
		{
			subMeshIndices.Add(new List<int>());
		}
		matCnt = 0;
		_baseIndex = 0;
		Array.Copy(BlockExtendFlagsResetBuffer, BlockExtendFlags, 512);
		for (int j = 1; j < 9; j++)
		{
			for (int k = 1; k < 9; k++)
			{
				for (int l = 1; l < 9; l++)
				{
					processBlockSM(l, k, j);
				}
			}
		}
		int index = 0;
		materialMap = new int[256];
		for (int m = 0; m < 256; m++)
		{
			if (subMeshIndices[index].Count <= 0)
			{
				subMeshIndices.RemoveAt(index);
				continue;
			}
			materialMap[index++] = m;
			matCnt++;
		}
	}

	public Mesh RebuildMeshSM()
	{
		Rebuild();
		Mesh mesh = new Mesh();
		mesh.vertices = verts.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.subMeshCount = matCnt;
		for (int i = 0; i < matCnt; i++)
		{
			mesh.SetTriangles(subMeshIndices[i].ToArray(), i);
		}
		mesh.RecalculateNormals();
		TangentSolver(mesh);
		return mesh;
	}

	public static void TangentSolver(Mesh theMesh)
	{
		int vertexCount = theMesh.vertexCount;
		Vector3[] vertices = theMesh.vertices;
		Vector3[] normals = theMesh.normals;
		Vector2[] uv = theMesh.uv;
		int[] triangles = theMesh.triangles;
		int num = triangles.Length / 3;
		Vector4[] array = new Vector4[vertexCount];
		Vector3[] array2 = new Vector3[vertexCount];
		Vector3[] array3 = new Vector3[vertexCount];
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			int num3 = triangles[num2];
			int num4 = triangles[num2 + 1];
			int num5 = triangles[num2 + 2];
			Vector3 vector = vertices[num3];
			Vector3 vector2 = vertices[num4];
			Vector3 vector3 = vertices[num5];
			Vector2 vector4 = uv[num3];
			Vector2 vector5 = uv[num4];
			Vector2 vector6 = uv[num5];
			float num6 = vector2.x - vector.x;
			float num7 = vector3.x - vector.x;
			float num8 = vector2.y - vector.y;
			float num9 = vector3.y - vector.y;
			float num10 = vector2.z - vector.z;
			float num11 = vector3.z - vector.z;
			float num12 = vector5.x - vector4.x;
			float num13 = vector6.x - vector4.x;
			float num14 = vector5.y - vector4.y;
			float num15 = vector6.y - vector4.y;
			float num16 = 1f / (num12 * num15 - num13 * num14);
			Vector3 vector7 = new Vector3((num15 * num6 - num14 * num7) * num16, (num15 * num8 - num14 * num9) * num16, (num15 * num10 - num14 * num11) * num16);
			Vector3 vector8 = new Vector3((num12 * num7 - num13 * num6) * num16, (num12 * num9 - num13 * num8) * num16, (num12 * num11 - num13 * num10) * num16);
			array2[num3] += vector7;
			array2[num4] += vector7;
			array2[num5] += vector7;
			array3[num3] += vector8;
			array3[num4] += vector8;
			array3[num5] += vector8;
			num2 += 3;
		}
		for (int j = 0; j < vertexCount; j++)
		{
			Vector3 normal = normals[j];
			Vector3 tangent = array2[j];
			Vector3.OrthoNormalize(ref normal, ref tangent);
			array[j].x = tangent.x;
			array[j].y = tangent.y;
			array[j].z = tangent.z;
			array[j].w = ((!((double)Vector3.Dot(Vector3.Cross(normal, tangent), array3[j]) < 0.0)) ? 1f : (-1f));
		}
		theMesh.tangents = array;
	}

	private void processExtendedBlockSM(int x, int y, int z, byte byte0, byte byte1)
	{
		int rotationId = byte0 & 3;
		int num = byte1 & 3;
		int num2 = _2BitToExDir[num * 3];
		int num3 = _2BitToExDir[num * 3 + 1];
		int num4 = _2BitToExDir[num * 3 + 2];
		int num5 = 1 + (byte1 >> 2);
		Vector3 extensionVec = new Vector3(num2 * num5, num3 * num5, num4 * num5);
		byte b = chunkData[OneIndexNoPrefix(x + num2, y + num3, z + num4)];
		byte b2 = chunkData[OneIndexNoPrefix(x + num2, y + num3, z + num4) + 1];
		int primitiveType = (b & 0x7F) >> 2;
		int output_slot = b2 >> 2;
		Vector3 xyz = new Vector3(x - 1, y - 1, z - 1);
		appendInteriorVertsExSM(primitiveType, rotationId, xyz, output_slot, num, extensionVec);
		for (int i = 0; i < 6; i++)
		{
			appendEdgeVertsExSM(primitiveType, rotationId, 0, 0, i, xyz, output_slot, num, extensionVec);
		}
	}

	private void processBlockSM(int x, int y, int z)
	{
		byte b = chunkData[OneIndexNoPrefix(x, y, z) + 1];
		byte b2 = chunkData[OneIndexNoPrefix(x, y, z)];
		int num = b2 >> 2;
		if (num == 0 || BlockExtendFlags[OneIndex4Flags(x - 1, y - 1, z - 1)] != 0)
		{
			return;
		}
		if (b2 >= 128)
		{
			if (num == 63)
			{
				processExtendedBlockSM(x, y, z, b2, b);
			}
		}
		else
		{
			if (b >= MaterialGroups.Length)
			{
				return;
			}
			int output_slot = b;
			int rotationId = b2 & 3;
			Vector3 xyz = new Vector3(x - 1, y - 1, z - 1);
			addInteriorTrianglesSM_c(num, rotationId, xyz, output_slot);
			int num2 = OneIndexNoPrefix(x - 1, y, z);
			int num3 = OneIndexNoPrefix(x, y, z - 1);
			int num4 = OneIndexNoPrefix(x + 1, y, z);
			int num5 = OneIndexNoPrefix(x, y, z + 1);
			int num6 = OneIndexNoPrefix(x, y - 1, z);
			int num7 = OneIndexNoPrefix(x, y + 1, z);
			t_neighbourPrimitiveTypes[0] = chunkData[num2];
			t_neighbourPrimitiveTypes[1] = chunkData[num3];
			t_neighbourPrimitiveTypes[2] = chunkData[num4];
			t_neighbourPrimitiveTypes[3] = chunkData[num5];
			t_neighbourPrimitiveTypes[4] = chunkData[num6];
			t_neighbourPrimitiveTypes[5] = chunkData[num7];
			t_neighbourMaterialTypes[0] = chunkData[num2 + 1];
			t_neighbourMaterialTypes[1] = chunkData[num3 + 1];
			t_neighbourMaterialTypes[2] = chunkData[num4 + 1];
			t_neighbourMaterialTypes[3] = chunkData[num5 + 1];
			t_neighbourMaterialTypes[4] = chunkData[num6 + 1];
			t_neighbourMaterialTypes[5] = chunkData[num7 + 1];
			for (int i = 0; i < 6; i++)
			{
				int neighbourPrimitiveType = t_neighbourPrimitiveTypes[i] >> 2;
				int neighbourRotationId = t_neighbourPrimitiveTypes[i] & 3;
				bool forceCreate = false;
				if (t_neighbourPrimitiveTypes[i] >= 128 || t_neighbourMaterialTypes[i] >= MaterialGroups.Length || MaterialGroups[b] != MaterialGroups[t_neighbourMaterialTypes[i]])
				{
					forceCreate = true;
				}
				addEdgeTrianglesSM_c(num, rotationId, neighbourPrimitiveType, neighbourRotationId, i, xyz, output_slot, forceCreate);
			}
		}
	}

	private Vector3 ExtendindexedCoords(int vecIdx, int ExDirCode, Vector3 ExtensionVec)
	{
		Vector3 result = indexedCoords[vecIdx];
		result.x *= ExtensionVec.x + 1f;
		result.y *= ExtensionVec.y + 1f;
		result.z *= ExtensionVec.z + 1f;
		return result;
	}

	private void appendInteriorVertsExSM(int primitiveType, int rotationId, Vector3 xyz, int output_slot, int ExDirCode, Vector3 ExtensionVec)
	{
		int num = -1;
		int num2 = primitiveType * 2 + 1;
		if (num2 < BlockTypeTable.Length)
		{
			num = BlockTypeTable[num2];
		}
		if (num < 0)
		{
			return;
		}
		for (int i = num; i < num + 30; i += 3)
		{
			int num3 = InteriorFaceVertTable[i];
			if (num3 < 0)
			{
				break;
			}
			num3 = rotateInteriorVertId(num3, rotationId);
			verts.Add(ExtendindexedCoords(num3, ExDirCode, ExtensionVec) + xyz);
			uvs.Add(InteriorVertUV[i]);
			subMeshIndices[output_slot].Add(_baseIndex++);
			num3 = InteriorFaceVertTable[i + 1];
			num3 = rotateInteriorVertId(num3, rotationId);
			verts.Add(ExtendindexedCoords(num3, ExDirCode, ExtensionVec) + xyz);
			uvs.Add(InteriorVertUV[i + 1]);
			subMeshIndices[output_slot].Add(_baseIndex++);
			num3 = InteriorFaceVertTable[i + 2];
			num3 = rotateInteriorVertId(num3, rotationId);
			verts.Add(ExtendindexedCoords(num3, ExDirCode, ExtensionVec) + xyz);
			uvs.Add(InteriorVertUV[i + 2]);
			subMeshIndices[output_slot].Add(_baseIndex++);
		}
	}

	private void addInteriorVert(int vecIdx, int rotationId, Vector3 xyz)
	{
		if (vecIdx > 8)
		{
			verts.Add(rotate90(indexedCoords[vecIdx], rotationId) + xyz);
			return;
		}
		vecIdx = rotateInteriorVertId(vecIdx, rotationId);
		verts.Add(indexedCoords[vecIdx] + xyz);
	}

	private void addUV(int uvi)
	{
		if (uvi >= InteriorVertUV.Length)
		{
			uvs.Add(Vector2.zero);
		}
		else
		{
			uvs.Add(InteriorVertUV[uvi]);
		}
	}

	private void addUVByIndex(int uvi)
	{
		if (uvi >= InteriorVertUVByIndex.Length)
		{
			uvs.Add(Vector2.zero);
		}
		else
		{
			uvs.Add(InteriorVertUVByIndex[uvi]);
		}
	}

	private void addUVByIndexXZ(int uvi)
	{
		if (uvi >= VertUVByIndexXZ.Length)
		{
			uvs.Add(Vector2.zero);
		}
		else if (uvi < 8)
		{
			uvs.Add(VertUVByIndexXZ[uvi]);
		}
	}

	private void addInteriorVertSM_c(int primitiveType, int i, int stIdx, int vecIdx, int rotationId, Vector3 xyz, int output_slot)
	{
		int num = i - stIdx;
		int[] array = SpecialUVIndexTable[primitiveType];
		if (array == null || array.Length <= num || array[num] < 0)
		{
			addUVByIndex(vecIdx);
		}
		else
		{
			uvs.Add(SpecialUVs[array[num]]);
		}
		if (vecIdx > 8)
		{
			verts.Add(rotate90(indexedCoords[vecIdx], rotationId) + xyz);
		}
		else
		{
			vecIdx = rotateInteriorVertId(vecIdx, rotationId);
			verts.Add(indexedCoords[vecIdx] + xyz);
		}
		subMeshIndices[output_slot].Add(_baseIndex++);
	}

	private void addInteriorTrianglesSM_c(int primitiveType, int rotationId, Vector3 xyz, int output_slot)
	{
		int num = -1;
		int num2 = primitiveType * 2 + 1;
		if (num2 < BlockTypeTable.Length)
		{
			num = BlockTypeTable[num2];
		}
		if (num < 0)
		{
			return;
		}
		for (int i = num; i < num + 256; i += 3)
		{
			int num3 = InteriorFaceVertTable[i];
			if (num3 < 0)
			{
				break;
			}
			addInteriorVertSM_c(primitiveType, i, num, num3, rotationId, xyz, output_slot);
			num3 = InteriorFaceVertTable[i + 1];
			addInteriorVertSM_c(primitiveType, i + 1, num, num3, rotationId, xyz, output_slot);
			num3 = InteriorFaceVertTable[i + 2];
			addInteriorVertSM_c(primitiveType, i + 2, num, num3, rotationId, xyz, output_slot);
		}
	}

	private void appendEdgeVertsExSM(int primitiveType, int rotationId, int neighbourPrimitiveType, int neighbourRotationId, int directionId, Vector3 xyz, int output_slot, int ExDirCode, Vector3 ExtensionVec)
	{
		try
		{
			if (directionId > 3)
			{
				int num = faceNeighbourFace[directionId];
				byte mask = rotateTopBottomFaceMask(faceTriMask[primitiveType * 6 + directionId], rotationId);
				byte mask2 = rotateTopBottomFaceMask(faceTriMask[neighbourPrimitiveType * 6 + num], neighbourRotationId);
				if (compareMaskTopBottom(mask, mask2))
				{
					return;
				}
				int num2 = EdgeFaceVertTable[36 * primitiveType + directionId * 6];
				if (num2 >= 0)
				{
					num2 = rotateCornerVertId(num2, rotationId);
					verts.Add(ExtendindexedCoords(num2, ExDirCode, ExtensionVec) + xyz);
					uvs.Add(EdgeVertUV[num2 * 6 + directionId]);
					subMeshIndices[output_slot].Add(_baseIndex++);
					num2 = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 1];
					num2 = rotateCornerVertId(num2, rotationId);
					verts.Add(ExtendindexedCoords(num2, ExDirCode, ExtensionVec) + xyz);
					uvs.Add(EdgeVertUV[num2 * 6 + directionId]);
					subMeshIndices[output_slot].Add(_baseIndex++);
					num2 = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 2];
					num2 = rotateCornerVertId(num2, rotationId);
					verts.Add(ExtendindexedCoords(num2, ExDirCode, ExtensionVec) + xyz);
					uvs.Add(EdgeVertUV[num2 * 6 + directionId]);
					subMeshIndices[output_slot].Add(_baseIndex++);
					num2 = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 3];
					if (num2 >= 0)
					{
						num2 = rotateCornerVertId(num2, rotationId);
						verts.Add(ExtendindexedCoords(num2, ExDirCode, ExtensionVec) + xyz);
						uvs.Add(EdgeVertUV[num2 * 6 + directionId]);
						subMeshIndices[output_slot].Add(_baseIndex++);
						num2 = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 4];
						num2 = rotateCornerVertId(num2, rotationId);
						verts.Add(ExtendindexedCoords(num2, ExDirCode, ExtensionVec) + xyz);
						uvs.Add(EdgeVertUV[num2 * 6 + directionId]);
						subMeshIndices[output_slot].Add(_baseIndex++);
						num2 = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 5];
						num2 = rotateCornerVertId(num2, rotationId);
						verts.Add(ExtendindexedCoords(num2, ExDirCode, ExtensionVec) + xyz);
						uvs.Add(EdgeVertUV[num2 * 6 + directionId]);
						subMeshIndices[output_slot].Add(_baseIndex++);
					}
				}
				return;
			}
			int num3 = rotateFaceId(directionId, rotationId);
			int faceId = faceNeighbourFace[directionId];
			faceId = rotateFaceId(faceId, neighbourRotationId);
			if (compareMask(faceTriMask[primitiveType * 6 + num3], faceTriMask[neighbourPrimitiveType * 6 + faceId]))
			{
				return;
			}
			int num4 = EdgeFaceVertTable[36 * primitiveType + num3 * 6];
			if (num4 >= 0)
			{
				num4 = rotateCornerVertId(num4, rotationId);
				verts.Add(ExtendindexedCoords(num4, ExDirCode, ExtensionVec) + xyz);
				uvs.Add(EdgeVertUV[num4 * 6 + directionId]);
				subMeshIndices[output_slot].Add(_baseIndex++);
				num4 = EdgeFaceVertTable[36 * primitiveType + num3 * 6 + 1];
				num4 = rotateCornerVertId(num4, rotationId);
				verts.Add(ExtendindexedCoords(num4, ExDirCode, ExtensionVec) + xyz);
				uvs.Add(EdgeVertUV[num4 * 6 + directionId]);
				subMeshIndices[output_slot].Add(_baseIndex++);
				num4 = EdgeFaceVertTable[36 * primitiveType + num3 * 6 + 2];
				num4 = rotateCornerVertId(num4, rotationId);
				verts.Add(ExtendindexedCoords(num4, ExDirCode, ExtensionVec) + xyz);
				uvs.Add(EdgeVertUV[num4 * 6 + directionId]);
				subMeshIndices[output_slot].Add(_baseIndex++);
				num4 = EdgeFaceVertTable[36 * primitiveType + num3 * 6 + 3];
				if (num4 >= 0)
				{
					num4 = rotateCornerVertId(num4, rotationId);
					verts.Add(ExtendindexedCoords(num4, ExDirCode, ExtensionVec) + xyz);
					uvs.Add(EdgeVertUV[num4 * 6 + directionId]);
					subMeshIndices[output_slot].Add(_baseIndex++);
					num4 = EdgeFaceVertTable[36 * primitiveType + num3 * 6 + 4];
					num4 = rotateCornerVertId(num4, rotationId);
					verts.Add(ExtendindexedCoords(num4, ExDirCode, ExtensionVec) + xyz);
					uvs.Add(EdgeVertUV[num4 * 6 + directionId]);
					subMeshIndices[output_slot].Add(_baseIndex++);
					num4 = EdgeFaceVertTable[36 * primitiveType + num3 * 6 + 5];
					num4 = rotateCornerVertId(num4, rotationId);
					verts.Add(ExtendindexedCoords(num4, ExDirCode, ExtensionVec) + xyz);
					uvs.Add(EdgeVertUV[num4 * 6 + directionId]);
					subMeshIndices[output_slot].Add(_baseIndex++);
				}
			}
		}
		catch
		{
		}
	}

	private void addEdgeVert_c(int vecIdx, int rotationId, Vector3 xyz, int output_slot)
	{
		addUVByIndex(vecIdx);
		vecIdx = rotateCornerVertId(vecIdx, rotationId);
		verts.Add(indexedCoords[vecIdx] + xyz);
		subMeshIndices[output_slot].Add(_baseIndex++);
	}

	private void addEdgeVertXZ_c(int vecIdx, int rotationId, Vector3 xyz, int output_slot)
	{
		addUVByIndexXZ(vecIdx);
		vecIdx = rotateCornerVertId(vecIdx, rotationId);
		verts.Add(indexedCoords[vecIdx] + xyz);
		subMeshIndices[output_slot].Add(_baseIndex++);
	}

	private void addEdgeTrianglesSM_c(int primitiveType, int rotationId, int neighbourPrimitiveType, int neighbourRotationId, int directionId, Vector3 xyz, int output_slot, bool forceCreate)
	{
		bool flag = forceCreate;
		if (directionId > 3)
		{
			if (!flag)
			{
				int num = faceNeighbourFace[directionId];
				byte mask = rotateTopBottomFaceMask(faceTriMask[primitiveType * 6 + directionId], rotationId);
				byte mask2 = rotateTopBottomFaceMask(faceTriMask[neighbourPrimitiveType * 6 + num], neighbourRotationId);
				flag = !compareMaskTopBottom(mask, mask2);
			}
			if (!flag)
			{
				return;
			}
			int num2 = EdgeFaceVertTable[36 * primitiveType + directionId * 6];
			if (num2 >= 0)
			{
				addEdgeVertXZ_c(num2, rotationId, xyz, output_slot);
				num2 = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 1];
				addEdgeVertXZ_c(num2, rotationId, xyz, output_slot);
				num2 = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 2];
				addEdgeVertXZ_c(num2, rotationId, xyz, output_slot);
				num2 = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 3];
				if (num2 >= 0)
				{
					addEdgeVertXZ_c(num2, rotationId, xyz, output_slot);
					num2 = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 4];
					addEdgeVertXZ_c(num2, rotationId, xyz, output_slot);
					num2 = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 5];
					addEdgeVertXZ_c(num2, rotationId, xyz, output_slot);
				}
			}
			return;
		}
		int num3 = rotateFaceId(directionId, rotationId);
		int faceId = faceNeighbourFace[directionId];
		faceId = rotateFaceId(faceId, neighbourRotationId);
		if (!flag)
		{
			flag = !compareMask(faceTriMask[primitiveType * 6 + num3], faceTriMask[neighbourPrimitiveType * 6 + faceId]);
		}
		if (!flag)
		{
			return;
		}
		int num4 = EdgeFaceVertTable[36 * primitiveType + num3 * 6];
		if (num4 >= 0)
		{
			addEdgeVert_c(num4, rotationId, xyz, output_slot);
			num4 = EdgeFaceVertTable[36 * primitiveType + num3 * 6 + 1];
			addEdgeVert_c(num4, rotationId, xyz, output_slot);
			num4 = EdgeFaceVertTable[36 * primitiveType + num3 * 6 + 2];
			addEdgeVert_c(num4, rotationId, xyz, output_slot);
			num4 = EdgeFaceVertTable[36 * primitiveType + num3 * 6 + 3];
			if (num4 >= 0)
			{
				addEdgeVert_c(num4, rotationId, xyz, output_slot);
				num4 = EdgeFaceVertTable[36 * primitiveType + num3 * 6 + 4];
				addEdgeVert_c(num4, rotationId, xyz, output_slot);
				num4 = EdgeFaceVertTable[36 * primitiveType + num3 * 6 + 5];
				addEdgeVert_c(num4, rotationId, xyz, output_slot);
			}
		}
	}

	public void UVGenerator()
	{
		uvgenforfullcyl(79, 91, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0f), 6);
		uvgenforfullcyl(92, 104, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0f), 6);
		uvgenforfullcyl(105, 117, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0f), 6);
		uvgenforfullcyl(118, 130, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0f), 6);
	}

	private void uvgen(int st, int end, Vector2 vecSt, Vector2 vecMid, Vector2 vecEnd, int stepnum = 3)
	{
		string text = "// " + st + " - " + end + "\n";
		Vector2 vector = (vecMid - vecSt) / stepnum;
		for (int i = 1; i < stepnum; i++)
		{
			Vector2 vector2 = vecSt + vector * i;
			string text2 = text;
			text = text2 + "new Vector2(" + vector2.x + ", " + vector2.y + "),\n";
		}
		vector = (vecEnd - vecMid) / stepnum;
		for (int j = 0; j < stepnum; j++)
		{
			Vector2 vector2 = vecMid + vector * j;
			string text2 = text;
			text = text2 + "new Vector2(" + vector2.x + ", " + vector2.y + "),\n";
		}
		MonoBehaviour.print(text);
	}

	private void uvgenforfullcyl(int st, int end, Vector2 vecSt, Vector2 vecMid, Vector2 vecEnd, int stepnum = 3)
	{
		string text = "// " + st + " - " + end + "\n";
		Vector2 vector = (vecMid - vecSt) / stepnum;
		for (int i = 0; i <= stepnum; i++)
		{
			Vector2 vector2 = vecSt + vector * i;
			string text2 = text;
			text = text2 + "new Vector2(" + vector2.x + ", " + vector2.y + "),\n";
		}
		vector = (vecEnd - vecMid) / stepnum;
		for (int j = 1; j <= stepnum; j++)
		{
			Vector2 vector2 = vecMid + vector * j;
			string text2 = text;
			text = text2 + "new Vector2(" + vector2.x + ", " + vector2.y + "),\n";
		}
		MonoBehaviour.print(text);
	}

	private Vector3 rotate90(Vector3 vec, int rot)
	{
		return rot switch
		{
			0 => vec, 
			1 => new Vector3(vec.z, vec.y, 1f - vec.x), 
			2 => new Vector3(1f - vec.x, vec.y, 1f - vec.z), 
			_ => new Vector3(1f - vec.z, vec.y, vec.x), 
		};
	}

	private bool compareMask(byte mask0, byte mask1)
	{
		bool flag = (mask1 & 4) > 0;
		mask1 = (((mask1 & 1) <= 0) ? ((byte)(mask1 & 0xB)) : ((byte)(mask1 | 4)));
		mask1 = ((!flag) ? ((byte)(mask1 & 0xE)) : ((byte)(mask1 | 1)));
		return (mask0 & mask1) == mask0;
	}

	private bool compareMaskTopBottom(byte mask0, byte mask1)
	{
		bool flag = (mask1 & 8) > 0;
		mask1 = (((mask1 & 2) <= 0) ? ((byte)(mask1 & 7)) : ((byte)(mask1 | 8)));
		mask1 = ((!flag) ? ((byte)(mask1 & 0xD)) : ((byte)(mask1 | 2)));
		return (mask0 & mask1) == mask0;
	}

	private int rotateCornerVertId(int faceId, int rotationId)
	{
		int num = (faceId + rotationId) % 4;
		return (faceId <= 3) ? num : (num + 4);
	}

	private int rotateInteriorVertId(int faceId, int rotationId)
	{
		if (faceId == 8)
		{
			return faceId;
		}
		int num = (faceId + rotationId) % 4;
		return (faceId <= 3) ? num : (num + 4);
	}

	private int rotateFaceId(int faceId, int rotationId)
	{
		return (faceId + rotationId) % 4;
	}

	private byte rotateTopBottomFaceMask(byte faceId, int rotationId)
	{
		return (byte)((byte)(faceId << rotationId) >> 4);
	}

	private Mesh RebuildMesh()
	{
		if (verts == null)
		{
			verts = new List<Vector3>();
		}
		verts.Clear();
		if (uvs == null)
		{
			uvs = new List<Vector2>();
		}
		uvs.Clear();
		for (int i = 1; i < 9; i++)
		{
			for (int j = 1; j < 9; j++)
			{
				for (int k = 1; k < 9; k++)
				{
					processBlock(k, j, i);
				}
			}
		}
		Mesh mesh = new Mesh();
		mesh.vertices = verts.ToArray();
		int[] array = new int[verts.Count];
		Array.Copy(indicesConst, array, verts.Count);
		mesh.SetTriangles(array, 0);
		mesh.uv = uvs.ToArray();
		mesh.RecalculateNormals();
		return mesh;
	}

	private List<Mesh> RebuildMeshByMaterial()
	{
		int tickCount = Environment.TickCount;
		if (vertsByMaterial == null)
		{
			vertsByMaterial = new List<List<Vector3>>();
		}
		vertsByMaterial.Clear();
		if (uvsByMaterial == null)
		{
			uvsByMaterial = new List<List<Vector2>>();
		}
		uvsByMaterial.Clear();
		for (int i = 0; i < 256; i++)
		{
			vertsByMaterial.Add(new List<Vector3>());
			uvsByMaterial.Add(new List<Vector2>());
		}
		for (int j = 1; j < 9; j++)
		{
			for (int k = 1; k < 9; k++)
			{
				for (int l = 1; l < 9; l++)
				{
					processBlockByMaterial(l, k, j);
				}
			}
		}
		List<Mesh> list = new List<Mesh>();
		for (int m = 0; m < 256; m++)
		{
			Mesh mesh = new Mesh();
			list.Add(mesh);
			if (vertsByMaterial[m].Count != 0)
			{
				mesh.vertices = vertsByMaterial[m].ToArray();
				int[] array = new int[vertsByMaterial[m].Count];
				Array.Copy(indicesConst, array, vertsByMaterial[m].Count);
				mesh.SetTriangles(array, 0);
				mesh.uv = uvsByMaterial[m].ToArray();
				mesh.RecalculateNormals();
			}
		}
		MonoBehaviour.print("rebuild mesh bm took " + (Environment.TickCount - tickCount) + " ms.");
		return list;
	}

	private void processBlock(int x, int y, int z)
	{
		byte b = chunkData[OneIndexNoPrefix(x, y, z)];
		int num = b >> 2;
		int rotationId = b & 3;
		if (num != 0)
		{
			Vector3 xyz = new Vector3(x - 1, y - 1, z - 1);
			appendInteriorVerts(num, rotationId, xyz);
			byte[] array = new byte[6]
			{
				chunkData[OneIndexNoPrefix(x - 1, y, z)],
				chunkData[OneIndexNoPrefix(x, y, z - 1)],
				chunkData[OneIndexNoPrefix(x + 1, y, z)],
				chunkData[OneIndexNoPrefix(x, y, z + 1)],
				chunkData[OneIndexNoPrefix(x, y - 1, z)],
				chunkData[OneIndexNoPrefix(x, y + 1, z)]
			};
			for (int i = 0; i < 6; i++)
			{
				int neighbourPrimitiveType = array[i] >> 2;
				int neighbourRotationId = array[i] & 3;
				appendEdgeVerts(num, rotationId, neighbourPrimitiveType, neighbourRotationId, i, xyz);
			}
		}
	}

	private void processBlockByMaterial(int x, int y, int z)
	{
		byte b = chunkData[OneIndexNoPrefix(x, y, z) + 1];
		int output_slot = b;
		byte b2 = chunkData[OneIndexNoPrefix(x, y, z)];
		int num = b2 >> 2;
		int rotationId = b2 & 3;
		if (num == 0)
		{
			return;
		}
		Vector3 xyz = new Vector3(x - 1, y - 1, z - 1);
		appendInteriorVertsByMaterial(num, rotationId, xyz, output_slot);
		byte[] array = new byte[6];
		byte[] array2 = new byte[6];
		array[0] = chunkData[OneIndexNoPrefix(x - 1, y, z)];
		array[1] = chunkData[OneIndexNoPrefix(x, y, z - 1)];
		array[2] = chunkData[OneIndexNoPrefix(x + 1, y, z)];
		array[3] = chunkData[OneIndexNoPrefix(x, y, z + 1)];
		array[4] = chunkData[OneIndexNoPrefix(x, y - 1, z)];
		array[5] = chunkData[OneIndexNoPrefix(x, y + 1, z)];
		array2[0] = chunkData[OneIndexNoPrefix(x - 1, y, z) + 1];
		array2[1] = chunkData[OneIndexNoPrefix(x, y, z - 1) + 1];
		array2[2] = chunkData[OneIndexNoPrefix(x + 1, y, z) + 1];
		array2[3] = chunkData[OneIndexNoPrefix(x, y, z + 1) + 1];
		array2[4] = chunkData[OneIndexNoPrefix(x, y - 1, z) + 1];
		array2[5] = chunkData[OneIndexNoPrefix(x, y + 1, z) + 1];
		for (int i = 0; i < 6; i++)
		{
			int neighbourPrimitiveType = array[i] >> 2;
			int neighbourRotationId = array[i] & 3;
			bool forceCreate = false;
			if (MaterialGroups[b] != MaterialGroups[array2[i]])
			{
				forceCreate = true;
			}
			appendEdgeVertsByMaterial(num, rotationId, neighbourPrimitiveType, neighbourRotationId, i, xyz, output_slot, forceCreate);
		}
	}

	private void appendEdgeVerts(int primitiveType, int rotationId, int neighbourPrimitiveType, int neighbourRotationId, int directionId, Vector3 xyz)
	{
		if (directionId > 3)
		{
			int num = faceNeighbourFace[directionId];
			byte mask = rotateTopBottomFaceMask(faceTriMask[primitiveType * 6 + directionId], rotationId);
			byte mask2 = rotateTopBottomFaceMask(faceTriMask[neighbourPrimitiveType * 6 + num], neighbourRotationId);
			if (compareMaskTopBottom(mask, mask2))
			{
				return;
			}
			int num2 = EdgeFaceVertTable[36 * primitiveType + directionId * 6];
			if (num2 >= 0)
			{
				num2 = rotateCornerVertId(num2, rotationId);
				verts.Add(indexedCoords[num2] + xyz);
				addUV(num2, directionId);
				num2 = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 1];
				num2 = rotateCornerVertId(num2, rotationId);
				verts.Add(indexedCoords[num2] + xyz);
				addUV(num2, directionId);
				num2 = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 2];
				num2 = rotateCornerVertId(num2, rotationId);
				verts.Add(indexedCoords[num2] + xyz);
				addUV(num2, directionId);
				num2 = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 3];
				if (num2 >= 0)
				{
					num2 = rotateCornerVertId(num2, rotationId);
					verts.Add(indexedCoords[num2] + xyz);
					addUV(num2, directionId);
					num2 = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 4];
					num2 = rotateCornerVertId(num2, rotationId);
					verts.Add(indexedCoords[num2] + xyz);
					addUV(num2, directionId);
					num2 = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 5];
					num2 = rotateCornerVertId(num2, rotationId);
					verts.Add(indexedCoords[num2] + xyz);
					addUV(num2, directionId);
				}
			}
			return;
		}
		int num3 = rotateFaceId(directionId, rotationId);
		int faceId = faceNeighbourFace[directionId];
		faceId = rotateFaceId(faceId, neighbourRotationId);
		if (compareMask(faceTriMask[primitiveType * 6 + num3], faceTriMask[neighbourPrimitiveType * 6 + faceId]))
		{
			return;
		}
		int num4 = EdgeFaceVertTable[36 * primitiveType + num3 * 6];
		if (num4 >= 0)
		{
			num4 = rotateCornerVertId(num4, rotationId);
			verts.Add(indexedCoords[num4] + xyz);
			addUV(num4, directionId);
			num4 = EdgeFaceVertTable[36 * primitiveType + num3 * 6 + 1];
			num4 = rotateCornerVertId(num4, rotationId);
			verts.Add(indexedCoords[num4] + xyz);
			addUV(num4, directionId);
			num4 = EdgeFaceVertTable[36 * primitiveType + num3 * 6 + 2];
			num4 = rotateCornerVertId(num4, rotationId);
			verts.Add(indexedCoords[num4] + xyz);
			addUV(num4, directionId);
			num4 = EdgeFaceVertTable[36 * primitiveType + num3 * 6 + 3];
			if (num4 >= 0)
			{
				num4 = rotateCornerVertId(num4, rotationId);
				verts.Add(indexedCoords[num4] + xyz);
				addUV(num4, directionId);
				num4 = EdgeFaceVertTable[36 * primitiveType + num3 * 6 + 4];
				num4 = rotateCornerVertId(num4, rotationId);
				verts.Add(indexedCoords[num4] + xyz);
				addUV(num4, directionId);
				num4 = EdgeFaceVertTable[36 * primitiveType + num3 * 6 + 5];
				num4 = rotateCornerVertId(num4, rotationId);
				verts.Add(indexedCoords[num4] + xyz);
				addUV(num4, directionId);
			}
		}
	}

	private void appendEdgeVertsByMaterial(int primitiveType, int rotationId, int neighbourPrimitiveType, int neighbourRotationId, int directionId, Vector3 xyz, int output_slot, bool forceCreate)
	{
		if (directionId > 3)
		{
			int num = faceNeighbourFace[directionId];
			byte mask = rotateTopBottomFaceMask(faceTriMask[primitiveType * 6 + directionId], rotationId);
			byte mask2 = rotateTopBottomFaceMask(faceTriMask[neighbourPrimitiveType * 6 + num], neighbourRotationId);
			if (compareMaskTopBottom(mask, mask2) && !forceCreate)
			{
				return;
			}
			int num2 = EdgeFaceVertTable[36 * primitiveType + directionId * 6];
			if (num2 >= 0)
			{
				num2 = rotateCornerVertId(num2, rotationId);
				vertsByMaterial[output_slot].Add(indexedCoords[num2] + xyz);
				uvsByMaterial[output_slot].Add(EdgeVertUV[num2 * 6 + directionId]);
				num2 = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 1];
				num2 = rotateCornerVertId(num2, rotationId);
				vertsByMaterial[output_slot].Add(indexedCoords[num2] + xyz);
				uvsByMaterial[output_slot].Add(EdgeVertUV[num2 * 6 + directionId]);
				num2 = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 2];
				num2 = rotateCornerVertId(num2, rotationId);
				vertsByMaterial[output_slot].Add(indexedCoords[num2] + xyz);
				uvsByMaterial[output_slot].Add(EdgeVertUV[num2 * 6 + directionId]);
				num2 = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 3];
				if (num2 >= 0)
				{
					num2 = rotateCornerVertId(num2, rotationId);
					vertsByMaterial[output_slot].Add(indexedCoords[num2] + xyz);
					uvsByMaterial[output_slot].Add(EdgeVertUV[num2 * 6 + directionId]);
					num2 = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 4];
					num2 = rotateCornerVertId(num2, rotationId);
					vertsByMaterial[output_slot].Add(indexedCoords[num2] + xyz);
					uvsByMaterial[output_slot].Add(EdgeVertUV[num2 * 6 + directionId]);
					num2 = EdgeFaceVertTable[36 * primitiveType + directionId * 6 + 5];
					num2 = rotateCornerVertId(num2, rotationId);
					vertsByMaterial[output_slot].Add(indexedCoords[num2] + xyz);
					uvsByMaterial[output_slot].Add(EdgeVertUV[num2 * 6 + directionId]);
				}
			}
			return;
		}
		int num3 = rotateFaceId(directionId, rotationId);
		int faceId = faceNeighbourFace[directionId];
		faceId = rotateFaceId(faceId, neighbourRotationId);
		if (compareMask(faceTriMask[primitiveType * 6 + num3], faceTriMask[neighbourPrimitiveType * 6 + faceId]) && !forceCreate)
		{
			return;
		}
		int num4 = EdgeFaceVertTable[36 * primitiveType + num3 * 6];
		if (num4 >= 0)
		{
			num4 = rotateCornerVertId(num4, rotationId);
			vertsByMaterial[output_slot].Add(indexedCoords[num4] + xyz);
			uvsByMaterial[output_slot].Add(EdgeVertUV[num4 * 6 + directionId]);
			num4 = EdgeFaceVertTable[36 * primitiveType + num3 * 6 + 1];
			num4 = rotateCornerVertId(num4, rotationId);
			vertsByMaterial[output_slot].Add(indexedCoords[num4] + xyz);
			uvsByMaterial[output_slot].Add(EdgeVertUV[num4 * 6 + directionId]);
			num4 = EdgeFaceVertTable[36 * primitiveType + num3 * 6 + 2];
			num4 = rotateCornerVertId(num4, rotationId);
			vertsByMaterial[output_slot].Add(indexedCoords[num4] + xyz);
			uvsByMaterial[output_slot].Add(EdgeVertUV[num4 * 6 + directionId]);
			num4 = EdgeFaceVertTable[36 * primitiveType + num3 * 6 + 3];
			if (num4 >= 0)
			{
				num4 = rotateCornerVertId(num4, rotationId);
				vertsByMaterial[output_slot].Add(indexedCoords[num4] + xyz);
				uvsByMaterial[output_slot].Add(EdgeVertUV[num4 * 6 + directionId]);
				num4 = EdgeFaceVertTable[36 * primitiveType + num3 * 6 + 4];
				num4 = rotateCornerVertId(num4, rotationId);
				vertsByMaterial[output_slot].Add(indexedCoords[num4] + xyz);
				uvsByMaterial[output_slot].Add(EdgeVertUV[num4 * 6 + directionId]);
				num4 = EdgeFaceVertTable[36 * primitiveType + num3 * 6 + 5];
				num4 = rotateCornerVertId(num4, rotationId);
				vertsByMaterial[output_slot].Add(indexedCoords[num4] + xyz);
				uvsByMaterial[output_slot].Add(EdgeVertUV[num4 * 6 + directionId]);
			}
		}
	}

	private void appendInteriorVerts(int primitiveType, int rotationId, Vector3 xyz)
	{
		int num = -1;
		int num2 = primitiveType * 2 + 1;
		if (num2 < BlockTypeTable.Length)
		{
			num = BlockTypeTable[num2];
		}
		if (num < 0)
		{
			return;
		}
		for (int i = num; i < num + 30; i += 3)
		{
			int num3 = InteriorFaceVertTable[i];
			if (num3 < 0)
			{
				break;
			}
			num3 = rotateInteriorVertId(num3, rotationId);
			verts.Add(indexedCoords[num3] + xyz);
			addInteriorUV(i);
			num3 = InteriorFaceVertTable[i + 1];
			num3 = rotateInteriorVertId(num3, rotationId);
			verts.Add(indexedCoords[num3] + xyz);
			addInteriorUV(i + 1);
			num3 = InteriorFaceVertTable[i + 2];
			num3 = rotateInteriorVertId(num3, rotationId);
			verts.Add(indexedCoords[num3] + xyz);
			addInteriorUV(i + 2);
		}
	}

	private void appendInteriorVertsByMaterial(int primitiveType, int rotationId, Vector3 xyz, int output_slot)
	{
		int num = -1;
		int num2 = primitiveType * 2 + 1;
		if (num2 < BlockTypeTable.Length)
		{
			num = BlockTypeTable[num2];
		}
		if (num < 0)
		{
			return;
		}
		for (int i = num; i < num + 30; i += 3)
		{
			int num3 = InteriorFaceVertTable[i];
			if (num3 < 0)
			{
				break;
			}
			num3 = rotateInteriorVertId(num3, rotationId);
			vertsByMaterial[output_slot].Add(indexedCoords[num3] + xyz);
			uvsByMaterial[output_slot].Add(InteriorVertUV[i]);
			num3 = InteriorFaceVertTable[i + 1];
			num3 = rotateInteriorVertId(num3, rotationId);
			vertsByMaterial[output_slot].Add(indexedCoords[num3] + xyz);
			uvsByMaterial[output_slot].Add(InteriorVertUV[i + 1]);
			num3 = InteriorFaceVertTable[i + 2];
			num3 = rotateInteriorVertId(num3, rotationId);
			vertsByMaterial[output_slot].Add(indexedCoords[num3] + xyz);
			uvsByMaterial[output_slot].Add(InteriorVertUV[i + 2]);
		}
	}

	private void addUV(int vertId, int directionId)
	{
		uvs.Add(EdgeVertUV[vertId * 6 + directionId]);
	}

	private void addInteriorUV(int index)
	{
		uvs.Add(InteriorVertUV[index]);
	}
}
