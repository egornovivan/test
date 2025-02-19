using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FMOD.Studio;

public class EventInstance : HandleBase
{
	public EventInstance(IntPtr raw)
		: base(raw)
	{
	}

	public RESULT getDescription(out EventDescription description)
	{
		description = null;
		IntPtr description2;
		RESULT rESULT = FMOD_Studio_EventInstance_GetDescription(rawPtr, out description2);
		if (rESULT != 0)
		{
			return rESULT;
		}
		description = new EventDescription(description2);
		return rESULT;
	}

	public RESULT getVolume(out float volume)
	{
		return FMOD_Studio_EventInstance_GetVolume(rawPtr, out volume);
	}

	public RESULT setVolume(float volume)
	{
		return FMOD_Studio_EventInstance_SetVolume(rawPtr, volume);
	}

	public RESULT getPitch(out float pitch)
	{
		return FMOD_Studio_EventInstance_GetPitch(rawPtr, out pitch);
	}

	public RESULT setPitch(float pitch)
	{
		return FMOD_Studio_EventInstance_SetPitch(rawPtr, pitch);
	}

	public RESULT get3DAttributes(out ATTRIBUTES_3D attributes)
	{
		return FMOD_Studio_EventInstance_Get3DAttributes(rawPtr, out attributes);
	}

	public RESULT set3DAttributes(ATTRIBUTES_3D attributes)
	{
		return FMOD_Studio_EventInstance_Set3DAttributes(rawPtr, ref attributes);
	}

	public RESULT getProperty(EVENT_PROPERTY index, out float value)
	{
		return FMOD_Studio_EventInstance_GetProperty(rawPtr, index, out value);
	}

	public RESULT setProperty(EVENT_PROPERTY index, float value)
	{
		return FMOD_Studio_EventInstance_SetProperty(rawPtr, index, value);
	}

	public RESULT getPaused(out bool paused)
	{
		return FMOD_Studio_EventInstance_GetPaused(rawPtr, out paused);
	}

	public RESULT setPaused(bool paused)
	{
		return FMOD_Studio_EventInstance_SetPaused(rawPtr, paused);
	}

	public RESULT start()
	{
		return FMOD_Studio_EventInstance_Start(rawPtr);
	}

	public RESULT stop(STOP_MODE mode)
	{
		return FMOD_Studio_EventInstance_Stop(rawPtr, mode);
	}

	public RESULT getTimelinePosition(out int position)
	{
		return FMOD_Studio_EventInstance_GetTimelinePosition(rawPtr, out position);
	}

	public RESULT setTimelinePosition(int position)
	{
		return FMOD_Studio_EventInstance_SetTimelinePosition(rawPtr, position);
	}

	public RESULT getPlaybackState(out PLAYBACK_STATE state)
	{
		return FMOD_Studio_EventInstance_GetPlaybackState(rawPtr, out state);
	}

	public RESULT getChannelGroup(out ChannelGroup group)
	{
		group = null;
		IntPtr group2 = default(IntPtr);
		RESULT rESULT = FMOD_Studio_EventInstance_GetChannelGroup(rawPtr, out group2);
		if (rESULT != 0)
		{
			return rESULT;
		}
		group = new ChannelGroup(group2);
		return rESULT;
	}

	public RESULT release()
	{
		return FMOD_Studio_EventInstance_Release(rawPtr);
	}

	public RESULT isVirtual(out bool virtualState)
	{
		return FMOD_Studio_EventInstance_IsVirtual(rawPtr, out virtualState);
	}

	public RESULT getParameter(string name, out ParameterInstance instance)
	{
		instance = null;
		IntPtr parameter = default(IntPtr);
		RESULT rESULT = FMOD_Studio_EventInstance_GetParameter(rawPtr, Encoding.UTF8.GetBytes(name + '\0'), out parameter);
		if (rESULT != 0)
		{
			return rESULT;
		}
		instance = new ParameterInstance(parameter);
		return rESULT;
	}

	public RESULT getParameterCount(out int count)
	{
		return FMOD_Studio_EventInstance_GetParameterCount(rawPtr, out count);
	}

	public RESULT getParameterByIndex(int index, out ParameterInstance instance)
	{
		instance = null;
		IntPtr parameter = default(IntPtr);
		RESULT rESULT = FMOD_Studio_EventInstance_GetParameterByIndex(rawPtr, index, out parameter);
		if (rESULT != 0)
		{
			return rESULT;
		}
		instance = new ParameterInstance(parameter);
		return rESULT;
	}

