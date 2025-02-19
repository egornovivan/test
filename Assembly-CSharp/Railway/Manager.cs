using System.Collections.Generic;
using System.IO;
using ItemAsset;
using Pathea;
using PeEvent;
using PETools;
using UnityEngine;

namespace Railway;

public class Manager : ArchivableSingleton<Manager>
{
	public class PointChanged : EventArg
	{
		public bool bAdd;

		public Point point;
	}

	public class RouteChanged : EventArg
	{
		public bool bAdd;

		public Route route;
	}

	public const int Version1 = 1;

	public const int Version2 = 2;

	public const int Version3 = 3;

	public const int Version4 = 4;

	public const int CurrentVersion = 4;

	public const int InvalId = -1;

	private const float RailwayRadius = 3f;

	public const float JointMinDistance = 5f;

	public const float JointMaxDistance = 80f;

	public static float DefaultStayTime = 10f * GameTime.NormalTimeSpeed;

	public static float TrainSteerSpeed = 40f / GameTime.NormalTimeSpeed;

	private Dictionary<int, Point> mPointDic = new Dictionary<int, Point>();

	private List<Route> mRouteList = new List<Route>();

	public int saveVersion;

	private Event<PointChanged> mPointChangedEventor;

	private Event<RouteChanged> mRouteChangedEventor;

	private static Transform sRailRoot = null;

	public Event<PointChanged> pointChangedEventor
	{
		get
		{
			if (mPointChangedEventor == null)
			{
				mPointChangedEventor = new Event<PointChanged>(this);
			}
			return mPointChangedEventor;
		}
	}

	public Event<RouteChanged> routeChangedEventor
	{
		get
		{
			if (mRouteChangedEventor == null)
			{
				mRouteChangedEventor = new Event<RouteChanged>(this);
			}
			return mRouteChangedEventor;
		}
	}

	public static Transform railRoot
	{
		get
		{
			if (sRailRoot == null)
			{
				sRailRoot = new GameObject("RailRoot").transform;
			}
			return sRailRoot;
		}
	}

	public void UpdateTrain(float deltaTime)
	{
		foreach (Route mRoute in mRouteList)
		{
			mRoute.Update(deltaTime);
		}
	}

