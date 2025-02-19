using UnityEngine;

public class StormSoundCtrl : MonoBehaviour
{
	private void Update()
	{
		if (GetComponent<AudioSource>() != null)
		{
			GetComponent<AudioSource>().volume = SystemSettingData.Instance.EffectVolume * SystemSettingData.Instance.SoundVolume;
		}
	}
}
