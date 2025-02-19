using System.Collections.Generic;
using ItemAsset;
using Pathea;
using Railway;
using UnityEngine;

public class TestRailway : MonoBehaviour
{
	public bool m_AddRoute;

	public bool m_SetTrain;

	public bool m_PlayerGetOn;

	public bool m_PlayerGetOff;

	public bool m_CheckClosetRoute;

	public Vector3 targetPos = new Vector3(10000f, 150f, 10000f);

	private void Start()
	{
	}

	private void SetTrain()
	{
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.CreateItem(1031);
		using List<Route>.Enumerator enumerator = PeSingleton<Manager>.Instance.GetRoutes().GetEnumerator();
		if (enumerator.MoveNext())
		{
			Route current = enumerator.Current;
			PeSingleton<RailwayOperate>.Instance.RequestSetRouteTrain(current.id, itemObject.instanceId);
		}
	}

	private void AddRoute()
	{
		PeSingleton<RailwayOperate>.Instance.DoCreateRoute(1, "zhujiangbo route");
	}

	private void AddTrainObjToPkg()
	{
		PlayerPackageCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
		if (!(null == cmpt))
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.CreateItem(1031);
			cmpt.package.AddItem(itemObject);
		}
	}

	private void PlayerGetOn()
	{
		foreach (Route route in PeSingleton<Manager>.Instance.GetRoutes())
		{
			PeSingleton<RailwayOperate>.Instance.RequestGetOnTrain(route.id, PeSingleton<PeCreature>.Instance.mainPlayerId);
		}
	}

	private void PlayerGetOff()
	{
		PassengerCmpt cmpt = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PassengerCmpt>();
		PeSingleton<RailwayOperate>.Instance.RequestGetOffTrain(cmpt.railRouteId, PeSingleton<PeCreature>.Instance.mainPlayerId, base.transform.position);
	}

	private void GetClosetRailway()
	{
		PeSingleton<Manager>.Instance.GetTwoPointClosest(PeSingleton<MainPlayer>.Instance.entity.position, targetPos, out var start, out var end, out var _, out var _);
		if (start != null && end != null)
		{
			Debug.LogError("ClosetPoint start:" + start.name + "\nEnd:" + end.name);
		}
	}

	private void Update()
	{
		if (m_AddRoute)
		{
			m_AddRoute = false;
			AddRoute();
		}
		if (m_SetTrain)
		{
			m_SetTrain = false;
			SetTrain();
		}
		if (m_PlayerGetOn)
		{
			m_PlayerGetOn = false;
			PlayerGetOn();
		}
		if (m_PlayerGetOff)
		{
			m_PlayerGetOff = false;
			PlayerGetOff();
		}
		if (m_CheckClosetRoute)
		{
			m_CheckClosetRoute = false;
			GetClosetRailway();
		}
	}
}
