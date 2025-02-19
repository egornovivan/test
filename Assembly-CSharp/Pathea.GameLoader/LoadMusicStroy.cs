using UnityEngine;

namespace Pathea.GameLoader;

internal class LoadMusicStroy : PeLauncher.ILaunchable
{
	public void Launch()
	{
		GameObject gameObject = Resources.Load("Prefab/Audio/bg_music_story") as GameObject;
		if (gameObject != null)
		{
			Object.Instantiate(gameObject);
		}
	}
}
