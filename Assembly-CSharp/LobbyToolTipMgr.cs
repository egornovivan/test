using System.Collections.Generic;
using UnityEngine;

public class LobbyToolTipMgr : MonoBehaviour
{
	private static LobbyToolTipMgr mInstance;

	public GameObject mContent;

	public UILabel mLbRoomNumber;

	public UILabel mLbGameMode;

	public UILabel mLbMapName;

	public UILabel mLbTeamNo;

	public UILabel mLbSeedNo;

	public UILabel mLbAlienCamp;

	public UILabel mLbTown;

	public UILabel mLbMapSize;

	public UILabel mLbElevation;

	private void Awake()
	{
		mInstance = this;
	}

	private void SetText(LobbyToolTipInfo info)
	{
		if (info != null)
		{
			List<string> list = info.ToList();
			mContent.SetActive(value: true);
			mLbRoomNumber.text = ((list[0].Length <= 0) ? "N/A" : list[0]);
			mLbGameMode.text = ((list[1].Length <= 0) ? "N/A" : list[0]);
			mLbMapName.text = ((list[2].Length <= 0) ? "N/A" : list[0]);
			mLbTeamNo.text = ((list[3].Length <= 0) ? "N/A" : list[0]);
			mLbSeedNo.text = ((list[4].Length <= 0) ? "N/A" : list[0]);
			mLbAlienCamp.text = ((list[5].Length <= 0) ? "N/A" : list[0]);
			mLbTown.text = ((list[6].Length <= 0) ? "N/A" : list[0]);
			mLbMapSize.text = ((list[7].Length <= 0) ? "N/A" : list[0]);
			mLbElevation.text = ((list[8].Length <= 0) ? "N/A" : list[0]);
		}
		else
		{
			mContent.SetActive(value: false);
		}
	}

	public static void ShowText(LobbyToolTipInfo info)
	{
		if ((bool)mInstance)
		{
			mInstance.SetText(info);
		}
	}
}
