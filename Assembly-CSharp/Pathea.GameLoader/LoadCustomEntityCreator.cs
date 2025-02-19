using PeCustom;

namespace Pathea.GameLoader;

internal class LoadCustomEntityCreator : ModuleLoader
{
	private YirdData mInfo;

	public LoadCustomEntityCreator(bool bNew, YirdData info)
		: base(bNew)
	{
		mInfo = info;
	}

	protected override void New()
	{
		CreateCreature();
	}

	protected override void Restore()
	{
		PeCustomScene.Self.SceneRestore(mInfo);
	}

	private void CreateCreature()
	{
		PeCustomScene.Self.SceneNew(mInfo);
	}
}
