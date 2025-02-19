using System.Collections.Generic;
using ItemAsset;
using Pathea;
using Railway;
using UnityEngine;

public class PERailwayCtrl : MonoBehaviour
{
	private static PERailwayCtrl mInstance;

	private bool mInit;

	public static PERailwayCtrl Instance => mInstance;

	private UIMonoRailCtrl railwayView => UIMonoRailCtrl.Instance;

	private void Awake()
	{
		mInstance = this;
	}

	private void Update()
	{
		if (!mInit && railwayView != null)
		{
			Init();
		}
	}

	private void Init()
	{
		if (railwayView.isShow)
		{
			UpdateRailway();
			railwayView.e_BtnStop += OnDeleteRoute;
			railwayView.e_BtnStar += OnCreateRoute;
			railwayView.e_SetTrain += OnSetRouteTrain;
			railwayView.e_SetTrainToStation += OnSetTrainToStation;
			railwayView.e_ResetPointName += OnResetPointName;
			railwayView.e_ResetRouteName += OnResetRouteName;
			railwayView.e_ResetPointTime += OnResetPointTime;
			PeSingleton<Manager>.Instance.pointChangedEventor.Subscribe(PointChange);
			PeSingleton<Manager>.Instance.routeChangedEventor.Subscribe(RouteChanged);
			mInit = true;
		}
	}

	private void RouteChanged(object sender, Manager.RouteChanged routeEventor)
	{
		if (routeEventor.bAdd)
		{
			railwayView.AddMonoRail(routeEventor.route);
		}
		else
		{
			railwayView.RemoveMonoRail(routeEventor.route);
		}
		UpdateUnActiveLinks();
		railwayView.UpdateSelectedStation();
	}

	private void PointChange(object sender, Manager.PointChanged pointEventor)
	{
		UpdateUnActiveLinks();
	}

	private void UpdateRailway()
	{
		foreach (Route route in PeSingleton<Manager>.Instance.GetRoutes())
		{
			if ((bool)railwayView)
			{
				railwayView.AddMonoRail(route);
			}
		}
		UpdateUnActiveLinks();
	}

	private void OnDeleteRoute(Point point)
	{
		Route route = PeSingleton<Manager>.Instance.GetRoute(point.routeId);
		if (route != null)
		{
			if (route.HasPassenger())
			{
				MessageBox_N.ShowOkBox(UIMsgBoxInfo.RailwayDeleteNotice.GetString());
			}
			else
			{
				PeSingleton<RailwayOperate>.Instance.RequestDeleteRoute(point.routeId);
			}
		}
	}

	public void OnSetRouteTrain(int routeId, ItemObject trainItem)
	{
		if (trainItem != null)
		{
			PeSingleton<RailwayOperate>.Instance.RequestSetRouteTrain(routeId, trainItem.instanceId);
		}
	}

	public void OnSetTrainToStation(int routeId, int pointId)
	{
		PeSingleton<RailwayOperate>.Instance.RequestSetTrainToStation(routeId, pointId);
	}

	public void RemoveTrain(int packageIndex)
	{
		Point selPoint = railwayView.GetSelPoint();
		if (selPoint != null)
		{
			PeSingleton<Manager>.Instance.GetRoute(selPoint.routeId);
		}
	}

	public void OnResetPointName(int pointID, string name)
	{
		PeSingleton<RailwayOperate>.Instance.RequestSetPointName(pointID, name);
	}

	public void OnResetRouteName(int routeID, string name)
	{
		PeSingleton<RailwayOperate>.Instance.RequestSetRouteName(routeID, name);
	}

	public void OnResetPointTime(int pointID, float time)
	{
		PeSingleton<RailwayOperate>.Instance.RequestSetPointStayTime(pointID, time);
	}

	private void UpdateUnActiveLinks()
	{
		List<List<Point>> list = new List<List<Point>>();
		List<Point> list2 = new List<Point>();
		List<int> isolatePoint = PeSingleton<Manager>.Instance.GetIsolatePoint();
		foreach (int item in isolatePoint)
		{
			list2.Add(PeSingleton<Manager>.Instance.GetPoint(item));
		}
		while (list2.Count > 0)
		{
			List<Point> list3 = new List<Point>();
			Point point = list2[0];
			list3.Add(point);
			list2.Remove(point);
			Point point2 = point;
			while (point2.prePointId != -1)
			{
				point2 = PeSingleton<Manager>.Instance.GetPoint(point2.prePointId);
				if (point2.id == point.id)
				{
					break;
				}
				if (list2.Remove(point2))
				{
					list3.Insert(0, point2);
				}
			}
			point2 = point;
			while (point2.nextPointId != -1)
			{
				point2 = PeSingleton<Manager>.Instance.GetPoint(point2.nextPointId);
				if (point2.id == point.id)
				{
					break;
				}
				if (list2.Remove(point2))
				{
					list3.Add(point2);
				}
			}
			list.Add(list3);
		}
		if (railwayView != null)
		{
			railwayView.ReDrawDisRailLine(list);
		}
	}

	public static bool CheckRoute(Point point)
	{
		if (point.routeId != -1)
		{
			return false;
		}
		if (point.pointType == Point.EType.End)
		{
			Point point2 = point;
			while (point2.prePointId != -1)
			{
				point2 = PeSingleton<Manager>.Instance.GetPoint(point2.prePointId);
				if (point2 == point)
				{
					return false;
				}
				if (point2.pointType == Point.EType.End)
				{
					return true;
				}
			}
			point2 = point;
			while (point2.nextPointId != -1)
			{
				point2 = PeSingleton<Manager>.Instance.GetPoint(point2.nextPointId);
				if (point2 == point)
				{
					return false;
				}
				if (point2.pointType == Point.EType.End)
				{
					return true;
				}
			}
			return false;
		}
		if (point.prePointId == -1 || point.nextPointId == -1)
		{
			return false;
		}
		Point point3 = null;
		Point point4 = null;
		Point point5 = point;
		while (point5.prePointId != -1)
		{
			point5 = PeSingleton<Manager>.Instance.GetPoint(point5.prePointId);
			if (point5 == point)
			{
				return false;
			}
			if (point5.pointType == Point.EType.End)
			{
				point3 = point5;
				break;
			}
		}
		point5 = point;
		while (point5.nextPointId != -1)
		{
			point5 = PeSingleton<Manager>.Instance.GetPoint(point5.nextPointId);
			if (point5 == point)
			{
				return false;
			}
			if (point5.pointType == Point.EType.End)
			{
				point4 = point5;
				break;
			}
		}
		return point3 != null && null != point4;
	}

	public void OnCreateRoute(Point point, string routeName)
	{
		PeSingleton<RailwayOperate>.Instance.RequestCreateRoute(point.id, routeName);
	}

	public static bool HasRoute(Vector3 pos1, Vector3 pos2)
	{
		List<Point> nearPoint = PeSingleton<Manager>.Instance.GetNearPoint(pos1, 150f);
		List<Point> nearPoint2 = PeSingleton<Manager>.Instance.GetNearPoint(pos2, 150f);
		foreach (Point item in nearPoint)
		{
			foreach (Point item2 in nearPoint2)
			{
				if (item.pointType != 0 && item2.pointType != 0 && item.routeId == item2.routeId && item.routeId != -1)
				{
					Route route = PeSingleton<Manager>.Instance.GetRoute(item.routeId);
					if (route != null)
					{
						return route.trainId != -1;
					}
				}
			}
		}
		return false;
	}

	public static void RemoveTrain(ItemObject trainItem)
	{
	}
}
