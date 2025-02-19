using System;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using SoundAsset;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioController : MonoBehaviour
{
	private const string c_defAuName = "AuPool";

	private static Stack<AudioController> _audioPool = new Stack<AudioController>();

	private float mVolume;

	private float mVolumeSpeed;

	private float mVolumeStart;

	private float mVolumeStartTime;

	private float mVolumeTarget;

	private string mClipName;

	private bool mAutoDel;

	internal float mVolumeScale;

	internal AudioType mType;

	internal AudioSource mAudio;

	public Action<AudioController> DestroyEvent;

	private float volumeScale
	{
		get
		{
			if (mType == AudioType.Null)
			{
				return 1f;
			}
			if (mType == AudioType.Music)
			{
				return SystemSettingData.Instance.SoundVolume * SystemSettingData.Instance.MusicVolume;
			}
			if (mType == AudioType.Dialog)
			{
				return SystemSettingData.Instance.SoundVolume * SystemSettingData.Instance.DialogVolume;
			}
			if (mType == AudioType.Effect)
			{
				return SystemSettingData.Instance.SoundVolume * SystemSettingData.Instance.EffectVolume;
			}
			if (mType == AudioType.UI)
			{
				return SystemSettingData.Instance.SoundVolume * SystemSettingData.Instance.EffectVolume;
			}
			return 1f;
		}
	}

	public bool autoDel
	{
		set
		{
			mAutoDel = value;
		}
	}

	public bool isPlaying => mAudio != null && mAudio.isPlaying;

	public float volumeTarget => mVolumeTarget;

	public float volume => (!(mAudio != null)) ? 0f : mAudio.volume;

	public float time => (!(mAudio != null)) ? 0f : mAudio.time;

	public float length => (!(mAudio != null) || !(mAudio.clip != null)) ? 0f : mAudio.clip.length;

	public static void InitPool()
	{
		_audioPool.Clear();
	}

	public static AudioController GetAudio(SESoundBuff buff, Vector3 pos, Transform parent, bool bPlay, bool bAutoDel)
	{
		AudioController audioController = null;
		if (_audioPool.Count > 0)
		{
			audioController = _audioPool.Pop();
		}
		if (audioController == null)
		{
			GameObject gameObject = new GameObject();
			audioController = gameObject.AddComponent<AudioController>();
		}
		audioController.InitData(buff, bAutoDel);
		audioController.name = "Au" + buff.mID;
		audioController.transform.position = pos;
		audioController.transform.parent = parent;
		if (bPlay)
		{
			audioController.PlayAudio();
		}
		return audioController;
	}

	public static void FreeAudio(AudioController au)
	{
		if (!_audioPool.Contains(au))
		{
			au.mAudio.Stop();
			if (au.DestroyEvent != null)
			{
				au.DestroyEvent(au);
			}
			au.name = "AuPool";
			au.mClipName = string.Empty;
			au.mAutoDel = false;
			au.mAudio.clip = null;
			au.StopAllCoroutines();
			au.CancelInvoke();
			au.transform.parent = AudioManager.instance.transform;
			if (!au.gameObject.activeSelf)
			{
				au.gameObject.SetActive(value: true);
			}
			_audioPool.Push(au);
		}
	}

	public void OnUpdate()
	{
		if (mVolumeSpeed <= float.Epsilon)
		{
			mVolume = Mathf.Clamp01(mVolumeTarget);
		}
		else
		{
			mVolume = Mathf.Clamp01(Mathf.Lerp(mVolumeStart, mVolumeTarget, (Time.time - mVolumeStartTime) / mVolumeSpeed));
		}
		mVolume = ((!PeGameMgr.gamePause || mType == AudioType.UI) ? mVolume : 0f);
		mAudio.volume = mVolume * mVolumeScale * volumeScale;
	}

	public void SetVolume(float targetVolume, float delayTime = 0f)
	{
		if (Mathf.Abs(mVolumeTarget - targetVolume) > float.Epsilon)
		{
			mVolumeStart = mVolume;
			mVolumeStartTime = Time.time;
			mVolumeSpeed = delayTime;
			mVolumeTarget = targetVolume;
		}
	}

	public void PlayAudio(float delayTime = 0f)
	{
		SetVolume(1f, delayTime);
		CancelInvoke();
		StartCoroutine(PlayAudioEnumerator());
	}

	public void PauseAudio(float delayTime = 0f)
	{
		SetVolume(0f, delayTime);
		if (mAudio != null && mAudio.isPlaying && !IsInvoking("Pause"))
		{
			Invoke("Pause", delayTime);
		}
	}

	public void StopAudio(float delayTime = 0f)
	{
		SetVolume(0f, delayTime);
		if (mAudio != null && mAudio.isPlaying && !IsInvoking("Stop"))
		{
			Invoke("Stop", delayTime);
		}
	}

	public void Delete(float delayTime = 0f)
	{
		if (delayTime < 0.01f)
		{
			Free();
		}
		else if (!IsInvoking("Free"))
		{
			Invoke("Free", delayTime);
		}
	}

	private IEnumerator LoadClip()
	{
		mAudio.clip = null;
		while (mAudio.clip == null)
		{
			mAudio.clip = AudioManager.instance.GetAudioClip(mClipName);
			yield return null;
		}
	}

	private IEnumerator PlayAudioEnumerator()
	{
		while (mAudio.clip == null)
		{
			yield return null;
		}
		if (!mAudio.isPlaying)
		{
			mAudio.Play();
		}
		if (mAutoDel)
		{
			yield return new WaitForSeconds(mAudio.clip.length + 1f);
			while (mAudio.isPlaying)
			{
				yield return null;
			}
			Delete();
		}
	}

	private void Stop()
	{
		if (mAudio != null)
		{
			mAudio.Stop();
		}
	}

	private void Pause()
	{
		if (mAudio != null)
		{
			mAudio.Pause();
		}
	}

	private void Free()
	{
		if (mAudio != null)
		{
			FreeAudio(this);
		}
	}

	private void InitData(SESoundBuff buff, bool isAutoDel)
	{
		if (mAudio == null)
		{
			mAudio = GetComponent<AudioSource>();
		}
		if (mAudio == null)
		{
			mAudio = base.gameObject.AddComponent<AudioSource>();
		}
		mAutoDel = isAutoDel;
		mClipName = buff.mName;
		mType = (AudioType)buff.mAudioType;
		mVolumeScale = buff.mVolume;
		if (mAudio != null)
		{
			mAudio.loop = buff.mLoop;
			mAudio.dopplerLevel = buff.mDoppler;
			mAudio.spatialBlend = buff.mSpatial;
			mAudio.rolloffMode = buff.mMode;
			mAudio.minDistance = buff.mMinDistance;
			mAudio.maxDistance = buff.mMaxDistance;
			mAudio.playOnAwake = false;
		}
		CancelInvoke();
		StopAllCoroutines();
		StartCoroutine(LoadClip());
	}
}
