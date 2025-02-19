using System;
using Pathea.GameLoader;
using PETools;
using UnityEngine;

namespace Pathea;

public class FastTravel : MonoBehaviour
{
	public static bool bTraveling;

	public Vector3 travelPos = Vector3.zero;

	private bool bFirstRun = true;

	private void Start()
	{
		MessageBox_N.CancelMask(MsgInfoType.DungeonGeneratingMask);
		UILoadScenceEffect.Instance.EndScence(SetToTravelPos);
	}

	public static void TravelTo(Vector3 pos)
	{
		if (pos.y > -100f && RandomDungenMgr.Instance != null && RandomDungenMgrData.InDungeon)
		{
			RandomDungenMgr.Instance.TransFromDungeonMultiMode(pos);
			RandomDungenMgr.Instance.DestroyDungeon();
		}
		FastTravel component = new GameObject("FastTravel", typeof(FastTravel)).GetComponent<FastTravel>();
		component.travelPos = pos;
		PeSingleton<FastTravelMgr>.Instance.DispatchFastTravel(pos);
	}

	public void StartLoadScene()
	{
		for (int i = 0; i < 100; i++)
		{
			PeLauncher.Instance.Add(new Dummy());
		}
		bFirstRun = true;
		PeLauncher.Instance.endLaunch = delegate
		{
			if (PeGameMgr.IsMulti && !NetworkInterface.IsClient)
			{
				return true;
			}
			if (bFirstRun)
			{
				bFirstRun = false;
				VFVoxelTerrain.TerrainVoxelComplete = false;
				return false;
			}
			if (!VFVoxelTerrain.TerrainVoxelComplete)
			{
				return false;
			}
			PeEntity entity = PeSingleton<MainPlayer>.Instance.entity;
			if (null == entity)
			{
				return false;
			}
			MotionMgrCmpt cmpt = entity.GetCmpt<MotionMgrCmpt>();
			if (cmpt == null)
			{
				return false;
			}
			Vector3 safePos;
			if (PeGameMgr.IsMulti)
			{
				if (PlayerNetwork.mainPlayer != null && PlayerNetwork.mainPlayer._curSceneId == 0)
				{
					if (!PE.FindHumanSafePos(entity.position, out safePos))
					{
						entity.position += 10f * Vector3.up;
						cmpt.FreezePhySteateForSystem(v: true);
						return false;
					}
					entity.position = safePos;
					cmpt.FreezePhySteateForSystem(v: false);
				}
			}
			else
			{
				if (!PE.FindHumanSafePos(entity.position, out safePos))
				{
					entity.position += 10f * Vector3.up;
					cmpt.FreezePhySteateForSystem(v: true);
					return false;
				}
				entity.position = safePos;
				cmpt.FreezePhySteateForSystem(v: false);
			}
			UnityEngine.Object.Destroy(base.gameObject);
			Resources.UnloadUnusedAssets();
			GC.Collect();
			return true;
		};
		PeLauncher.Instance.StartLoad();
	}

	public void SetToTravelPos()
	{
		if (travelPos == Vector3.zero)
		{
			return;
		}
		PlayerNetwork mainPlayer = PlayerNetwork.mainPlayer;
		mainPlayer.transform.position = travelPos;
		if (null != mainPlayer._move)
		{
			mainPlayer._move.NetMoveTo(travelPos, Vector3.zero, immediately: true);
		}
		if (bTraveling)
		{
			return;
		}
		bTraveling = true;
		PeEntity mainPlayer2 = PeSingleton<PeCreature>.Instance.mainPlayer;
		if (null == mainPlayer2)
		{
			Debug.LogError("no main player");
			return;
		}
		PeTrans peTrans = mainPlayer2.peTrans;
		if (!(null == peTrans))
		{
			peTrans.position = travelPos;
			StartLoadScene();
		}
	}
}
