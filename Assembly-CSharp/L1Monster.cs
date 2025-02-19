using System.Collections.Generic;
using Pathea;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class L1Monster : MonoBehaviour
{
	public List<int> monsterProtoID;

	public List<int> dien1monsterID;

	public List<int> dien2monsterID;

	public List<int> dien3monsterID;

	public List<int> dien4monsterID;

	public List<int> dien5monsterID;

	private void OnTriggerEnter(Collider target)
	{
		if (null == target.GetComponentInParent<MainPlayerCmpt>())
		{
			return;
		}
		if (PeGameMgr.IsSingle)
		{
			if (SingleGameStory.curType == SingleGameStory.StoryScene.DienShip1)
			{
				for (int i = 0; i < dien1monsterID.Count; i++)
				{
					PeSingleton<SceneEntityCreatorArchiver>.Instance.SetFixedSpawnPointActive(dien1monsterID[i], active: true);
				}
			}
			else if (SingleGameStory.curType == SingleGameStory.StoryScene.DienShip2)
			{
				for (int j = 0; j < dien2monsterID.Count; j++)
				{
					PeSingleton<SceneEntityCreatorArchiver>.Instance.SetFixedSpawnPointActive(dien2monsterID[j], active: true);
				}
			}
			else if (SingleGameStory.curType == SingleGameStory.StoryScene.DienShip3)
			{
				for (int k = 0; k < dien3monsterID.Count; k++)
				{
					PeSingleton<SceneEntityCreatorArchiver>.Instance.SetFixedSpawnPointActive(dien3monsterID[k], active: true);
				}
			}
			else if (SingleGameStory.curType == SingleGameStory.StoryScene.DienShip4)
			{
				for (int l = 0; l < dien4monsterID.Count; l++)
				{
					PeSingleton<SceneEntityCreatorArchiver>.Instance.SetFixedSpawnPointActive(dien4monsterID[l], active: true);
				}
			}
			else if (SingleGameStory.curType == SingleGameStory.StoryScene.DienShip5)
			{
				for (int m = 0; m < dien5monsterID.Count; m++)
				{
					PeSingleton<SceneEntityCreatorArchiver>.Instance.SetFixedSpawnPointActive(dien5monsterID[m], active: true);
				}
			}
			else
			{
				for (int n = 0; n < monsterProtoID.Count; n++)
				{
					PeSingleton<SceneEntityCreatorArchiver>.Instance.SetFixedSpawnPointActive(monsterProtoID[n], active: true);
				}
			}
		}
		else if (PlayerNetwork.mainPlayer._curSceneId == 6)
		{
			for (int num = 0; num < dien1monsterID.Count; num++)
			{
				PeSingleton<SceneEntityCreatorArchiver>.Instance.SetFixedSpawnPointActive(dien1monsterID[num], active: true);
			}
		}
		else if (PlayerNetwork.mainPlayer._curSceneId == 7)
		{
			for (int num2 = 0; num2 < dien2monsterID.Count; num2++)
			{
				PeSingleton<SceneEntityCreatorArchiver>.Instance.SetFixedSpawnPointActive(dien2monsterID[num2], active: true);
			}
		}
		else if (PlayerNetwork.mainPlayer._curSceneId == 8)
		{
			for (int num3 = 0; num3 < dien3monsterID.Count; num3++)
			{
				PeSingleton<SceneEntityCreatorArchiver>.Instance.SetFixedSpawnPointActive(dien3monsterID[num3], active: true);
			}
		}
		else if (PlayerNetwork.mainPlayer._curSceneId == 9)
		{
			for (int num4 = 0; num4 < dien4monsterID.Count; num4++)
			{
				PeSingleton<SceneEntityCreatorArchiver>.Instance.SetFixedSpawnPointActive(dien4monsterID[num4], active: true);
			}
		}
		else if (PlayerNetwork.mainPlayer._curSceneId == 10)
		{
			for (int num5 = 0; num5 < dien5monsterID.Count; num5++)
			{
				PeSingleton<SceneEntityCreatorArchiver>.Instance.SetFixedSpawnPointActive(dien5monsterID[num5], active: true);
			}
		}
		else
		{
			for (int num6 = 0; num6 < monsterProtoID.Count; num6++)
			{
				PeSingleton<SceneEntityCreatorArchiver>.Instance.SetFixedSpawnPointActive(monsterProtoID[num6], active: true);
			}
		}
		GetComponent<Collider>().enabled = false;
	}
}
