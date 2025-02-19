using System.Collections.Generic;
using System.Linq;

public class TownGenData
{
	public const int PlayerStartTownCount = 4;

	public const int PlayerStartEmptyTownCount = 3;

	public const int EmptyTownMinDistanceSmall = 300;

	public const int EmptyTownMaxDistanceSmall = 800;

	public const int EmptyTownMinDistanceMiddle = 500;

	public const int EmptyTownMaxDistanceMiddle = 1000;

	public const int EmptyTownMinDistanceLarge = 1000;

	public const int EmptyTownMaxDistanceLarge = 1500;

	public const int PlayerAreaCountSmall = 4;

	public const int PlayerAreaCountMiddle = 2;

	public const int PlayerAreaCountlarge = 1;

	public const int BranchTownCountSmallMax = 0;

	public const int BranchTownCountMiddleMax = 1;

	public const int BranchTownCountLargeMax = 2;

	public const int BranchTownCountHugeMax = 3;

	public const int SpecifiedTownId = 6;

	public const int SpecifiedTownLevel = 2;

	public const int SpecifiedTownId1 = 4;

	public const int SpecifiedTownLevel1 = 2;

	public const int AreaLevelMax = 4;

	public const int BlockRadius = 500;

	public const float BranchTownCount = 0.33f;

	public const float EnemyMainTown = 0.8f;

	public const int PlayerAlly = 0;

	public const int ConnectionCutDist0 = 2000;

	public const int ConnectionCutDist1 = 1500;

	public const int ConnectionCutDist2 = 1000;

	public const int ConnectionCutDist3 = 500;

	public const float DistCutPer0 = 1f;

	public const float DistCutPer1 = 0.9f;

	public const float DistCutPer2 = 0.8f;

	public const float DistCutPer3 = 0.7f;

	public static readonly IntVector2[] AreaIndex = new IntVector2[16]
	{
		new IntVector2(-2, 2),
		new IntVector2(-1, 2),
		new IntVector2(1, 2),
		new IntVector2(2, 2),
		new IntVector2(-2, 1),
		new IntVector2(-1, 1),
		new IntVector2(1, 1),
		new IntVector2(2, 1),
		new IntVector2(-2, -1),
		new IntVector2(-1, -1),
		new IntVector2(1, -1),
		new IntVector2(2, -1),
		new IntVector2(-2, -2),
		new IntVector2(-1, -2),
		new IntVector2(1, -2),
		new IntVector2(2, -2)
	};

	public static readonly int[][] GenerationLine = new int[10][]
	{
		new int[16]
		{
			1, 0, 4, 5, 2, 3, 7, 6, 9, 8,
			12, 13, 14, 10, 11, 15
		},
		new int[16]
		{
			1, 0, 4, 5, 6, 2, 3, 7, 11, 15,
			14, 10, 9, 8, 12, 13
		},
		new int[16]
		{
			1, 0, 4, 8, 12, 13, 9, 5, 6, 2,
			3, 7, 11, 10, 14, 15
		},
		new int[16]
		{
			1, 2, 3, 7, 6, 5, 0, 4, 8, 9,
			10, 11, 15, 14, 13, 12
		},
		new int[16]
		{
			1, 2, 3, 7, 11, 15, 14, 13, 9, 10,
			6, 5, 0, 4, 8, 12
		},
		new int[16]
		{
			1, 2, 6, 3, 7, 11, 15, 14, 10, 9,
			5, 0, 4, 8, 12, 13
		},
		new int[16]
		{
			1, 5, 0, 4, 8, 12, 13, 14, 15, 11,
			10, 9, 6, 2, 3, 7
		},
		new int[16]
		{
			1, 5, 0, 4, 8, 12, 13, 9, 10, 6,
			2, 3, 7, 11, 15, 14
		},
		new int[16]
		{
			0, 1, 5, 4, 8, 12, 13, 9, 10, 6,
			2, 3, 7, 11, 15, 14
		},
		new int[16]
		{
			0, 4, 8, 12, 13, 9, 10, 14, 15, 11,
			7, 6, 5, 1, 2, 3
		}
	};

