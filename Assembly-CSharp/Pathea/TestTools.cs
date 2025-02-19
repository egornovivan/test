using System.Collections.Generic;
using UnityEngine;

namespace Pathea;

public class TestTools : MonoBehaviour
{
	private PeGameSummary[] mGameSummaryArray = new PeGameSummary[20];

	private GUIStyle archiveBtnStyle;

	private void InitStyle()
	{
		archiveBtnStyle = new GUIStyle();
		archiveBtnStyle.stretchHeight = true;
		archiveBtnStyle.stretchWidth = true;
	}

	private void Awake()
	{
		InitStyle();
		for (int i = 3; i < 23; i++)
		{
			PeSingleton<ArchiveMgr>.Instance.Load((ArchiveMgr.ESave)i);
			mGameSummaryArray[i - 3] = PeSingleton<PeGameSummary.Mgr>.Instance.Get();
		}
	}

	private void Start()
	{
		UILoadScenceEffect.Instance.BeginScence(null);
	}

	private void OnDestory()
	{
	}

	private void OnGUI()
	{
		GUILayout.BeginArea(new Rect(0f, 0f, 200f, 500f));
		DrawSingle();
		GUILayout.EndArea();
		GUILayout.BeginArea(new Rect(300f, 0f, 200f, 500f));
		DrawArchive();
		GUILayout.EndArea();
		GUILayout.BeginArea(new Rect(600f, 0f, 200f, 500f));
		DrawMulti();
		GUILayout.EndArea();
		GUILayout.BeginArea(new Rect(0f, 300f, 1000f, 500f));
		GUILayout.EndArea();
	}

	private void DrawStoryYird()
	{
		GUILayout.BeginVertical();
		if (GUILayout.Button("Story_main"))
		{
			PeGameMgr.loadArchive = ArchiveMgr.ESave.Min;
			PeGameMgr.yirdName = "main";
			PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.GameScene);
		}
		if (GUILayout.Button("Story_yird"))
		{
			PeGameMgr.loadArchive = ArchiveMgr.ESave.Min;
			PeGameMgr.yirdName = "sdfd";
			PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.GameScene);
		}
		GUILayout.EndVertical();
	}

	private void DrawCustomGame()
	{
		GUILayout.BeginHorizontal();
		List<CustomGameData> customGameList = PeSingleton<CustomGameData.Mgr>.Instance.GetCustomGameList();
		foreach (CustomGameData item in customGameList)
		{
			GUILayout.BeginVertical(GUILayout.MaxWidth(160f));
			if (GUILayout.Button(item.name, GUILayout.MaxWidth(128f)))
			{
				PeGameMgr.playerType = PeGameMgr.EPlayerType.Single;
				PeGameMgr.loadArchive = ArchiveMgr.ESave.MaxUser;
				PeGameMgr.sceneMode = PeGameMgr.ESceneMode.Custom;
				PeGameMgr.gameName = item.name;
				PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.GameScene);
			}
			GUILayout.Label(item.size.ToString());
			GUILayout.Label(item.screenshot, archiveBtnStyle, GUILayout.MaxWidth(128f), GUILayout.MaxHeight(128f));
			GUILayout.EndVertical();
		}
		GUILayout.EndHorizontal();
	}

	private void DrawSingle()
	{
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Story"))
		{
			PeGameMgr.playerType = PeGameMgr.EPlayerType.Single;
			PeGameMgr.loadArchive = ArchiveMgr.ESave.MaxUser;
			PeGameMgr.sceneMode = PeGameMgr.ESceneMode.Story;
			PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.GameScene);
		}
		DrawStoryYird();
		GUILayout.EndHorizontal();
		if (GUILayout.Button("Adventure"))
		{
			PeGameMgr.playerType = PeGameMgr.EPlayerType.Single;
			PeGameMgr.loadArchive = ArchiveMgr.ESave.MaxUser;
			PeGameMgr.sceneMode = PeGameMgr.ESceneMode.Adventure;
			PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.GameScene);
		}
		if (GUILayout.Button("Build"))
		{
			PeGameMgr.playerType = PeGameMgr.EPlayerType.Single;
			PeGameMgr.loadArchive = ArchiveMgr.ESave.MaxUser;
			PeGameMgr.sceneMode = PeGameMgr.ESceneMode.Build;
			PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.GameScene);
		}
		if (GUILayout.Button("Custom"))
		{
			PeGameMgr.playerType = PeGameMgr.EPlayerType.Single;
			PeGameMgr.loadArchive = ArchiveMgr.ESave.MaxUser;
			PeGameMgr.sceneMode = PeGameMgr.ESceneMode.Custom;
			PeGameMgr.gameName = "Pandora";
			PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.GameScene);
		}
		if (GUILayout.Button("Continue"))
		{
			PeGameMgr.playerType = PeGameMgr.EPlayerType.Single;
			PeGameMgr.loadArchive = ArchiveMgr.ESave.Min;
			PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.GameScene);
		}
		if (GUILayout.Button("Test Intro"))
		{
			IntroRunner.movieEnd = delegate
			{
				Debug.Log("<color=aqua>intro movie end.</color>");
			};
			PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.Intro);
		}
	}

	private void DrawMulti()
	{
		if (GUILayout.Button("MultiPlayer"))
		{
			PeGameMgr.playerType = PeGameMgr.EPlayerType.Multiple;
			PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.GameScene);
		}
		if (GUILayout.Button("CreationMode"))
		{
			PeGameMgr.playerType = PeGameMgr.EPlayerType.Creation;
			PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.GameScene);
		}
		if (GUILayout.Button("Tutorial"))
		{
			PeGameMgr.playerType = PeGameMgr.EPlayerType.Tutorial;
			PeGameMgr.tutorialMode = PeGameMgr.ETutorialMode.DigBuild;
			PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.GameScene);
		}
	}

	private void DrawArchive()
	{
		GUILayout.BeginVertical();
		for (int i = 3; i < 23; i++)
		{
			int num = i - 3;
			if (mGameSummaryArray[num] != null)
			{
				GUILayout.BeginArea(new Rect(0f, num * 100, 100f, 100f));
				if (GUILayout.Button(mGameSummaryArray[num].screenshot, archiveBtnStyle))
				{
					PeGameMgr.playerType = PeGameMgr.EPlayerType.Single;
					PeGameMgr.loadArchive = (ArchiveMgr.ESave)i;
					PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.GameScene);
				}
				GUILayout.EndArea();
			}
		}
		GUILayout.EndVertical();
	}
}
