using System;
using System.Collections.Generic;
using FMOD;
using FMOD.Studio;
using UnityEngine;

public class FMOD_StudioSystem : MonoBehaviour
{
	private FMOD.Studio.System system;

	private Dictionary<string, EventDescription> eventDescriptions = new Dictionary<string, EventDescription>();

	private bool isInitialized;

	private static FMOD_StudioSystem sInstance;

	public static FMOD_StudioSystem instance => sInstance;

	public FMOD.Studio.System System => system;

	public static void CreateMono(GameObject go)
	{
		if (sInstance == null)
		{
			sInstance = go.AddComponent<FMOD_StudioSystem>();
			if (!UnityUtil.ForceLoadLowLevelBinary())
			{
				UnityUtil.LogError("Unable to load low level binary!");
			}
			else
			{
				sInstance.Init();
			}
		}
	}

	public EventInstance GetEvent(FMODAsset asset)
	{
		return GetEvent(asset.id);
	}

	public EventInstance GetEvent(string path)
	{
		EventInstance eventInstance = null;
		if (string.IsNullOrEmpty(path))
		{
			UnityUtil.LogError("Empty event path!");
			return null;
		}
		if (eventDescriptions.ContainsKey(path) && eventDescriptions[path].isValid())
		{
			ERRCHECK(eventDescriptions[path].createInstance(out eventInstance));
		}
		else
		{
			Guid guid = default(Guid);
			if (path.StartsWith("{"))
			{
				ERRCHECK(FMOD.Studio.Util.ParseID(path, out guid));
			}
			else if (path.StartsWith("event:"))
			{
				ERRCHECK(system.lookupID(path, out guid));
			}
			else
			{
				UnityUtil.LogError("Expected event path to start with 'event:/'");
			}
			EventDescription _event = null;
			ERRCHECK(system.getEventByID(guid, out _event));
			if (_event != null && _event.isValid())
			{
				eventDescriptions[path] = _event;
				ERRCHECK(_event.createInstance(out eventInstance));
			}
		}
		if (eventInstance == null)
		{
			UnityUtil.Log("GetEvent FAILED: \"" + path + "\"");
		}
		return eventInstance;
	}

	public void PlayOneShot(FMODAsset asset, Vector3 position)
	{
		PlayOneShot(asset.id, position);
	}

	public void PlayOneShot(string path, Vector3 position)
	{
		PlayOneShot(path, position, 1f);
	}

	public void PlayOneShot(string path, Vector3 position, float volume)
	{
		EventInstance @event = GetEvent(path);
		if (@event == null)
		{
			UnityUtil.LogWarning("PlayOneShot couldn't find event: \"" + path + "\"");
			return;
		}
		FMOD.Studio.ATTRIBUTES_3D attributes = position.to3DAttributes();
		ERRCHECK(@event.set3DAttributes(attributes));
		ERRCHECK(@event.setVolume(volume));
		ERRCHECK(@event.start());
		ERRCHECK(@event.release());
	}

	private void Init()
	{
		UnityUtil.Log("FMOD_StudioSystem: Initialize");
		if (!isInitialized)
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			UnityUtil.Log("FMOD_StudioSystem: System_Create");
			ERRCHECK(FMOD.Studio.System.create(out this.system));
			FMOD.Studio.INITFLAGS iNITFLAGS = FMOD.Studio.INITFLAGS.NORMAL;
			iNITFLAGS |= FMOD.Studio.INITFLAGS.LIVEUPDATE;
			if (Application.unityVersion.StartsWith("5"))
			{
				UnityUtil.LogWarning("FMOD_StudioSystem: detected Unity 5, running on port 9265");
				ERRCHECK(this.system.getLowLevelSystem(out var system));
				FMOD.ADVANCEDSETTINGS settings = default(FMOD.ADVANCEDSETTINGS);
				settings.profilePort = 9265;
				ERRCHECK(system.setAdvancedSettings(ref settings));
			}
			UnityUtil.Log("FMOD_StudioSystem: system.init");
			UnityUtil.LogWarning("FMOD init flag = " + iNITFLAGS);
			RESULT rESULT = RESULT.OK;
			rESULT = this.system.initialize(1024, iNITFLAGS, FMOD.INITFLAGS.NORMAL, IntPtr.Zero);
			if (rESULT == RESULT.ERR_HEADER_MISMATCH)
			{
				UnityUtil.LogError("Version mismatch between C# script and FMOD binary, restart Unity and reimport the integration package to resolve this issue.");
			}
			else
			{
				ERRCHECK(rESULT);
			}
			ERRCHECK(this.system.flushCommands());
			rESULT = this.system.update();
			if (rESULT == RESULT.ERR_NET_SOCKET_ERROR)
			{
				UnityUtil.LogWarning("LiveUpdate disabled: socket in already in use");
				iNITFLAGS = (FMOD.Studio.INITFLAGS)((uint)iNITFLAGS & 0xFFFFFFFEu);
				ERRCHECK(this.system.release());
				ERRCHECK(FMOD.Studio.System.create(out this.system));
				ERRCHECK(this.system.getLowLevelSystem(out var _));
				rESULT = this.system.initialize(1024, iNITFLAGS, FMOD.INITFLAGS.NORMAL, IntPtr.Zero);
				ERRCHECK(rESULT);
			}
			isInitialized = true;
		}
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		if (this.system != null && this.system.isValid())
		{
			UnityUtil.Log("Pause state changed to: " + pauseStatus);
			ERRCHECK(this.system.getLowLevelSystem(out var system));
			if (system == null)
			{
				UnityUtil.LogError("Tried to suspend mixer, but no low level system found");
			}
			else if (pauseStatus)
			{
				ERRCHECK(system.mixerSuspend());
			}
			else
			{
				ERRCHECK(system.mixerResume());
			}
		}
	}

	private void Update()
	{
		if (isInitialized)
		{
			ERRCHECK(system.update());
		}
	}

	private void OnDisable()
	{
		if (isInitialized)
		{
			UnityUtil.Log("__ SHUT DOWN FMOD SYSTEM __");
			ERRCHECK(system.release());
			if (this == sInstance)
			{
				sInstance = null;
			}
		}
	}

	private static bool ERRCHECK(RESULT result)
	{
		return UnityUtil.ERRCHECK(result);
	}
}
