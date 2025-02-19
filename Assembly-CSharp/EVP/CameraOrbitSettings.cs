using System;
using UnityEngine;

namespace EVP;

[Serializable]
public class CameraOrbitSettings
{
	public float distance = 10f;

	[Space(5f)]
	public float horizontalSpeed = 5f;

	public float verticalSpeed = 2.5f;

	public float distanceSpeed = 10f;

	[Space(5f)]
	public float minVerticalAngle = -20f;

	public float maxVerticalAngle = 80f;

	public float minDistance = 5f;

	public float maxDistance = 50f;

	[Space(5f)]
	public float orbitDamping = 4f;

	public float distanceDamping = 4f;
}
