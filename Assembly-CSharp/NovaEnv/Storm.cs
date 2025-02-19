using System;
using UnityEngine;

namespace NovaEnv;

public class Storm : MonoBehaviour
{
	[HideInInspector]
	public Executor Executor;

	public float WindTiltCoef = 0.15f;

	private float strength;

	public AudioSource StormAudio;

	public AudioSource RainDropsAudio;

	public ParticleSystem RainDropsNear;

	public ParticleSystem RainDropsFar;

	public ParticleSystem RainCollisionNear;

	public ParticleSystem RainCollisionFar;

	public ParticleSystem RainSpray;

	public AnimationCurve StormSoundVolChange;

	public AnimationCurve RainDropsSoundVolChange;

	public AnimationCurve RainDropsCountChange;

	public AnimationCurve RainCollisionCountChange;

	public AnimationCurve RainSprayCountChange;

	public float Strength
	{
		get
		{
			return strength;
		}
		set
		{
			if (Mathf.Abs(strength - value) > 0.01f)
			{
				strength = value;
			}
		}
	}

	private void UpdateStrength()
	{
		float num = 1f;
		float num2 = 0f;
		if (Executor.Settings.MainCamera != null)
		{
			Vector3 position = Executor.Settings.MainCamera.transform.position;
			for (float num3 = 30f; num3 < 90f; num3 += 15f)
			{
				float num4 = 360f;
				num4 = ((num3 < 40f) ? 22.5f : ((num3 < 70f) ? 30f : ((!(num3 < 80f)) ? 360f : 45f)));
				for (float num5 = 0f; num5 < 359.9f; num5 += num4)
				{
					Ray ray = new Ray(position, new Vector3(Mathf.Cos(num5 * ((float)Math.PI / 180f)) * Mathf.Cos(num3 * ((float)Math.PI / 180f)), Mathf.Sin(num3 * ((float)Math.PI / 180f)), Mathf.Sin(num5 * ((float)Math.PI / 180f)) * Mathf.Cos(num3 * ((float)Math.PI / 180f))));
					Ray ray2 = new Ray(ray.GetPoint(50f), -ray.direction.normalized);
					if (Physics.Raycast(ray, out var hitInfo, 50f))
					{
						float distance = hitInfo.distance;
						num2 += Mathf.Sin(num3 * ((float)Math.PI / 180f)) * (num4 / 360f);
						if (num3 > 40f && Physics.Raycast(ray2, out hitInfo, 50f))
						{
							float num6 = Mathf.Clamp01((hitInfo.distance - distance) / 15f);
							num2 += Mathf.Sin(num3 * ((float)Math.PI / 180f)) * (num4 / 360f) * num6;
						}
					}
				}
			}
			num -= Mathf.Clamp01(num2 * 0.12f);
		}
		float num7 = 1f / Executor.UnderwaterDensity;
		float soundVolume = Executor.Settings.SoundVolume;
		float maxRainParticleEmissiveRate = Executor.Settings.MaxRainParticleEmissiveRate;
		StormAudio.volume = Mathf.Lerp(StormAudio.volume, soundVolume * StormSoundVolChange.Evaluate(Strength) * num * num7, 0.03f);
		RainDropsAudio.volume = Mathf.Lerp(RainDropsAudio.volume, soundVolume * RainDropsSoundVolChange.Evaluate(Strength) * num * num7, 0.03f);
		RainDropsNear.emissionRate = maxRainParticleEmissiveRate * RainDropsCountChange.Evaluate(Strength);
		RainDropsFar.emissionRate = maxRainParticleEmissiveRate * RainDropsCountChange.Evaluate(Strength);
		RainCollisionNear.emissionRate = maxRainParticleEmissiveRate * RainCollisionCountChange.Evaluate(Strength);
		RainCollisionFar.emissionRate = maxRainParticleEmissiveRate * RainCollisionCountChange.Evaluate(Strength);
		RainSpray.emissionRate = maxRainParticleEmissiveRate * RainSprayCountChange.Evaluate(Strength) * 0.2f;
		RainDropsFar.gameObject.SetActive(Executor.Underwater <= 0f);
		RainDropsNear.gameObject.SetActive(Executor.Underwater <= 0f);
		RainCollisionFar.gameObject.SetActive(Executor.Underwater <= 0f);
		RainCollisionNear.gameObject.SetActive(Executor.Underwater <= 0f);
		RainSpray.gameObject.SetActive(Executor.Underwater <= 0f);
	}

	private void Update()
	{
		UpdateStrength();
	}
}
