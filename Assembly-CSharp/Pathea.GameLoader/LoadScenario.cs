using UnityEngine;

namespace Pathea.GameLoader;

internal abstract class LoadScenario : ModuleLoader
{
	public LoadScenario(bool bNew)
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
		Object.Instantiate(Resources.Load<GameObject>("Prefab/Mission/MissionManager"));
		Singleton<ForceSetting>.Instance.Load(Resources.Load("ForceSetting/ForceSettings_Story") as TextAsset);
	}
}
