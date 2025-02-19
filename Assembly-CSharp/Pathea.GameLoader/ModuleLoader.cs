namespace Pathea.GameLoader;

internal abstract class ModuleLoader : PeLauncher.ILaunchable
{
	private bool mNew;

	public ModuleLoader(bool bNew)
	{
		mNew = bNew;
	}

	void PeLauncher.ILaunchable.Launch()
	{
		if (mNew)
		{
			New();
		}
		else
		{
			Restore();
		}
	}

	protected abstract void New();

	protected abstract void Restore();
}
