using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[AddComponentMenu("Scripts/RootMotion.FinalIK/Interaction System/Interaction Object")]
public class InteractionObject : MonoBehaviour
{
	[Serializable]
	public class InteractionEvent
	{
		public float time;

		public bool pause;

		public bool pickUp;

		public AnimatorEvent[] animations;

		public Message[] messages;

		public void Activate(Transform t)
		{
			AnimatorEvent[] array = animations;
			foreach (AnimatorEvent animatorEvent in array)
			{
				animatorEvent.Activate(pickUp);
			}
			Message[] array2 = messages;
			foreach (Message message in array2)
			{
				message.Send(t);
			}
		}
	}

	[Serializable]
	public class Message
	{
		private const string empty = "";

		public string function;

		public GameObject recipient;

		public void Send(Transform t)
		{
			if (!(recipient == null) && !(function == string.Empty) && !(function == string.Empty))
			{
				recipient.SendMessage(function, t, SendMessageOptions.RequireReceiver);
			}
		}
	}

	[Serializable]
	public class AnimatorEvent
	{
		private const string empty = "";

		public Animator animator;

		public Animation animation;

		public string animationState;

		public float crossfadeTime = 0.3f;

		public int layer;

		public bool resetNormalizedTime;

		public void Activate(bool pickUp)
		{
			if (animator != null)
			{
				if (pickUp)
				{
					animator.applyRootMotion = false;
				}
				Activate(animator);
			}
			if (animation != null)
			{
				Activate(animation);
			}
		}

		private void Activate(Animator animator)
		{
			if (!(animationState == string.Empty))
			{
				if (resetNormalizedTime)
				{
					animator.CrossFade(animationState, crossfadeTime, layer, 0f);
				}
				else
				{
					animator.CrossFade(animationState, crossfadeTime, layer);
				}
			}
		}

		private void Activate(Animation animation)
		{
			if (!(animationState == string.Empty))
			{
				if (resetNormalizedTime)
				{
					animation[animationState].normalizedTime = 0f;
				}
				animation[animationState].layer = layer;
				animation.CrossFade(animationState, crossfadeTime);
			}
		}
	}

	[Serializable]
	public class WeightCurve
	{
		[Serializable]
		public enum Type
		{
			PositionWeight,
			RotationWeight,
			PositionOffsetX,
			PositionOffsetY,
			PositionOffsetZ,
			Pull,
			Reach,
			RotateBoneWeight,
			Push,
			PushParent,
			PoserWeight
		}

		public Type type;

		public AnimationCurve curve;

		public float GetValue(float timer)
		{
			return curve.Evaluate(timer);
		}
	}

	[Serializable]
	public class Multiplier
	{
		public WeightCurve.Type curve;

		public float multiplier = 1f;

		public WeightCurve.Type result;

		public float GetValue(WeightCurve weightCurve, float timer)
		{
			return weightCurve.GetValue(timer) * multiplier;
		}
	}

	public Transform otherLookAtTarget;

	public Transform otherTargetsRoot;

	public Transform positionOffsetSpace;

	public WeightCurve[] weightCurves;

	public Multiplier[] multipliers;

	public InteractionEvent[] events;

	private InteractionTarget[] targets = new InteractionTarget[0];

	public float length { get; private set; }

	public Transform lookAtTarget
	{
		get
		{
			if (otherLookAtTarget != null)
			{
				return otherLookAtTarget;
			}
			return base.transform;
		}
	}

	public Transform targetsRoot
	{
		get
		{
			if (otherTargetsRoot != null)
			{
				return otherTargetsRoot;
			}
			return base.transform;
		}
	}

	public void Initiate()
	{
		for (int i = 0; i < weightCurves.Length; i++)
		{
			if (weightCurves[i].curve.length > 0)
			{
				float time = weightCurves[i].curve.keys[weightCurves[i].curve.length - 1].time;
				length = Mathf.Clamp(length, time, length);
			}
		}
		for (int j = 0; j < events.Length; j++)
		{
			length = Mathf.Clamp(length, events[j].time, length);
		}
		targets = targetsRoot.GetComponentsInChildren<InteractionTarget>();
	}

	public InteractionTarget[] GetTargets()
	{
		return targets;
	}

	public Transform GetTarget(FullBodyBipedEffector effectorType, string tag)
	{
		if (tag == string.Empty || tag == string.Empty)
		{
			return GetTarget(effectorType);
		}
		for (int i = 0; i < targets.Length; i++)
		{
			if (targets[i].effectorType == effectorType && targets[i].tag == tag)
			{
				return targets[i].transform;
			}
		}
		return base.transform;
	}

