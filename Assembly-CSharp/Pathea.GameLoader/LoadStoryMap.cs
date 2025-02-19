using PeMap;

namespace Pathea.GameLoader;

internal class LoadStoryMap : LoadMap
{
	public LoadStoryMap(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		PeSingleton<DetectedTownMgr>.Instance.RegistAtFirst();
		base.New();
		StoryStaticPoint.Load();
		MapRunner.UpdateTile(value: false);
	}

	protected override void Restore()
	{
		PeSingleton<DetectedTownMgr>.Instance.RegistAtFirst();
		base.Restore();
		MapRunner.UpdateTile(value: false);
	}
}
