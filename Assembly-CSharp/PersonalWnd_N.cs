using System.Collections.Generic;
using UnityEngine;

public class PersonalWnd_N : MonoBehaviour
{
	public UILabel mGroupName;

	public PlayerScoreItem_N mPerfab;

	public Transform mListRoot;

	private List<PlayerScoreItem_N> mPlayerItemList = new List<PlayerScoreItem_N>();

	public void Init()
	{
		for (int i = 0; i < 8; i++)
		{
			PlayerScoreItem_N playerScoreItem_N = Object.Instantiate(mPerfab);
			playerScoreItem_N.transform.parent = mListRoot;
			playerScoreItem_N.transform.localScale = Vector3.one;
			playerScoreItem_N.transform.localPosition = new Vector3(0f, 88f - (float)i * 30f, 0f);
			mPlayerItemList.Add(playerScoreItem_N);
		}
	}

	internal void SetInfo(List<PlayerBattleInfo> playerList)
	{
		for (int i = 0; i < playerList.Count; i++)
		{
			if (i < mPlayerItemList.Count)
			{
				mPlayerItemList[i].SetInfo(playerList[i].RoleName, playerList[i].Info._killCount, playerList[i].Info._deathCount, (int)playerList[i].Info._point);
			}
		}
	}
}
