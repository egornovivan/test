using System;

namespace WhiteCat.Internal;

[Serializable]
public class PathSpline : Spline
{
	public float pathLength;

	public PathSpline(float error)
	{
		base.error = error;
	}
}
