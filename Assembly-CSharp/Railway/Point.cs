using System;
using System.Collections.Generic;
using System.IO;
using Pathea;
using PETools;
using UnityEngine;

namespace Railway;

public class Point
{
	public enum EType
	{
		Joint,
		Station,
		End
	}

	private EType mType;

	private int mId;

	private Vector3 mPosition;

	private string mName;

	private int mPrePointId;

	private int mNextPointId;

	public int routeId = -1;

	private Vector3 mRotation;

	public float stayTime;

	public int itemInstanceId;

	private RailwayStation mStation;

	public EType pointType => mType;

	public int id
	{
		get
		{
			return mId;
		}
		set
		{
			mId = value;
			station.pointId = value;
		}
	}

	public Vector3 position
	{
		get
		{
			return mPosition;
		}
		set
		{
			mPosition = value;
			station.SetPos(mPosition);
			UpdatePrePointLink();
		}
	}

	public string name
	{
		get
		{
			if (string.IsNullOrEmpty(mName))
			{
				return pointType.ToString() + id;
			}
			return mName;
		}
		set
		{
			mName = value;
		}
	}

	public int prePointId
	{
		get
		{
			return mPrePointId;
		}
		private set
		{
			mPrePointId = value;
		}
	}

	public int nextPointId
	{
		get
		{
			return mNextPointId;
		}
		private set
		{
			mNextPointId = value;
			UpdateLinkTarget();
		}
	}

	public Vector3 rotation
	{
		get
		{
			return mRotation;
		}
		set
		{
			mRotation = value;
			station.SetRot(mRotation);
			UpdatePrePointLink();
		}
	}

	public float realStayTime => stayTime;

	public RailwayStation station
	{
		get
		{
			if (mStation == null)
			{
				mStation = CreateModel(pointType);
			}
			return mStation;
		}
	}

	private Point()
	{
	}

	public Point(int identifier, EType type)
	{
		mType = type;
		id = identifier;
		prePointId = -1;
		nextPointId = -1;
		routeId = -1;
		rotation = Vector3.zero;
	}

	public void ChangePrePoint(int preId)
	{
		if (preId != prePointId)
		{
			Point prePoint = GetPrePoint();
			prePointId = preId;
			prePoint?.ChangeNextPoint(-1);
			PeSingleton<Manager>.Instance.GetPoint(preId)?.ChangeNextPoint(id);
			UpdateRotation();
		}
	}

	public void ChangeNextPoint(int nextID)
	{
		if (nextID != nextPointId)
		{
			Point nextPoint = GetNextPoint();
			nextPointId = nextID;
			nextPoint?.ChangePrePoint(-1);
			PeSingleton<Manager>.Instance.GetPoint(nextID)?.ChangePrePoint(id);
			UpdateRotation();
		}
	}

	private void UpdateRotation()
	{
		Vector3 vector = Vector3.zero;
		Point prePoint = GetPrePoint();
		Point nextPoint = GetNextPoint();
		Vector3 upwards = Quaternion.Euler(rotation) * Vector3.up;
		if (pointType != 0)
		{
			upwards = Vector3.up;
		}
		if (prePoint != null)
		{
			vector = (position - prePoint.position).normalized;
		}
		vector = ((nextPoint == null) ? (-vector) : (vector.normalized + (nextPoint.position - position).normalized).normalized);
		if (vector != Vector3.zero)
		{
			rotation = Quaternion.LookRotation(vector, upwards).eulerAngles;
		}
	}

	public void RotUpDir(float angle)
	{
		Quaternion quaternion = Quaternion.Euler(rotation);
		Vector3 axis = quaternion * Vector3.forward;
		rotation = (Quaternion.AngleAxis(angle, axis) * quaternion).eulerAngles;
	}

	private void UpdatePrePointLink()
	{
		GetPrePoint()?.station.UpdateLink();
	}

	public void UpdateLinkTarget()
	{
		RailwayStation targetStation = null;
		Point nextPoint = GetNextPoint();
		if (nextPoint != null)
		{
			targetStation = nextPoint.station;
		}
		station.LinkTo(targetStation);
	}