	public static readonly int[][] AreaLevel = new int[7][]
	{
		new int[16]
		{
			0, 0, 1, 1, 1, 2, 2, 2, 3, 3,
			3, 3, 4, 4, 4, 4
		},
		new int[16]
		{
			0, 0, 0, 1, 1, 1, 2, 2, 2, 3,
			3, 3, 3, 4, 4, 4
		},
		new int[16]
		{
			0, 0, 0, 0, 1, 1, 1, 1, 2, 2,
			2, 3, 3, 3, 4, 4
		},
		new int[16]
		{
			0, 0, 1, 1, 1, 2, 2, 2, 2, 3,
			3, 3, 3, 4, 4, 4
		},
		new int[16]
		{
			0, 1, 1, 1, 2, 2, 2, 2, 2, 3,
			3, 3, 3, 3, 4, 4
		},
		new int[16]
		{
			0, 0, 1, 1, 2, 2, 2, 3, 3, 3,
			3, 4, 4, 4, 4, 4
		},
		new int[16]
		{
			0, 1, 1, 2, 2, 3, 3, 3, 3, 3,
			4, 4, 4, 4, 4, 4
		}
	};

	public static readonly int[] AreaTownAmountMin = new int[5] { 4, 2, 1, 0, 0 };

	public static readonly int[] AreaTownAmountMax = new int[5] { 8, 4, 2, 1, 1 };

	public static readonly int[][] EnemyNpcAlly = new int[5][]
	{
		new int[1] { 15 },
		new int[2] { 7, 15 },
		new int[3] { 5, 10, 15 },
		new int[4] { 4, 8, 12, 15 },
		new int[5] { 3, 6, 9, 12, 15 }
	};

	public static readonly int[][] NativeAlly = new int[5][]
	{
		new int[1] { 4 },
		new int[2] { 1, 4 },
		new int[3] { 0, 2, 4 },
		new int[4] { 0, 1, 2, 4 },
		new int[5] { 0, 1, 2, 3, 4 }
	};

	public static int AreaCount => AreaIndex.Length;

	public static int AreaRadius => RandomMapConfig.Instance.mapRadius / 2;

	public static int GetTownAmountMin()
	{
		return AreaTownAmountMin[RandomMapConfig.mapSize];
	}

	public static int GetTownAmountMax()
	{
		return AreaTownAmountMax[RandomMapConfig.mapSize];
	}

	public static IntVector2 GetAreaIndex(IntVector2 pos)
	{
		int num = pos.x % AreaRadius;
		int num2 = pos.x / AreaRadius;
		if (num > 0)
		{
			num2++;
		}
		else if (num < 0)
		{
			num2--;
		}
		else if (num2 == 0)
		{
			num2++;
		}
		if (num2 < -2)
		{
			num2 = -2;
		}
		if (num2 > 2)
		{
			num2 = 2;
		}
		int num3 = pos.y % AreaRadius;
		int num4 = pos.y / AreaRadius;
		if (num3 > 0)
		{
			num4++;
		}
		else if (num3 < 0)
		{
			num4--;
		}
		else if (num4 == 0)
		{
			num4++;
		}
		if (num4 < -2)
		{
			num4 = -2;
		}
		if (num4 > 2)
		{
			num4 = 2;
		}
		return new IntVector2(num2, num4);
	}

	public static int GetAreaId(IntVector2 pos)
	{
		IntVector2 index = GetAreaIndex(pos);
		List<IntVector2> list = AreaIndex.ToList();
		return list.FindIndex((IntVector2 it) => it.Equals(index));
	}

	public static int GetLevel(int pickedGenerationLine, int pickedLevelLine, int areaId)
	{
		int num = GenerationLine[pickedGenerationLine].ToList().FindIndex((int it) => it == areaId);
		return AreaLevel[pickedLevelLine][num];
	}

	public static int GetLevel(int pickedLevelLine, int lineIndex)
	{
		return AreaLevel[pickedLevelLine][lineIndex];
	}
}
