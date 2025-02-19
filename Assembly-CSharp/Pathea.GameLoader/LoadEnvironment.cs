namespace Pathea.GameLoader;

internal class LoadEnvironment : PeLauncher.ILaunchable
{
	void PeLauncher.ILaunchable.Launch()
	{
		PeEnv.Init();
	}
}
