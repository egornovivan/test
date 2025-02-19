using System.Collections.Generic;

namespace Pathea.GameLoader;

internal class LoadCustomDoodad : ModuleLoader
{
	private IEnumerable<WEDoodad> mItems;

	public LoadCustomDoodad(bool bNew, IEnumerable<WEDoodad> items)
		: base(bNew)
	{
		mItems = items;
	}

	protected override void New()
	{
		if (mItems != null)
		{
		}
	}

	protected override void Restore()
	{
	}
}
