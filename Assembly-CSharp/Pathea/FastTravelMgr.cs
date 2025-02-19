using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathea;

public class FastTravelMgr : MonoLikeSingleton<FastTravelMgr>
{
	public interface IEnable
	{
		bool Enable();
	}

	private List<IEnable> mList = new List<IEnable>(1);

	public event Action<Vector3> OnFastTravel;

	public void DispatchFastTravel(Vector3 pos)
	{
		if (this.OnFastTravel != null)
		{
			this.OnFastTravel(pos);
		}
	}

	public void TravelTo(string yirdName, Vector3 pos)
	{
		PeGameMgr.targetYird = yirdName;
		TravelTo(pos);
	}

	public void TravelTo(int worldIndex, Vector3 pos)
	{
		FastTravel.bTraveling = true;
		YirdData yirdData = PeSingleton<CustomGameData.Mgr>.Instance.curGameData.GetYirdData(worldIndex);
		if (yirdData != null)
		{
			PeSingleton<CustomGameData.Mgr>.Instance.curGameData.WorldIndex = worldIndex;
			PeGameMgr.targetYird = yirdData.name;
			TravelTo(pos);
		}
	}

	public void TravelTo(Vector3 pos)
	{
		if (!TravelEnable())
		{
			Debug.Log("<color=aqua>fast travel enable = false</color>");
			return;
		}
		PeEntity mainPlayer = PeSingleton<PeCreature>.Instance.mainPlayer;
		if (null == mainPlayer)
		{
			Debug.LogError("no main player");
			return;
		}
		PeTrans peTrans = mainPlayer.peTrans;
		if (!(null == peTrans))
		{
			peTrans.fastTravelPos = pos;
			if (PeGameMgr.yirdName == AdventureScene.Dungen.ToString())
			{
				RandomDungenMgr.Instance.TransFromDungeon(pos);
				PeGameMgr.targetYird = AdventureScene.MainAdventure.ToString();
			}
			DispatchFastTravel(pos);
			TravelWithLoadScene();
		}
	}

	private void TravelWithLoadScene()
	{
		PeGameMgr.loadArchive = ArchiveMgr.ESave.Min;
		PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.GameScene);
	}

	public void Add(IEnable e)
	{
		mList.Add(e);
	}

	public bool Remove(IEnable e)
	{
		return mList.Remove(e);
	}

	public bool TravelEnable()
	{
		foreach (IEnable m in mList)
		{
			if (!m.Enable())
			{
				return false;
			}
		}
		return true;
	}
}
