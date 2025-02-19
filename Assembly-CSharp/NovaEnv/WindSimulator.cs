using System;
using UnityEngine;

namespace NovaEnv;

public class WindSimulator : MonoBehaviour
{
	[HideInInspector]
	public Executor Executor;

	public float FreqX = 3.31f;

	public float FreqY = 1.53f;

	public float FreqZ = 2.67f;

	public Vector3 WindDirection;

	private void Update()
	{
		WindDirection.x = (float)Math.Sin(Executor.LocalDay * (double)FreqX + 0.78);
		WindDirection.y = (float)Math.Sin(Executor.LocalDay * (double)FreqY + 3.14);
		WindDirection.z = (float)Math.Sin(Executor.LocalDay * (double)FreqZ + 1.57);
		WindDirection *= Mathf.Pow(Executor.WetCoef, 3f) * 2f + 1f;
	}
}
