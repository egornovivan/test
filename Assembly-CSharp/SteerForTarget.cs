using System;
using UnityEngine;

public class SteerForTarget : Steering
{
	public Transform Target;

	private void Awake()
	{
		if (Target == null)
		{
			UnityEngine.Object.Destroy(this);
			throw new Exception("SteerForTarget need a target transform. Dying.");
		}
	}

	protected override Vector3 CalculateForce()
	{
		return base.Vehicle.GetSeekVector(Target.position);
	}
}
