using System.Collections.Generic;

namespace Pathea.GameLoader;

internal class LoadCustomDragItem : PeLauncher.ILaunchable
{
	private IEnumerable<WEItem> mItems;

	public LoadCustomDragItem(IEnumerable<WEItem> items)
	{
		mItems = items;
	}

	void PeLauncher.ILaunchable.Launch()
	{
		if (mItems != null)
		{
		}
	}
}
