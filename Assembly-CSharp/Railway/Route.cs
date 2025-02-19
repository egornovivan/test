using System.IO;
using Pathea;
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

		private int mNextPointIndex;

		public float timeToLeavePoint;

		private float[] currentTable => (moveDir != 1) ? mScheduleBackward : mScheduleForward;

		public double singleTripTime => mScheduleForward[mScheduleForward.Length - 1];

		public int nextPointIndex => (moveDir != 1) ? (currentTable.Length - 1 - mNextPointIndex) : mNextPointIndex;

		public float timeToLeaveCurPoint => timeToLeavePoint;

		private void ToggleTrainDirect()
		{
			moveDir *= -1;
		}

		public void SetCurrentStation(int index)
		{
			if (index >= currentTable.Length - 1)
			{
				mNextPointIndex = 0;
				timeToLeavePoint = 30f;
				ToggleTrainDirect();
			}
			else
			{
				mNextPointIndex = index;
				timeToLeavePoint = 30f;
			}
		}

		public void Reset()
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

		public void SyncRunState(int direction, int nextIndex, float timeToLeave)
		{
			if (moveDir != direction || Mathf.Abs(nextIndex - mNextPointIndex) > 1 || Mathf.Abs(timeToLeave - timeToLeavePoint) > 5f)
			{
				moveDir = direction;
				mNextPointIndex = nextIndex;
				timeToLeavePoint = timeToLeave;
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

	private const float AngleLerpF = 0.025f;

	private int mId;

	private string mName;

	private int[] mPointIdList;

	private Point[] pointList;

	private int mTrainId = -1;

	public RunState mRunState = new RunState();

	private RailwayTrain mTrain;

	private float mLerpF;

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

	public int moveDir
	{
		get
		{
			return mRunState.moveDir;
		}
		set
		{
			mRunState.moveDir = value;
		}
	}

	public float TimeToLeavePoint
	{
		get
		{
			return mRunState.timeToLeavePoint;
		}
		set
		{
			mRunState.timeToLeavePoint = value;
		}
	}

	public double singleTripTime => mRunState.singleTripTime;

	public int trainId => mTrainId;

	public Point stayStation { get; set; }

	public bool trainRunning => mTrainId != -1;

	public RailwayTrain train => mTrain;

	private Route()
	{
	}

	public Route(int id)
	{
		mId = id;
	}

	public Point GetPointByIndex(int index)
	{
		if (pointList == null || pointList.Length <= index)
		{
			return null;
		}
		return pointList[index];
	}

	public Point[] GetPointList()
	{
		return pointList;
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

	public void SetTrain(int trainItemId)
	{
		DestroyTrain();
		mTrainId = trainItemId;
		if (mTrainId != -1)
		{
			mTrain = Manager.GetTrain(mTrainId);
			if (null != mTrain)
			{
				mTrain.mRoute = this;
			}
			else
			{
				mTrainId = -1;
			}
		}
		Reset();
	}

	private void Reset()
	{
		mRunState.Reset();
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
		Point point = PeSingleton<Manager>.Instance.GetPoint(pointId);
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
			float num2 = num / Manager.TrainSteerSpeed;
			array[i] = array[i - 1] + num2 + pointList[i].stayTime;
		}
		float[] array2 = new float[pointList.Length];
		array2[0] = pointList[pointList.Length - 1].stayTime;
		int num3 = 1;
		int num4 = pointList.Length - 2;
		while (num4 >= 0)
		{
			float num5 = Vector3.Distance(pointList[num4].position, pointList[num4 + 1].position) / Manager.TrainSteerSpeed;
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
			pointList[i] = PeSingleton<Manager>.Instance.GetPoint(mPointIdList[i]);
			pointList[i].routeId = id;
		}
		UpdateTimeSchedule();
	}

	private void DestroyTrain()
	{
		if (!(null == mTrain))
		{
			mTrain.ClearPassenger();
			Object.Destroy(mTrain.gameObject);
			mTrain = null;
		}
	}

	public void Destroy()
	{
		Point[] array = pointList;
		foreach (Point point in array)
		{
			point.routeId = -1;
		}
		DestroyTrain();
	}

	public void Update(float deltaTime)
	{
		if (trainRunning)
		{
			mRunState.Update(deltaTime);
			UpdateTrain();
		}
	}

	private void UpdateTrain()
	{
		if (!(train == null))
		{
			Vector3 up;
			Vector3 forward;
			Vector3 trainPosition = GetTrainPosition(out forward, out up);
			train.transform.position = trainPosition;
			Vector3 vector = mTrain.transform.rotation * Vector3.forward;
			float b = Mathf.Min(Vector3.Angle(vector, forward) * 0.025f, 1f);
			mLerpF = Mathf.Lerp(mLerpF, b, 4f * Time.deltaTime);
			forward = Vector3.Lerp(vector, forward, mLerpF);
			train.transform.rotation = Quaternion.LookRotation(forward, up);
		}
	}

	private int GetNextPointIndex(out double timeToLeave)
	{
		timeToLeave = mRunState.timeToLeaveCurPoint;
		return mRunState.nextPointIndex;
	}

	public Vector3 GetTrainPosition(out Vector3 forward, out Vector3 up)
	{
		double timeToLeave;
		int nextPointIndex = GetNextPointIndex(out timeToLeave);
		Point point = PeSingleton<Manager>.Instance.GetPoint(pointList[nextPointIndex].id);
		if (pointList[nextPointIndex].pointType != 0 && timeToLeave <= (double)(pointList[nextPointIndex].stayTime + float.Epsilon))
		{
			forward = point.GetJointRotation() * Vector3.forward;
			if (nextPointIndex == pointCount - 1)
			{
				forward = -1f * forward;
			}
			up = Quaternion.Euler(point.rotation) * Vector3.up;
			stayStation = PeSingleton<Manager>.Instance.GetPoint(pointList[nextPointIndex].id);
			return stayStation.GetJointPosition();
		}
		stayStation = null;
		float num = (float)timeToLeave;
		if (pointList[nextPointIndex].pointType != 0)
		{
			num -= pointList[nextPointIndex].stayTime;
		}
		Vector3 jointPosition = point.GetJointPosition();
		Point point2 = PeSingleton<Manager>.Instance.GetPoint(pointList[nextPointIndex - moveDir].id);
		Vector3 jointPosition2 = point2.GetJointPosition();
		forward = moveDir * (jointPosition - jointPosition2).normalized;
		up = Vector3.Lerp(Quaternion.Euler(point.rotation) * Vector3.up, Quaternion.Euler(point2.rotation) * Vector3.up, num * Manager.TrainSteerSpeed / (jointPosition - jointPosition2).magnitude);
		return jointPosition + (jointPosition2 - jointPosition).normalized * Manager.TrainSteerSpeed * num;
	}

	public float GetArriveTime(int pointId)
	{
		if (!trainRunning)
		{
			return 0f;
		}
		int pointIndex = GetPointIndex(pointId);
		if (pointIndex == -1)
		{
			return 0f;
		}
		return mRunState.GetLeaveTime(pointIndex) - pointList[pointIndex].stayTime;
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
			int num = r.ReadInt32();
			SetTrain(num);
			int num2 = r.ReadInt32();
			int[] array = new int[num2];
			for (int i = 0; i < num2; i++)
			{
				array[i] = r.ReadInt32();
			}
			SetPoints(array);
			byte[] data2 = Serialize.ReadBytes(r);
			mRunState.Import(data2);
			Reset();
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

	public bool AddPassenger(IPassenger passenger)
	{
		if (train == null)
		{
			return false;
		}
		return train.AddPassenger(passenger);
	}

	public bool RemovePassenger(IPassenger passenger)
	{
		if (train == null)
		{
			return false;
		}
		return train.RemovePassenger(passenger);
	}

	public bool RemovePassenger(IPassenger passenger, Vector3 getOffPos)
	{
		if (train == null)
		{
			return false;
		}
		return train.RemovePassenger(passenger, getOffPos);
	}

	public static Route Deserialize(byte[] data)
	{
		Route route = new Route();
		route.Import(data);
		return route;
	}
}
