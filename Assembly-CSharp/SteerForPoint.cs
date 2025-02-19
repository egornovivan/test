using UnityEngine;

public class SteerForPoint : Steering
{
	public Vector3 TargetPoint;

	private void Awake()
	{
		if (TargetPoint == Vector3.zero)
		{
			TargetPoint = base.transform.position;
		}
	}

	protected override Vector3 CalculateForce()
	{
		return base.Vehicle.GetSeekVector(TargetPoint);
	}
}
