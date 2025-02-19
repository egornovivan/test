using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadMultiCustomCreature : PeLauncher.ILaunchable
{
	void PeLauncher.ILaunchable.Launch()
	{
		BaseNetwork.MainPlayer.RequestPlayerLogin(Vector3.zero);
	}
}
