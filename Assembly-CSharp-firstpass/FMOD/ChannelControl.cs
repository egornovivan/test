using System;
using System.Runtime.InteropServices;

namespace FMOD;

public class ChannelControl : HandleBase
{
	protected ChannelControl(IntPtr raw)
		: base(raw)
	{
	}

	public RESULT getSystemObject(out System system)
	{
		system = null;
		IntPtr system2;
		RESULT result = FMOD5_ChannelGroup_GetSystemObject(rawPtr, out system2);
		system = new System(system2);
		return result;
	}

	public RESULT stop()
	{
		return FMOD5_ChannelGroup_Stop(rawPtr);
	}

	public RESULT setPaused(bool paused)
	{
		return FMOD5_ChannelGroup_SetPaused(rawPtr, paused);
	}

	public RESULT getPaused(out bool paused)
	{
		return FMOD5_ChannelGroup_GetPaused(rawPtr, out paused);
	}

	public RESULT setVolume(float volume)
	{
		return FMOD5_ChannelGroup_SetVolume(rawPtr, volume);
	}

	public RESULT getVolume(out float volume)
	{
		return FMOD5_ChannelGroup_GetVolume(rawPtr, out volume);
	}

	public RESULT setVolumeRamp(bool ramp)
	{
		return FMOD5_ChannelGroup_SetVolumeRamp(rawPtr, ramp);
	}

	public RESULT getVolumeRamp(out bool ramp)
	{
		return FMOD5_ChannelGroup_GetVolumeRamp(rawPtr, out ramp);
	}

	public RESULT getAudibility(out float audibility)
	{
		return FMOD5_ChannelGroup_GetAudibility(rawPtr, out audibility);
	}

	public RESULT setPitch(float pitch)
	{
		return FMOD5_ChannelGroup_SetPitch(rawPtr, pitch);
	}

	public RESULT getPitch(out float pitch)
	{
		return FMOD5_ChannelGroup_GetPitch(rawPtr, out pitch);
	}

	public RESULT setMute(bool mute)
	{
		return FMOD5_ChannelGroup_SetMute(rawPtr, mute);
	}

	public RESULT getMute(out bool mute)
	{
		return FMOD5_ChannelGroup_GetMute(rawPtr, out mute);
	}

	public RESULT setReverbProperties(int instance, float wet)
	{
		return FMOD5_ChannelGroup_SetReverbProperties(rawPtr, instance, wet);
	}

	public RESULT getReverbProperties(int instance, out float wet)
	{
		return FMOD5_ChannelGroup_GetReverbProperties(rawPtr, instance, out wet);
	}

	public RESULT setLowPassGain(float gain)
	{
		return FMOD5_ChannelGroup_SetLowPassGain(rawPtr, gain);
	}

	public RESULT getLowPassGain(out float gain)
	{
		return FMOD5_ChannelGroup_GetLowPassGain(rawPtr, out gain);
	}

	public RESULT setMode(MODE mode)
	{
		return FMOD5_ChannelGroup_SetMode(rawPtr, mode);
	}

	public RESULT getMode(out MODE mode)
	{
		return FMOD5_ChannelGroup_GetMode(rawPtr, out mode);
	}

	public RESULT setCallback(CHANNEL_CALLBACK callback)
	{
		return FMOD5_ChannelGroup_SetCallback(rawPtr, callback);
	}

	public RESULT isPlaying(out bool isplaying)
	{
		return FMOD5_ChannelGroup_IsPlaying(rawPtr, out isplaying);
	}

	public RESULT setPan(float pan)
	{
		return FMOD5_ChannelGroup_SetPan(rawPtr, pan);
	}

	public RESULT setMixLevelsOutput(float frontleft, float frontright, float center, float lfe, float surroundleft, float surroundright, float backleft, float backright)
	{
		return FMOD5_ChannelGroup_SetMixLevelsOutput(rawPtr, frontleft, frontright, center, lfe, surroundleft, surroundright, backleft, backright);
	}

	public RESULT setMixLevelsInput(float[] levels, int numlevels)
	{
		return FMOD5_ChannelGroup_SetMixLevelsInput(rawPtr, levels, numlevels);
	}

	public RESULT setMixMatrix(float[] matrix, int outchannels, int inchannels, int inchannel_hop)
	{
		return FMOD5_ChannelGroup_SetMixMatrix(rawPtr, matrix, outchannels, inchannels, inchannel_hop);
	}

