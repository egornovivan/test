using System.Collections.Generic;
using UnityEngine;

namespace PETools;

public static class AreaHelper
{
	public const int LenthPerArea = 128;

	public static int boundaryWest = -20000;

	public static int boundaryEast = 20000;

	public static int boundarySouth = -20000;

	public static int boundaryNorth = 20000;

	public static int mapRadius = 20000;

	public static float boundStart = 0.5f;

	public static int boundOffset = 200;

	public static int BorderOffset = 300;

	public static int BoudaryEdgeDistance => mapRadius + BorderOffset;

	public static int NumPerSide => HalfPerSide * 2;

	public static int HalfLength => Length / 2;

	public static int Length => Mathf.FloorToInt(Mathf.Max(MapSize.x, MapSize.y));

	public static int HalfPerSide
	{
		get
		{
			int num = HalfLength / 128;
			return (HalfLength % 128 != 0) ? (num + 1) : num;
		}
	}

	public static Vector2 MapSize => new Vector2(BoudaryEdgeDistance * 2, BoudaryEdgeDistance * 2);

	public static void SetMapParam()
	{
		switch (ServerConfig.MapSize)
		{
		case 0:
			SetBoundary(-20000, 20000, -20000, 20000, 200);
			break;
		case 1:
			SetBoundary(-10000, 10000, -10000, 10000, 100);
			break;
		case 2:
			SetBoundary(-4000, 4000, -4000, 4000, 40);
			break;
		case 3:
			SetBoundary(-2000, 2000, -2000, 2000, 20);
			break;
		case 4:
			SetBoundary(-1000, 1000, -1000, 1000, 10);
			break;
		}
	}

	public static void SetBoundary(int west, int east, int south, int north, int offset)
	{
		boundaryWest = west;
		boundaryEast = east;
		boundarySouth = south;
		boundaryNorth = north;
		mapRadius = east;
		boundOffset = offset;
	}

	public static int Vector2Index(Vector3 pos)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		num = ((!(pos.x > 0f)) ? (-128) : 128);
		num2 = (int)(pos.x + (float)num) / 256;
		num = ((!(pos.z > 0f)) ? (-128) : 128);
		num3 = (int)(pos.z + (float)num) / 256;
		return (num2 << 16) | num3;
	}

	public static int PosToIndex(Vector3 pos)
	{
		int num = Mathf.FloorToInt(pos.x / 128f) + 1024;
		int num2 = Mathf.FloorToInt(pos.y / 128f) + 1;
		int num3 = Mathf.FloorToInt(pos.z / 128f) + 1024;
		return (num << 20) | (num2 << 12) | num3;
	}

	public static int PosToIndex(IntVector3 pos)
	{
		int num = pos.x / 128 + 1024;
		int num2 = pos.y / 128 + 1;
		int num3 = pos.z / 128 + 1024;
		return (num << 20) | (num2 << 12) | num3;
	}

	public static int PosXToIndex(float x)
	{
		return Mathf.FloorToInt(x / 128f) + 1024;
	}

	public static int PosXToIndex(int x)
	{
		return x / 128 + 1024;
	}

	public static int PosYToIndex(float y)
	{
		return Mathf.FloorToInt(y / 128f) + 1;
	}

	public static int PosYToIndex(int y)
	{
		return y / 128 + 1;
	}

	public static int PosZToIndex(float z)
	{
		return Mathf.FloorToInt(z / 128f) + 1024;
	}

	public static int PosZToIndex(int z)
	{
		return z / 128 + 1024;
	}

	public static int ToIndex(int x, int y, int z)
	{
		x += 1024;
		y++;
		z += 1024;
		return (x << 20) | (y << 12) | z;
	}

	public static int IndexX(int index)
	{
		return (index >> 20) - 1024;
	}

	public static int IndexY(int index)
	{
		return ((index & 0xFF000) >> 12) - 1;
	}

	public static int IndexZ(int index)
	{
		return (index & 0xFFF) - 1024;
	}

	public static IEnumerable<int> SudokuIndexes(int centerIndex)
	{
		int centerX = IndexX(centerIndex);
		int centerY = IndexY(centerIndex);
		int centerZ = IndexZ(centerIndex);
		for (int x = centerX - 1; x <= centerX + 1; x++)
		{
			for (int y = centerY - 1; y <= centerY + 1; y++)
			{
				for (int z = centerZ - 1; z <= centerZ + 1; z++)
				{
					int index = ToIndex(x, y, z);
					if (index != centerIndex)
					{
						yield return index;
					}
				}
			}
		}
	}

	public static int Vector2Int(Vector3 pos, int area = 128)
	{
		int num = Mathf.FloorToInt(pos.x / (float)area) + HalfPerSide;
		int num2 = Mathf.FloorToInt(pos.z / (float)area) + HalfPerSide;
		return num + num2 * NumPerSide;
	}

	public static int IntVector2Int(IntVector3 pos)
	{
		int num = pos.x / 128 + HalfPerSide;
		int num2 = pos.z / 128 + HalfPerSide;
		return num + num2 * NumPerSide;
	}

	public static int IntVector2Int(IntVector2 pos)
	{
		int num = pos.x / 128 + HalfPerSide;
		int num2 = pos.y / 128 + HalfPerSide;
		return num + num2 * NumPerSide;
	}

	public static List<int> GetNeighborIndex(int centerIndex)
	{
		int num = centerIndex % NumPerSide;
		int num2 = centerIndex / NumPerSide;
		List<int> list = new List<int>();
		for (int i = num - 1; i <= num + 1; i++)
		{
			for (int j = num2 - 1; j <= num2 + 1; j++)
			{
				if (i >= 0 && i < NumPerSide && j >= 0 && j < NumPerSide)
				{
					int item = i + j * NumPerSide;
					list.Add(item);
				}
			}
		}
		return list;
	}
}
