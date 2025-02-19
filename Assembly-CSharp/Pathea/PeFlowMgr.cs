using System;
using UnityEngine;

namespace Pathea;

public class PeFlowMgr : PeSingleton<PeFlowMgr>
{
	public enum EPeScene
	{
		StartScene,
		RoleScene,
		LobbyScene,
		MainMenuScene,
		ClientScene,
		CreationScene,
		MultiRoleScene,
		GameScene,
		Intro,
		Max
	}

	private const string GameSceneName = "PeGame";

	private const string IntroSceneName = "Intro";

	private string[] mSceneMap = new string[9] { "GameStart", "GameRoleCustom", "GameLobby", "GameMainMenu", "GameClient", "CreationSystem", "MLoginScene", "PeGame", "Intro" };

	public EPeScene curScene { get; private set; }

	private bool LoadUnityScene(string unitySceneName)
	{
		Application.LoadLevel(unitySceneName);
		return true;
	}

	public void LoadScene(EPeScene ePeScene, bool save = true)
	{
		AudioListener.pause = true;
		if (curScene == EPeScene.GameScene && save)
		{
			AutoArchiveRunner.QuitSave();
		}
		curScene = ePeScene;
		SystemSettingData.Instance.ResetVSync();
		Resources.UnloadUnusedAssets();
		GC.Collect();
		UILoadScenceEffect.Instance.EnableProgress(enable: false);
		bool bNeedProgress = ePeScene == EPeScene.GameScene;
		UILoadScenceEffect.Instance.EndScence(delegate
		{
			LoadUnityScene(mSceneMap[(int)ePeScene]);
			UILoadScenceEffect.Instance.BeginScence(null, bNeedProgress);
		}, bNeedProgress);
	}
}
