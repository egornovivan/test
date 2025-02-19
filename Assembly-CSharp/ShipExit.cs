using Pathea;
using UnityEngine;

public class ShipExit : MonoBehaviour
{
	public int doorId;

	private bool isShow;

	private void OnTriggerEnter(Collider target)
	{
		if (!(null == target.GetComponentInParent<MainPlayerCmpt>()) && !isShow)
		{
			isShow = true;
			MessageBox_N.ShowYNBox(PELocalization.GetString(82209002), SceneTranslate, SetFalse);
		}
	}

	public void SceneTranslate()
	{
		MissionManager.Instance.yirdName = "main";
		if (PeGameMgr.IsMultiStory)
		{
			if (PlayerNetwork.mainPlayer._curSceneId == 2)
			{
				MissionManager.Instance.transPoint = new Vector3(14819.54f, 106.1666f, 8347.545f);
			}
			else if (PlayerNetwork.mainPlayer._curSceneId == 6)
			{
				MissionManager.Instance.transPoint = new Vector3(16545f, 219f, 10748f);
			}
			else if (PlayerNetwork.mainPlayer._curSceneId == 7)
			{
				MissionManager.Instance.transPoint = new Vector3(2890.597f, 385.7521f, 9852.657f);
			}
			else if (PlayerNetwork.mainPlayer._curSceneId == 8)
			{
				MissionManager.Instance.transPoint = new Vector3(13863.91f, 173.9639f, 15278.54f);
			}
			else if (PlayerNetwork.mainPlayer._curSceneId == 9)
			{
				MissionManager.Instance.transPoint = new Vector3(12562.67f, 643.8412f, 13587.89f);
			}
			else if (PlayerNetwork.mainPlayer._curSceneId == 10)
			{
				MissionManager.Instance.transPoint = new Vector3(7844.74f, 455.2281f, 14668.19f);
			}
			else if (PlayerNetwork.mainPlayer._curSceneId == 1)
			{
				MissionManager.Instance.transPoint = new Vector3(9684.722f, 368.9954f, 12795.33f);
			}
			else if (PlayerNetwork.mainPlayer._curSceneId == 4)
			{
				MissionManager.Instance.transPoint = new Vector3(1570f, 118f, 8024f);
			}
			else if (PlayerNetwork.mainPlayer._curSceneId == 5)
			{
				if (doorId == 1)
				{
					MissionManager.Instance.transPoint = new Vector3(1674f, 237f, 10365f);
				}
				else if (doorId == 2)
				{
					MissionManager.Instance.transPoint = new Vector3(1886f, 267f, 10392f);
				}
			}
		}
		else if (SingleGameStory.curType == SingleGameStory.StoryScene.DienShip0)
		{
			MissionManager.Instance.transPoint = new Vector3(14819.54f, 106.1666f, 8347.545f);
		}
		else if (SingleGameStory.curType == SingleGameStory.StoryScene.DienShip1)
		{
			MissionManager.Instance.transPoint = new Vector3(16545f, 219f, 10748f);
		}
		else if (SingleGameStory.curType == SingleGameStory.StoryScene.DienShip2)
		{
			MissionManager.Instance.transPoint = new Vector3(2890.597f, 385.7521f, 9852.657f);
		}
		else if (SingleGameStory.curType == SingleGameStory.StoryScene.DienShip3)
		{
			MissionManager.Instance.transPoint = new Vector3(13863.91f, 173.9639f, 15278.54f);
		}
		else if (SingleGameStory.curType == SingleGameStory.StoryScene.DienShip4)
		{
			MissionManager.Instance.transPoint = new Vector3(12562.67f, 643.8412f, 13587.89f);
		}
		else if (SingleGameStory.curType == SingleGameStory.StoryScene.DienShip5)
		{
			MissionManager.Instance.transPoint = new Vector3(7844.74f, 455.2281f, 14668.19f);
		}
		else if (SingleGameStory.curType == SingleGameStory.StoryScene.L1Ship)
		{
			MissionManager.Instance.transPoint = new Vector3(9679.52f, 371.66f, 12795.33f);
		}
		else if (SingleGameStory.curType == SingleGameStory.StoryScene.PajaShip)
		{
			MissionManager.Instance.transPoint = new Vector3(1570f, 118f, 8024f);
		}
		else if (SingleGameStory.curType == SingleGameStory.StoryScene.LaunchCenter)
		{
			if (doorId == 1)
			{
				MissionManager.Instance.transPoint = new Vector3(1674f, 237f, 10365f);
			}
			else if (doorId == 2)
			{
				MissionManager.Instance.transPoint = new Vector3(1886f, 267f, 10392f);
			}
		}
		MissionManager.Instance.SceneTranslate();
		SetFalse();
	}

	public void SetFalse()
	{
		isShow = false;
	}
}
