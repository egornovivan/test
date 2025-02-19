using PeCustom;

namespace Pathea.GameLoader;

internal class LoadMultiCustom : LoadScenario
{
	private CustomGameData mData;

	public LoadMultiCustom(bool bNew, CustomGameData data)
		: base(bNew)
	{
		mData = data;
	}

	protected override void Init()
	{
		PeCustomScene.Self.ScenarioInit(mData);
	}

	protected override void New()
	{
		base.New();
		Init();
	}

	protected override void Restore()
	{
		base.Restore();
		Init();
		PeCustomScene.Self.ScenarioRestore();
	}
}
