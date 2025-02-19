using System;

public class MotionAnalyzerDrawOptions
{
	[NonSerialized]
	public int currentLeg;

	[NonSerialized]
	public bool drawAllFeet;

	[NonSerialized]
	public bool drawHeelToe = true;

	[NonSerialized]
	public bool drawFootBase;

	[NonSerialized]
	public bool drawTrajectories = true;

	[NonSerialized]
	public bool drawTrajectoriesProjected;

	[NonSerialized]
	public bool drawThreePoints;

	[NonSerialized]
	public bool drawGraph = true;

	[NonSerialized]
	public bool normalizeGraph = true;

	[NonSerialized]
	public bool drawStanceMarkers;

	[NonSerialized]
	public bool drawBalanceCurve;

	[NonSerialized]
	public bool drawLiftedCurve;

	[NonSerialized]
	public bool isolateVertical;

	[NonSerialized]
	public bool isolateHorisontal = true;

	[NonSerialized]
	public float graphScaleH;

	[NonSerialized]
	public float graphScaleV;

	[NonSerialized]
	public bool drawFootPrints = true;
}
