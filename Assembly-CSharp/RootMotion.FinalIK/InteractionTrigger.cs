using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[AddComponentMenu("Scripts/RootMotion.FinalIK/Interaction System/Interaction Trigger")]
public class InteractionTrigger : MonoBehaviour
{
	[Serializable]
	public class Range
	{
		[Serializable]
		public class Interaction
		{
			public InteractionObject interactionObject;

			public FullBodyBipedEffector[] effectors;
		}

		public Interaction[] interactions;

		public Vector3 positionOffset;

		public bool orbit;

		public float maxDistance = 0.5f;

		[Range(-180f, 180f)]
		public float angleOffset;

		[Range(0f, 180f)]
		public float maxAngle = 50f;

		public bool IsInRange(Vector3 transformPosition, Vector3 triggerPosition, Vector3 objectPosition, Transform character, out float angle)
		{
			angle = 180f;
			if (orbit)
			{
				float magnitude = positionOffset.magnitude;
				float num = Vector3.Distance(character.position, transformPosition);
				if (num < magnitude - maxDistance || num > magnitude + maxDistance)
				{
					return false;
				}
			}
			else if (Vector3.Distance(character.position, triggerPosition) > maxDistance)
			{
				return false;
			}
			if (character.position == objectPosition)
			{
				return true;
			}
			Vector3 tangent = objectPosition - character.position;
			Vector3 normal = character.up;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			tangent = Quaternion.AngleAxis(angleOffset, normal) * tangent;
			Vector3 forward = character.forward;
			angle = Vector3.Angle(tangent, forward);
			if (angle > maxAngle)
			{
				return false;
			}
			return true;
		}
	}

	public Transform target;

	public Range[] ranges;

	public int GetBestRangeIndex(Transform character)
	{
		if (GetComponent<Collider>() == null)
		{
			Warning.Log("Using the InteractionTrigger requires a Collider component.", base.transform);
			return -1;
		}
		if (target == null)
		{
			Warning.Log("InteractionTrigger has no target Transform.", base.transform);
			return -1;
		}
		int result = -1;
		float num = 180f;
		for (int i = 0; i < ranges.Length; i++)
		{
			Vector3 triggerPosition = base.transform.position + base.transform.rotation * ranges[i].positionOffset;
			float angle = 0f;
			if (ranges[i].IsInRange(base.transform.position, triggerPosition, target.position, character, out angle) && angle <= num)
			{
				num = angle;
				result = i;
			}
		}
		return result;
	}
}
