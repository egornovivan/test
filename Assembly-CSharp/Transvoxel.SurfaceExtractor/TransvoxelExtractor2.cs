using System.Collections.Generic;
using Transvoxel.Lengyel;
using UnityEngine;

namespace Transvoxel.SurfaceExtractor;

internal static class TransvoxelExtractor2
{
	public const int ChunkWidth = 32;

	public const int BlockWidth = 32;

	public const int Primary = 0;

	public const int Secondary = 1;

	private const float S = 0.00390625f;

	private const byte IsoLevel = 128;

	public static readonly Vector3 Unused = new Vector3(1000f, 1000f, 1000f);

	public static int deltaCnt = 0;

	private static byte HiNibble(byte b)
	{
		return (byte)((b >> 4) & 0xF);
	}

	private static byte LoNibble(byte b)
	{
		return (byte)(b & 0xF);
	}

	private static int Sign(sbyte b)
	{
		return (b >> 7) & 1;
	}

	private static Vector3 ComputeDelta(Vector3 v, int k, int s)
	{
		if (k < 1)
		{
			return Vector3.zero;
		}
		float num = 1 << k;
		float num2 = num * 0.25f;
		float[] array = new float[3];
		float[] array2 = new float[3] { v.x, v.y, v.z };
		for (int i = 0; i < 3; i++)
		{
			if (array2[i] < num)
			{
				array[i] = (1f - array2[i] / num) * num2;
			}
			else if (array2[i] > num * (float)(s - 1))
			{
				array[i] = (num * (float)s - 1f - array2[i]) * num2;
			}
		}
		return new Vector3(array[0], array[1], array[2]);
	}

	private static Vector3 ProjectNormal(Vector3 n, Vector3 delta)
	{
		Matrix3X3 matrix3X = new Matrix3X3(1f - n.x * n.x, (0f - n.x) * n.y, (0f - n.x) * n.z, (0f - n.x) * n.y, 1f - n.y * n.y, (0f - n.y) * n.z, (0f - n.x) * n.z, (0f - n.y) * n.z, 1f - n.z * n.z);
		return matrix3X * delta;
	}

	private static IntVector3 PrevOffset(byte dir)
	{
		return new IntVector3(-(dir & 1), -((dir >> 1) & 1), -((dir >> 2) & 1));
	}

