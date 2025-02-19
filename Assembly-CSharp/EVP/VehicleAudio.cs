using System;
using UnityEngine;

namespace EVP;

[RequireComponent(typeof(VehicleController))]
public class VehicleAudio : MonoBehaviour
{
	public float maxWheelRpmChangeRate = 400f;

	[Header("Engine")]
	public AudioSource engineAudio;

	public float engineAudioBaseRpm = 5000f;

	[Space(5f)]
	public float engineIdleRpm = 600f;

	public float engineGearDownRpm = 3000f;

	public float engineGearUpRpm = 5000f;

	public float engineMaxRpm = 6000f;

	public int engineGearCount = 5;

	public float engineTransmissionRatio = 13.6f;

	public float engineRpmChangeRateUp = 5f;

	public float engineRpmChangeRateDown = 20f;

	[Space(5f)]
	public float engineVolumeAtRest = 0.4f;

	public float engineVolumeAtFullLoad = 0.8f;

	public float engineVolumeChangeRateUp = 24f;

	public float engineVolumeChangeRateDown = 8f;

	[Header("Impacts")]
	public AudioClip bodyImpactHardAudio;

	public AudioClip bodyImpactSoftAudio;

	[Range(0f, 1f)]
	public float impactSoftnessThreshold = 0.4f;

	[Space(5f)]
	public float impactMinSpeed = 0.2f;

	public float impactMaxSpeed = 12f;

	[Space(5f)]
	public float impactMinPitch = 0.3f;

	public float impactMaxPitch = 0.6f;

	public float impactRandomPitch = 0.2f;

	[Space(5f)]
	public float impactMinVolume = 0.3f;

	public float impactMaxVolume = 1f;

	public float impactRandomVolume = 0.2f;

	private VehicleController m_vehicle;

	private float m_engineRpm;

	private float m_engineRpmDamp;

	private float m_wheelsRpm;

	private int m_gear;

	private int m_lastGear;

	public int simulatedGear => m_gear;

	public float simulatedEngineRpm => m_engineRpm;

	private void OnEnable()
	{
		m_vehicle = GetComponent<VehicleController>();
		m_vehicle.processContacts = true;
		VehicleController vehicle = m_vehicle;
		vehicle.onImpact = (VehicleController.OnImpact)Delegate.Combine(vehicle.onImpact, new VehicleController.OnImpact(DoImpactAudio));
		if (engineGearCount < 2)
		{
			engineGearCount = 2;
		}
		m_engineRpmDamp = engineRpmChangeRateUp;
		m_wheelsRpm = 0f;
	}

	private void OnDisable()
	{
		StopAudio(engineAudio);
	}

	private void FixedUpdate()
	{
		DoEngineAudio();
	}

