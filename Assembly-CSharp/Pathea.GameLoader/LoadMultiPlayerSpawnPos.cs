using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadMultiPlayerSpawnPos : PeLauncher.ILaunchable
{
	void PeLauncher.ILaunchable.Launch()
	{
		if (!(null == BaseNetwork.MainPlayer))
		{
			Vector3 pos = BaseNetwork.MainPlayer._pos;
			if (BaseNetwork.MainPlayer.UseNewPos)
			{
				IntVector2 spawnPos = VArtifactUtil.GetSpawnPos();
				pos = new Vector3(spawnPos.x, VFDataRTGen.GetPosTop(spawnPos), spawnPos.y);
			}
			PeSingleton<PlayerSpawnPosProvider>.Instance.SetPos(pos);
		}
	}
}
