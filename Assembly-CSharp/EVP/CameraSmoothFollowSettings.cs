using System;
using UnityEngine;

namespace EVP;

[Serializable]
public class CameraSmoothFollowSettings
{
	public float distance = 10f;

	public float height = 5f;

	public float viewHeightRatio = 0.5f;

	[Space(5f)]
	public float heightDamping = 2f;

	public float rotationDamping = 3f;

	[Space(5f)]
	public bool followVelocity = true;

	public float velocityDamping = 5f;
}
