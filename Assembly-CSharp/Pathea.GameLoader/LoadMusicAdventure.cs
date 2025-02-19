using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadMusicAdventure : PeLauncher.ILaunchable
{
	public void Launch()
	{
		GameObject gameObject = Resources.Load("Prefab/Audio/bg_music_adventure") as GameObject;
		if (gameObject != null)
		{
			Object.Instantiate(gameObject);
		}
	}
}
