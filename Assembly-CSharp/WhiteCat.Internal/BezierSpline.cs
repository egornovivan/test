using System;

namespace WhiteCat.Internal;

[Serializable]
public class BezierSpline : PathSpline
{
	public BezierSpline(float error)
		: base(error)
	{
	}
}
