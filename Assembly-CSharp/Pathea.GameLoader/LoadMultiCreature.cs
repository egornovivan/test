using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadMultiCreature : PeLauncher.ILaunchable
{
	void PeLauncher.ILaunchable.Launch()
	{
		Vector3 pos;
		if (PeGameMgr.IsMultiStory)
		{
			pos = new Vector3(12227f, 121.5f, 6095f);
		}
		else
		{
			VArtifactUtil.GetSpawnPos();
			pos = PeSingleton<PlayerSpawnPosProvider>.Instance.GetPos();
		}
		BaseNetwork.MainPlayer.RequestPlayerLogin(pos);
	}
}
