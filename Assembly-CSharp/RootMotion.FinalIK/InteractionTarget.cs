using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[AddComponentMenu("Scripts/RootMotion.FinalIK/Interaction System/Interaction Target")]
public class InteractionTarget : MonoBehaviour
{
	[Serializable]
	public class Multiplier
	{
		public InteractionObject.WeightCurve.Type curve;

		public float multiplier;
	}

	public FullBodyBipedEffector effectorType;

	public Multiplier[] multipliers;

	public float interactionSpeedMlp = 1f;

	public Transform pivot;

	public Vector3 twistAxis = Vector3.up;

	public float twistWeight = 1f;

	public float swingWeight;

	public bool rotateOnce = true;

	private Quaternion defaultLocalRotation;

	private Transform lastPivot;

	public float GetValue(InteractionObject.WeightCurve.Type curveType)
	{
		for (int i = 0; i < multipliers.Length; i++)
		{
			if (multipliers[i].curve == curveType)
			{
				return multipliers[i].multiplier;
			}
		}
		return 1f;
	}

	public void ResetRotation()
	{
		if (pivot != null)
		{
			pivot.localRotation = defaultLocalRotation;
		}
	}

	public void RotateTo(Vector3 position)
	{
		if (!(pivot == null))
		{
			if (pivot != lastPivot)
			{
				defaultLocalRotation = pivot.localRotation;
				lastPivot = pivot;
			}
			pivot.localRotation = defaultLocalRotation;
			if (twistWeight > 0f)
			{
				Vector3 tangent = base.transform.position - pivot.position;
				Vector3 vector = pivot.rotation * twistAxis;
				Vector3 normal = vector;
				Vector3.OrthoNormalize(ref normal, ref tangent);
				normal = vector;
				Vector3 tangent2 = position - pivot.position;
				Vector3.OrthoNormalize(ref normal, ref tangent2);
				Quaternion b = QuaTools.FromToAroundAxis(tangent, tangent2, vector);
				pivot.rotation = Quaternion.Lerp(Quaternion.identity, b, twistWeight) * pivot.rotation;
			}
			if (swingWeight > 0f)
			{
				Quaternion b2 = Quaternion.FromToRotation(base.transform.position - pivot.position, position - pivot.position);
				pivot.rotation = Quaternion.Lerp(Quaternion.identity, b2, swingWeight) * pivot.rotation;
			}
		}
	}

	[ContextMenu("User Manual")]
	private void OpenUserManual()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/page10.html");
	}

	[ContextMenu("Scrpt Reference")]
	private void OpenScriptReference()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_interaction_target.html");
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.1f);
	}
}