	public RESULT getMixMatrix(float[] matrix, out int outchannels, out int inchannels, int inchannel_hop)
	{
		return FMOD5_ChannelGroup_GetMixMatrix(rawPtr, matrix, out outchannels, out inchannels, inchannel_hop);
	}

	public RESULT getDSPClock(out ulong dspclock, out ulong parentclock)
	{
		return FMOD5_ChannelGroup_GetDSPClock(rawPtr, out dspclock, out parentclock);
	}

	public RESULT setDelay(ulong dspclock_start, ulong dspclock_end, bool stopchannels)
	{
		return FMOD5_ChannelGroup_SetDelay(rawPtr, dspclock_start, dspclock_end, stopchannels);
	}

	public RESULT getDelay(out ulong dspclock_start, out ulong dspclock_end, out bool stopchannels)
	{
		return FMOD5_ChannelGroup_GetDelay(rawPtr, out dspclock_start, out dspclock_end, out stopchannels);
	}

	public RESULT addFadePoint(ulong dspclock, float volume)
	{
		return FMOD5_ChannelGroup_AddFadePoint(rawPtr, dspclock, volume);
	}

	public RESULT setFadePointRamp(ulong dspclock, float volume)
	{
		return FMOD5_ChannelGroup_SetFadePointRamp(rawPtr, dspclock, volume);
	}

	public RESULT removeFadePoints(ulong dspclock_start, ulong dspclock_end)
	{
		return FMOD5_ChannelGroup_RemoveFadePoints(rawPtr, dspclock_start, dspclock_end);
	}

	public RESULT getFadePoints(ref uint numpoints, ulong[] point_dspclock, float[] point_volume)
	{
		return FMOD5_ChannelGroup_GetFadePoints(rawPtr, ref numpoints, point_dspclock, point_volume);
	}

	public RESULT getDSP(int index, out DSP dsp)
	{
		dsp = null;
		IntPtr dsp2;
		RESULT result = FMOD5_ChannelGroup_GetDSP(rawPtr, index, out dsp2);
		dsp = new DSP(dsp2);
		return result;
	}

	public RESULT addDSP(int index, DSP dsp)
	{
		return FMOD5_ChannelGroup_AddDSP(rawPtr, index, dsp.getRaw());
	}

	public RESULT removeDSP(DSP dsp)
	{
		return FMOD5_ChannelGroup_RemoveDSP(rawPtr, dsp.getRaw());
	}

	public RESULT getNumDSPs(out int numdsps)
	{
		return FMOD5_ChannelGroup_GetNumDSPs(rawPtr, out numdsps);
	}

	public RESULT setDSPIndex(DSP dsp, int index)
	{
		return FMOD5_ChannelGroup_SetDSPIndex(rawPtr, dsp.getRaw(), index);
	}

	public RESULT getDSPIndex(DSP dsp, out int index)
	{
		return FMOD5_ChannelGroup_GetDSPIndex(rawPtr, dsp.getRaw(), out index);
	}

	public RESULT overridePanDSP(DSP pan)
	{
		return FMOD5_ChannelGroup_OverridePanDSP(rawPtr, pan.getRaw());
	}

	public RESULT set3DAttributes(ref VECTOR pos, ref VECTOR vel, ref VECTOR alt_pan_pos)
	{
		return FMOD5_ChannelGroup_Set3DAttributes(rawPtr, ref pos, ref vel, ref alt_pan_pos);
	}

	public RESULT get3DAttributes(out VECTOR pos, out VECTOR vel, out VECTOR alt_pan_pos)
	{
		return FMOD5_ChannelGroup_Get3DAttributes(rawPtr, out pos, out vel, out alt_pan_pos);
	}

	public RESULT set3DMinMaxDistance(float mindistance, float maxdistance)
	{
		return FMOD5_ChannelGroup_Set3DMinMaxDistance(rawPtr, mindistance, maxdistance);
	}

	public RESULT get3DMinMaxDistance(out float mindistance, out float maxdistance)
	{
		return FMOD5_ChannelGroup_Get3DMinMaxDistance(rawPtr, out mindistance, out maxdistance);
	}

	public RESULT set3DConeSettings(float insideconeangle, float outsideconeangle, float outsidevolume)
	{
		return FMOD5_ChannelGroup_Set3DConeSettings(rawPtr, insideconeangle, outsideconeangle, outsidevolume);
	}

