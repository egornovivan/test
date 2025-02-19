using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class RadioManager : MonoBehaviour
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct RadioFileInfo
	{
		public string Name { get; private set; }

		public string FilePath { get; private set; }

		public string Extension { get; private set; }

		public string ExtensionNotDot { get; private set; }

		public bool PlayError { get; private set; }

		public RadioFileInfo(string path)
		{
			FilePath = path;
			Name = Path.GetFileNameWithoutExtension(path);
			Extension = Path.GetExtension(path).ToLower();
			ExtensionNotDot = Extension.Substring(1, Extension.Length - 1);
			PlayError = false;
		}

		public void SetPlayError(bool isPlayError)
		{
			PlayError = isPlayError;
		}
	}

	public enum SoundPlayState
	{
		Playing,
		Stop,
		Pause
	}

	public enum SoundPlayMode
	{
		Single,
		SingleLoop,
		Order,
		ListLoop,
		Random
	}

	private static RadioManager m_Instance;

	public Action<int> PlayErrorEvent;

	public Action UpdateSelectItemEvent;

	private List<string> m_NAudioSupportFormat;

	private List<string> m_UnitySupportFormat;

	private AudioSource m_AudioSource;

	private bool m_SwitchSound;

	private float m_StartTime;

	private float m_BackupBgMusicVolumeValue;

	public static RadioManager Instance
	{
		get
		{
			if (m_Instance == null && Application.isPlaying)
			{
				GameObject gameObject = Resources.Load<GameObject>("Prefab/GameUI/RadioManager");
				if (gameObject != null)
				{
					m_Instance = UnityEngine.Object.Instantiate(gameObject).GetComponent<RadioManager>();
				}
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
			}
			return m_Instance;
		}
	}

	public List<RadioFileInfo> SoundsInfoList { get; private set; }

	public SoundPlayState PlayState { get; private set; }

	public SoundPlayMode PlayMode { get; set; }

	public int CurSoundsIndex { get; private set; }

	public float CurTime => (!(null == m_AudioSource)) ? m_AudioSource.time : 0f;

	public float TotalTime => (!(null == m_AudioSource) && !(null == m_AudioSource.clip)) ? m_AudioSource.clip.length : 0f;

	public float CurVolume => (!(null == m_AudioSource)) ? m_AudioSource.volume : 0f;

	public int CurTimeSamples => (!(null == m_AudioSource)) ? m_AudioSource.timeSamples : 0;

	public int Frequency => (!(null == m_AudioSource) && !(null == m_AudioSource.clip)) ? m_AudioSource.clip.frequency : 0;

	public RadioFileInfo CurSoundInfo { get; private set; }

	public float SwitchTime { get; private set; }

	public bool OpenBgMusic { get; private set; }

	private void Update()
	{
		if (PlayState == SoundPlayState.Playing && null != m_AudioSource && !m_AudioSource.isPlaying)
		{
			PlayState = SoundPlayState.Stop;
			SetTime(0f);
			if (PlayMode != 0)
			{
				m_SwitchSound = true;
				m_StartTime = Time.realtimeSinceStartup;
			}
		}
		if (!m_SwitchSound || !(Time.realtimeSinceStartup - m_StartTime >= SwitchTime))
		{
			return;
		}
		switch (PlayMode)
		{
		case SoundPlayMode.SingleLoop:
			PlaySounds(CurSoundsIndex);
			break;
		case SoundPlayMode.Order:
			if (CurSoundsIndex >= 0 && CurSoundsIndex < SoundsInfoList.Count - 1)
			{
				PlayNextSound();
			}
			break;
		case SoundPlayMode.ListLoop:
			PlayNextSound();
			break;
		case SoundPlayMode.Random:
			PlaySounds(UnityEngine.Random.Range(0, SoundsInfoList.Count));
			break;
		}
		m_SwitchSound = false;
	}

	public void Init()
	{
		PlayState = SoundPlayState.Stop;
		PlayMode = SoundPlayMode.ListLoop;
		m_SwitchSound = false;
		SwitchTime = 1.5f;
		m_StartTime = Time.realtimeSinceStartup;
		m_NAudioSupportFormat = new List<string>();
		m_NAudioSupportFormat.AddRange(Enum.GetNames(typeof(NAudioPlayer.SupportFormatType)));
		if (Application.platform != RuntimePlatform.WindowsPlayer && Application.platform != RuntimePlatform.WindowsEditor)
		{
			m_NAudioSupportFormat.Remove(NAudioPlayer.SupportFormatType.mp3.ToString());
		}
		m_UnitySupportFormat = new List<string>();
		m_UnitySupportFormat.Add("ogg");
		m_UnitySupportFormat.Add("wav");
		SoundsInfoList = new List<RadioFileInfo>();
		RefreshSoundsList();
		if (null == m_AudioSource)
		{
			m_AudioSource = base.gameObject.AddComponent<AudioSource>();
			m_AudioSource.loop = false;
			m_AudioSource.clip = null;
		}
		OpenBgMusic = true;
		m_BackupBgMusicVolumeValue = SystemSettingData.Instance.MusicVolume;
		UIOption instance = UIOption.Instance;
		instance.VolumeChangeEvent = (Action)Delegate.Combine(instance.VolumeChangeEvent, (Action)delegate
		{
			m_BackupBgMusicVolumeValue = SystemSettingData.Instance.MusicVolume;
		});
	}

	public void RefreshSoundsList()
	{
		SoundsInfoList.Clear();
		LoadSoundsByPath(GameConfig.RadioSoundsPath);
		LoadSoundsByPath(GameConfig.OSTSoundsPath);
		if (string.IsNullOrEmpty(CurSoundInfo.FilePath))
		{
			return;
		}
		if (File.Exists(CurSoundInfo.FilePath))
		{
			CurSoundsIndex = SoundsInfoList.FindIndex((RadioFileInfo a) => a.FilePath == CurSoundInfo.FilePath);
		}
		else
		{
			StopCurSound();
			CurSoundsIndex = -1;
		}
	}

	public bool PlaySounds(int index)
	{
		StopCurSound();
		if (index < 0 || index >= SoundsInfoList.Count)
		{
			return false;
		}
		CurSoundInfo = SoundsInfoList[index];
		CurSoundsIndex = index;
		if (UpdateSelectItemEvent != null)
		{
			UpdateSelectItemEvent();
		}
		if (!File.Exists(CurSoundInfo.FilePath))
		{
			return false;
		}
		if (m_NAudioSupportFormat.Contains(CurSoundInfo.ExtensionNotDot))
		{
			NAudioPlayer.SupportFormatType supportFormatType = Convert2NAduioSFT(CurSoundInfo.ExtensionNotDot);
			if (supportFormatType == NAudioPlayer.SupportFormatType.NULL)
			{
				return false;
			}
			StartCoroutine(LoadFileByNAudio(CurSoundInfo.FilePath, supportFormatType));
			return true;
		}
		if (m_UnitySupportFormat.Contains(CurSoundInfo.ExtensionNotDot))
		{
			StartCoroutine(LoadFileByUnity(CurSoundInfo.FilePath));
			return true;
		}
		Debug.Log("Does not support audio formats:" + CurSoundInfo.FilePath);
		return false;
	}

	public bool PlayDefaultSound()
	{
		if (SoundsInfoList != null && SoundsInfoList.Count > 0)
		{
			if (CurSoundsIndex < 0 || CurSoundsIndex >= SoundsInfoList.Count)
			{
				CurSoundsIndex = 0;
			}
			return PlaySounds(CurSoundsIndex);
		}
		return false;
	}

	public bool PlayNextSound()
	{
		if (SoundsInfoList != null && SoundsInfoList.Count > 0)
		{
			CurSoundsIndex++;
			if (CurSoundsIndex < 0 || CurSoundsIndex >= SoundsInfoList.Count)
			{
				CurSoundsIndex = 0;
			}
			return PlaySounds(CurSoundsIndex);
		}
		return false;
	}

	public bool PlayPreviousSounds()
	{
		if (SoundsInfoList != null && SoundsInfoList.Count > 0)
		{
			CurSoundsIndex--;
			if (CurSoundsIndex < 0 || CurSoundsIndex >= SoundsInfoList.Count)
			{
				CurSoundsIndex = SoundsInfoList.Count - 1;
			}
			return PlaySounds(CurSoundsIndex);
		}
		return false;
	}

	public void PauseCurSound()
	{
		if (null != m_AudioSource && null != m_AudioSource.clip && m_AudioSource.isPlaying)
		{
			m_AudioSource.Pause();
			PlayState = SoundPlayState.Pause;
		}
	}

	public void ContinueCurSound()
	{
		if (null != m_AudioSource && null != m_AudioSource.clip)
		{
			if (!m_AudioSource.isPlaying)
			{
				m_AudioSource.Play();
				PlayState = SoundPlayState.Playing;
			}
		}
		else
		{
			PlayDefaultSound();
		}
	}

	public void StopCurSound()
	{
		if (null != m_AudioSource && null != m_AudioSource.clip && m_AudioSource.isPlaying)
		{
			m_AudioSource.Stop();
			SetTime(0f);
			m_AudioSource.clip = null;
			PlayState = SoundPlayState.Stop;
		}
	}

	public void SetTime(float time)
	{
		if (null != m_AudioSource)
		{
			m_AudioSource.time = Mathf.Clamp(time, 0f, TotalTime);
		}
	}

	public void SetVolume(float volmue)
	{
		if (null != m_AudioSource)
		{
			m_AudioSource.volume = Mathf.Clamp01(volmue);
		}
	}

	public bool GetSoundData(float[] dataArray, int offsetSamples)
	{
		if (null == m_AudioSource || null == m_AudioSource.clip || !m_AudioSource.clip.GetData(dataArray, offsetSamples))
		{
			return false;
		}
		return true;
	}

	public void GetSpectrumData(float[] dataArray, int channel, FFTWindow fftWindow)
	{
		if (!(null == m_AudioSource) && !(null == m_AudioSource.clip))
		{
			m_AudioSource.GetSpectrumData(dataArray, channel, fftWindow);
		}
	}

	public void GetOutputData(float[] dataArray, int channel)
	{
		if (!(null == m_AudioSource) && !(null == m_AudioSource.clip))
		{
			m_AudioSource.GetOutputData(dataArray, channel);
		}
	}

	public void SetBgMusicState(bool state)
	{
		SystemSettingData.Instance.MusicVolume = ((!state) ? 0f : m_BackupBgMusicVolumeValue);
		OpenBgMusic = state;
	}

	private void LoadSoundsByPath(string path)
	{
		if (!Directory.Exists(path))
		{
			return;
		}
		string[] files = Directory.GetFiles(path);
		if (files == null || files.Length <= 0)
		{
			return;
		}
		string tempStr;
		for (int i = 0; i < files.Length; i++)
		{
			tempStr = files[i];
			if (m_NAudioSupportFormat.Any((string a) => tempStr.ToLower().EndsWith("." + a.ToLower())) || m_UnitySupportFormat.Any((string b) => tempStr.ToLower().EndsWith("." + b.ToLower())))
			{
				SoundsInfoList.Add(new RadioFileInfo(tempStr));
			}
		}
	}

	private IEnumerator LoadFileByNAudio(string filePath, NAudioPlayer.SupportFormatType type)
	{
		if (type == NAudioPlayer.SupportFormatType.NULL)
		{
			yield break;
		}
		WWW www = new WWW("file://" + filePath);
		yield return www;
		if (www == null || www.bytes == null || www.bytes.Length <= 0)
		{
			yield break;
		}
		AudioClip clip = NAudioPlayer.GetClipByType(www.bytes, type);
		if (null != clip)
		{
			while (clip.loadState == AudioDataLoadState.Loading)
			{
				yield return null;
			}
			if (clip.loadState == AudioDataLoadState.Loaded)
			{
				clip.name = Path.GetFileNameWithoutExtension(filePath);
			}
		}
		if (!PlaySounds(clip))
		{
			MarkPlayErrorItem(filePath);
		}
	}

	private IEnumerator LoadFileByUnity(string filePath)
	{
		WWW www = new WWW("file://" + filePath);
		yield return www;
		if (www == null)
		{
			yield break;
		}
		AudioClip clip = www.audioClip;
		if (null != clip)
		{
			while (clip.loadState == AudioDataLoadState.Loading)
			{
				yield return null;
			}
			if (clip.loadState == AudioDataLoadState.Loaded)
			{
				clip.name = Path.GetFileNameWithoutExtension(filePath);
			}
		}
		if (!PlaySounds(clip))
		{
			MarkPlayErrorItem(filePath);
		}
	}

	private void MarkPlayErrorItem(string path)
	{
		if (SoundsInfoList == null || path == null)
		{
			return;
		}
		for (int i = 0; i < SoundsInfoList.Count; i++)
		{
			if (string.Equals(SoundsInfoList[i].FilePath, path))
			{
				SoundsInfoList[i].SetPlayError(isPlayError: true);
				if (PlayErrorEvent != null)
				{
					PlayErrorEvent(i);
				}
				break;
			}
		}
	}

	private NAudioPlayer.SupportFormatType Convert2NAduioSFT(string extension)
	{
		return extension switch
		{
			"mp3" => NAudioPlayer.SupportFormatType.mp3, 
			"flac" => NAudioPlayer.SupportFormatType.flac, 
			_ => NAudioPlayer.SupportFormatType.NULL, 
		};
	}

	private bool PlaySounds(AudioClip clip)
	{
		if (null != m_AudioSource)
		{
			if (null != clip)
			{
				if (null != m_AudioSource.clip)
				{
					m_AudioSource.Stop();
				}
				m_AudioSource.clip = clip;
				m_AudioSource.Play();
				PlayState = SoundPlayState.Playing;
				return true;
			}
			PeTipMsg.Register($"{CurSoundInfo.Name}:{PELocalization.GetString(9500106)}", PeTipMsg.EMsgLevel.Error);
		}
		m_SwitchSound = true;
		return false;
	}
}
