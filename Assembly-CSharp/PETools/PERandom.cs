using System;

namespace PETools;

public class PERandom
{
	private static int Seed = 100;

	private static Random s_Seed = new Random(Seed);

	private static Random _BehaveSeed;

	private static int[] s_BehaveSeeds = new int[6] { 12, 45, 89, 56, 4164, 89898 };

	public static Random BehaveSeed
	{
		get
		{
			if (_BehaveSeed == null)
			{
				_BehaveSeed = new Random(s_BehaveSeeds[s_Seed.Next(0, s_BehaveSeeds.Length)]);
			}
			return _BehaveSeed;
		}
	}
}
