using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadGameGraphInfo : PeLauncher.ILaunchable
{
	private Vector2 mSize;

	public LoadGameGraphInfo(Vector2 size)
	{
		mSize = size;
	}

	void PeLauncher.ILaunchable.Launch()
	{
		PeSingleton<PeMappingMgr>.Instance.Init(mSize);
	}
}
