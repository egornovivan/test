using System.IO;
using PETools;
using UnityEngine;

namespace Pathea;

public class SinglePlayerTypeLoader : PlayerTypeLoader
{
	private const int VERSION_0000 = 0;

	private const int VERSION_0001 = 1;

	private const int VERSION_0002 = 2;

	private const int CURRENT_VERSION = 2;

	private string mGameName;

	private string mYirdName;

	private string mUID;

	private bool bNewGame = true;

	public string yirdName => mYirdName;

	public string gameName => mGameName;

	public string UID => mUID;

	public void SetYirdName(string yirdName)
	{
		mYirdName = yirdName;
	}

	public override void Load()
	{
		SingleGame singleGame = null;
		if (base.sceneMode == PeGameMgr.ESceneMode.Story)
		{
			singleGame = new SingleGameStory();
		}
		else if (base.sceneMode == PeGameMgr.ESceneMode.Adventure)
		{
			singleGame = new SingleGameAdventure();
		}
		else if (base.sceneMode == PeGameMgr.ESceneMode.Build)
		{
			singleGame = new SingleGameBuild();
		}
		else if (base.sceneMode == PeGameMgr.ESceneMode.Custom)
		{
			singleGame = new SingleGameCustom(mUID, mGameName);
		}
		else
		{
			Debug.LogError("error scene mode:" + base.sceneMode);
		}
		if (singleGame != null)
		{
			singleGame.Load(bNewGame, mYirdName);
			mYirdName = singleGame.yirdName;
		}
	}

	public void New(PeGameMgr.ESceneMode eSceneMode, string uid, string gameName)
	{
		bNewGame = true;
		base.sceneMode = eSceneMode;
		mGameName = gameName;
		mYirdName = null;
		mUID = uid;
		InitNew(eSceneMode);
	}

	private static void InitNew(PeGameMgr.ESceneMode eSceneMode)
	{
		switch (eSceneMode)
		{
		case PeGameMgr.ESceneMode.Story:
			GameTime.Timer.Day = 94.35;
			GameTime.Timer.ElapseSpeed = GameTime.NormalTimeSpeed;
			Money.Digital = false;
			break;
		case PeGameMgr.ESceneMode.Adventure:
			GameTime.Timer.Day = 94.35;
			GameTime.Timer.ElapseSpeed = GameTime.NormalTimeSpeed;
			Money.Digital = true;
			break;
		case PeGameMgr.ESceneMode.Build:
			GameTime.Timer.Day = 94.5;
			GameTime.Timer.ElapseSpeed = 0f;
			Money.Digital = true;
			break;
		case PeGameMgr.ESceneMode.Custom:
			GameTime.Timer.Day = 94.35;
			GameTime.Timer.ElapseSpeed = GameTime.NormalTimeSpeed;
			Money.Digital = true;
			break;
		}
	}

	public void Import(byte[] buffer)
	{
		bNewGame = false;
		Serialize.Import(buffer, delegate(BinaryReader r)
		{
			int num = r.ReadInt32();
			if (num > 2)
			{
				Debug.LogError("error version:" + num);
			}
			GameTime.Timer.Second = r.ReadDouble();
			if (num >= 1)
			{
				GameTime.Timer.ElapseSpeed = r.ReadSingle();
				GameTime.Timer.ElapseSpeedBak = -1f;
			}
			else
			{
				GameTime.Timer.ElapseSpeed = GameTime.NormalTimeSpeed;
				GameTime.Timer.ElapseSpeedBak = -1f;
			}
			GameTime.PlayTime.Second = r.ReadInt32();
			base.sceneMode = (PeGameMgr.ESceneMode)r.ReadInt32();
			mGameName = Serialize.ReadNullableString(r);
			mYirdName = Serialize.ReadNullableString(r);
			if (num >= 2)
			{
				mUID = Serialize.ReadNullableString(r);
			}
			Money.Digital = r.ReadBoolean();
		});
	}

	public void Export(BinaryWriter w)
	{
		w.Write(2);
		w.Write(GameTime.Timer.Second);
		if (GameTime.Timer.ElapseSpeedBak < 0f)
		{
			w.Write(GameTime.Timer.ElapseSpeed);
		}
		else
		{
			w.Write(GameTime.Timer.ElapseSpeedBak);
		}
		w.Write((int)GameTime.PlayTime.Second);
		w.Write((int)base.sceneMode);
		Serialize.WriteNullableString(w, mGameName);
		if (mYirdName == AdventureScene.Dungen.ToString())
		{
			Debug.LogError("save yird = dungen!!!");
			mYirdName = AdventureScene.MainAdventure.ToString();
		}
		Serialize.WriteNullableString(w, mYirdName);
		Serialize.WriteNullableString(w, mUID);
		w.Write(Money.Digital);
	}
}
