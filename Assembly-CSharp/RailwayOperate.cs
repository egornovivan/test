using System.Collections.Generic;
using ItemAsset;
using Pathea;
using Railway;
using UnityEngine;

public class RailwayOperate : PeSingleton<RailwayOperate>
{
	private static PackageCmpt mPkg;

	private static PackageCmpt pkg
	{
		get
		{
			if (null == mPkg)
			{
				mPkg = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PackageCmpt>();
			}
			return mPkg;
		}
	}

	private static bool AddItemToPlayerPkg(int instanceId)
	{
		if (null == pkg)
		{
			return false;
		}
		if (PlayerPackageCmpt.LockStackCount)
		{
			PeSingleton<ItemMgr>.Instance.DestroyItem(instanceId);
		}
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(instanceId);
		if (itemObject == null)
		{
			return false;
		}
		return pkg.Add(itemObject);
	}

	private static void RemoveItemFromPlayerPkg(int itemId)
	{
		if (null == pkg)
		{
			return;
		}
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(itemId);
		if (itemObject == null)
		{
			return;
		}
		pkg.Remove(itemObject);
		if (PlayerPackageCmpt.LockStackCount)
		{
			PlayerPackageCmpt playerPackageCmpt = pkg as PlayerPackageCmpt;
			if (playerPackageCmpt != null)
			{
				playerPackageCmpt.package.Add(itemObject.protoId, 1);
			}
		}
	}

	public void RequestAddPoint(Vector3 pos, Point.EType type, int prePointId, int itemObjId)
	{
		RemoveItemFromPlayerPkg(itemObjId);
		Point point = DoAddPoint(pos, type, prePointId);
		if (point != null)
		{
			point.itemInstanceId = itemObjId;
		}
	}

	public Point DoAddPoint(Vector3 pos, Point.EType type, int prePointId, int pointId = -1)
	{
		Point point = PeSingleton<Manager>.Instance.GetPoint(prePointId);
		if (point != null && point.prePointId == -1 && point.nextPointId != -1)
		{
			Point.ReverseNext(point);
		}
		return PeSingleton<Manager>.Instance.AddPoint(pos, prePointId, type, pointId);
	}

