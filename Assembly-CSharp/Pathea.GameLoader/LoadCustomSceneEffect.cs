using System.Collections.Generic;

namespace Pathea.GameLoader;

internal class LoadCustomSceneEffect : PeLauncher.ILaunchable
{
	private IEnumerable<WEEffect> mEffects;

	public LoadCustomSceneEffect(IEnumerable<WEEffect> effects)
	{
		mEffects = effects;
	}

	void PeLauncher.ILaunchable.Launch()
	{
		if (mEffects != null)
		{
		}
	}
}
