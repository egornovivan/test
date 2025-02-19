public class TownGenData
{
	public const int AreaLevelMax = 4;

	public const int BlockRadius = 500;

	public const float BranchTownCount = 0.33f;

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
}
