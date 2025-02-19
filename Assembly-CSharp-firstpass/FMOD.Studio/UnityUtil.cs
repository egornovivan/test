using UnityEngine;

namespace FMOD.Studio;

public static class UnityUtil
{
	public static VECTOR toFMODVector(this Vector3 vec)
	{
		VECTOR result = default(VECTOR);
		result.x = vec.x;
		result.y = vec.y;
		result.z = vec.z;
		return result;
	}

	public static ATTRIBUTES_3D to3DAttributes(this Vector3 pos)
	{
		ATTRIBUTES_3D result = default(ATTRIBUTES_3D);
		result.forward = Vector3.forward.toFMODVector();
		result.up = Vector3.up.toFMODVector();
		result.position = pos.toFMODVector();
		return result;
	}

	public static ATTRIBUTES_3D to3DAttributes(GameObject go, Rigidbody rigidbody = null)
	{
		ATTRIBUTES_3D result = default(ATTRIBUTES_3D);
		result.forward = go.transform.forward.toFMODVector();
		result.up = go.transform.up.toFMODVector();
		result.position = go.transform.position.toFMODVector();
		if ((bool)rigidbody)
		{
			result.velocity = rigidbody.velocity.toFMODVector();
		}
		return result;
	}

	public static void Log(string msg)
	{
	}

	public static void LogWarning(string msg)
	{
		UnityEngine.Debug.LogWarning(msg);
	}

	public static void LogError(string msg)
	{
		UnityEngine.Debug.LogError(msg);
	}

	public static bool ForceLoadLowLevelBinary()
	{
		Log("Attempting to call Memory_GetStats");
		if (!ERRCHECK(Memory.GetStats(out var _, out var _)))
		{
			LogError("Memory_GetStats returned an error");
			return false;
		}
		Log("Calling Memory_GetStats succeeded!");
		return true;
	}

	public static bool ERRCHECK(RESULT result)
	{
		if (result != 0)
		{
			LogWarning("FMOD Error (" + result.ToString() + "): " + Error.String(result));
		}
		return result == RESULT.OK;
	}
}
