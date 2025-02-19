using UnityEngine;

public class EffectSoundCtrl : MonoBehaviour
{
	private void Update()
	{
		if ((bool)GetComponent<AudioSource>())
		{
			GetComponent<AudioSource>().volume = SystemSettingData.Instance.EffectVolume * SystemSettingData.Instance.SoundVolume;
		}
	}
}
