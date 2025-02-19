using CustomCharactor;
using UnityEngine;

namespace Pathea;

public static class PeGender
{
	public static PeSex Random()
	{
		return (PeSex)UnityEngine.Random.Range(1, 3);
	}

	public static PeSex Convert(int v)
	{
		return v switch
		{
			0 => PeSex.Undefined, 
			1 => PeSex.Female, 
			2 => PeSex.Male, 
			_ => PeSex.Max, 
		};
	}

	public static PeSex Convert(ESex v)
	{
		return v switch
		{
			ESex.Female => PeSex.Female, 
			ESex.Male => PeSex.Male, 
			_ => PeSex.Max, 
		};
	}

	public static ESex Convert(PeSex v)
	{
		return v switch
		{
			PeSex.Male => ESex.Male, 
			PeSex.Female => ESex.Female, 
			_ => ESex.Max, 
		};
	}

	public static bool IsMatch(PeSex sex, PeSex require)
	{
		if (sex == PeSex.Undefined)
		{
			return true;
		}
		if (sex == require)
		{
			return true;
		}
		return false;
	}
}
