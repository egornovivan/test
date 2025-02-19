using UnityEngine;

namespace Pathfinding;

public class ModifierConverter
{
	public static bool AllBits(ModifierData a, ModifierData b)
	{
		return (a & b) == b;
	}

	public static bool AnyBits(ModifierData a, ModifierData b)
	{
		return (a & b) != 0;
	}

	public static ModifierData Convert(Path p, ModifierData input, ModifierData output)
	{
		if (!CanConvert(input, output))
		{
			Debug.LogError(string.Concat("Can't convert ", input, " to ", output));
			return ModifierData.None;
		}
		if (AnyBits(input, output))
		{
			return input;
		}
		if (AnyBits(input, ModifierData.Nodes) && AnyBits(output, ModifierData.Vector))
		{
			p.vectorPath.Clear();
			for (int i = 0; i < p.vectorPath.Count; i++)
			{
				p.vectorPath.Add((Vector3)p.path[i].position);
			}
			return (ModifierData)(8 | (AnyBits(input, ModifierData.StrictNodePath) ? 4 : 0));
		}
		Debug.LogError(string.Concat("This part should not be reached - Error in ModifierConverted\nInput: ", input, " (", (int)input, ")\nOutput: ", output, " (", (int)output, ")"));
		return ModifierData.None;
	}

	public static bool CanConvert(ModifierData input, ModifierData output)
	{
		ModifierData b = CanConvertTo(input);
		return AnyBits(output, b);
	}

	public static ModifierData CanConvertTo(ModifierData a)
	{
		if (a == ModifierData.All)
		{
			return ModifierData.All;
		}
		ModifierData modifierData = a;
		if (AnyBits(a, ModifierData.Nodes))
		{
			modifierData |= ModifierData.VectorPath;
		}
		if (AnyBits(a, ModifierData.StrictNodePath))
		{
			modifierData |= ModifierData.StrictVectorPath;
		}
		if (AnyBits(a, ModifierData.StrictVectorPath))
		{
			modifierData |= ModifierData.VectorPath;
		}
		return modifierData;
	}
}