	private void DoEngineAudio()
	{
		if (engineAudio == null)
		{
			return;
		}
		float num = 0f;
		int num2 = 0;
		WheelData[] wheelData = m_vehicle.wheelData;
		foreach (WheelData wheelData2 in wheelData)
		{
			if (wheelData2.wheel.drive)
			{
				num2++;
				num += wheelData2.angularVelocity;
			}
		}
		if (num2 == 0)
		{
			engineAudio.Stop();
			return;
		}
		num /= (float)num2;
		m_wheelsRpm = Mathf.MoveTowards(m_wheelsRpm, num * 57.29578f / 6f, maxWheelRpmChangeRate * Time.deltaTime);
		float num3 = m_wheelsRpm * engineTransmissionRatio;
		float value;
		if (Mathf.Abs(m_wheelsRpm) < 1f)
		{
			m_gear = 0;
			value = engineIdleRpm + Mathf.Abs(num3);
		}
		else if (num3 >= 0f)
		{
			float num4 = engineGearUpRpm - engineIdleRpm;
			if (num3 < num4)
			{
				m_gear = 1;
				value = num3 + engineIdleRpm;
			}
			else
			{
				float num5 = engineGearUpRpm - engineGearDownRpm;
				m_gear = 2 + (int)((num3 - num4) / num5);
				if (m_gear > engineGearCount)
				{
					m_gear = engineGearCount;
					value = num3 - num4 - (float)(engineGearCount - 2) * num5 + engineGearDownRpm;
				}
				else
				{
					value = Mathf.Repeat(num3 - num4, num5) + engineGearDownRpm;
				}
			}
		}
		else
		{
			m_gear = -1;
			value = Mathf.Abs(num3) + engineIdleRpm;
		}
		value = Mathf.Clamp(value, 10f, engineMaxRpm);
		if (m_gear != m_lastGear)
		{
			m_engineRpmDamp = ((m_gear <= m_lastGear) ? engineRpmChangeRateDown : engineRpmChangeRateUp);
			m_lastGear = m_gear;
		}
		m_engineRpm = Mathf.Lerp(m_engineRpm, value, m_engineRpmDamp * Time.deltaTime);
		ProcessContinuousAudio(engineAudio, engineAudioBaseRpm, m_engineRpm);
		ProcessVolume(engineAudio, engineVolumeAtRest, engineVolumeAtFullLoad, Mathf.Abs(m_vehicle.throttleInput), engineVolumeChangeRateUp, engineVolumeChangeRateDown);
	}

	private void DoImpactAudio()
	{
		if (!bodyImpactHardAudio && !bodyImpactSoftAudio)
		{
			return;
		}
		float magnitude = m_vehicle.localImpactVelocity.magnitude;
		if (magnitude > impactMinSpeed)
		{
			float t = Mathf.InverseLerp(impactMinSpeed, impactMaxSpeed, magnitude);
			AudioClip audioClip = null;
			audioClip = (bodyImpactSoftAudio ? ((!(m_vehicle.localImpactFriction < impactSoftnessThreshold)) ? bodyImpactHardAudio : bodyImpactSoftAudio) : bodyImpactHardAudio);
			if ((bool)audioClip)
			{
				PlayOneTime(audioClip, m_vehicle.cachedTransform.TransformPoint(m_vehicle.localImpactPosition), Mathf.Lerp(impactMinVolume, impactMaxVolume, t) + UnityEngine.Random.Range(0f - impactRandomVolume, impactRandomVolume), Mathf.Lerp(impactMinPitch, impactMaxPitch, t) + UnityEngine.Random.Range(0f - impactRandomPitch, impactRandomPitch));
			}
		}
	}

	private void StopAudio(AudioSource audio)
	{
		if (audio != null)
		{
			audio.Stop();
		}
	}

	private void ProcessContinuousAudio(AudioSource audio, float baseValue, float value)
	{
		if (!(audio == null))
		{
			float pitch = value / baseValue;
			audio.pitch = pitch;
			if (!audio.isPlaying)
			{
				audio.Play();
			}
			audio.loop = true;
		}
	}

	private void ProcessVolume(AudioSource audio, float minVolume, float maxVolume, float ratio, float changeRateUp, float changeRateDown)
	{
		float num = Mathf.Lerp(minVolume, maxVolume, ratio);
		float num2 = ((!(num > audio.volume)) ? changeRateDown : changeRateUp);
		audio.volume = Mathf.Lerp(audio.volume, num, Time.deltaTime * num2);
	}

	private void PlayOneTime(AudioClip clip, Vector3 position, float volume)
	{
		PlayOneTime(clip, position, volume, 1f);
	}

	private void PlayOneTime(AudioClip clip, Vector3 position, float volume, float pitch)
	{
		if (!(clip == null))
		{
			GameObject gameObject = new GameObject("One shot audio");
			gameObject.transform.parent = m_vehicle.cachedTransform;
			gameObject.transform.position = position;
			AudioSource audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.spatialBlend = 1f;
			audioSource.clip = clip;
			audioSource.volume = volume;
			audioSource.pitch = pitch;
			audioSource.Play();
			UnityEngine.Object.Destroy(gameObject, clip.length);
		}
	}
}
