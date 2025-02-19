using Pathea;
using UnityEngine;

public class BGEStory : BGEffect
{
	internal override int GetMapID(Vector3 position)
	{
		Vector2 targetPos = new Vector2(position.x, position.z);
		return PeSingleton<PeMappingMgr>.Instance.GetAiSpawnMapId(targetPos);
	}
}
