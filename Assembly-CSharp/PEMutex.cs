using System;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

public class PEMutex : MonoBehaviour
{
	private static Mutex mutex;

	private static string s_MutexStr = "Planet Explorers Instance";

	public static bool Errored;

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(this);
	}

	private void Start()
	{
		mutex = null;
	}

	private void Update()
	{
		if (Time.frameCount != 100)
		{
			return;
		}
		try
		{
			mutex = new Mutex(initiallyOwned: true, s_MutexStr);
			if (mutex != null)
			{
				UnityEngine.Debug.Log("MUTEX Created.");
			}
			else
			{
				UnityEngine.Debug.LogWarning("Unable to create MUTEX");
			}
		}
		catch (Exception)
		{
			UnityEngine.Debug.LogWarning("Unable to create MUTEX");
		}
	}

	private void OnDestroy()
	{
		try
		{
			if (mutex != null)
			{
				mutex.Close();
				mutex = null;
				UnityEngine.Debug.Log("MUTEX Deleted.");
			}
		}
		catch (Exception)
		{
			UnityEngine.Debug.LogWarning("Unable to close MUTEX");
		}
		if (!Application.isEditor)
		{
			Process.GetCurrentProcess().Kill();
		}
	}
}
