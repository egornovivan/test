using System.IO;
using ItemAsset;
using PETools;
using UnityEngine;

namespace Railway;

public class Route
{
	public class Stats
	{
		public int stationNum;

		public int jointNum;

		public float totalDis;

		public int totalIntDis => (int)totalDis;
	}

	public class RunState
	{
		private float[] mScheduleForward;

		private float[] mScheduleBackward;

		public int moveDir;

		public int mNextPointIndex;

		public float timeToLeavePoint;

		private float[] currentTable => (moveDir != 1) ? mScheduleBackward : mScheduleForward;

		public double singleTripTime => mScheduleForward[mScheduleForward.Length - 1];

		public int nextPointIndex => (moveDir != 1) ? (currentTable.Length - 1 - mNextPointIndex) : mNextPointIndex;

		public float timeToLeaveCurPoint => timeToLeavePoint;

		public void SetCurrentStation(int index)
		{
			if (index >= currentTable.Length)
			{
				timeToLeavePoint = 0f;
				mNextPointIndex = 0;
				ToggleTrainDirect();
			}
			else if (index > 0)
			{
				mNextPointIndex = index - 1;
				timeToLeavePoint = 30f;
			}
			else
			{
				mNextPointIndex = 0;
				timeToLeavePoint = 30f;
			}
		}

		private void ToggleTrainDirect()
		{
			moveDir *= -1;
		}

		public void Start()
		{
			moveDir = 1;
			mNextPointIndex = 0;
			timeToLeavePoint = 0f;
		}

		public void Update(float deltaTime)
		{
			timeToLeavePoint -= deltaTime;
			if (timeToLeavePoint < 0f)
			{
				mNextPointIndex++;
				if (mNextPointIndex >= currentTable.Length)
				{
					timeToLeavePoint = 0f;
					mNextPointIndex = 0;
					ToggleTrainDirect();
				}
				else
				{
					timeToLeavePoint += currentTable[mNextPointIndex] - currentTable[mNextPointIndex - 1];
				}
			}
		}

		public void SetSchedule(float[] forwardSchedule, float[] backwardSchedule)
		{
			mScheduleForward = forwardSchedule;
			mScheduleBackward = backwardSchedule;
		}

		public float GetLeaveTime(int pointIndex)
		{
			if (moveDir == 1)
			{
				if (pointIndex >= mNextPointIndex)
				{
					return mScheduleForward[pointIndex] - mScheduleForward[mNextPointIndex] + timeToLeaveCurPoint;
				}
				return mScheduleForward[mScheduleForward.Length - 1] - mScheduleForward[mNextPointIndex] + timeToLeaveCurPoint + mScheduleBackward[mScheduleBackward.Length - 1 - pointIndex];
			}
			pointIndex = mScheduleBackward.Length - 1 - pointIndex;
			if (pointIndex >= mNextPointIndex)
			{
				return mScheduleBackward[pointIndex] - mScheduleBackward[mNextPointIndex] + timeToLeaveCurPoint;
			}
			return mScheduleBackward[mScheduleBackward.Length - 1] - mScheduleBackward[mNextPointIndex] + timeToLeaveCurPoint + mScheduleForward[mScheduleForward.Length - 1 - pointIndex];
		}

		public byte[] Export()
		{
			return Serialize.Export(delegate(BinaryWriter w)
			{
				w.Write(moveDir);
				w.Write(mNextPointIndex);
				w.Write(timeToLeavePoint);
			});
		}

		public void Import(byte[] data)
		{
			Serialize.Import(data, delegate(BinaryReader r)
			{
				moveDir = r.ReadInt32();
				mNextPointIndex = r.ReadInt32();
				timeToLeavePoint = r.ReadSingle();
			});
		}
	}

	private const float AngleLerpF = 0.0055f;

	private int mId;

	private string mName;

	private int[] mPointIdList;

	private Point[] pointList;

	private int mTrainId;

	private RunState mRunState = new RunState();

	private RailwayTrain mTrain;

	private float lastSend;

	public int id => mId;

	public string name
	{
		get
		{
			if (string.IsNullOrEmpty(mName))
			{
				return "Line:" + id;
			}
			return mName;
		}
		set
		{
			mName = value;
		}
	}

	public int pointCount
	{
		get
		{
			if (mPointIdList == null)
			{
				return 0;
			}
			return mPointIdList.Length;
		}
	}

	public int moveDir => mRunState.moveDir;

	public float TimeToLeavePoint => mRunState.timeToLeavePoint;

	public int NextPointIndex => mRunState.mNextPointIndex;

	public double singleTripTime => mRunState.singleTripTime;

	public int trainID
	{
		get
		{
			return mTrainId;
		}
		set
		{
			mTrainId = value;
			if (mTrainId == -1)
			{
				mTrain = null;
			}
			Run();
		}
	}

	public bool trainRunning => mTrainId != -1;

	public RailwayTrain train
	{
		get
		{
			if (mTrain == null)
			{
				mTrain = new RailwayTrain(ItemManager.GetItemByID(mTrainId), this);
			}
			return mTrain;
		}
	}

	private Route()
	{
	}

	public Route(int id)
	{
		mId = id;
	}

	public void SetTrainToStation(int pointID)
	{
		if (mRunState != null)
		{
			int pointIndex = GetPointIndex(pointID);
			if (pointIndex != -1)
			{
				mRunState.SetCurrentStation(pointIndex);
			}
		}
	}

