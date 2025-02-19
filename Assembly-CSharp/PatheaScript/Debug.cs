using UnityEngine;

namespace PatheaScript;

public static class Debug
{
	public static void Log(object msg)
	{
		UnityEngine.Debug.Log(msg);
	}

	public static void LogWarning(object msg)
	{
		UnityEngine.Debug.LogWarning(msg);
	}

	public static void LogError(object msg)
	{
		UnityEngine.Debug.LogError(msg);
	}
}
