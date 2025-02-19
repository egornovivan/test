using CustomCharactor;
using CustomData;
using Pathea;
using UnityEngine;

public class PeSceneCtrl : MonoBehaviour
{
	private static PeSceneCtrl mInstance;

	public static PeSceneCtrl Instance => mInstance;

	private void Awake()
	{
		mInstance = this;
	}

	public void GotoMainMenuScene()
	{
		GameClientLobby.Disconnect();
		GameClientNetwork.Disconnect();
		if (PeSingleton<PeFlowMgr>.Instance.curScene == PeFlowMgr.EPeScene.GameScene && RandomDungenMgrData.InDungeon && PeGameMgr.IsSingleAdventure)
		{
			RandomDungenMgr.Instance.SaveInDungeon();
			RandomDungenMgr.Instance.DestroyDungeon();
		}
		PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.MainMenuScene);
	}

	public void GotoRoleScene()
	{
		PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.RoleScene);
	}

	public void GotoLobbyScene()
	{
		GameClientNetwork.Disconnect();
		PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.LobbyScene);
	}

	public void GotoMultiRoleScene()
	{
		GameClientNetwork.Disconnect();
		PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.MultiRoleScene);
	}

	public void GotToTutorial(RoleInfo role)
	{
		TutorialExit.type = ((PeGameMgr.playerType == PeGameMgr.EPlayerType.Multiple) ? TutorialExit.TutorialType.MultiLobby : TutorialExit.TutorialType.Story);
		SystemSettingData.Instance.Tutorialed = true;
		string charactorName = role.name;
		CustomDataMgr.Instance.Current = role.CreateCustomData();
		CustomDataMgr.Instance.Current.charactorName = charactorName;
		PeGameMgr.playerType = PeGameMgr.EPlayerType.Tutorial;
		PeGameMgr.sceneMode = PeGameMgr.ESceneMode.Tutorial;
		PeGameMgr.tutorialMode = PeGameMgr.ETutorialMode.DigBuild;
		PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.GameScene);
	}

	public void GotoGameSence()
	{
		PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.GameScene);
	}
}