	public static int PolygonizeTransitionCell(int x, int y, int dirIndex, float cellSize, VFVoxelChunkData chunkData, TransVertices verts, List<int> indices, TransitionCache cache)
	{
		int lOD = chunkData.LOD;
		int num = 1;
		int num2 = 2;
		int num3 = 1 << lOD;
		IntVector3 intVector = Tables.TransitionFaceCoords[dirIndex, 0];
		IntVector3 intVector2 = Tables.TransitionFaceCoords[dirIndex, 1];
		IntVector3 intVector3 = Tables.TransitionFaceCoords[dirIndex, 2];
		IntVector3 intVector4 = Tables.TransitionFaceCoords[dirIndex, 4];
		IntVector3 intVector5 = Tables.TransitionFaceCoords[dirIndex, 3] * 32 + intVector * (x * num2) + intVector2 * (y * num2);
		Matrix3X3 matrix3X = new Matrix3X3(intVector * num, intVector2 * num, intVector3 * num);
		IntVector3[] array = new IntVector3[13]
		{
			intVector5 + matrix3X * Tables.TransitionCornerCoords[0],
			intVector5 + matrix3X * Tables.TransitionCornerCoords[1],
			intVector5 + matrix3X * Tables.TransitionCornerCoords[2],
			intVector5 + matrix3X * Tables.TransitionCornerCoords[3],
			intVector5 + matrix3X * Tables.TransitionCornerCoords[4],
			intVector5 + matrix3X * Tables.TransitionCornerCoords[5],
			intVector5 + matrix3X * Tables.TransitionCornerCoords[6],
			intVector5 + matrix3X * Tables.TransitionCornerCoords[7],
			intVector5 + matrix3X * Tables.TransitionCornerCoords[8],
			intVector5 + matrix3X * Tables.TransitionCornerCoords[9],
			intVector5 + matrix3X * Tables.TransitionCornerCoords[10],
			intVector5 + matrix3X * Tables.TransitionCornerCoords[11],
			intVector5 + matrix3X * Tables.TransitionCornerCoords[12]
		};
		VFVoxel[] array2 = new VFVoxel[9]
		{
			chunkData[array[0]],
			chunkData[array[1]],
			chunkData[array[2]],
			chunkData[array[3]],
			chunkData[array[4]],
			chunkData[array[5]],
			chunkData[array[6]],
			chunkData[array[7]],
			chunkData[array[8]]
		};
		uint num4 = (uint)(((array2[0].Volume >> 7) & 1) | ((array2[1].Volume >> 6) & 2) | ((array2[2].Volume >> 5) & 4) | ((array2[5].Volume >> 4) & 8) | ((array2[8].Volume >> 3) & 0x10) | ((array2[7].Volume >> 2) & 0x20) | ((array2[6].Volume >> 1) & 0x40) | (array2[3].Volume & 0x80) | ((array2[4].Volume << 1) & 0x100));
		if (num4 == 0 || num4 == 511)
		{
			return 0;
		}
		cache[x, y].CaseIndex = (byte)num4;
		byte[] array3 = new byte[13]
		{
			array2[0].Volume,
			array2[1].Volume,
			array2[2].Volume,
			array2[3].Volume,
			array2[4].Volume,
			array2[5].Volume,
			array2[6].Volume,
			array2[7].Volume,
			array2[8].Volume,
			array2[0].Volume,
			array2[2].Volume,
			array2[6].Volume,
			array2[8].Volume
		};
		byte[] array4 = new byte[13]
		{
			array2[0].Type,
			array2[1].Type,
			array2[2].Type,
			array2[3].Type,
			array2[4].Type,
			array2[5].Type,
			array2[6].Type,
			array2[7].Type,
			array2[8].Type,
			array2[0].Type,
			array2[2].Type,
			array2[6].Type,
			array2[8].Type
		};
		Vector3[] array5 = new Vector3[13];
		for (int i = 0; i < 9; i++)
		{
			IntVector3 intVector6 = array[i];
			float x2 = ((intVector6.x < 1) ? (chunkData[intVector6 + IntVector3.UnitX].Volume - array3[i]) : (chunkData[intVector6 + IntVector3.UnitX].Volume - chunkData[intVector6 - IntVector3.UnitX].Volume));
			float y2 = ((intVector6.y < 1) ? (chunkData[intVector6 + IntVector3.UnitY].Volume - array3[i]) : (chunkData[intVector6 + IntVector3.UnitY].Volume - chunkData[intVector6 - IntVector3.UnitY].Volume));
			float z = ((intVector6.z < 1) ? (chunkData[intVector6 + IntVector3.UnitZ].Volume - array3[i]) : (chunkData[intVector6 + IntVector3.UnitZ].Volume - chunkData[intVector6 - IntVector3.UnitZ].Volume));
			ref Vector3 reference = ref array5[i];
			reference = new Vector3(x2, y2, z);
		}
		ref Vector3 reference2 = ref array5[9];
		reference2 = array5[0];
		ref Vector3 reference3 = ref array5[10];
		reference3 = array5[2];
		ref Vector3 reference4 = ref array5[11];
		reference4 = array5[6];
		ref Vector3 reference5 = ref array5[12];
		reference5 = array5[8];
		byte b = 0;
		if (intVector5.x == 0)
		{
			b |= 1;
		}
		if (intVector5.x == 32)
		{
			b |= 2;
		}
		if (intVector5.y == 0)
		{
			b |= 4;
		}
		if (intVector5.y == 32)
		{
			b |= 8;
		}
		if (intVector5.z == 0)
		{
			b |= 0x10;
		}
		if (intVector5.z == 32)
		{
			b |= 0x20;
		}
		byte b2 = (byte)(((x > 0) ? 1u : 0u) | (((y > 0) ? 1u : 0u) << 1));
		byte b3 = Tables.TransitionCellClass[num4];
		Tables.RegularCell regularCell = Tables.TransitionRegularCellData[b3 & 0x7F];
		bool flag = (b3 & 0x80) != 0;
		int[] array6 = new int[12];
		int num5 = (int)regularCell.GetVertexCount();
		int num6 = (int)regularCell.GetTriangleCount();
		for (int j = 0; j < num5; j++)
		{
			ushort num7 = Tables.TransitionVertexData[num4][j];
			byte b4 = (byte)num7;
			byte b5 = (byte)(num7 >> 8);
			byte b6 = HiNibble(b4);
			byte b7 = LoNibble(b4);
			byte b8 = array3[b6];
			byte b9 = array3[b7];
			int num8 = (128 - b8 << 8) / (b9 - b8);
			int num9 = 256 - num8;
			float num10 = (float)num9 * 0.00390625f;
			float num11 = (float)num8 * 0.00390625f;
			byte b10 = 0;
			byte b11;
			byte b12;
			bool flag2;
			bool flag3;
			bool flag4;
			if ((num8 & 0xFF) != 0)
			{
				b11 = HiNibble(b5);
				b12 = LoNibble(b5);
				flag2 = b6 > 8 && b7 > 8;
				flag3 = (b11 & 8) != 0;
				flag4 = false;
			}
			else
			{
				b10 = ((num8 != 0) ? b7 : b6);
				byte b13 = Tables.TransitionCornerData[b10];
				b11 = HiNibble(b13);
				b12 = LoNibble(b13);
				flag2 = b10 > 8;
				flag3 = true;
				flag4 = true;
			}
			bool flag5 = (b11 & b2) == b11;
			if (flag5)
			{
				ReuseCell reuseCell = cache[x - (b11 & 1), y - ((b11 >> 1) & 1)];
				if (reuseCell.CaseIndex == 0 || reuseCell.CaseIndex == 511)
				{
					array6[j] = -1;
				}
				else
				{
					array6[j] = reuseCell.Verts[b12];
				}
			}
			if (flag5 && array6[j] >= 0)
			{
				continue;
			}
			array6[j] = verts.Count;
			if (flag3)
			{
				cache[x, y].Verts[b12] = array6[j];
			}
			verts.IsLowside.Add(flag2);
			byte b14 = array4[b10];
			byte b15 = array4[b6];
			byte b16 = array4[b7];
			if (b14 == 0 || b15 == 0 || b16 == 0)
			{
				if (b14 == 0)
				{
					b14 = b15;
				}
				if (b14 == 0)
				{
					b14 = (b15 = b16);
				}
				if (b14 == 0)
				{
					b14 = (b15 = (b16 = 1));
				}
				else
				{
					if (b15 == 0)
					{
						b15 = b14;
					}
					if (b16 == 0)
					{
						b16 = b14;
					}
				}
			}
			byte b17 = (flag4 ? b14 : ((!(num10 < 0.5f)) ? b15 : b16));
			Vector3 vector = array5[b7] * num11 + array5[b6] * num10;
			verts.Normal_t.Add(new Vector4(vector.x / 256f, vector.y / 256f, vector.z / 256f, (int)b17));
			Vector3 vector2 = ((!flag4) ? ((Vector3)array[b7] * num11 + (Vector3)array[b6] * num10) : ((Vector3)array[b10]));
			if (flag2)
			{
				Vector3 zero = Vector3.zero;
				switch (intVector4.x)
				{
				case 0:
					zero.x = (float)intVector4.y * cellSize;
					vector2.x = intVector5.x;
					break;
				case 1:
					zero.y = (float)intVector4.y * cellSize;
					vector2.y = intVector5.y;
					break;
				case 2:
					zero.z = (float)intVector4.y * cellSize;
					vector2.z = intVector5.z;
					break;
				}
				verts.Near.Add(b);
				verts.Position.Add((zero + vector2) * num3);
			}
			else
			{
				verts.Near.Add(0);
				verts.Position.Add(vector2 * num3);
			}
		}
		for (int k = 0; k < num6; k++)
		{
			if (flag)
			{
				indices.Add(array6[regularCell[k * 3]]);
				indices.Add(array6[regularCell[k * 3 + 1]]);
				indices.Add(array6[regularCell[k * 3 + 2]]);
			}
			else
			{
				indices.Add(array6[regularCell[k * 3 + 2]]);
				indices.Add(array6[regularCell[k * 3 + 1]]);
				indices.Add(array6[regularCell[k * 3]]);
			}
		}
		return num6;
	}

	public static void generateNegativeXTransitionCells(VFVoxelChunkData chunkData, float cellSize, TransVertices verts, List<int> indices)
	{
		int dirIndex = 1;
		TransitionCache cache = new TransitionCache();
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				PolygonizeTransitionCell(j, i, dirIndex, cellSize, chunkData, verts, indices, cache);
			}
		}
	}

	public static void BuildTransitionCells(int faceMask, VFVoxelChunkData chunkData, float cellSize, TransVertices verts, List<int> indices)
	{
		for (int i = 0; i < 6; i++)
		{
			if ((faceMask & (1 << i)) == 0)
			{
				continue;
			}
			int dirIndex = i;
			TransitionCache cache = new TransitionCache();
			int num = 16;
			for (int j = 0; j < num; j++)
			{
				for (int k = 0; k < num; k++)
				{
					PolygonizeTransitionCell(k, j, dirIndex, cellSize, chunkData, verts, indices, cache);
				}
			}
		}
	}
}
