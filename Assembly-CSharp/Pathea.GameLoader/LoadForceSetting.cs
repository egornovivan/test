using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadForceSetting : ModuleLoader
{
	public LoadForceSetting(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		Init();
	}

	protected override void Restore()
	{
		Init();
	}

	protected virtual void Init()
	{
		Singleton<ForceSetting>.Instance.Load(Resources.Load("ForceSetting/ForceSettings_Story") as TextAsset);
	}
}
