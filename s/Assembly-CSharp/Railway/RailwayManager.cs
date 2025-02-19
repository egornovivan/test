using System.Collections.Generic;
using System.IO;
using ItemAsset;
using Mono.Data.SqliteClient;
using PETools;
using UnityEngine;

namespace Railway;

public class RailwayManager : MonoBehaviour
{
	public const int Version1 = 1;

	public const int Version2 = 2;

	public const int Version3 = 3;

	public const int CurrentVersion = 3;

	public const int InvalId = -1;

	public const float JointMinDistance = 5f;

	public const float JointMaxDistance = 80f;

	private static RailwayManager mInstance;

	private bool _hasRecord;

	public static float DefaultStayTime = 60f;

	public static float TrainSteerSpeed = 1.6666666f;

	private Dictionary<int, Point> mPointDic = new Dictionary<int, Point>();

	private List<Route> mRouteList = new List<Route>();

	public int saveVersion;

	private static Transform sRailRoot;

	public static RailwayManager Instance => mInstance;

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

	private void Awake()
	{
		mInstance = this;
		_hasRecord = false;
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
			point.stayTime = DefaultStayTime / 2f;
			break;
		default:
			point.stayTime = 0f;
			break;
		}
		mPointDic[point.id] = point;
		point.ChangePrePoint(prePointId);
		return point;
	}

	public bool RemovePoint(Player player, int id)
	{
		if (player == null)
		{
			return false;
		}
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
		ItemObject itemByID = ItemManager.GetItemByID(point.itemInstanceId);
		if (itemByID != null && player.Package.GetEmptyGridCount(itemByID.protoData) > 0)
		{
			player.Package.AddItem(itemByID);
			player.SyncPackageIndex();
		}
		point.Destroy();
		return mPointDic.Remove(id);
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
		if (!Instance.IsRouteNameExist(name))
		{
			route.name = name;
		}
		route.SetPoints(pointArray);
		mRouteList.Add(route);
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
			if (mRoute.trainID == trainId)
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

	public IEnumerable<Route> GetRoutes()
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
		ItemObject itemByID = ItemManager.GetItemByID(itemInstanceId);
		if (itemByID == null)
		{
			return null;
		}
		return null;
	}

	public byte[] Export()
	{
		return Serialize.Export(delegate(BinaryWriter w)
		{
			w.Write(3);
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
		});
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
		});
	}

	protected byte[] GetData()
	{
		return Export();
	}

	protected void SetData(byte[] data)
	{
		Import(data);
	}

	public void SaveData()
	{
		byte[] data = Export();
		RailwayDbData railwayDbData = new RailwayDbData();
		railwayDbData.ExportData(1, data);
		AsyncSqlite.AddRecord(railwayDbData);
	}

	public void LoadData()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT * FROM railwaydata WHERE id = 1;");
			pEDbOp.BindReaderHandler(LoadComplete);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	public void LoadComplete(SqliteDataReader dataReader)
	{
		if (dataReader.Read())
		{
			int @int = dataReader.GetInt32(dataReader.GetOrdinal("ver"));
			byte[] array = (byte[])dataReader.GetValue(dataReader.GetOrdinal("data"));
			if (array.Length > 0)
			{
				Import(array);
				_hasRecord = true;
			}
		}
	}

	public int DoRemovePassenger(int type, int passengerID)
	{
		foreach (Route mRoute in mRouteList)
		{
			if (mRoute.RemovePassenger(passengerID))
			{
				return passengerID;
			}
		}
		return -1;
	}

	public void DoRemoveAllPassengers()
	{
		foreach (Route mRoute in mRouteList)
		{
			mRoute.RemoveAllPassengers();
		}
	}

	public bool IsPointInCompletedLine(int pointId)
	{
		Point point = GetPoint(pointId);
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
}
