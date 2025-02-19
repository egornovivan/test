using UnityEngine;

namespace PeAudio;

public class BGAInst : MonoBehaviour
{
	public FMODAudioSource audioSrc;

	public float targetVolume;

	private float silenceTime;

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
		if (targetVolume < 0.001f && audioSrc.volume < 0.005f)
		{
			silenceTime += Time.deltaTime;
			if (silenceTime > 5f)
			{
				if (base.gameObject.GetComponents<BGAInst>().Length == 1)
				{
					Object.Destroy(base.gameObject);
				}
				else
				{
					Object.Destroy(this);
				}
				return;
			}
		}
		else
		{
			silenceTime = 0f;
		}
		audioSrc.volume = Mathf.Lerp(audioSrc.volume, targetVolume, 0.05f);
	}
}