	public void RequestRemovePoint(int pointID)
	{
		if (GameConfig.IsMultiMode)
		{
			if (null != PlayerNetwork.mainPlayer)
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_Recycle, pointID);
			}
		}
		else
		{
			Point point = PeSingleton<Manager>.Instance.GetPoint(pointID);
			if (point != null)
			{
				AddItemToPlayerPkg(point.itemInstanceId);
				DoRemovePoint(pointID);
			}
		}
	}

	public void DoRemovePoint(int pointID)
	{
		PeSingleton<Manager>.Instance.RemovePoint(pointID);
	}

	public void RequestChangePrePoint(int pointId, int preID)
	{
		if (GameConfig.IsMultiMode)
		{
			if (null != PlayerNetwork.mainPlayer)
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_PrePointChange, pointId, preID);
			}
		}
		else
		{
			DoChangePrePoint(pointId, preID);
		}
	}

	public void DoChangePrePoint(int pointId, int preID)
	{
		Point point = PeSingleton<Manager>.Instance.GetPoint(pointId);
		Point point2 = PeSingleton<Manager>.Instance.GetPoint(preID);
		if (point2 != null && point2.prePointId == -1 && point2.nextPointId != -1)
		{
			List<Point> list = new List<Point>();
			list.Add(point);
			list.Insert(0, point2);
			while (point2.nextPointId != -1)
			{
				point2 = PeSingleton<Manager>.Instance.GetPoint(point2.nextPointId);
				list.Insert(0, point2);
			}
			list[0].ChangePrePoint(-1);
			for (int i = 0; i < list.Count - 1; i++)
			{
				list[i].ChangeNextPoint(list[i + 1].id);
			}
		}
		else
		{
			point.ChangePrePoint(preID);
		}
		if (PeSingleton<Manager>.Instance.pointChangedEventor != null)
		{
			PeSingleton<Manager>.Instance.pointChangedEventor.Dispatch(new Manager.PointChanged
			{
				bAdd = false,
				point = point
			});
		}
	}

	public void RequestChangeNextPoint(Point point, int nextID)
	{
		if (GameConfig.IsMultiMode)
		{
			if (null != PlayerNetwork.mainPlayer)
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_NextPointChange, point.id, nextID);
			}
		}
		else
		{
			DoChangeNextPoint(point, nextID);
		}
	}

	public void DoChangeNextPoint(Point point, int nextID)
	{
		Point point2 = PeSingleton<Manager>.Instance.GetPoint(nextID);
		if (point2 != null && point2.prePointId != -1 && point2.nextPointId == -1)
		{
			List<Point> list = new List<Point>();
			list.Add(point);
			list.Add(point2);
			while (point2.prePointId != -1)
			{
				point2 = PeSingleton<Manager>.Instance.GetPoint(point2.prePointId);
				list.Add(point2);
			}
			for (int i = 0; i < list.Count - 1; i++)
			{
				list[i].ChangeNextPoint(list[i + 1].id);
			}
			list[list.Count - 1].ChangeNextPoint(-1);
		}
		else
		{
			point.ChangeNextPoint(nextID);
		}
		if (PeSingleton<Manager>.Instance.pointChangedEventor != null)
		{
			PeSingleton<Manager>.Instance.pointChangedEventor.Dispatch(new Manager.PointChanged
			{
				bAdd = false,
				point = point
			});
		}
	}

	public void RequestCreateRoute(int pointId, string routeName)
	{
		if (GameConfig.IsMultiMode)
		{
			if (null != PlayerNetwork.mainPlayer)
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_Route, pointId, routeName);
			}
		}
		else
		{
			DoCreateRoute(pointId, routeName);
		}
	}

	public bool IsPointInCompletedLine(int pointId)
	{
		Point point = PeSingleton<Manager>.Instance.GetPoint(pointId);
		if (point == null)
		{
			return false;
		}
		Point header = Point.GetHeader(point);
		if (header == null)
		{
			return false;
		}
		if (header.pointType != Point.EType.End)
		{
			return false;
		}
		Point tail = Point.GetTail(point);
		if (tail == null)
		{
			return false;
		}
		if (tail.pointType != Point.EType.End)
		{
			return false;
		}
		bool ret = true;
		Point.Travel(header, delegate(Point p)
		{
			if (p != header && p != tail && p.pointType == Point.EType.End)
			{
				ret = false;
			}
			if (p.routeId != -1)
			{
				ret = false;
			}
		});
		return ret;
	}

	public Route DoCreateRoute(int pointId, string routeName)
	{
		if (!IsPointInCompletedLine(pointId))
		{
			return null;
		}
		Point point = PeSingleton<Manager>.Instance.GetPoint(pointId);
		if (point == null)
		{
			return null;
		}
		Point header = Point.GetHeader(point);
		if (header.pointType != Point.EType.End)
		{
			return null;
		}
		List<int> list = new List<int>();
		for (point = header; point != null; point = point.GetNextPoint())
		{
			list.Add(point.id);
		}
		return PeSingleton<Manager>.Instance.CreateRoute(routeName, list.ToArray());
	}

	public void RequestDeleteRoute(int routeId)
	{
		Route route = PeSingleton<Manager>.Instance.GetRoute(routeId);
		if (route == null)
		{
			return;
		}
		if (GameConfig.IsMultiMode)
		{
			if (null != PlayerNetwork.mainPlayer)
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_DeleteRoute, routeId);
			}
		}
		else
		{
			AddItemToPlayerPkg(route.trainId);
			DoDeleteRoute(routeId);
		}
	}

	public bool DoDeleteRoute(int routeId)
	{
		Route route = PeSingleton<Manager>.Instance.GetRoute(routeId);
		if (route == null)
		{
			return false;
		}
		route.SetTrain(-1);
		return PeSingleton<Manager>.Instance.RemoveRoute(route.id);
	}

	public bool DoGetOnTrain(int entityId, int routeId, bool checkState = true)
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(entityId);
		if (null == peEntity)
		{
			Debug.LogError("cant find entity:" + entityId);
			return false;
		}
		PassengerCmpt cmpt = peEntity.GetCmpt<PassengerCmpt>();
		if (null == cmpt)
		{
			Debug.LogError("no Pathea.RailPassengerCmpt");
			return false;
		}
		return cmpt.GetOn(routeId, checkState);
	}

	public bool RequestGetOnTrain(int routeId, int entityId)
	{
		if (!GameConfig.IsMultiMode)
		{
			return DoGetOnTrain(entityId, routeId);
		}
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(entityId);
		if (null == peEntity)
		{
			Debug.LogError("cant find entity:" + entityId);
			return false;
		}
		MotionMgrCmpt cmpt = peEntity.GetCmpt<MotionMgrCmpt>();
		if (null == cmpt)
		{
			Debug.LogError("no Pathea.RailPassengerCmpt");
			return false;
		}
		if (cmpt.CanDoAction(PEActionType.GetOnTrain) && null != PlayerNetwork.mainPlayer)
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_GetOnTrain, routeId, entityId);
		}
		return true;
	}

	public bool DoGetOffTrain(int routeId, int entityId, Vector3 pos)
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(entityId);
		if (null == peEntity)
		{
			Debug.LogError("cant find entity:" + entityId);
			return false;
		}
		PassengerCmpt cmpt = peEntity.GetCmpt<PassengerCmpt>();
		if (null == cmpt)
		{
			Debug.LogError("no Pathea.RailPassengerCmpt");
			return false;
		}
		if (cmpt.railRouteId != routeId)
		{
			return false;
		}
		return cmpt.GetOff(pos);
	}

	public void RequestGetOffTrain(int routeId, int passengerID, Vector3 pos)
	{
		if (!GameConfig.IsMultiMode)
		{
			DoGetOffTrain(routeId, passengerID, pos);
		}
		else if (null != PlayerNetwork.mainPlayer)
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_GetOffTrain, routeId, passengerID, pos);
		}
	}

	public void RequestSetRouteTrain(int routeId, int itemObjId)
	{
		if (!GameConfig.IsMultiMode)
		{
			if (DoSetRouteTrain(routeId, itemObjId))
			{
				RemoveItemFromPlayerPkg(itemObjId);
			}
		}
		else if (null != PlayerNetwork.mainPlayer)
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_SetRouteTrain, routeId, itemObjId);
		}
	}

	public void RequestSetTrainToStation(int routeId, int pointId)
	{
		if (!GameConfig.IsMultiMode)
		{
			DoSetTrainToStation(routeId, pointId);
		}
	}

	public void RequestAutoCreateRoute(int pointID, int itemObjID)
	{
		if (!GameConfig.IsMultiMode)
		{
			DoAutoCreateRoute(pointID, itemObjID);
		}
		else if (null != PlayerNetwork.mainPlayer)
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_AutoCreateRoute, pointID, itemObjID);
		}
	}

	public void DoAutoCreateRoute(int pointID, int itemObjID)
	{
		Point point = PeSingleton<Manager>.Instance.GetPoint(pointID);
		DoCreateRoute(pointID, string.Empty);
		DoSetRouteTrain(point.routeId, itemObjID);
		if (DoSetRouteTrain(point.routeId, itemObjID))
		{
			RemoveItemFromPlayerPkg(itemObjID);
		}
		DoSetTrainToStation(point.routeId, pointID);
	}

	public void DoSetTrainToStation(int routeId, int pointId)
	{
		PeSingleton<Manager>.Instance.GetRoute(routeId)?.SetTrainToStation(pointId);
	}

	public bool DoSetRouteTrain(int routeId, int trainItemObjId)
	{
		Route route = PeSingleton<Manager>.Instance.GetRoute(routeId);
		if (route == null)
		{
			return false;
		}
		route.SetTrain(trainItemObjId);
		return true;
	}

	public void RequestChangePointRot(int pointID, Vector3 rot)
	{
		if (GameConfig.IsMultiMode)
		{
			if (null != PlayerNetwork.mainPlayer)
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_ChangeStationRot, pointID, rot);
			}
		}
		else
		{
			DoChangePointRot(pointID, rot);
		}
	}

	public void DoChangePointRot(int pointID, Vector3 rot)
	{
		Point point = PeSingleton<Manager>.Instance.GetPoint(pointID);
		if (point != null)
		{
			point.rotation = rot;
		}
	}

	public void DoRemovePassenger(int type, int passengerID)
	{
	}

	public void RequestSetPointName(int pointID, string name)
	{
		if (GameConfig.IsMultiMode)
		{
			if (null != PlayerNetwork.mainPlayer)
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_ResetPointName, pointID, name);
			}
		}
		else
		{
			DoResetPointName(pointID, name);
		}
	}

	public void DoResetPointName(int pointID, string name)
	{
		Point point = PeSingleton<Manager>.Instance.GetPoint(pointID);
		if (point != null)
		{
			point.name = name;
		}
	}

	public void RequestSetRouteName(int routeID, string name)
	{
		if (GameConfig.IsMultiMode)
		{
			if (null != PlayerNetwork.mainPlayer)
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_ResetRouteName, routeID, name);
			}
		}
		else
		{
			DoResetRouteName(routeID, name);
		}
	}

	public void DoResetRouteName(int routeID, string name)
	{
		Route route = PeSingleton<Manager>.Instance.GetRoute(routeID);
		if (route != null)
		{
			route.name = name;
		}
	}

	public void RequestSetPointStayTime(int pointID, float time)
	{
		if (GameConfig.IsMultiMode)
		{
			if (null != PlayerNetwork.mainPlayer)
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Railway_ResetPointTime, pointID, time);
			}
		}
		else
		{
			DoResetPointTime(pointID, time);
		}
	}

	public void DoResetPointTime(int pointID, float time)
	{
		Point point = PeSingleton<Manager>.Instance.GetPoint(pointID);
		if (point != null)
		{
			point.stayTime = time;
		}
	}

	public void DoSyncRunState(int routeId, int moveDir, int nextPoint, float time)
	{
		PeSingleton<Manager>.Instance.GetRoute(routeId)?.mRunState.SyncRunState(moveDir, nextPoint, time);
	}
}