	private int GetPointIndex(int pointID)
	{
		int result = -1;
		for (int i = 0; i < pointCount; i++)
		{
			if (pointID == mPointIdList[i])
			{
				result = i;
				break;
			}
		}
		return result;
	}

	public Point GetPointByIndex(int index)
	{
		if (pointList == null)
		{
			return null;
		}
		return pointList[index];
	}

	private void Run()
	{
		mRunState.Start();
	}

	public Stats GetStats()
	{
		Stats stats = new Stats();
		Point point = null;
		Point[] array = pointList;
		foreach (Point point2 in array)
		{
			if (point != null)
			{
				stats.totalDis += Vector3.Distance(point2.position, point.position);
			}
			point = point2;
			if (point2.pointType != 0)
			{
				stats.stationNum++;
			}
			else
			{
				stats.jointNum++;
			}
		}
		return stats;
	}

	public bool SetStayTime(int pointId, float time)
	{
		if (trainRunning)
		{
			return false;
		}
		Point point = RailwayManager.Instance.GetPoint(pointId);
		if (point == null)
		{
			return false;
		}
		if (point.routeId != id)
		{
			return false;
		}
		point.stayTime = time;
		UpdateTimeSchedule();
		return true;
	}

	private void UpdateTimeSchedule()
	{
		float[] array = new float[pointList.Length];
		array[0] = pointList[0].stayTime;
		for (int i = 1; i < pointList.Length; i++)
		{
			float num = Vector3.Distance(pointList[i].position, pointList[i - 1].position);
			float num2 = num / RailwayManager.TrainSteerSpeed;
			array[i] = array[i - 1] + num2 + pointList[i].stayTime;
		}
		float[] array2 = new float[pointList.Length];
		array2[0] = pointList[pointList.Length - 1].stayTime;
		int num3 = 1;
		int num4 = pointList.Length - 2;
		while (num4 >= 0)
		{
			float num5 = Vector3.Distance(pointList[num4].position, pointList[num4 + 1].position) / RailwayManager.TrainSteerSpeed;
			array2[num3] = array2[num3 - 1] + num5 + pointList[num4].stayTime;
			num4--;
			num3++;
		}
		mRunState.SetSchedule(array, array2);
	}

	public void SetPoints(int[] pointIdList)
	{
		mPointIdList = pointIdList;
		pointList = new Point[mPointIdList.Length];
		for (int i = 0; i < mPointIdList.Length; i++)
		{
			pointList[i] = RailwayManager.Instance.GetPoint(mPointIdList[i]);
			pointList[i].routeId = id;
		}
		UpdateTimeSchedule();
	}

	public void Destroy()
	{
		Point[] array = pointList;
		foreach (Point point in array)
		{
			point.routeId = -1;
		}
		if (mTrain != null)
		{
			mTrain.ClearPassenger();
			mTrain = null;
		}
	}

	public void Update(float deltaTime)
	{
		if (!trainRunning)
		{
			return;
		}
		mRunState.Update(deltaTime);
		if (Time.time - lastSend > 5f)
		{
			Player randomPlayer = Player.GetRandomPlayer();
			if (randomPlayer != null)
			{
				randomPlayer.RPCOthers(EPacketType.PT_InGame_Railway_UpdateRoute, id, moveDir, NextPointIndex, TimeToLeavePoint);
			}
			lastSend = Time.time;
		}
	}

	private int GetNextPointIndex(out double timeToLeave)
	{
		timeToLeave = mRunState.timeToLeaveCurPoint;
		return mRunState.nextPointIndex;
	}

	public float GetArriveTime(int pointId)
	{
		if (!trainRunning)
		{
			return 0f;
		}
		int num = -1;
		for (int i = 0; i < pointCount; i++)
		{
			if (pointId == mPointIdList[i])
			{
				num = i;
			}
		}
		if (num == -1)
		{
			return 0f;
		}
		return mRunState.GetLeaveTime(num) - pointList[num].stayTime;
	}

	public byte[] Export()
	{
		return Serialize.Export(delegate(BinaryWriter w)
		{
			w.Write(mId);
			Serialize.WriteNullableString(w, mName);
			w.Write(mTrainId);
			w.Write(mPointIdList.Length);
			for (int i = 0; i < mPointIdList.Length; i++)
			{
				w.Write(mPointIdList[i]);
			}
			Serialize.WriteBytes(mRunState.Export(), w);
		});
	}

	public void Import(byte[] data)
	{
		Serialize.Import(data, delegate(BinaryReader r)
		{
			mId = r.ReadInt32();
			mName = Serialize.ReadNullableString(r);
			mTrainId = r.ReadInt32();
			int num = r.ReadInt32();
			int[] array = new int[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = r.ReadInt32();
			}
			SetPoints(array);
			byte[] data2 = Serialize.ReadBytes(r);
			mRunState.Import(data2);
			Run();
		});
	}

	public bool HasPassenger()
	{
		if (train == null)
		{
			return false;
		}
		if (!train.HasPassenger())
		{
			return false;
		}
		return true;
	}

	public bool AddPassenger(int passenger)
	{
		if (train == null)
		{
			return false;
		}
		return train.AddPassenger(passenger);
	}

	public bool RemovePassenger(int passenger)
	{
		if (train == null)
		{
			return false;
		}
		return train.RemovePassenger(passenger);
	}

	public bool RemoveAllPassengers()
	{
		if (train == null)
		{
			return false;
		}
		train.ClearPassenger();
		return true;
	}

	public static Route Deserialize(byte[] data)
	{
		Route route = new Route();
		route.Import(data);
		return route;
	}
}
