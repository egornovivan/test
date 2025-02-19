using System;
using UnityEngine;

namespace WhiteCat.Internal;

[Serializable]
public class CardinalNode : PathNode
{
	public CardinalNode(Vector3 localPosition, Quaternion localRotation)
		: base(localPosition, localRotation)
	{
	}
}
