namespace Pathea.GameLoader;

internal class LoadPathFindingEx : PeLauncher.ILaunchable
{
	void PeLauncher.ILaunchable.Launch()
	{
		if (AstarPath.active != null)
		{
			AstarPath.active.Scan(253);
		}
	}
}
