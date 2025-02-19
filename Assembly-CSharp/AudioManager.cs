using System;
using System.Collections;
using System.Collections.Generic;
using SoundAsset;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	private static AudioManager _instance;

	private static Dictionary<string, AudioClip> s_AudioClipCaches = new Dictionary<string, AudioClip>();

	public List<AudioController> m_allAudios = new List<AudioController>();

	public List<AudioController> m_animFootStepAudios = new List<AudioController>();

	public static AudioManager instance
	{
		get
		{
			if (_instance == null)
			{
				GameObject gameObject = new GameObject("AudioManager");
				_instance = gameObject.AddComponent<AudioManager>();
				AudioController.InitPool();
			}
			return _instance;
		}
	}

	public AudioController CreateFootStepAudio(Vector3 position, int clipId, Transform parent = null, bool isPlay = true, bool isDelete = true)
	{
		AudioController audioController = Create(position, clipId, parent, isPlay, isDelete);
		if (audioController != null)
		{
			audioController.DestroyEvent = (Action<AudioController>)Delegate.Combine(audioController.DestroyEvent, new Action<AudioController>(OnDeleteFoot));
			m_animFootStepAudios.Add(audioController);
		}
		return audioController;
	}

	public AudioController Create(Vector3 position, int clipId, Transform parent = null, bool isPlay = true, bool isDelete = true)
	{
		if (clipId <= 0)
		{
			return null;
		}
		SESoundBuff sESoundData = SESoundBuff.GetSESoundData(clipId);
		if (sESoundData == null)
		{
			Debug.LogError("Can't find sound : " + clipId);
			return null;
		}
		AudioController audio = AudioController.GetAudio(sESoundData, position, (!parent) ? base.transform : parent, isPlay, isDelete);
		audio.DestroyEvent = (Action<AudioController>)Delegate.Combine(audio.DestroyEvent, new Action<AudioController>(OnDelete));
		m_allAudios.Add(audio);
		return audio;
	}

	public AudioClip GetAudioClip(string clipName)
	{
		AudioClip value = null;
		if (s_AudioClipCaches.TryGetValue(clipName, out value))
		{
			return value;
		}
		_instance.StartCoroutine(LoadAudio(clipName));
		return null;
	}

	private IEnumerator LoadAudio(string clipName)
	{
		s_AudioClipCaches[clipName] = null;
		ResourceRequest rr = Resources.LoadAsync<AudioClip>("Sound/" + clipName);
		AudioClip clip;
		while (true)
		{
			if (rr.isDone)
			{
				clip = rr.asset as AudioClip;
				if (clip != null && clip.loadState == AudioDataLoadState.Loaded)
				{
					break;
				}
			}
			yield return null;
		}
		s_AudioClipCaches[clipName] = clip;
	}

	private void OnDeleteFoot(AudioController audioCtrl)
	{
		audioCtrl.DestroyEvent = (Action<AudioController>)Delegate.Remove(audioCtrl.DestroyEvent, new Action<AudioController>(OnDeleteFoot));
		m_animFootStepAudios.Remove(audioCtrl);
	}

	private void OnDelete(AudioController audioCtrl)
	{
		audioCtrl.DestroyEvent = (Action<AudioController>)Delegate.Remove(audioCtrl.DestroyEvent, new Action<AudioController>(OnDelete));
		m_allAudios.Remove(audioCtrl);
	}

	private void Update()
	{
		int count = m_allAudios.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			if ((bool)m_allAudios[num])
			{
				m_allAudios[num].OnUpdate();
			}
			else
			{
				m_allAudios.RemoveAt(num);
			}
		}
	}
}
