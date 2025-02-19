using PeMap;

namespace Pathea.GameLoader;

internal class LoadCustomMap : LoadMap
{
	public LoadCustomMap(bool bNew)
		: base(bNew)
	{
	}

	protected override void New()
	{
		base.New();
		MapRunner.UpdateTile(value: false);
	}

	protected override void Restore()
	{
		base.Restore();
		MapRunner.UpdateTile(value: false);
	}
}
