using UnityEngine;

public class BGEAdventure : BGEffect
{
	internal override int GetMapID(Vector3 position)
	{
		return VFDataRTGen.GetXZMapType((int)position.x, (int)position.z) switch
		{
			RandomMapType.GrassLand => 2, 
			RandomMapType.Forest => 8, 
			RandomMapType.Desert => 13, 
			RandomMapType.Redstone => 18, 
			RandomMapType.Rainforest => 10, 
			_ => 0, 
		};
	}
}
