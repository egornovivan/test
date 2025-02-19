using System;
using System.IO;
using PETools;
using UnityEngine;

namespace Pathea;

public class PeGameSummary
{
	public class Mgr : ArchivableSingleton<Mgr>
	{
		private const string ArchiveKey = "ArchiveKeyGameSummary";

		protected override string GetArchiveKey()
		{
			return "ArchiveKeyGameSummary";
		}

		protected override bool GetYird()
		{
			return false;
		}

		protected override void WriteData(BinaryWriter bw)
		{
			Gather().Export(bw);
		}

		protected override void SetData(byte[] data)
		{
		}

		private PeGameSummary Gather()
		{
			PeGameSummary peGameSummary = new PeGameSummary();
			peGameSummary.screenshot = ((!PeSingleton<ArchiveMgr>.Instance.autoSave) ? PeScreenshot.GetTex() : null);
			PeTrans peTrans = PeSingleton<MainPlayer>.Instance.entity.peTrans;
			if (peTrans != null)
			{
				peGameSummary.playerPos = ((!peTrans.fastTravel) ? peTrans.position : peTrans.fastTravelPos);
			}
			peGameSummary.gameTime = GameTime.Timer.Second;
			peGameSummary.playTime = (int)GameTime.PlayTime.Second;
			peGameSummary.saveTime = DateTime.Now;
			peGameSummary.sceneMode = PeGameMgr.sceneMode;
			peGameSummary.seed = ((peGameSummary.sceneMode != 0) ? RandomMapConfig.SeedString : "NA");
			PeEntity mainPlayer = PeSingleton<PeCreature>.Instance.mainPlayer;
			peGameSummary.playerName = ((!(null == mainPlayer)) ? mainPlayer.ToString() : "NA");
			peGameSummary.gameLevel = PeGameMgr.gameLevel;
			return peGameSummary;
		}

		public PeGameSummary Get()
		{
			byte[] data = PeSingleton<ArchiveMgr>.Instance.GetData("ArchiveKeyGameSummary");
			if (data == null || data.Length <= 0)
			{
				return null;
			}
			PeGameSummary peGameSummary = new PeGameSummary();
			return (!peGameSummary.Import(data)) ? null : peGameSummary;
		}

		public void Init()
		{
			Debug.Log("game summary");
		}
	}

	private const int VERSION_0000 = 0;

	private const int VERSION_0001 = 1;

	private const int VERSION_0002 = 2;

	private const int VERSION_0003 = 3;

	private const int VERSION_0004 = 4;

	private const int CURRENT_VERSION = 4;

	public Vector3 playerPos { get; private set; }

	public Texture2D screenshot { get; private set; }

	public double gameTime { get; private set; }

	public int playTime { get; private set; }

	public string seed { get; private set; }

	public DateTime saveTime { get; private set; }

	public PeGameMgr.ESceneMode sceneMode { get; private set; }

	public string playerName { get; private set; }

	public PeGameMgr.EGameLevel gameLevel { get; private set; }

	private PeGameSummary()
	{
	}

	private void Export(BinaryWriter w)
	{
		w.Write(4);
		byte[] buff = null;
		if (screenshot != null)
		{
			buff = screenshot.EncodeToPNG();
		}
		Serialize.WriteBytes(buff, w);
		w.Write(gameTime);
		w.Write(playTime);
		w.Write(seed);
		w.Write(saveTime.Ticks);
		w.Write((int)sceneMode);
		w.Write(playerName);
		Serialize.WriteVector3(w, playerPos);
		w.Write((int)gameLevel);
	}

	private bool Import(byte[] buffer)
	{
		try
		{
			using MemoryStream input = new MemoryStream(buffer, writable: false);
			using BinaryReader binaryReader = new BinaryReader(input);
			int num = binaryReader.ReadInt32();
			if (num > 4)
			{
				Debug.LogError("[GameSummary]error version:" + num);
				return false;
			}
			byte[] array = Serialize.ReadBytes(binaryReader);
			if (array != null && array.Length > 0)
			{
				screenshot = new Texture2D(2, 2, TextureFormat.RGB24, mipmap: false);
				screenshot.LoadImage(array);
				screenshot.Apply();
			}
			gameTime = binaryReader.ReadDouble();
			playTime = binaryReader.ReadInt32();
			seed = binaryReader.ReadString();
			saveTime = new DateTime(binaryReader.ReadInt64());
			sceneMode = (PeGameMgr.ESceneMode)binaryReader.ReadInt32();
			if (sceneMode == PeGameMgr.ESceneMode.Adventure && num < 4)
			{
				Debug.LogError("[GameSummary]Deprecated version of adv save");
				return false;
			}
			playerName = binaryReader.ReadString();
			playerPos = ((num < 1) ? Vector3.zero : Serialize.ReadVector3(binaryReader));
			gameLevel = ((num < 2) ? PeGameMgr.EGameLevel.Normal : ((PeGameMgr.EGameLevel)binaryReader.ReadInt32()));
			return true;
		}
		catch
		{
			Debug.LogError("[GameSummary]Corrupt save data");
		}
		return false;
	}
}
