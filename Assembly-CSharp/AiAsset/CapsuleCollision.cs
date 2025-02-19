using System;
using UnityEngine;

namespace AiAsset;

[Serializable]
public class CapsuleCollision
{
	public Vector3 center = Vector3.zero;

	public float radius;

	public float height;

	public Direction direction = Direction.Y_Axis;
}
