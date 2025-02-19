using System;
using UnityEngine;

[Serializable]
public class MotionAnalyzerBackwards : IMotionAnalyzer
{
	public MotionAnalyzer orig;

	public LegCycleData[] m_cycles;

	public float m_cycleOffset;

	public override LegCycleData[] cycles => m_cycles;

	public override int samples => orig.samples;

	public override Vector3 cycleDirection => -orig.cycleDirection;

	public override float cycleDistance => orig.cycleDistance;

	public override Vector3 cycleVector => -orig.cycleVector;

	public override float cycleDuration => orig.cycleDuration;

	public override float cycleSpeed => orig.cycleSpeed;

	public override Vector3 cycleVelocity => -orig.cycleVelocity;

	public override float cycleOffset
	{
		get
		{
			return m_cycleOffset;
		}
		set
		{
			m_cycleOffset = value;
		}
	}

	public override Vector3 GetFlightFootPosition(int leg, float flightTime, int phase)
	{
		Vector3 flightFootPosition = orig.GetFlightFootPosition(leg, 1f - flightTime, 2 - phase);
		return new Vector3(0f - flightFootPosition.x, flightFootPosition.y, 1f - flightFootPosition.z);
	}

	public override void Analyze(GameObject o)
	{
		animation = orig.animation;
		name = animation.name + "_bk";
		motionType = orig.motionType;
		motionGroup = orig.motionGroup;
		LegController legController = o.GetComponent(typeof(LegController)) as LegController;
		int num = legController.legs.Length;
		m_cycles = new LegCycleData[num];
		for (int i = 0; i < num; i++)
		{
			cycles[i] = new LegCycleData();
			cycles[i].cycleCenter = orig.cycles[i].cycleCenter;
			cycles[i].cycleScaling = orig.cycles[i].cycleScaling;
			cycles[i].cycleDirection = -orig.cycles[i].cycleDirection;
			cycles[i].stanceTime = 1f - orig.cycles[i].stanceTime;
			cycles[i].liftTime = 1f - orig.cycles[i].landTime;
			cycles[i].liftoffTime = 1f - orig.cycles[i].strikeTime;
			cycles[i].postliftTime = 1f - orig.cycles[i].prelandTime;
			cycles[i].prelandTime = 1f - orig.cycles[i].postliftTime;
			cycles[i].strikeTime = 1f - orig.cycles[i].liftoffTime;
			cycles[i].landTime = 1f - orig.cycles[i].liftTime;
			cycles[i].cycleDistance = orig.cycles[i].cycleDistance;
			cycles[i].stancePosition = orig.cycles[i].stancePosition;
			cycles[i].heelToetipVector = orig.cycles[i].heelToetipVector;
		}
	}
}
