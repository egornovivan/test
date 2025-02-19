using System;
using UnityEngine;

namespace VehiclePhysics;

[Serializable]
public class VehicleWheel
{
	public WheelCollider wheel;

	public Transform model;

	public float maxMotorTorque;

	public float staticBrakeTorque;

	public float dynamicBrakeTorque;

	public float footBrakeTorque;

	public float handBrakeTorque;

	private VehicleEngine engine;

	private Rigidbody rigid;

	[NonSerialized]
	public float motorTorque;

	[NonSerialized]
	public float brakeTorque;

	public void Init(VehicleEngine e)
	{
		engine = e;
		rigid = engine.GetComponent<Rigidbody>();
		wheel.wheelDampingRate = 0.25f;
		wheel.suspensionDistance = 0.2f;
		JointSpring suspensionSpring = default(JointSpring);
		suspensionSpring.spring = 3f * rigid.mass;
		suspensionSpring.damper = 2f * rigid.mass;
		suspensionSpring.targetPosition = 0.3f;
		wheel.suspensionSpring = suspensionSpring;
		wheel.center = new Vector3(0f, (1f - wheel.suspensionSpring.targetPosition) * wheel.suspensionDistance, 0f);
		wheel.forceAppPointDistance = 0.7f;
		wheel.steerAngle = 0f;
		wheel.brakeTorque = 0f;
		wheel.motorTorque = 0f;
		wheel.ConfigureVehicleSubsteps(1f, 20, 20);
	}

	public void SyncModel()
	{
		wheel.GetWorldPose(out var pos, out var quat);
		if (pos != Vector3.zero)
		{
			model.transform.position = pos;
			model.transform.rotation = quat;
		}
	}

	public void SetWheelTorques()
	{
		wheel.motorTorque = motorTorque;
		wheel.brakeTorque = brakeTorque;
	}
}
