using System;
using UnityEngine;

[Serializable]
public class MotionGroupInfo
{
	public string name;

	public IMotionAnalyzer[] motions;

	public Interpolator interpolator;

	public float[] GetMotionWeights(Vector3 velocity)
	{
		return interpolator.Interpolate(new float[3] { velocity.x, velocity.y, velocity.z });
	}
}
