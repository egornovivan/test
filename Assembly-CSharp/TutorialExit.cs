using Pathea;
using UnityEngine;

public class TutorialExit : MonoBehaviour
{
	public enum TutorialType
	{
		Story,
		Mainmenu,
		MultiLobby
	}

	public static TutorialType type;

	private bool isShow;

	private void OnTriggerEnter(Collider target)
	{
		if (!(null == target.GetComponentInParent<MainPlayerCmpt>()) && !isShow)
		{
			isShow = true;
			if (MissionManager.Instance.HadCompleteMission(756))
			{
				MessageBox_N.ShowYNBox(PELocalization.GetString(8000505), SceneTranslate, SetFalse);
			}
			else
			{
				MessageBox_N.ShowYNBox(PELocalization.GetString(8000506), SceneTranslate, SetFalse);
			}
		}
	}

	private void SceneTranslate()
	{
		if (type == TutorialType.Story)
		{
			IntroRunner.movieEnd = delegate
			{
				Debug.Log("<color=aqua>intro movie end.</color>");
				PeGameMgr.playerType = PeGameMgr.EPlayerType.Single;
				PeGameMgr.sceneMode = PeGameMgr.ESceneMode.Story;
				PeSceneCtrl.Instance.GotoGameSence();
			};
			PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.Intro);
			SetFalse();
		}
		else if (type == TutorialType.MultiLobby)
		{
			PeGameMgr.playerType = PeGameMgr.EPlayerType.Multiple;
			GameClientLobby.Self.TryEnterLobby(MLPlayerInfo.Instance.GetRoleInfo(MLPlayerInfo.Instance.mRolesCtrl.GetSelectedIndex()).mRoleInfo.roleID);
		}
		else
		{
			PeSceneCtrl.Instance.GotoMainMenuScene();
		}
	}

	private void SetFalse()
	{
		isShow = false;
	}
}