	public void Apply(IKSolverFullBodyBiped solver, FullBodyBipedEffector effector, InteractionTarget target, float timer, float weight)
	{
		for (int i = 0; i < weightCurves.Length; i++)
		{
			float num = ((!(target == null)) ? target.GetValue(weightCurves[i].type) : 1f);
			Apply(solver, effector, weightCurves[i].type, weightCurves[i].GetValue(timer), weight * num);
		}
		for (int j = 0; j < multipliers.Length; j++)
		{
			if (multipliers[j].curve == multipliers[j].result && !Warning.logged)
			{
				Warning.Log("InteractionObject Multiplier 'Curve' " + multipliers[j].curve.ToString() + "and 'Result' are the same.", base.transform);
			}
			int weightCurveIndex = GetWeightCurveIndex(multipliers[j].curve);
			if (weightCurveIndex != -1)
			{
				float num2 = ((!(target == null)) ? target.GetValue(multipliers[j].result) : 1f);
				Apply(solver, effector, multipliers[j].result, multipliers[j].GetValue(weightCurves[weightCurveIndex], timer), weight * num2);
			}
			else if (!Warning.logged)
			{
				Warning.Log("InteractionObject Multiplier curve " + multipliers[j].curve.ToString() + "does not exist.", base.transform);
			}
		}
	}

	public float GetValue(WeightCurve.Type weightCurveType, InteractionTarget target, float timer)
	{
		int weightCurveIndex = GetWeightCurveIndex(weightCurveType);
		if (weightCurveIndex != -1)
		{
			float num = ((!(target == null)) ? target.GetValue(weightCurveType) : 1f);
			return weightCurves[weightCurveIndex].GetValue(timer) * num;
		}
		for (int i = 0; i < multipliers.Length; i++)
		{
			if (multipliers[i].result == weightCurveType)
			{
				int weightCurveIndex2 = GetWeightCurveIndex(multipliers[i].curve);
				if (weightCurveIndex2 != -1)
				{
					float num2 = ((!(target == null)) ? target.GetValue(multipliers[i].result) : 1f);
					return multipliers[i].GetValue(weightCurves[weightCurveIndex2], timer) * num2;
				}
			}
		}
		return 0f;
	}

	private void Awake()
	{
		Initiate();
	}

	private void Apply(IKSolverFullBodyBiped solver, FullBodyBipedEffector effector, WeightCurve.Type type, float value, float weight)
	{
		switch (type)
		{
		case WeightCurve.Type.PositionWeight:
			solver.GetEffector(effector).positionWeight = Mathf.Lerp(solver.GetEffector(effector).positionWeight, value, weight);
			break;
		case WeightCurve.Type.RotationWeight:
			solver.GetEffector(effector).rotationWeight = Mathf.Lerp(solver.GetEffector(effector).rotationWeight, value, weight);
			break;
		case WeightCurve.Type.PositionOffsetX:
			solver.GetEffector(effector).position += ((!(positionOffsetSpace != null)) ? solver.GetRoot().rotation : positionOffsetSpace.rotation) * Vector3.right * value * weight;
			break;
		case WeightCurve.Type.PositionOffsetY:
			solver.GetEffector(effector).position += ((!(positionOffsetSpace != null)) ? solver.GetRoot().rotation : positionOffsetSpace.rotation) * Vector3.up * value * weight;
			break;
		case WeightCurve.Type.PositionOffsetZ:
			solver.GetEffector(effector).position += ((!(positionOffsetSpace != null)) ? solver.GetRoot().rotation : positionOffsetSpace.rotation) * Vector3.forward * value * weight;
			break;
		case WeightCurve.Type.Pull:
			solver.GetChain(effector).pull = Mathf.Lerp(solver.GetChain(effector).pull, value, weight);
			break;
		case WeightCurve.Type.Reach:
			solver.GetChain(effector).reach = Mathf.Lerp(solver.GetChain(effector).reach, value, weight);
			break;
		case WeightCurve.Type.Push:
			solver.GetChain(effector).push = Mathf.Lerp(solver.GetChain(effector).push, value, weight);
			break;
		case WeightCurve.Type.PushParent:
			solver.GetChain(effector).pushParent = Mathf.Lerp(solver.GetChain(effector).pushParent, value, weight);
			break;
		case WeightCurve.Type.RotateBoneWeight:
			break;
		}
	}

	private Transform GetTarget(FullBodyBipedEffector effectorType)
	{
		for (int i = 0; i < targets.Length; i++)
		{
			if (targets[i].effectorType == effectorType)
			{
				return targets[i].transform;
			}
		}
		return base.transform;
	}

	private int GetWeightCurveIndex(WeightCurve.Type weightCurveType)
	{
		for (int i = 0; i < weightCurves.Length; i++)
		{
			if (weightCurves[i].type == weightCurveType)
			{
				return i;
			}
		}
		return -1;
	}

	private int GetMultiplierIndex(WeightCurve.Type weightCurveType)
	{
		for (int i = 0; i < multipliers.Length; i++)
		{
			if (multipliers[i].result == weightCurveType)
			{
				return i;
			}
		}
		return -1;
	}

	[ContextMenu("User Manual")]
	private void OpenUserManual()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/page10.html");
	}

	[ContextMenu("Scrpt Reference")]
	private void OpenScriptReference()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_interaction_object.html");
	}
}
