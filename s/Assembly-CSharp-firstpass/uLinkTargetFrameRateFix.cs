using System;
using System.Runtime.InteropServices;
using UnityEngine;

[AddComponentMenu("")]
[ExecuteInEditMode]
public class uLinkTargetFrameRateFix : MonoBehaviour
{
	private const float BONUS_TICKS = 2.5f;

	private const uint QS_ALLEVENTS = 1215u;

	private static uint prevTicksSinceStartup;

	private static int prevSleepTicks;

	private static int targetDeltaTicks;

	private static uLinkTargetFrameRateFix singleton;

	[DllImport("User32.dll")]
	private static extern uint MsgWaitForMultipleObjectsEx(uint nCount, IntPtr[] pHandles, uint dwMilliseconds, uint dwWakeMask, uint dwFlags);

	[DllImport("Winmm.dll")]
	private static extern uint timeGetTime();

	private void Awake()
	{
		if (singleton != null)
		{
			UnityEngine.Object.DestroyImmediate(this);
			return;
		}
		singleton = this;
		UnityEngine.Object.DontDestroyOnLoad(this);
		prevTicksSinceStartup = timeGetTime();
	}

	private void LateUpdate()
	{
		if (Application.isPlaying)
		{
			uint num = timeGetTime();
			int num2 = (int)(num - prevTicksSinceStartup);
			prevTicksSinceStartup = num;
			int num3 = num2 - prevSleepTicks;
			int num4 = targetDeltaTicks - num3;
			if (num4 < 0 && prevSleepTicks < 0)
			{
				num4 = 0;
			}
			prevSleepTicks = num4;
			if (num4 > 0)
			{
				MsgWaitForMultipleObjectsEx(0u, null, (uint)num4, 1215u, 0u);
			}
		}
		else if (Application.isEditor)
		{
			base.gameObject.hideFlags = HideFlags.None;
			UnityEngine.Object.DestroyImmediate(base.gameObject);
		}
	}

	public static void SetTargetFrameRate(int frameRate)
	{
		if (QualitySettings.vSyncCount != 0)
		{
			return;
		}
		Application.targetFrameRate = -1;
		if (Application.platform != RuntimePlatform.WindowsPlayer)
		{
			Application.targetFrameRate = frameRate;
			return;
		}
		try
		{
			timeGetTime();
			MsgWaitForMultipleObjectsEx(0u, null, 0u, 0u, 0u);
		}
		catch (Exception)
		{
			Application.targetFrameRate = frameRate;
			return;
		}
		if (frameRate == 0 || frameRate == -1)
		{
			if (singleton != null)
			{
				UnityEngine.Object.DestroyImmediate(singleton);
				singleton = null;
			}
			targetDeltaTicks = 0;
			return;
		}
		targetDeltaTicks = Mathf.RoundToInt(1000f / (float)frameRate + 2.5f);
		if (!(singleton != null))
		{
			GameObject gameObject = new GameObject("uLinkTargetFrameRateFix", typeof(uLinkTargetFrameRateFix));
			gameObject.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
		}
	}
}
