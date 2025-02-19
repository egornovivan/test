using System;
using System.Runtime.InteropServices;

namespace FMOD;

public class DSPConnection : HandleBase
{
	public DSPConnection(IntPtr raw)
		: base(raw)
	{
	}

	public RESULT getInput(out DSP input)
	{
		input = null;
		IntPtr input2;
		RESULT result = FMOD5_DSPConnection_GetInput(rawPtr, out input2);
		input = new DSP(input2);
		return result;
	}

	public RESULT getOutput(out DSP output)
	{
		output = null;
		IntPtr output2;
		RESULT result = FMOD5_DSPConnection_GetOutput(rawPtr, out output2);
		output = new DSP(output2);
		return result;
	}

	public RESULT setMix(float volume)
	{
		return FMOD5_DSPConnection_SetMix(rawPtr, volume);
	}

	public RESULT getMix(out float volume)
	{
		return FMOD5_DSPConnection_GetMix(rawPtr, out volume);
	}

	public RESULT setMixMatrix(float[] matrix, int outchannels, int inchannels, int inchannel_hop)
	{
		return FMOD5_DSPConnection_SetMixMatrix(rawPtr, matrix, outchannels, inchannels, inchannel_hop);
	}

	public RESULT getMixMatrix(float[] matrix, out int outchannels, out int inchannels, int inchannel_hop)
	{
		return FMOD5_DSPConnection_GetMixMatrix(rawPtr, matrix, out outchannels, out inchannels, inchannel_hop);
	}

	public RESULT getType(out DSPCONNECTION_TYPE type)
	{
		return FMOD5_DSPConnection_GetType(rawPtr, out type);
	}

	public RESULT setUserData(IntPtr userdata)
	{
		return FMOD5_DSPConnection_SetUserData(rawPtr, userdata);
	}

	public RESULT getUserData(out IntPtr userdata)
	{
		return FMOD5_DSPConnection_GetUserData(rawPtr, out userdata);
	}

	[DllImport("fmod")]
	private static extern RESULT FMOD5_DSPConnection_GetInput(IntPtr dspconnection, out IntPtr input);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_DSPConnection_GetOutput(IntPtr dspconnection, out IntPtr output);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_DSPConnection_SetMix(IntPtr dspconnection, float volume);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_DSPConnection_GetMix(IntPtr dspconnection, out float volume);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_DSPConnection_SetMixMatrix(IntPtr dspconnection, float[] matrix, int outchannels, int inchannels, int inchannel_hop);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_DSPConnection_GetMixMatrix(IntPtr dspconnection, float[] matrix, out int outchannels, out int inchannels, int inchannel_hop);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_DSPConnection_GetType(IntPtr dspconnection, out DSPCONNECTION_TYPE type);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_DSPConnection_SetUserData(IntPtr dspconnection, IntPtr userdata);

	[DllImport("fmod")]
	private static extern RESULT FMOD5_DSPConnection_GetUserData(IntPtr dspconnection, out IntPtr userdata);
}
