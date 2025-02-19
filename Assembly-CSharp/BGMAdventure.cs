using PETools;
using UnityEngine;

public class BGMAdventure : BGManager
{
	protected override int GetCurrentBgMusicID()
	{
		Vector3 position = PEUtil.MainCamTransform.position;
		if (AiUtil.CheckPositionInCave(position, 128f, AiUtil.groundedLayer))
		{
			return 836;
		}
		int num = (int)position.x;
		int num2 = (int)position.z;
		if (num <= int.MinValue)
		{
			Debug.LogError("x value too small!");
			num = -2147483647;
		}
		if (num2 <= int.MinValue)
		{
			Debug.LogError("z value too small!");
			num2 = -2147483647;
		}
		if (VFDataRTGen.IsSea(num, num2))
		{
			return AISpawnDataStory.GetBackGroundMusic(new Color(1f, 20f / 51f, 0.5882353f, 1f));
		}
		return VFDataRTGen.GetXZMapType(num, num2) switch
		{
			RandomMapType.GrassLand => AISpawnDataStory.GetBackGroundMusic(new Color(0.11764706f, 10f / 51f, 10f / 51f, 1f)), 
			RandomMapType.Forest => AISpawnDataStory.GetBackGroundMusic(new Color(14f / 51f, 14f / 51f, 14f / 51f, 1f)), 
			RandomMapType.Desert => AISpawnDataStory.GetBackGroundMusic(new Color(28f / 51f, 20f / 51f, 10f / 51f, 1f)), 
			RandomMapType.Redstone => AISpawnDataStory.GetBackGroundMusic(new Color(2f / 3f, 14f / 51f, 10f / 51f, 1f)), 
			RandomMapType.Rainforest => AISpawnDataStory.GetBackGroundMusic(new Color(0.3529412f, 0.3529412f, 0.3529412f, 1f)), 
			RandomMapType.Mountain => AISpawnDataStory.GetBackGroundMusic(new Color(2f / 3f, 14f / 51f, 0.5882353f, 1f)), 
			RandomMapType.Swamp => AISpawnDataStory.GetBackGroundMusic(new Color(20f / 51f, 10f / 51f, 10f / 51f, 1f)), 
			RandomMapType.Crater => AISpawnDataStory.GetBackGroundMusic(new Color(0.7058824f, 0.7058824f, 0.7058824f, 1f)), 
			_ => 0, 
		};
	}
}
