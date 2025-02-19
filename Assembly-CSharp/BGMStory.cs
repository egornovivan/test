using Pathea;
using PETools;
using UnityEngine;

public class BGMStory : BGManager
{
	private const float compareValue = 2f;

	protected override int GetCurrentBgMusicID()
	{
		Vector3 position = PEUtil.MainCamTransform.position;
		if (AiUtil.CheckPositionInCave(position, 128f, AiUtil.groundedLayer))
		{
			return 836;
		}
		Vector2 targetPos = new Vector2(position.x, position.z);
		return AISpawnDataStory.GetBgMusicID(PeSingleton<PeMappingMgr>.Instance.GetAiSpawnMapId(targetPos));
	}
}
