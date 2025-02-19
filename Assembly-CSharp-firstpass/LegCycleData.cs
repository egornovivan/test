using System;
using UnityEngine;

[Serializable]
public class LegCycleData
{
	public Vector3 cycleCenter;

	public float cycleScaling;

	public Vector3 cycleDirection;

	public float stanceTime;

	public float liftTime;

	public float liftoffTime;

	public float postliftTime;

	public float prelandTime;

	public float strikeTime;

	public float landTime;

	public float cycleDistance;

	public Vector3 stancePosition;

	public Vector3 heelToetipVector;

	public LegCycleSample[] samples;

	public int stanceIndex;

	public CycleDebugInfo debugInfo;
}
