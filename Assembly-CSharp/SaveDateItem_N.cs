using Pathea;
using UnityEngine;

public class SaveDateItem_N : MonoBehaviour
{
	public delegate void OnSelected(int index, PeGameSummary summary);

	public UICheckbox mCheckbox;

	public UILabel mCharacterName;

	public UILabel mGameType;

	public UILabel mSaveTime;

	public UILabel mIndexTex;

	private int mIndex;

	public OnSelected mOnSelected;

	private PeGameSummary mSummary;

	public PeGameSummary Summary => mSummary;

	public void Init(int index, OnSelected onSelected)
	{
		mIndex = index;
		mOnSelected = onSelected;
	}

	public void SetArchive(PeGameSummary summary)
	{
		mSummary = summary;
		if (mSummary == null)
		{
			ClearInfo();
			return;
		}
		mCharacterName.text = summary.playerName;
		mSaveTime.text = summary.saveTime.ToString();
		switch (summary.sceneMode)
		{
		case PeGameMgr.ESceneMode.Story:
			mGameType.text = "Story";
			break;
		case PeGameMgr.ESceneMode.Adventure:
			mGameType.text = "Adventure";
			break;
		case PeGameMgr.ESceneMode.Build:
			mGameType.text = "Build";
			break;
		case PeGameMgr.ESceneMode.Custom:
			mGameType.text = "Custom";
			break;
		case PeGameMgr.ESceneMode.TowerDefense:
			break;
		}
	}

	private void ClearInfo()
	{
		mCharacterName.text = string.Empty;
		mGameType.text = string.Empty;
		mSaveTime.text = string.Empty;
	}

	private void OnActivate(bool active)
	{
		if (active && mOnSelected != null)
		{
			mOnSelected(mIndex, mSummary);
		}
	}
}
