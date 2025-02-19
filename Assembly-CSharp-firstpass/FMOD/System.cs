using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FMOD;

public class System : HandleBase
{
	public System(IntPtr raw)
		: base(raw)
	{
	}

	public RESULT release()
	{
		RESULT rESULT = FMOD5_System_Release(rawPtr);
		if (rESULT == RESULT.OK)
		{
			rawPtr = IntPtr.Zero;
		}
		return rESULT;
	}

	public RESULT setOutput(OUTPUTTYPE output)
	{
		return FMOD5_System_SetOutput(rawPtr, output);
	}

	public RESULT getOutput(out OUTPUTTYPE output)
	{
		return FMOD5_System_GetOutput(rawPtr, out output);
	}

	public RESULT getNumDrivers(out int numdrivers)
	{
		return FMOD5_System_GetNumDrivers(rawPtr, out numdrivers);
	}

	public RESULT getDriverInfo(int id, StringBuilder name, int namelen, out Guid guid, out int systemrate, out SPEAKERMODE speakermode, out int speakermodechannels)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(name.Capacity);
		RESULT result = FMOD5_System_GetDriverInfo(rawPtr, id, intPtr, namelen, out guid, out systemrate, out speakermode, out speakermodechannels);
		StringMarshalHelper.NativeToBuilder(name, intPtr);
		Marshal.FreeHGlobal(intPtr);
		return result;
	}

	public RESULT setDriver(int driver)
	{
		return FMOD5_System_SetDriver(rawPtr, driver);
	}

	public RESULT getDriver(out int driver)
	{
		return FMOD5_System_GetDriver(rawPtr, out driver);
	}

	public RESULT setSoftwareChannels(int numsoftwarechannels)
	{
		return FMOD5_System_SetSoftwareChannels(rawPtr, numsoftwarechannels);
	}

	public RESULT getSoftwareChannels(out int numsoftwarechannels)
	{
		return FMOD5_System_GetSoftwareChannels(rawPtr, out numsoftwarechannels);
	}

	public RESULT setSoftwareFormat(int samplerate, SPEAKERMODE speakermode, int numrawspeakers)
	{
		return FMOD5_System_SetSoftwareFormat(rawPtr, samplerate, speakermode, numrawspeakers);
	}

	public RESULT getSoftwareFormat(out int samplerate, out SPEAKERMODE speakermode, out int numrawspeakers)
	{
		return FMOD5_System_GetSoftwareFormat(rawPtr, out samplerate, out speakermode, out numrawspeakers);
	}

	public RESULT setDSPBufferSize(uint bufferlength, int numbuffers)
	{
		return FMOD5_System_SetDSPBufferSize(rawPtr, bufferlength, numbuffers);
	}

	public RESULT getDSPBufferSize(out uint bufferlength, out int numbuffers)
	{
		return FMOD5_System_GetDSPBufferSize(rawPtr, out bufferlength, out numbuffers);
	}

	public RESULT setFileSystem(FILE_OPENCALLBACK useropen, FILE_CLOSECALLBACK userclose, FILE_READCALLBACK userread, FILE_SEEKCALLBACK userseek, FILE_ASYNCREADCALLBACK userasyncread, FILE_ASYNCCANCELCALLBACK userasynccancel, int blockalign)
	{
		return FMOD5_System_SetFileSystem(rawPtr, useropen, userclose, userread, userseek, userasyncread, userasynccancel, blockalign);
	}

	public RESULT attachFileSystem(FILE_OPENCALLBACK useropen, FILE_CLOSECALLBACK userclose, FILE_READCALLBACK userread, FILE_SEEKCALLBACK userseek)
	{
		return FMOD5_System_AttachFileSystem(rawPtr, useropen, userclose, userread, userseek);
	}

	public RESULT setAdvancedSettings(ref ADVANCEDSETTINGS settings)
	{
		settings.cbSize = Marshal.SizeOf(settings);
		return FMOD5_System_SetAdvancedSettings(rawPtr, ref settings);
	}

	public RESULT getAdvancedSettings(ref ADVANCEDSETTINGS settings)
	{
		settings.cbSize = Marshal.SizeOf(settings);
		return FMOD5_System_GetAdvancedSettings(rawPtr, ref settings);
	}

	public RESULT setCallback(SYSTEM_CALLBACK callback, SYSTEM_CALLBACK_TYPE callbackmask)
	{
		return FMOD5_System_SetCallback(rawPtr, callback, callbackmask);
	}

	public RESULT setPluginPath(string path)
	{
		return FMOD5_System_SetPluginPath(rawPtr, Encoding.UTF8.GetBytes(path + '\0'));
	}

	public RESULT loadPlugin(string filename, out uint handle, uint priority)
	{
		return FMOD5_System_LoadPlugin(rawPtr, Encoding.UTF8.GetBytes(filename + '\0'), out handle, priority);
	}

	public RESULT loadPlugin(string filename, out uint handle)
	{
		return loadPlugin(filename, out handle, 0u);
	}

	public RESULT unloadPlugin(uint handle)
	{
		return FMOD5_System_UnloadPlugin(rawPtr, handle);
	}

	public RESULT getNumPlugins(PLUGINTYPE plugintype, out int numplugins)
	{
		return FMOD5_System_GetNumPlugins(rawPtr, plugintype, out numplugins);
	}

	public RESULT getPluginHandle(PLUGINTYPE plugintype, int index, out uint handle)
	{
		return FMOD5_System_GetPluginHandle(rawPtr, plugintype, index, out handle);
	}

	public RESULT getPluginInfo(uint handle, out PLUGINTYPE plugintype, StringBuilder name, int namelen, out uint version)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(name.Capacity);
		RESULT result = FMOD5_System_GetPluginInfo(rawPtr, handle, out plugintype, intPtr, namelen, out version);
		StringMarshalHelper.NativeToBuilder(name, intPtr);
		Marshal.FreeHGlobal(intPtr);
		return result;
	}

	public RESULT setOutputByPlugin(uint handle)
	{
		return FMOD5_System_SetOutputByPlugin(rawPtr, handle);
	}

	public RESULT getOutputByPlugin(out uint handle)
	{
		return FMOD5_System_GetOutputByPlugin(rawPtr, out handle);
	}

	public RESULT createDSPByPlugin(uint handle, out DSP dsp)
	{
		dsp = null;
		IntPtr dsp2;
		RESULT result = FMOD5_System_CreateDSPByPlugin(rawPtr, handle, out dsp2);
		dsp = new DSP(dsp2);
		return result;
	}

	public RESULT getDSPInfoByPlugin(uint handle, out IntPtr description)
	{
		return FMOD5_System_GetDSPInfoByPlugin(rawPtr, handle, out description);
	}

	public RESULT registerDSP(ref DSP_DESCRIPTION description, out uint handle)
	{
		return FMOD5_System_RegisterDSP(rawPtr, ref description, out handle);
	}

	public RESULT init(int maxchannels, INITFLAGS flags, IntPtr extradriverdata)
	{
		return FMOD5_System_Init(rawPtr, maxchannels, flags, extradriverdata);
	}

	public RESULT close()
	{
		return FMOD5_System_Close(rawPtr);
	}

	public RESULT update()
	{
		return FMOD5_System_Update(rawPtr);
	}

	public RESULT setSpeakerPosition(SPEAKER speaker, float x, float y, bool active)
	{
		return FMOD5_System_SetSpeakerPosition(rawPtr, speaker, x, y, active);
	}

	public RESULT getSpeakerPosition(SPEAKER speaker, out float x, out float y, out bool active)
	{
		return FMOD5_System_GetSpeakerPosition(rawPtr, speaker, out x, out y, out active);
	}

	public RESULT setStreamBufferSize(uint filebuffersize, TIMEUNIT filebuffersizetype)
	{
		return FMOD5_System_SetStreamBufferSize(rawPtr, filebuffersize, filebuffersizetype);
	}

	public RESULT getStreamBufferSize(out uint filebuffersize, out TIMEUNIT filebuffersizetype)
	{
		return FMOD5_System_GetStreamBufferSize(rawPtr, out filebuffersize, out filebuffersizetype);
	}

	public RESULT set3DSettings(float dopplerscale, float distancefactor, float rolloffscale)
	{
		return FMOD5_System_Set3DSettings(rawPtr, dopplerscale, distancefactor, rolloffscale);
	}

	public RESULT get3DSettings(out float dopplerscale, out float distancefactor, out float rolloffscale)
	{
		return FMOD5_System_Get3DSettings(rawPtr, out dopplerscale, out distancefactor, out rolloffscale);
	}

	public RESULT set3DNumListeners(int numlisteners)
	{
		return FMOD5_System_Set3DNumListeners(rawPtr, numlisteners);
	}

	public RESULT get3DNumListeners(out int numlisteners)
	{
		return FMOD5_System_Get3DNumListeners(rawPtr, out numlisteners);
	}

	public RESULT set3DListenerAttributes(int listener, ref VECTOR pos, ref VECTOR vel, ref VECTOR forward, ref VECTOR up)
	{
		return FMOD5_System_Set3DListenerAttributes(rawPtr, listener, ref pos, ref vel, ref forward, ref up);
	}

	public RESULT get3DListenerAttributes(int listener, out VECTOR pos, out VECTOR vel, out VECTOR forward, out VECTOR up)
	{
		return FMOD5_System_Get3DListenerAttributes(rawPtr, listener, out pos, out vel, out forward, out up);
	}

	public RESULT set3DRolloffCallback(CB_3D_ROLLOFFCALLBACK callback)
	{
		return FMOD5_System_Set3DRolloffCallback(rawPtr, callback);
	}

	public RESULT mixerSuspend()
	{
		return FMOD5_System_MixerSuspend(rawPtr);
	}

	public RESULT mixerResume()
	{
		return FMOD5_System_MixerResume(rawPtr);
	}

	public RESULT getDefaultMixMatrix(SPEAKERMODE sourcespeakermode, SPEAKERMODE targetspeakermode, float[] matrix, int matrixhop)
	{
		return FMOD5_System_GetDefaultMixMatrix(rawPtr, sourcespeakermode, targetspeakermode, matrix, matrixhop);
	}

	public RESULT getSpeakerModeChannels(SPEAKERMODE mode, out int channels)
	{
		return FMOD5_System_GetSpeakerModeChannels(rawPtr, mode, out channels);
	}

	public RESULT getVersion(out uint version)
	{
		return FMOD5_System_GetVersion(rawPtr, out version);
	}

	public RESULT getOutputHandle(out IntPtr handle)
	{
		return FMOD5_System_GetOutputHandle(rawPtr, out handle);
	}

	public RESULT getChannelsPlaying(out int channels)
	{
		return FMOD5_System_GetChannelsPlaying(rawPtr, out channels);
	}

	public RESULT getCPUUsage(out float dsp, out float stream, out float geometry, out float update, out float total)
	{
		return FMOD5_System_GetCPUUsage(rawPtr, out dsp, out stream, out geometry, out update, out total);
	}

	public RESULT getSoundRAM(out int currentalloced, out int maxalloced, out int total)
	{
		return FMOD5_System_GetSoundRAM(rawPtr, out currentalloced, out maxalloced, out total);
	}

	public RESULT createSound(string name, MODE mode, ref CREATESOUNDEXINFO exinfo, out Sound sound)
	{
		sound = null;
		byte[] bytes = Encoding.UTF8.GetBytes(name + '\0');
		exinfo.cbsize = Marshal.SizeOf(exinfo);
		IntPtr sound2;
		RESULT result = FMOD5_System_CreateSound(rawPtr, bytes, mode, ref exinfo, out sound2);
		sound = new Sound(sound2);
		return result;
	}

	public RESULT createSound(byte[] data, MODE mode, ref CREATESOUNDEXINFO exinfo, out Sound sound)
	{
		sound = null;
		exinfo.cbsize = Marshal.SizeOf(exinfo);
		IntPtr sound2;
		RESULT result = FMOD5_System_CreateSound(rawPtr, data, mode, ref exinfo, out sound2);
		sound = new Sound(sound2);
		return result;
	}

	public RESULT createSound(string name, MODE mode, out Sound sound)
	{
		CREATESOUNDEXINFO exinfo = default(CREATESOUNDEXINFO);
		exinfo.cbsize = Marshal.SizeOf(exinfo);
		return createSound(name, mode, ref exinfo, out sound);
	}

	public RESULT createStream(string name, MODE mode, ref CREATESOUNDEXINFO exinfo, out Sound sound)
	{
		sound = null;
		byte[] bytes = Encoding.UTF8.GetBytes(name + '\0');
		exinfo.cbsize = Marshal.SizeOf(exinfo);
		IntPtr sound2;
		RESULT result = FMOD5_System_CreateStream(rawPtr, bytes, mode, ref exinfo, out sound2);
		sound = new Sound(sound2);
		return result;
	}

	public RESULT createStream(byte[] data, MODE mode, ref CREATESOUNDEXINFO exinfo, out Sound sound)
	{
		sound = null;
		exinfo.cbsize = Marshal.SizeOf(exinfo);
		IntPtr sound2;
		RESULT result = FMOD5_System_CreateStream(rawPtr, data, mode, ref exinfo, out sound2);
		sound = new Sound(sound2);
		return result;
	}

	public RESULT createStream(string name, MODE mode, out Sound sound)
	{
		CREATESOUNDEXINFO exinfo = default(CREATESOUNDEXINFO);
		exinfo.cbsize = Marshal.SizeOf(exinfo);
		return createStream(name, mode, ref exinfo, out sound);
	}

	public RESULT createDSP(ref DSP_DESCRIPTION description, out DSP dsp)
	{
		dsp = null;
		IntPtr dsp2;
		RESULT result = FMOD5_System_CreateDSP(rawPtr, ref description, out dsp2);
		dsp = new DSP(dsp2);
		return result;
	}

	public RESULT createDSPByType(DSP_TYPE type, out DSP dsp)
	{
		dsp = null;
		IntPtr dsp2;
		RESULT result = FMOD5_System_CreateDSPByType(rawPtr, type, out dsp2);
		dsp = new DSP(dsp2);
		return result;
	}

	public RESULT createChannelGroup(string name, out ChannelGroup channelgroup)
	{
		channelgroup = null;
		byte[] bytes = Encoding.UTF8.GetBytes(name + '\0');
		IntPtr channelgroup2;
		RESULT result = FMOD5_System_CreateChannelGroup(rawPtr, bytes, out channelgroup2);
		channelgroup = new ChannelGroup(channelgroup2);
		return result;
	}

	public RESULT createSoundGroup(string name, out SoundGroup soundgroup)
	{
		soundgroup = null;
		byte[] bytes = Encoding.UTF8.GetBytes(name + '\0');
		IntPtr soundgroup2;
		RESULT result = FMOD5_System_CreateSoundGroup(rawPtr, bytes, out soundgroup2);
		soundgroup = new SoundGroup(soundgroup2);
		return result;
	}

	public RESULT createReverb3D(out Reverb3D reverb)
	{
		IntPtr reverb2;
		RESULT result = FMOD5_System_CreateReverb3D(rawPtr, out reverb2);
		reverb = new Reverb3D(reverb2);
		return result;
	}

	public RESULT playSound(Sound sound, ChannelGroup channelGroup, bool paused, out Channel channel)
	{
		channel = null;
		IntPtr channelGroup2 = ((!(channelGroup != null)) ? IntPtr.Zero : channelGroup.getRaw());
		IntPtr channel2;
		RESULT result = FMOD5_System_PlaySound(rawPtr, sound.getRaw(), channelGroup2, paused, out channel2);
		channel = new Channel(channel2);
		return result;
	}

	public RESULT playDSP(DSP dsp, ChannelGroup channelGroup, bool paused, out Channel channel)
	{
		channel = null;
		IntPtr channelGroup2 = ((!(channelGroup != null)) ? IntPtr.Zero : channelGroup.getRaw());
		IntPtr channel2;
		RESULT result = FMOD5_System_PlayDSP(rawPtr, dsp.getRaw(), channelGroup2, paused, out channel2);
		channel = new Channel(channel2);
		return result;
	}

	public RESULT getChannel(int channelid, out Channel channel)
	{
		channel = null;
		IntPtr channel2;
		RESULT result = FMOD5_System_GetChannel(rawPtr, channelid, out channel2);
		channel = new Channel(channel2);
		return result;
	}

	public RESULT getMasterChannelGroup(out ChannelGroup channelgroup)
	{
		channelgroup = null;
		IntPtr channelgroup2;
		RESULT result = FMOD5_System_GetMasterChannelGroup(rawPtr, out channelgroup2);
		channelgroup = new ChannelGroup(channelgroup2);
		return result;
	}

	public RESULT getMasterSoundGroup(out SoundGroup soundgroup)
	{
		soundgroup = null;
		IntPtr soundgroup2;
		RESULT result = FMOD5_System_GetMasterSoundGroup(rawPtr, out soundgroup2);
		soundgroup = new SoundGroup(soundgroup2);
		return result;
	}

	public RESULT attachChannelGroupToPort(uint portType, ulong portIndex, ChannelGroup channelgroup, bool passThru = false)
	{
		return FMOD5_System_AttachChannelGroupToPort(rawPtr, portType, portIndex, channelgroup.getRaw(), passThru);
	}

	public RESULT detachChannelGroupFromPort(ChannelGroup channelgroup)
	{
		return FMOD5_System_DetachChannelGroupFromPort(rawPtr, channelgroup.getRaw());
	}

	public RESULT setReverbProperties(int instance, ref REVERB_PROPERTIES prop)
	{
		return FMOD5_System_SetReverbProperties(rawPtr, instance, ref prop);
	}

	public RESULT getReverbProperties(int instance, out REVERB_PROPERTIES prop)
	{
		return FMOD5_System_GetReverbProperties(rawPtr, instance, out prop);
	}

	public RESULT lockDSP()
	{
		return FMOD5_System_LockDSP(rawPtr);
	}

	public RESULT unlockDSP()
	{
		return FMOD5_System_UnlockDSP(rawPtr);
	}

	public RESULT getRecordNumDrivers(out int numdrivers, out int numconnected)
	{
		return FMOD5_System_GetRecordNumDrivers(rawPtr, out numdrivers, out numconnected);
	}

	public RESULT getRecordDriverInfo(int id, StringBuilder name, int namelen, out Guid guid, out int systemrate, out SPEAKERMODE speakermode, out int speakermodechannels, out DRIVER_STATE state)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(name.Capacity);
		RESULT result = FMOD5_System_GetRecordDriverInfo(rawPtr, id, intPtr, namelen, out guid, out systemrate, out speakermode, out speakermodechannels, out state);
		StringMarshalHelper.NativeToBuilder(name, intPtr);
		Marshal.FreeHGlobal(intPtr);
		return result;
	}

	public RESULT getRecordPosition(int id, out uint position)
	{
		return FMOD5_System_GetRecordPosition(rawPtr, id, out position);
	}

	public RESULT recordStart(int id, Sound sound, bool loop)
	{
		return FMOD5_System_RecordStart(rawPtr, id, sound.getRaw(), loop);
	}

	public RESULT recordStop(int id)
	{
		return FMOD5_System_RecordStop(rawPtr, id);
	}

	public RESULT isRecording(int id, out bool recording)
	{
		return FMOD5_System_IsRecording(rawPtr, id, out recording);
	}

	public RESULT createGeometry(int maxpolygons, int maxvertices, out Geometry geometry)
	{
		geometry = null;
		IntPtr geometry2;
		RESULT result = FMOD5_System_CreateGeometry(rawPtr, maxpolygons, maxvertices, out geometry2);
		geometry = new Geometry(geometry2);
		return result;
	}

	public RESULT setGeometrySettings(float maxworldsize)
	{
		return FMOD5_System_SetGeometrySettings(rawPtr, maxworldsize);
	}

	public RESULT getGeometrySettings(out float maxworldsize)
	{
		return FMOD5_System_GetGeometrySettings(rawPtr, out maxworldsize);
	}

	public RESULT loadGeometry(IntPtr data, int datasize, out Geometry geometry)
	{
		geometry = null;
		IntPtr geometry2;
		RESULT result = FMOD5_System_LoadGeometry(rawPtr, data, datasize, out geometry2);
		geometry = new Geometry(geometry2);
		return result;
	}

	public RESULT getGeometryOcclusion(ref VECTOR listener, ref VECTOR source, out float direct, out float reverb)
	{
		return FMOD5_System_GetGeometryOcclusion(rawPtr, ref listener, ref source, out direct, out reverb);
	}

	public RESULT setNetworkProxy(string proxy)
	{
		return FMOD5_System_SetNetworkProxy(rawPtr, Encoding.UTF8.GetBytes(proxy + '\0'));
	}

	public RESULT getNetworkProxy(StringBuilder proxy, int proxylen)
	{
		IntPtr intPtr = Marshal.AllocHGlobal(proxy.Capacity);
		RESULT result = FMOD5_System_GetNetworkProxy(rawPtr, intPtr, proxylen);
		StringMarshalHelper.NativeToBuilder(proxy, intPtr);
		Marshal.FreeHGlobal(intPtr);
		return result;
	}

	public RESULT setNetworkTimeout(int timeout)
	{
		return FMOD5_System_SetNetworkTimeout(rawPtr, timeout);
	}

	public RESULT getNetworkTimeout(out int timeout)
	{
		return FMOD5_System_GetNetworkTimeout(rawPtr, out timeout);
	}

	public RESULT setUserData(IntPtr userdata)
	{
		return FMOD5_System_SetUserData(rawPtr, userdata);
	}

	public RESULT getUserData(out IntPtr userdata)
	{
		return FMOD5_System_GetUserData(rawPtr, out userdata);
	}

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_Release(IntPtr system);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_SetOutput(IntPtr system, OUTPUTTYPE output);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetOutput(IntPtr system, out OUTPUTTYPE output);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetNumDrivers(IntPtr system, out int numdrivers);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetDriverInfo(IntPtr system, int id, IntPtr name, int namelen, out Guid guid, out int systemrate, out SPEAKERMODE speakermode, out int speakermodechannels);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_SetDriver(IntPtr system, int driver);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetDriver(IntPtr system, out int driver);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_SetSoftwareChannels(IntPtr system, int numsoftwarechannels);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetSoftwareChannels(IntPtr system, out int numsoftwarechannels);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_SetSoftwareFormat(IntPtr system, int samplerate, SPEAKERMODE speakermode, int numrawspeakers);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetSoftwareFormat(IntPtr system, out int samplerate, out SPEAKERMODE speakermode, out int numrawspeakers);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_SetDSPBufferSize(IntPtr system, uint bufferlength, int numbuffers);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetDSPBufferSize(IntPtr system, out uint bufferlength, out int numbuffers);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_SetFileSystem(IntPtr system, FILE_OPENCALLBACK useropen, FILE_CLOSECALLBACK userclose, FILE_READCALLBACK userread, FILE_SEEKCALLBACK userseek, FILE_ASYNCREADCALLBACK userasyncread, FILE_ASYNCCANCELCALLBACK userasynccancel, int blockalign);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_AttachFileSystem(IntPtr system, FILE_OPENCALLBACK useropen, FILE_CLOSECALLBACK userclose, FILE_READCALLBACK userread, FILE_SEEKCALLBACK userseek);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_SetPluginPath(IntPtr system, byte[] path);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_LoadPlugin(IntPtr system, byte[] filename, out uint handle, uint priority);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_UnloadPlugin(IntPtr system, uint handle);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetNumPlugins(IntPtr system, PLUGINTYPE plugintype, out int numplugins);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetPluginHandle(IntPtr system, PLUGINTYPE plugintype, int index, out uint handle);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetPluginInfo(IntPtr system, uint handle, out PLUGINTYPE plugintype, IntPtr name, int namelen, out uint version);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_CreateDSPByPlugin(IntPtr system, uint handle, out IntPtr dsp);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_SetOutputByPlugin(IntPtr system, uint handle);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetOutputByPlugin(IntPtr system, out uint handle);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetDSPInfoByPlugin(IntPtr system, uint handle, out IntPtr description);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_RegisterDSP(IntPtr system, ref DSP_DESCRIPTION description, out uint handle);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_Init(IntPtr system, int maxchannels, INITFLAGS flags, IntPtr extradriverdata);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_Close(IntPtr system);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_Update(IntPtr system);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_SetAdvancedSettings(IntPtr system, ref ADVANCEDSETTINGS settings);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetAdvancedSettings(IntPtr system, ref ADVANCEDSETTINGS settings);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_Set3DRolloffCallback(IntPtr system, CB_3D_ROLLOFFCALLBACK callback);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_MixerSuspend(IntPtr system);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_MixerResume(IntPtr system);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetDefaultMixMatrix(IntPtr system, SPEAKERMODE sourcespeakermode, SPEAKERMODE targetspeakermode, float[] matrix, int matrixhop);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetSpeakerModeChannels(IntPtr system, SPEAKERMODE mode, out int channels);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_SetCallback(IntPtr system, SYSTEM_CALLBACK callback, SYSTEM_CALLBACK_TYPE callbackmask);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_SetSpeakerPosition(IntPtr system, SPEAKER speaker, float x, float y, bool active);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetSpeakerPosition(IntPtr system, SPEAKER speaker, out float x, out float y, out bool active);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_Set3DSettings(IntPtr system, float dopplerscale, float distancefactor, float rolloffscale);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_Get3DSettings(IntPtr system, out float dopplerscale, out float distancefactor, out float rolloffscale);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_Set3DNumListeners(IntPtr system, int numlisteners);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_Get3DNumListeners(IntPtr system, out int numlisteners);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_Set3DListenerAttributes(IntPtr system, int listener, ref VECTOR pos, ref VECTOR vel, ref VECTOR forward, ref VECTOR up);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_Get3DListenerAttributes(IntPtr system, int listener, out VECTOR pos, out VECTOR vel, out VECTOR forward, out VECTOR up);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_SetStreamBufferSize(IntPtr system, uint filebuffersize, TIMEUNIT filebuffersizetype);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetStreamBufferSize(IntPtr system, out uint filebuffersize, out TIMEUNIT filebuffersizetype);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetVersion(IntPtr system, out uint version);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetOutputHandle(IntPtr system, out IntPtr handle);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetChannelsPlaying(IntPtr system, out int channels);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetCPUUsage(IntPtr system, out float dsp, out float stream, out float geometry, out float update, out float total);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetSoundRAM(IntPtr system, out int currentalloced, out int maxalloced, out int total);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_CreateSound(IntPtr system, byte[] name_or_data, MODE mode, ref CREATESOUNDEXINFO exinfo, out IntPtr sound);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_CreateStream(IntPtr system, byte[] name_or_data, MODE mode, ref CREATESOUNDEXINFO exinfo, out IntPtr sound);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_CreateDSP(IntPtr system, ref DSP_DESCRIPTION description, out IntPtr dsp);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_CreateDSPByType(IntPtr system, DSP_TYPE type, out IntPtr dsp);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_CreateChannelGroup(IntPtr system, byte[] name, out IntPtr channelgroup);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_CreateSoundGroup(IntPtr system, byte[] name, out IntPtr soundgroup);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_CreateReverb3D(IntPtr system, out IntPtr reverb);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_PlaySound(IntPtr system, IntPtr sound, IntPtr channelGroup, bool paused, out IntPtr channel);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_PlayDSP(IntPtr system, IntPtr dsp, IntPtr channelGroup, bool paused, out IntPtr channel);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetChannel(IntPtr system, int channelid, out IntPtr channel);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetMasterChannelGroup(IntPtr system, out IntPtr channelgroup);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetMasterSoundGroup(IntPtr system, out IntPtr soundgroup);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_AttachChannelGroupToPort(IntPtr system, uint portType, ulong portIndex, IntPtr channelgroup, bool passThru);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_DetachChannelGroupFromPort(IntPtr system, IntPtr channelgroup);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_SetReverbProperties(IntPtr system, int instance, ref REVERB_PROPERTIES prop);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetReverbProperties(IntPtr system, int instance, out REVERB_PROPERTIES prop);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_LockDSP(IntPtr system);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_UnlockDSP(IntPtr system);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetRecordNumDrivers(IntPtr system, out int numdrivers, out int numconnected);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetRecordDriverInfo(IntPtr system, int id, IntPtr name, int namelen, out Guid guid, out int systemrate, out SPEAKERMODE speakermode, out int speakermodechannels, out DRIVER_STATE state);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetRecordPosition(IntPtr system, int id, out uint position);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_RecordStart(IntPtr system, int id, IntPtr sound, bool loop);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_RecordStop(IntPtr system, int id);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_IsRecording(IntPtr system, int id, out bool recording);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_CreateGeometry(IntPtr system, int maxpolygons, int maxvertices, out IntPtr geometry);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_SetGeometrySettings(IntPtr system, float maxworldsize);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetGeometrySettings(IntPtr system, out float maxworldsize);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_LoadGeometry(IntPtr system, IntPtr data, int datasize, out IntPtr geometry);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetGeometryOcclusion(IntPtr system, ref VECTOR listener, ref VECTOR source, out float direct, out float reverb);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_SetNetworkProxy(IntPtr system, byte[] proxy);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetNetworkProxy(IntPtr system, IntPtr proxy, int proxylen);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_SetNetworkTimeout(IntPtr system, int timeout);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetNetworkTimeout(IntPtr system, out int timeout);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_SetUserData(IntPtr system, IntPtr userdata);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_System_GetUserData(IntPtr system, out IntPtr userdata);
}