	public RESULT setParameterValue(string name, float value)
	{
		return FMOD_Studio_EventInstance_SetParameterValue(rawPtr, Encoding.UTF8.GetBytes(name + '\0'), value);
	}

	public RESULT setParameterValueByIndex(int index, float value)
	{
		return FMOD_Studio_EventInstance_SetParameterValueByIndex(rawPtr, index, value);
	}

	public RESULT getCue(string name, out CueInstance instance)
	{
		instance = null;
		IntPtr cue = default(IntPtr);
		RESULT rESULT = FMOD_Studio_EventInstance_GetCue(rawPtr, Encoding.UTF8.GetBytes(name + '\0'), out cue);
		if (rESULT != 0)
		{
			return rESULT;
		}
		instance = new CueInstance(cue);
		return rESULT;
	}

	public RESULT getCueByIndex(int index, out CueInstance instance)
	{
		instance = null;
		IntPtr cue = default(IntPtr);
		RESULT rESULT = FMOD_Studio_EventInstance_GetCueByIndex(rawPtr, index, out cue);
		if (rESULT != 0)
		{
			return rESULT;
		}
		instance = new CueInstance(cue);
		return rESULT;
	}

	public RESULT getCueCount(out int count)
	{
		return FMOD_Studio_EventInstance_GetCueCount(rawPtr, out count);
	}

	public RESULT setCallback(EVENT_CALLBACK callback, EVENT_CALLBACK_TYPE callbackmask = EVENT_CALLBACK_TYPE.ALL)
	{
		return FMOD_Studio_EventInstance_SetCallback(rawPtr, callback, callbackmask);
	}

	public RESULT getUserData(out IntPtr userData)
	{
		return FMOD_Studio_EventInstance_GetUserData(rawPtr, out userData);
	}

	public RESULT setUserData(IntPtr userData)
	{
		return FMOD_Studio_EventInstance_SetUserData(rawPtr, userData);
	}

	[DllImport("fmodstudio")]
	private static extern bool FMOD_Studio_EventInstance_IsValid(IntPtr _event);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_GetDescription(IntPtr _event, out IntPtr description);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_GetVolume(IntPtr _event, out float volume);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_SetVolume(IntPtr _event, float volume);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_GetPitch(IntPtr _event, out float pitch);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_SetPitch(IntPtr _event, float pitch);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_Get3DAttributes(IntPtr _event, out ATTRIBUTES_3D attributes);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_Set3DAttributes(IntPtr _event, ref ATTRIBUTES_3D attributes);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_GetProperty(IntPtr _event, EVENT_PROPERTY index, out float value);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_SetProperty(IntPtr _event, EVENT_PROPERTY index, float value);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_GetPaused(IntPtr _event, out bool paused);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_SetPaused(IntPtr _event, bool paused);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_Start(IntPtr _event);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_Stop(IntPtr _event, STOP_MODE mode);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_GetTimelinePosition(IntPtr _event, out int position);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_SetTimelinePosition(IntPtr _event, int position);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_GetPlaybackState(IntPtr _event, out PLAYBACK_STATE state);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_GetChannelGroup(IntPtr _event, out IntPtr group);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_Release(IntPtr _event);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_IsVirtual(IntPtr _event, out bool virtualState);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_GetParameter(IntPtr _event, byte[] name, out IntPtr parameter);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_GetParameterByIndex(IntPtr _event, int index, out IntPtr parameter);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_GetParameterCount(IntPtr _event, out int count);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_SetParameterValue(IntPtr _event, byte[] name, float value);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_SetParameterValueByIndex(IntPtr _event, int index, float value);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_GetCue(IntPtr _event, byte[] name, out IntPtr cue);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_GetCueByIndex(IntPtr _event, int index, out IntPtr cue);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_GetCueCount(IntPtr _event, out int count);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_SetCallback(IntPtr _event, EVENT_CALLBACK callback, EVENT_CALLBACK_TYPE callbackmask);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_GetUserData(IntPtr _event, out IntPtr userData);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_EventInstance_SetUserData(IntPtr _event, IntPtr userData);

	protected override bool isValidInternal()
	{
		return FMOD_Studio_EventInstance_IsValid(rawPtr);
	}
}
