using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadBuildPlayerSpawnPos : LoadPlayerSpawnPos
{
	public LoadBuildPlayerSpawnPos(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		SetPos(GetPlayerSpawnPos());
	}

	private Vector3 GetPlayerSpawnPos()
	{
		IntVector2 spawnPos = VArtifactUtil.GetSpawnPos();
		return new Vector3(spawnPos.x, VFDataRTGen.GetPosTop(spawnPos), spawnPos.y);
	}
}