	public RESULT get3DConeSettings(out float insideconeangle, out float outsideconeangle, out float outsidevolume)
	{
		return FMOD5_ChannelGroup_Get3DConeSettings(rawPtr, out insideconeangle, out outsideconeangle, out outsidevolume);
	}

	public RESULT set3DConeOrientation(ref VECTOR orientation)
	{
		return FMOD5_ChannelGroup_Set3DConeOrientation(rawPtr, ref orientation);
	}

	public RESULT get3DConeOrientation(out VECTOR orientation)
	{
		return FMOD5_ChannelGroup_Get3DConeOrientation(rawPtr, out orientation);
	}

	public RESULT set3DCustomRolloff(ref VECTOR points, int numpoints)
	{
		return FMOD5_ChannelGroup_Set3DCustomRolloff(rawPtr, ref points, numpoints);
	}

	public RESULT get3DCustomRolloff(out IntPtr points, out int numpoints)
	{
		return FMOD5_ChannelGroup_Get3DCustomRolloff(rawPtr, out points, out numpoints);
	}

	public RESULT set3DOcclusion(float directocclusion, float reverbocclusion)
	{
		return FMOD5_ChannelGroup_Set3DOcclusion(rawPtr, directocclusion, reverbocclusion);
	}

	public RESULT get3DOcclusion(out float directocclusion, out float reverbocclusion)
	{
		return FMOD5_ChannelGroup_Get3DOcclusion(rawPtr, out directocclusion, out reverbocclusion);
	}

	public RESULT set3DSpread(float angle)
	{
		return FMOD5_ChannelGroup_Set3DSpread(rawPtr, angle);
	}

	public RESULT get3DSpread(out float angle)
	{
		return FMOD5_ChannelGroup_Get3DSpread(rawPtr, out angle);
	}

	public RESULT set3DLevel(float level)
	{
		return FMOD5_ChannelGroup_Set3DLevel(rawPtr, level);
	}

	public RESULT get3DLevel(out float level)
	{
		return FMOD5_ChannelGroup_Get3DLevel(rawPtr, out level);
	}

	public RESULT set3DDopplerLevel(float level)
	{
		return FMOD5_ChannelGroup_Set3DDopplerLevel(rawPtr, level);
	}

	public RESULT get3DDopplerLevel(out float level)
	{
		return FMOD5_ChannelGroup_Get3DDopplerLevel(rawPtr, out level);
	}

	public RESULT set3DDistanceFilter(bool custom, float customLevel, float centerFreq)
	{
		return FMOD5_ChannelGroup_Set3DDistanceFilter(rawPtr, custom, customLevel, centerFreq);
	}

	public RESULT get3DDistanceFilter(out bool custom, out float customLevel, out float centerFreq)
	{
		return FMOD5_ChannelGroup_Get3DDistanceFilter(rawPtr, out custom, out customLevel, out centerFreq);
	}

	public RESULT setUserData(IntPtr userdata)
	{
		return FMOD5_ChannelGroup_SetUserData(rawPtr, userdata);
	}

