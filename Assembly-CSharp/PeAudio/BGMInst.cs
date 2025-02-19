using UnityEngine;

namespace PeAudio;

public class BGMInst : MonoBehaviour
{
	public FMODAudioSource audioSrc;

	private float targetVolume;

	private float preDampRate = 1f;

	private float postDampRate = 1f;

	private float pretime;

	private float posttime;

	private string nextAudioPath = string.Empty;

	private void Awake()
	{
		audioSrc = base.gameObject.AddComponent<FMODAudioSource>();
	}

	private void OnDestroy()
	{
		Object.Destroy(audioSrc);
	}

	private void Update()
	{
		if (nextAudioPath != audioSrc.path)
		{
			if (pretime > 0f)
			{
				pretime -= Time.deltaTime;
			}
			else
			{
				targetVolume = 0f;
				if (audioSrc.volume < 0.005f)
				{
					posttime -= Time.deltaTime;
					if (posttime <= 0f)
					{
						audioSrc.path = nextAudioPath;
					}
				}
			}
			if (Mathf.Abs(audioSrc.volume - targetVolume) > 0.002f)
			{
				audioSrc.volume = Mathf.Lerp(audioSrc.volume, targetVolume, preDampRate);
			}
			else
			{
				audioSrc.volume = targetVolume;
			}
		}
		else
		{
			pretime = 99999f;
			targetVolume = 1f;
			posttime = 99999f;
			if (Mathf.Abs(audioSrc.volume - targetVolume) > 0.002f)
			{
				audioSrc.volume = Mathf.Lerp(audioSrc.volume, targetVolume, postDampRate);
			}
			else
			{
				audioSrc.volume = targetVolume;
			}
		}
	}

	public void ChangeBGM(string path, float prewarm, float postwarm, float predamp, float postdamp)
	{
		nextAudioPath = path;
		pretime = prewarm;
		posttime = postwarm;
		preDampRate = predamp;
		postDampRate = postdamp;
	}
}
