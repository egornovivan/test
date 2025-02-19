using System;
using UnityEngine;

namespace EVP;

[Serializable]
public class Wheel
{
	public WheelCollider wheelCollider;

	public Transform wheelTransform;

	public bool steer;

	public bool drive;

	public bool brake = true;

	public bool handbrake;
}
