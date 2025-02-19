using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FMOD.Studio;

public class VCA : HandleBase
{
	public VCA(IntPtr raw)
		: base(raw)
	{
	}

	public RESULT getID(out Guid id)
	{
		return FMOD_Studio_VCA_GetID(rawPtr, out id);
	}

	public RESULT getPath(out string path)
	{
		path = null;
		byte[] array = new byte[256];
		int retrieved = 0;
		RESULT rESULT = FMOD_Studio_VCA_GetPath(rawPtr, array, array.Length, out retrieved);
		if (rESULT == RESULT.ERR_TRUNCATED)
		{
			array = new byte[retrieved];
			rESULT = FMOD_Studio_VCA_GetPath(rawPtr, array, array.Length, out retrieved);
		}
		if (rESULT == RESULT.OK)
		{
			path = Encoding.UTF8.GetString(array, 0, retrieved - 1);
		}
		return rESULT;
	}

	public RESULT getFaderLevel(out float volume)
	{
		return FMOD_Studio_VCA_GetFaderLevel(rawPtr, out volume);
	}

	public RESULT setFaderLevel(float volume)
	{
		return FMOD_Studio_VCA_SetFaderLevel(rawPtr, volume);
	}

	[DllImport("fmodstudio")]
	private static extern bool FMOD_Studio_VCA_IsValid(IntPtr vca);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_VCA_GetID(IntPtr vca, out Guid id);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_VCA_GetPath(IntPtr vca, [Out] byte[] path, int size, out int retrieved);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_VCA_GetFaderLevel(IntPtr vca, out float value);

	[DllImport("fmodstudio")]
	private static extern RESULT FMOD_Studio_VCA_SetFaderLevel(IntPtr vca, float value);

	protected override bool isValidInternal()
	{
		return FMOD_Studio_VCA_IsValid(rawPtr);
	}
}
