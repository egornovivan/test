using Pathea;
using UnityEngine;

public class RoomInfo_N : MonoBehaviour
{
	public UILabel GameModeLabel;

	public UILabel GameTypeLabel;

	public UILabel MapNameLabel;

	public UILabel MapSizeLabel;

	public UILabel PlayerNumLabel;

	public UILabel MapInfoLabel;

	public UISlider IsoProcessSlider;

	public UILabel IsoSpeedLabel;

	public UILabel IsoCountLabel;

	public void UpdateInfo()
	{
		UpdateGameMode();
		UpdateGameType();
		UpdateMapName();
		UpdateMapSize();
		UpdatePlayerCount();
		UpdateMapInfo(string.Empty);
	}

	public void UpdateMapInfo(string info)
	{
		if (info != null)
		{
			MapInfoLabel.text = info;
		}
	}

	public void UpdateIsoProcess(float processVal)
	{
		if (processVal > 0f)
		{
			IsoProcessSlider.sliderValue = processVal;
		}
	}

	public void UpdateIsoSpeed(string speedStr)
	{
		if (speedStr != null)
		{
			IsoSpeedLabel.text = speedStr;
		}
	}

	public void UpdateIsoCount(string countStr)
	{
		if (countStr != null)
		{
			IsoCountLabel.text = countStr;
		}
	}

	private void UpdateGameMode()
	{
		switch (PeGameMgr.sceneMode)
		{
		case PeGameMgr.ESceneMode.Story:
		case PeGameMgr.ESceneMode.Adventure:
		case PeGameMgr.ESceneMode.Build:
		case PeGameMgr.ESceneMode.Custom:
			GameModeLabel.text = PeGameMgr.sceneMode.ToString();
			break;
		case PeGameMgr.ESceneMode.TowerDefense:
			break;
		}
	}

	private void UpdateGameType()
	{
		switch (PeGameMgr.gameType)
		{
		case PeGameMgr.EGameType.Cooperation:
		case PeGameMgr.EGameType.VS:
		case PeGameMgr.EGameType.Survive:
			GameTypeLabel.text = PeGameMgr.gameType.ToString();
			break;
		}
	}

	private void UpdateMapName()
	{
		MapNameLabel.text = GameClientNetwork.ServerName;
	}

	private void UpdateMapSize()
	{
		string text = string.Empty;
		switch (RandomMapConfig.mapSize)
		{
		case 0:
			text = "40km * 40km";
			break;
		case 1:
			text = "20km * 20km";
			break;
		case 2:
			text = "8km * 8km";
			break;
		case 3:
			text = "4km * 4km";
			break;
		case 4:
			text = "2km * 2km";
			break;
		}
		MapSizeLabel.text = text;
	}

	private void UpdatePlayerCount()
	{
		PlayerNumLabel.text = (BattleManager.teamNum * BattleManager.numberTeam).ToString();
	}
}