	public RESULT getUserData(out IntPtr userdata)
	{
		return FMOD5_ChannelGroup_GetUserData(rawPtr, out userdata);
	}

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_Stop(IntPtr channelgroup);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_SetPaused(IntPtr channelgroup, bool paused);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_GetPaused(IntPtr channelgroup, out bool paused);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_GetVolume(IntPtr channelgroup, out float volume);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_SetVolumeRamp(IntPtr channelgroup, bool ramp);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_GetVolumeRamp(IntPtr channelgroup, out bool ramp);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_GetAudibility(IntPtr channelgroup, out float audibility);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_SetPitch(IntPtr channelgroup, float pitch);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_GetPitch(IntPtr channelgroup, out float pitch);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_SetMute(IntPtr channelgroup, bool mute);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_GetMute(IntPtr channelgroup, out bool mute);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_SetReverbProperties(IntPtr channelgroup, int instance, float wet);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_GetReverbProperties(IntPtr channelgroup, int instance, out float wet);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_SetLowPassGain(IntPtr channelgroup, float gain);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_GetLowPassGain(IntPtr channelgroup, out float gain);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_SetMode(IntPtr channelgroup, MODE mode);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_GetMode(IntPtr channelgroup, out MODE mode);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_SetCallback(IntPtr channelgroup, CHANNEL_CALLBACK callback);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_IsPlaying(IntPtr channelgroup, out bool isplaying);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_SetPan(IntPtr channelgroup, float pan);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_SetMixLevelsOutput(IntPtr channelgroup, float frontleft, float frontright, float center, float lfe, float surroundleft, float surroundright, float backleft, float backright);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_SetMixLevelsInput(IntPtr channelgroup, float[] levels, int numlevels);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_SetMixMatrix(IntPtr channelgroup, float[] matrix, int outchannels, int inchannels, int inchannel_hop);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_GetMixMatrix(IntPtr channelgroup, float[] matrix, out int outchannels, out int inchannels, int inchannel_hop);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_GetDSPClock(IntPtr channelgroup, out ulong dspclock, out ulong parentclock);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_SetDelay(IntPtr channelgroup, ulong dspclock_start, ulong dspclock_end, bool stopchannels);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_GetDelay(IntPtr channelgroup, out ulong dspclock_start, out ulong dspclock_end, out bool stopchannels);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_AddFadePoint(IntPtr channelgroup, ulong dspclock, float volume);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_SetFadePointRamp(IntPtr channelgroup, ulong dspclock, float volume);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_RemoveFadePoints(IntPtr channelgroup, ulong dspclock_start, ulong dspclock_end);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_GetFadePoints(IntPtr channelgroup, ref uint numpoints, ulong[] point_dspclock, float[] point_volume);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_Set3DAttributes(IntPtr channelgroup, ref VECTOR pos, ref VECTOR vel, ref VECTOR alt_pan_pos);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_Get3DAttributes(IntPtr channelgroup, out VECTOR pos, out VECTOR vel, out VECTOR alt_pan_pos);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_Set3DMinMaxDistance(IntPtr channelgroup, float mindistance, float maxdistance);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_Get3DMinMaxDistance(IntPtr channelgroup, out float mindistance, out float maxdistance);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_Set3DConeSettings(IntPtr channelgroup, float insideconeangle, float outsideconeangle, float outsidevolume);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_Get3DConeSettings(IntPtr channelgroup, out float insideconeangle, out float outsideconeangle, out float outsidevolume);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_Set3DConeOrientation(IntPtr channelgroup, ref VECTOR orientation);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_Get3DConeOrientation(IntPtr channelgroup, out VECTOR orientation);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_Set3DCustomRolloff(IntPtr channelgroup, ref VECTOR points, int numpoints);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_Get3DCustomRolloff(IntPtr channelgroup, out IntPtr points, out int numpoints);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_Set3DOcclusion(IntPtr channelgroup, float directocclusion, float reverbocclusion);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_Get3DOcclusion(IntPtr channelgroup, out float directocclusion, out float reverbocclusion);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_Set3DSpread(IntPtr channelgroup, float angle);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_Get3DSpread(IntPtr channelgroup, out float angle);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_Set3DLevel(IntPtr channelgroup, float level);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_Get3DLevel(IntPtr channelgroup, out float level);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_Set3DDopplerLevel(IntPtr channelgroup, float level);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_Get3DDopplerLevel(IntPtr channelgroup, out float level);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_Set3DDistanceFilter(IntPtr channelgroup, bool custom, float customLevel, float centerFreq);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_Get3DDistanceFilter(IntPtr channelgroup, out bool custom, out float customLevel, out float centerFreq);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_GetSystemObject(IntPtr channelgroup, out IntPtr system);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_SetVolume(IntPtr channelgroup, float volume);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_GetDSP(IntPtr channelgroup, int index, out IntPtr dsp);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_AddDSP(IntPtr channelgroup, int index, IntPtr dsp);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_RemoveDSP(IntPtr channelgroup, IntPtr dsp);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_GetNumDSPs(IntPtr channelgroup, out int numdsps);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_SetDSPIndex(IntPtr channelgroup, IntPtr dsp, int index);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_GetDSPIndex(IntPtr channelgroup, IntPtr dsp, out int index);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_OverridePanDSP(IntPtr channelgroup, IntPtr pan);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_SetUserData(IntPtr channelgroup, IntPtr userdata);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_ChannelGroup_GetUserData(IntPtr channelgroup, out IntPtr userdata);
}
