using System.Collections.Generic;
using Pathea;

public class LobbyToolTipInfo
{
	public string mRoomNumber;

	public string mGameMode;

	public string mMapName;

	public string mTeamNo;

	public string mSeedNo;

	public string mAlienCamp;

	public string mTown;

	public string mMapSize;

	public string mElevation;

	public LobbyToolTipInfo(ServerRegistered _server)
	{
		mRoomNumber = _server.ServerID.ToString();
		switch ((PeGameMgr.ESceneMode)_server.GameMode)
		{
		case PeGameMgr.ESceneMode.Adventure:
			mGameMode = "Adventure";
			break;
		case PeGameMgr.ESceneMode.Build:
			mGameMode = "Build";
			break;
		case PeGameMgr.ESceneMode.Story:
			mGameMode = "Story";
			break;
		case PeGameMgr.ESceneMode.Custom:
			mGameMode = "Custom";
			break;
		default:
			mGameMode = "Adventure";
			break;
		}
		mMapName = string.Empty;
		mTeamNo = string.Empty;
		mSeedNo = string.Empty;
		mAlienCamp = string.Empty;
		mTown = string.Empty;
		mMapSize = string.Empty;
		mElevation = string.Empty;
	}

	public List<string> ToList()
	{
		List<string> list = new List<string>();
		list.Add(mRoomNumber);
		list.Add(mGameMode);
		list.Add(mMapName);
		list.Add(mTeamNo);
		list.Add(mSeedNo);
		list.Add(mAlienCamp);
		list.Add(mTown);
		list.Add(mMapSize);
		list.Add(mElevation);
		return list;
	}
}
