using System.IO;
using PETools;
using UnityEngine;

namespace Pathea;

public class MultiPlayerTypeLoader : PlayerTypeLoader
{
	private const int VERSION_0000 = 0;

	private const int VERSION_0001 = 1;

	private const int CURRENT_VERSION = 1;

	private string mGameName;

	private string mYirdName;

	private bool bNewGame = true;

	public string yirdName => mYirdName;

	public string gameName => mGameName;

	public void SetYirdName(string yirdName)
	{
		mYirdName = yirdName;
	}

	public void New(PeGameMgr.ESceneMode eSceneMode, string gameName)
	{
		bNewGame = true;
		base.sceneMode = eSceneMode;
		mGameName = gameName;
		mYirdName = null;
	}

	public override void Load()
	{
		Money.Digital = true;
		MultiGame multiGame = null;
		if (base.sceneMode == PeGameMgr.ESceneMode.Adventure)
		{
			multiGame = new MultiGameAdventure();
		}
		else if (base.sceneMode == PeGameMgr.ESceneMode.Story)
		{
			multiGame = new MultiGameStory();
		}
		else if (base.sceneMode == PeGameMgr.ESceneMode.Build)
		{
			multiGame = new MultiGameBuild();
		}
		else if (base.sceneMode == PeGameMgr.ESceneMode.Custom)
		{
			multiGame = new MultiGameCustom();
		}
		if (multiGame != null)
		{
			multiGame.Load(bNewGame, mYirdName);
			mYirdName = multiGame.yirdName;
		}
	}

	public void Import(byte[] buffer)
	{
		bNewGame = false;
		Serialize.Import(buffer, delegate(BinaryReader r)
		{
			int num = r.ReadInt32();
			if (num > 1)
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
			Money.Digital = r.ReadBoolean();
		});
	}

	public void Export(BinaryWriter w)
	{
		w.Write(1);
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
		Serialize.WriteNullableString(w, mYirdName);
		w.Write(Money.Digital);
	}
}