	public Point GetNextPoint()
	{
		return PeSingleton<Manager>.Instance.GetPoint(nextPointId);
	}

	public Point GetPrePoint()
	{
		return PeSingleton<Manager>.Instance.GetPoint(prePointId);
	}

	public Vector3 GetLinkPosition()
	{
		return station.mLinkPoint.position;
	}

	public Vector3 GetJointPosition()
	{
		return station.mJointPoint.position;
	}

	public Quaternion GetJointRotation()
	{
		if (null != station)
		{
			return station.mJointPoint.rotation;
		}
		return Quaternion.Euler(rotation);
	}

	public void Destroy()
	{
		GetPrePoint()?.ChangeNextPoint(-1);
		GetNextPoint()?.ChangePrePoint(-1);
		if (null != station)
		{
			UnityEngine.Object.Destroy(station.gameObject);
		}
	}

	private static RailwayStation CreateModel(EType type)
	{
		string text = "TaskJoint";
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(type switch
		{
			EType.Joint => "TaskJoint", 
			EType.Station => "TaskStation", 
			_ => "TaskEnd", 
		}));
		if (null == gameObject)
		{
			return null;
		}
		gameObject.transform.parent = Manager.railRoot;
		gameObject.transform.localScale = Vector3.one;
		return gameObject.GetComponent<RailwayStation>();
	}

	public Transform GetTrans()
	{
		return station.transform;
	}

	public float GetArriveTime()
	{
		return PeSingleton<Manager>.Instance.GetRoute(routeId)?.GetArriveTime(id) ?? 0f;
	}

	public byte[] Export()
	{
		return Serialize.Export(delegate(BinaryWriter w)
		{
			w.Write(itemInstanceId);
			Serialize.WriteNullableString(w, mName);
			w.Write(stayTime);
			w.Write((int)mType);
			w.Write(id);
			Serialize.WriteVector3(w, position);
			Serialize.WriteVector3(w, rotation);
			w.Write(prePointId);
			w.Write(nextPointId);
		});
	}

	public void Import(byte[] data)
	{
		Serialize.Import(data, delegate(BinaryReader r)
		{
			itemInstanceId = r.ReadInt32();
			mName = Serialize.ReadNullableString(r);
			stayTime = r.ReadSingle();
			mType = (EType)r.ReadInt32();
			id = r.ReadInt32();
			position = Serialize.ReadVector3(r);
			rotation = Serialize.ReadVector3(r);
			prePointId = r.ReadInt32();
			nextPointId = r.ReadInt32();
		});
	}

	public static Point GetHeader(Point point)
	{
		if (point == null)
		{
			return null;
		}
		while (true)
		{
			Point prePoint = point.GetPrePoint();
			if (prePoint == null)
			{
				break;
			}
			point = prePoint;
		}
		return point;
	}

	public static void Travel(Point point, Action<Point> action)
	{
		if (point == null)
		{
			return;
		}
		while (true)
		{
			action(point);
			Point nextPoint = point.GetNextPoint();
			if (nextPoint == null)
			{
				break;
			}
			point = nextPoint;
		}
	}

	public static Point GetTail(Point point)
	{
		if (point == null)
		{
			return null;
		}
		while (true)
		{
			Point nextPoint = point.GetNextPoint();
			if (nextPoint == null)
			{
				break;
			}
			point = nextPoint;
		}
		return point;
	}

	public static void ReverseNext(Point point)
	{
		if (point != null)
		{
			List<Point> list = new List<Point>();
			while (point != null)
			{
				list.Add(point);
				point = point.GetNextPoint();
			}
			list[list.Count - 1].ChangePrePoint(-1);
			for (int i = 0; i < list.Count - 1; i++)
			{
				list[i].ChangePrePoint(list[i + 1].id);
			}
		}
	}

	public static Point Deserialize(byte[] data)
	{
		Point point = new Point();
		point.Import(data);
		return point;
	}
}
