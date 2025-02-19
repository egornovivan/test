using System;
using UnityEngine;

namespace WhiteCat.Internal;

[Serializable]
public class PathNode
{
	public Vector3 localPosition;

	public Quaternion localRotation;

	public PathNode(Vector3 localPosition, Quaternion localRotation)
	{
		this.localPosition = localPosition;
		this.localRotation = localRotation;
	}
}
