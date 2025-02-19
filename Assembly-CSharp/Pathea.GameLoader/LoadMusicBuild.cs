using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadMusicBuild : PeLauncher.ILaunchable
{
	public void Launch()
	{
		GameObject gameObject = Resources.Load("Prefab/Audio/bg_music_build") as GameObject;
		if (gameObject != null)
		{
			Object.Instantiate(gameObject);
		}
	}
}
