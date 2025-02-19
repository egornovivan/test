using System;
using System.Collections;
using System.Diagnostics;
using Pathea;
using UnityEngine;

public class StartGameCtrl : MonoBehaviour
{
	[HideInInspector]
	public static bool IsStarted;

	private string[] _preloadAssets = new string[4] { "Prefab/GlobalAssets/_Env_", "Prefab/GlobalAssets/Voxel Creation System", "Prefab/GlobalAssets/GameClient", "Prefab/GlobalAssets/FMOD Audio System" };

	private void Awake()
	{
		IsStarted = false;
		PECommandLine.ParseArgs();
	}

	private void Start()
	{
		StartCoroutine(ApplyResolution());
		ApplyOclPara();
		if (!IsStarted)
		{
			UILoadScenceEffect.Instance.PalyLogoTexture(LoadGlobalAssets, LoadGameMenuSence);
		}
	}

	private IEnumerator ApplyResolution()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		Screen.SetResolution(PECommandLine.W, PECommandLine.H, PECommandLine.FullScreen);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		UnityEngine.Debug.Log("Game resolution in use[" + Screen.width + "X" + Screen.height + "] fs[" + Screen.fullScreen + "]");
	}

	private void ApplyOclPara()
	{
		if (!string.IsNullOrEmpty(PECommandLine.OclPara))
		{
			oclManager.CurOclOpt = PECommandLine.OclPara;
		}
	}

	private void ApplyInvitePara()
	{
		if (!string.IsNullOrEmpty(PECommandLine.InvitePara))
		{
			PeSteamFriendMgr.Instance.ReciveInvite(0uL, Convert.ToInt64(PECommandLine.InvitePara));
		}
	}

	private void LoadGlobalAssets()
	{
		Stopwatch stopwatch = new Stopwatch();
		if (_preloadAssets != null)
		{
			int num = _preloadAssets.Length;
			for (int i = 0; i < num; i++)
			{
				stopwatch.Start();
				if (_preloadAssets[i] != null)
				{
					UnityEngine.Object.Instantiate(Resources.Load(_preloadAssets[i]));
				}
				stopwatch.Stop();
				UnityEngine.Debug.LogError("Load " + i + ":" + stopwatch.ElapsedMilliseconds);
				stopwatch.Reset();
			}
		}
		ApplyInvitePara();
	}

	private void LoadGameMenuSence()
	{
		IsStarted = true;
		PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.MainMenuScene);
	}
}
