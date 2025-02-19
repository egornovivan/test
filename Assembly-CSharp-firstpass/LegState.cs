using System.Collections.Generic;
using UnityEngine;

public class LegState
{
	public Vector3 stepFromPosition;

	public Vector3 stepToPosition;

	public Vector3 stepToPositionGoal;

	public Matrix4x4 stepFromMatrix;

	public Matrix4x4 stepToMatrix;

	public float stepFromTime;

	public float stepToTime;

	public int stepNr;

	public float cycleTime = 1f;

	public float designatedCycleTimePrev = 0.9f;

	public Vector3 hipReference;

	public Vector3 ankleReference;

	public Vector3 footBase;

	public Quaternion footBaseRotation;

	public Vector3 ankle;

	public float stanceTime;

	public float liftTime = 0.1f;

	public float liftoffTime = 0.2f;

	public float postliftTime = 0.3f;

	public float prelandTime = 0.7f;

	public float strikeTime = 0.8f;

	public float landTime = 0.9f;

	public LegCyclePhase phase;

	public bool parked;

	public Vector3 stancePosition;

	public Vector3 heelToetipVector;

	public List<string> debugHistory = new List<string>();

	public float GetFootGrounding(float time)
	{
		if (time <= liftTime || time >= landTime)
		{
			return 0f;
		}
		if (time >= postliftTime && time <= prelandTime)
		{
			return 1f;
		}
		if (time < postliftTime)
		{
			return (time - liftTime) / (postliftTime - liftTime);
		}
		return 1f - (time - prelandTime) / (landTime - prelandTime);
	}
}