	public bool IsRouteNameExist(string newName)
	{
		foreach (Route mRoute in mRouteList)
		{
			if (mRoute.name == newName)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsPointNameExist(string newName)
	{
		foreach (Point value in mPointDic.Values)
		{
			if (value.name == newName)
			{
				return true;
			}
		}
		return false;
	}

	public Point AddPoint(Vector3 pos, int prePointId, Point.EType type = Point.EType.Joint, int pointId = -1)
	{
		if (GetPoint(pointId) != null)
		{
			return null;
		}
		if (pointId == -1)
		{
			pointId = GetValidPointId();
		}
		Point point = new Point(pointId, type);
		point.position = pos;
		switch (type)
		{
		case Point.EType.Station:
			point.stayTime = DefaultStayTime;
			break;
		case Point.EType.End:
			point.stayTime = DefaultStayTime;
			break;
		default:
			point.stayTime = 0f;
			break;
		}
		mPointDic[point.id] = point;
		point.ChangePrePoint(prePointId);
		pointChangedEventor.Dispatch(new PointChanged
		{
			bAdd = true,
			point = point
		});
		return point;
	}

	public bool RemovePoint(int id)
	{
		Point point = GetPoint(id);
		if (point == null)
		{
			return false;
		}
		Route routeByPointId = GetRouteByPointId(id);
		if (routeByPointId != null)
		{
			return false;
		}
		point.Destroy();
		bool result = mPointDic.Remove(id);
		pointChangedEventor.Dispatch(new PointChanged
		{
			bAdd = false,
			point = point
		});
		return result;
	}

	public Route CreateRoute(string name, int[] pointArray, int id = -1)
	{
		if (GetRoute(id) != null)
		{
			return null;
		}
		if (id == -1)
		{
			id = GetValidRouteId();
		}
		Route route = new Route(id);
		if (!PeSingleton<Manager>.Instance.IsRouteNameExist(name))
		{
			route.name = name;
		}
		route.SetPoints(pointArray);
		mRouteList.Add(route);
		routeChangedEventor.Dispatch(new RouteChanged
		{
			bAdd = true,
			route = route
		});
		return route;
	}

	public bool RemoveRoute(int ID)
	{
		Route route = GetRoute(ID);
		if (route == null)
		{
			return false;
		}
		mRouteList.Remove(route);
		route.Destroy();
		routeChangedEventor.Dispatch(new RouteChanged
		{
			bAdd = false,
			route = route
		});
		return true;
	}

	private int GetValidPointId()
	{
		int num = 0;
		while (mPointDic.ContainsKey(++num))
		{
		}
		return num;
	}

	public Point GetPoint(int pointID)
	{
		if (mPointDic.ContainsKey(pointID))
		{
			return mPointDic[pointID];
		}
		return null;
	}

	public Route GetRouteByTrainId(int trainId)
	{
		foreach (Route mRoute in mRouteList)
		{
			if (mRoute.trainId == trainId)
			{
				return mRoute;
			}
		}
		return null;
	}

	public Route GetRouteByPointId(int pointId)
	{
		Point point = GetPoint(pointId);
		if (point == null)
		{
			return null;
		}
		return GetRoute(point.routeId);
	}

	public Point GetPoint(string pointName)
	{
		foreach (Point value in mPointDic.Values)
		{
			if (value.name == pointName)
			{
				return value;
			}
		}
		return null;
	}

	public List<Point> GetNearPoint(Point centerPoint)
	{
		List<Point> nearPoint = GetNearPoint(centerPoint.position, 80f);
		nearPoint.Remove(centerPoint);
		return nearPoint;
	}

	public List<Point> GetNearPoint(Vector3 pos, float dis = 50f)
	{
		List<Point> list = new List<Point>();
		foreach (Point value in mPointDic.Values)
		{
			if (Vector3.Distance(value.position, pos) < dis)
			{
				list.Add(value);
			}
		}
		return list;
	}

	private int GetValidRouteId()
	{
		int num = 0;
		while (GetRoute(++num) != null)
		{
		}
		return num;
	}

	public Route GetRoute(int routeId)
	{
		return mRouteList.Find((Route route) => (routeId == route.id) ? true : false);
	}

	public List<Route> GetRoutes()
	{
		return mRouteList;
	}

	public List<int> GetIsolatePoint()
	{
		List<int> list = new List<int>(5);
		foreach (KeyValuePair<int, Point> item in mPointDic)
		{
			if (item.Value.routeId == -1)
			{
				list.Add(item.Key);
			}
		}
		return list;
	}

	public Point GetStation(Vector3 pos, float range)
	{
		float num = range * range;
		foreach (Point value in mPointDic.Values)
		{
			if ((value.pointType == Point.EType.End || value.pointType == Point.EType.Station) && (value.position - pos).sqrMagnitude < num)
			{
				return value;
			}
		}
		return null;
	}

	public void GetTwoPointClosest(Vector3 origin, Vector3 dest, out Point start, out Point end, out int startIndex, out int endIndex)
	{
		start = null;
		end = null;
		startIndex = -1;
		endIndex = -1;
		if (mRouteList == null || mRouteList.Count == 0)
		{
			return;
		}
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		int num4 = 0;
		int num5 = 0;
		for (int i = 0; i < mRouteList.Count; i++)
		{
			Route route = mRouteList[i];
			if (route == null)
			{
				continue;
			}
			Point[] pointList = route.GetPointList();
			if (pointList == null)
			{
				continue;
			}
			num4 = -1;
			num5 = -1;
			num2 = -1f;
			num3 = -1f;
			for (int j = 0; j < pointList.Length; j++)
			{
				Point point = pointList[j];
				if (point != null && point.pointType != 0)
				{
					float num6 = Vector3.Distance(origin, point.position);
					if (num2 <= float.Epsilon || num6 < num2)
					{
						num2 = num6;
						num4 = j;
					}
					float num7 = Vector3.Distance(dest, point.position);
					if (num3 <= float.Epsilon || num7 < num3)
					{
						num3 = num7;
						num5 = j;
					}
				}
			}
			float num8 = num2 + num3;
			if (num < float.Epsilon || num8 < num)
			{
				num = num8;
				start = pointList[num4];
				startIndex = num4;
				end = pointList[num5];
				endIndex = num5;
			}
		}
	}

	public Point GetEndPoint(Vector3 pos, float range)
	{
		float num = range * range;
		foreach (Point value in mPointDic.Values)
		{
			if (value.pointType == Point.EType.End && (value.position - pos).sqrMagnitude < num)
			{
				return value;
			}
		}
		return null;
	}

	public Point GetAnotherEndPoint(Point endPoint)
	{
		Route route = GetRoute(endPoint.routeId);
		if (route != null)
		{
			if (route.GetPointByIndex(0) == endPoint)
			{
				return route.GetPointByIndex(route.pointCount - 1);
			}
			return route.GetPointByIndex(0);
		}
		return null;
	}

	public static RailwayTrain GetTrain(int itemInstanceId)
	{
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(itemInstanceId);
		if (itemObject == null)
		{
			return null;
		}
		Instantiate cmpt = itemObject.GetCmpt<Instantiate>();
		if (cmpt == null)
		{
			return null;
		}
		GameObject gameObject = cmpt.CreateViewGameObj(null);
		if (null == gameObject)
		{
			return null;
		}
		gameObject.transform.parent = railRoot;
		return gameObject.GetComponent<RailwayTrain>();
	}

	public static bool CheckLinkState(Vector3 linkPos1, Vector3 linkPos2, Transform trans1, Transform trans2)
	{
		int num = 6866176;
		Vector3 vector = linkPos1 - linkPos2;
		Vector3 origin = linkPos2 + 3f * Vector3.up;
		Ray ray = new Ray(origin, vector.normalized);
		RaycastHit[] array = Physics.SphereCastAll(ray, 3f, vector.magnitude, -1 & ~num, QueryTriggerInteraction.Ignore);
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit raycastHit = array2[i];
			if (Vector3.Distance(raycastHit.point, linkPos1) > 3f && Vector3.Distance(raycastHit.point, linkPos2) > 3f && !raycastHit.transform.IsChildOf(trans1) && !raycastHit.transform.IsChildOf(trans2))
			{
				return false;
			}
		}
		return true;
	}

	public static bool CheckLinkState(Point point1, Point point2)
	{
		if (point1 == null || point2 == null)
		{
			return false;
		}
		return CheckLinkState(point1.GetLinkPosition(), point2.GetLinkPosition(), point1.GetTrans(), point2.GetTrans());
	}

	private void Export(BinaryWriter w)
	{
		w.Write(4);
		w.Write(mPointDic.Count);
		foreach (Point value in mPointDic.Values)
		{
			Serialize.WriteBytes(value.Export(), w);
		}
		w.Write(mRouteList.Count);
		foreach (Route mRoute in mRouteList)
		{
			Serialize.WriteBytes(mRoute.Export(), w);
		}
		w.Write(StroyManager.m_Passengers.Count);
		foreach (KeyValuePair<int, PassengerInfo> passenger in StroyManager.m_Passengers)
		{
			Serialize.WriteBytes(StroyManager.m_Passengers[passenger.Key].Export(), w);
		}
	}

	public void Import(byte[] data)
	{
		Serialize.Import(data, delegate(BinaryReader r)
		{
			saveVersion = r.ReadInt32();
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				Point point = Point.Deserialize(Serialize.ReadBytes(r));
				mPointDic[point.id] = point;
			}
			num = r.ReadInt32();
			for (int j = 0; j < num; j++)
			{
				Route item = Route.Deserialize(Serialize.ReadBytes(r));
				mRouteList.Add(item);
			}
			if (saveVersion >= 4)
			{
				StroyManager.m_Passengers.Clear();
				int num2 = r.ReadInt32();
				for (int k = 0; k < num2; k++)
				{
					byte[] data2 = Serialize.ReadBytes(r);
					PassengerInfo passengerInfo = new PassengerInfo();
					passengerInfo.Import(data2);
					StroyManager.m_Passengers.Add(passengerInfo.npcID, passengerInfo);
				}
			}
		});
		foreach (Point value in mPointDic.Values)
		{
			value.UpdateLinkTarget();
		}
	}

	protected override void WriteData(BinaryWriter bw)
	{
		Export(bw);
	}

	protected override void SetData(byte[] data)
	{
		Import(data);
	}

	protected override string GetArchiveKey()
	{
		return "ArchiveKeyRailwaySystem";
	}
}
