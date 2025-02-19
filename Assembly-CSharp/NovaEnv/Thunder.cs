using UnityEngine;

namespace NovaEnv;

public class Thunder : MonoBehaviour
{
	[HideInInspector]
	public Executor Executor;

	public Light ThunderLight;

	public AudioSource ThunderAudio;

	public ThunderPrototype[] Protos = new ThunderPrototype[0];

	private bool isPlaying;

	private int protoIndex;

	private float playTime;

	private float strength;

	public float Freq = 4f;

	private float checkTime;

	private bool valid
	{
		get
		{
			if (Executor == null)
			{
				return false;
			}
			if (Protos == null)
			{
				return false;
			}
			if (Protos.Length == 0)
			{
				return false;
			}
			if (ThunderLight == null)
			{
				return false;
			}
			if (ThunderAudio == null)
			{
				return false;
			}
			return true;
		}
	}

	private void Play()
	{
		if (valid && !isPlaying)
		{
			isPlaying = true;
			protoIndex = Mathf.FloorToInt(Random.value * ((float)Protos.Length - 0.001f));
			strength = Mathf.Sqrt(Random.value);
			ThunderAudio.clip = Protos[protoIndex].Sound;
			base.transform.localPosition = new Vector3(Random.value * 500f, 300f, Random.value * 500f);
			playTime = 0f;
			ThunderLight.intensity = 0f;
			ThunderLight.cullingMask = Executor.Settings.LightCullingMask;
			ThunderLight.enabled = true;
			ThunderAudio.volume = 0f;
			ThunderAudio.enabled = false;
		}
	}

	private void EndPlaying()
	{
		isPlaying = false;
		ThunderLight.intensity = 0f;
		ThunderLight.enabled = false;
		ThunderAudio.volume = 0f;
		ThunderAudio.enabled = false;
	}

	private void Update()
	{
		if (!valid)
		{
			EndPlaying();
			return;
		}
		if (isPlaying)
		{
			playTime += Time.deltaTime;
			if (playTime > Protos[protoIndex].Delay && !ThunderAudio.enabled)
			{
				ThunderAudio.enabled = true;
				ThunderAudio.volume = strength * Executor.Settings.SoundVolume;
				ThunderAudio.Play();
			}
			if (ThunderAudio.clip.length - ThunderAudio.time < 1f)
			{
				ThunderAudio.volume = strength * Executor.Settings.SoundVolume * (ThunderAudio.clip.length - ThunderAudio.time);
			}
			ThunderLight.intensity = Protos[protoIndex].LightIntensityChange.Evaluate(playTime) * strength;
			if (playTime > 15f)
			{
				EndPlaying();
			}
			return;
		}
		checkTime += Time.deltaTime;
		if (checkTime > Freq)
		{
			checkTime = 0f;
			float num = (Executor.WetCoef - 0.6f) * 0.5f;
			if (Random.value < num)
			{
				Play();
			}
		}
	}
}
