using System;
using System.Collections.Generic;
using System.IO;
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

	public int routeId;

	private Vector3 mRotation;

	public float stayTime;

	public int itemInstanceId;

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
		}
	}

	public float realStayTime
	{
		get
		{
			if (pointType == EType.End)
			{
				return 2f * stayTime;
			}
			return stayTime;
		}
	}

	public Point()
	{
		prePointId = -1;
		nextPointId = -1;
		routeId = -1;
		rotation = Vector3.zero;
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
			RailwayManager.Instance.GetPoint(preId)?.ChangeNextPoint(id);
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
			RailwayManager.Instance.GetPoint(nextID)?.ChangePrePoint(id);
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

	public Point GetNextPoint()
	{
		return RailwayManager.Instance.GetPoint(nextPointId);
	}

	public Point GetPrePoint()
	{
		return RailwayManager.Instance.GetPoint(prePointId);
	}

	public void Destroy()
	{
		GetPrePoint()?.ChangeNextPoint(-1);
		GetNextPoint()?.ChangePrePoint(-1);
	}

	public float GetArriveTime()
	{
		return RailwayManager.Instance.GetRoute(routeId)?.GetArriveTime(id) ?? 0f;
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
			w.Write(position.x);
			w.Write(position.y);
			w.Write(position.z);
			w.Write(rotation.x);
			w.Write(rotation.y);
			w.Write(rotation.z);
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
			position = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
			rotation = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
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

	public static void ReverseNext(Point findPoint)
	{
		if (findPoint != null)
		{
			List<Point> list = new List<Point>();
			while (findPoint != null)
			{
				list.Add(findPoint);
				findPoint = findPoint.GetNextPoint();
			}
			list[list.Count - 1].ChangePrePoint(-1);
			for (int i = 0; i < list.Count - 1; i++)
			{
				list[i].ChangePrePoint(list[i + 1].id);
			}
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

	public static Point Deserialize(byte[] data)
	{
		Point point = new Point();
		point.Import(data);
		return point;
	}
}
