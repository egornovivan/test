using System;
using System.Collections.Generic;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class InteractionEffector
{
	private Poser poser;

	private IKEffector effector;

	private float timer;

	private float length;

	private float weight;

	private float fadeInSpeed;

	private float defaultPull;

	private float defaultReach;

	private float defaultPush;

	private float defaultPushParent;

	private float resetTimer;

	private bool pickedUp;

	private bool defaults;

	private bool pickUpOnPostFBBIK;

	private Vector3 pickUpPosition;

	private Vector3 pausePositionRelative;

	private Quaternion pickUpRotation;

	private Quaternion pauseRotationRelative;

	private InteractionTarget interactionTarget;

	private Transform target;

	private List<bool> triggered = new List<bool>();

	private InteractionSystem interactionSystem;

	public FullBodyBipedEffector effectorType { get; private set; }

	public bool isPaused { get; private set; }

	public InteractionObject interactionObject { get; private set; }

	public bool inInteraction => interactionObject != null;

	public float progress
	{
		get
		{
			if (!inInteraction)
			{
				return 0f;
			}
			if (length == 0f)
			{
				return 0f;
			}
			return timer / length;
		}
	}

	public InteractionEffector(FullBodyBipedEffector effectorType)
	{
		this.effectorType = effectorType;
	}

	public void Initiate(InteractionSystem interactionSystem, IKSolverFullBodyBiped solver)
	{
		this.interactionSystem = interactionSystem;
		if (effector == null)
		{
			effector = solver.GetEffector(effectorType);
			poser = effector.bone.GetComponent<Poser>();
		}
		defaultPull = solver.GetChain(effectorType).pull;
		defaultReach = solver.GetChain(effectorType).reach;
		defaultPush = solver.GetChain(effectorType).push;
		defaultPushParent = solver.GetChain(effectorType).pushParent;
	}

	public bool ResetToDefaults(IKSolverFullBodyBiped solver, float speed)
	{
		if (inInteraction)
		{
			return false;
		}
		if (isPaused)
		{
			return false;
		}
		if (defaults)
		{
			return false;
		}
		resetTimer = Mathf.Clamp(resetTimer -= Time.deltaTime * speed, 0f, 1f);
		if (effector.isEndEffector)
		{
			solver.GetChain(effectorType).pull = Mathf.Lerp(defaultPull, solver.GetChain(effectorType).pull, resetTimer);
			solver.GetChain(effectorType).reach = Mathf.Lerp(defaultReach, solver.GetChain(effectorType).reach, resetTimer);
			solver.GetChain(effectorType).push = Mathf.Lerp(defaultPush, solver.GetChain(effectorType).push, resetTimer);
			solver.GetChain(effectorType).pushParent = Mathf.Lerp(defaultPushParent, solver.GetChain(effectorType).pushParent, resetTimer);
		}
		effector.positionWeight = Mathf.Lerp(0f, effector.positionWeight, resetTimer);
		effector.rotationWeight = Mathf.Lerp(0f, effector.rotationWeight, resetTimer);
		if (resetTimer <= 0f)
		{
			defaults = true;
		}
		return true;
	}

	public bool Pause()
	{
		if (!inInteraction)
		{
			return false;
		}
		isPaused = true;
		pausePositionRelative = target.InverseTransformPoint(effector.position);
		pauseRotationRelative = Quaternion.Inverse(target.rotation) * effector.rotation;
		if (interactionSystem.OnInteractionPause != null)
		{
			interactionSystem.OnInteractionPause(effectorType, interactionObject);
		}
		return true;
	}

	public bool Resume()
	{
		if (!inInteraction)
		{
			return false;
		}
		isPaused = false;
		if (interactionSystem.OnInteractionResume != null)
		{
			interactionSystem.OnInteractionResume(effectorType, interactionObject);
		}
		return true;
	}

	public bool Start(InteractionObject interactionObject, string tag, float fadeInTime, bool interrupt)
	{
		if (!inInteraction)
		{
			effector.position = effector.bone.position;
			effector.rotation = effector.bone.rotation;
		}
		else if (!interrupt)
		{
			return false;
		}
		target = interactionObject.GetTarget(effectorType, tag);
		if (target == null)
		{
			return false;
		}
		interactionTarget = target.GetComponent<InteractionTarget>();
		this.interactionObject = interactionObject;
		if (interactionSystem.OnInteractionStart != null)
		{
			interactionSystem.OnInteractionStart(effectorType, interactionObject);
		}
		triggered.Clear();
		for (int i = 0; i < interactionObject.events.Length; i++)
		{
			triggered.Add(item: false);
		}
		if (poser != null)
		{
			if (poser.poseRoot == null)
			{
				poser.weight = 0f;
			}
			if (interactionTarget != null)
			{
				poser.poseRoot = target.transform;
			}
			else
			{
				poser.poseRoot = null;
			}
			poser.AutoMapping();
		}
		timer = 0f;
		weight = 0f;
		fadeInSpeed = ((!(fadeInTime > 0f)) ? 1000f : (1f / fadeInTime));
		length = interactionObject.length;
		isPaused = false;
		pickedUp = false;
		pickUpPosition = Vector3.zero;
		pickUpRotation = Quaternion.identity;
		if (interactionTarget != null)
		{
			interactionTarget.RotateTo(effector.bone.position);
		}
		return true;
	}

	public void Update(Transform root, IKSolverFullBodyBiped solver, float speed)
	{
		if (!inInteraction)
		{
			return;
		}
		if (interactionTarget != null && !interactionTarget.rotateOnce)
		{
			interactionTarget.RotateTo(effector.bone.position);
		}
		if (isPaused)
		{
			effector.position = target.TransformPoint(pausePositionRelative);
			effector.rotation = target.rotation * pauseRotationRelative;
			interactionObject.Apply(solver, effectorType, interactionTarget, timer, weight);
			return;
		}
		timer += Time.deltaTime * speed * ((!(interactionTarget != null)) ? 1f : interactionTarget.interactionSpeedMlp);
		weight = Mathf.Clamp(weight + Time.deltaTime * fadeInSpeed, 0f, 1f);
		bool pickUp = false;
		bool pause = false;
		TriggerUntriggeredEvents(checkTime: true, out pickUp, out pause);
		Vector3 b = ((!pickedUp) ? target.position : pickUpPosition);
		Quaternion b2 = ((!pickedUp) ? target.rotation : pickUpRotation);
		effector.position = Vector3.Lerp(effector.bone.position, b, weight);
		effector.rotation = Quaternion.Lerp(effector.bone.rotation, b2, weight);
		interactionObject.Apply(solver, effectorType, interactionTarget, timer, weight);
		if (pickUp)
		{
			PickUp(root);
		}
		if (pause)
		{
			Pause();
		}
		if (poser != null)
		{
			poser.weight = Mathf.Lerp(poser.weight, interactionObject.GetValue(InteractionObject.WeightCurve.Type.PoserWeight, interactionTarget, timer), weight);
		}
		if (timer >= length)
		{
			Stop();
		}
	}

	private void TriggerUntriggeredEvents(bool checkTime, out bool pickUp, out bool pause)
	{
		pickUp = false;
		pause = false;
		for (int i = 0; i < triggered.Count; i++)
		{
			if (triggered[i] || (checkTime && !(interactionObject.events[i].time < timer)))
			{
				continue;
			}
			interactionObject.events[i].Activate(effector.bone);
			if (interactionObject.events[i].pickUp)
			{
				if (timer >= interactionObject.events[i].time)
				{
					timer = interactionObject.events[i].time;
				}
				pickUp = true;
			}
			if (interactionObject.events[i].pause)
			{
				if (timer >= interactionObject.events[i].time)
				{
					timer = interactionObject.events[i].time;
				}
				pause = true;
			}
			if (interactionSystem.OnInteractionEvent != null)
			{
				interactionSystem.OnInteractionEvent(effectorType, interactionObject, interactionObject.events[i]);
			}
			triggered[i] = true;
		}
	}

	private void PickUp(Transform root)
	{
		pickUpPosition = effector.position;
		pickUpRotation = effector.rotation;
		pickUpOnPostFBBIK = true;
		pickedUp = true;
		if (interactionObject.targetsRoot.GetComponent<Rigidbody>() != null)
		{
			if (!interactionObject.targetsRoot.GetComponent<Rigidbody>().isKinematic)
			{
				interactionObject.targetsRoot.GetComponent<Rigidbody>().isKinematic = true;
			}
			if (root.GetComponent<Collider>() != null)
			{
				Collider[] componentsInChildren = interactionObject.targetsRoot.GetComponentsInChildren<Collider>();
				Collider[] array = componentsInChildren;
				foreach (Collider collider in array)
				{
					if (!collider.isTrigger)
					{
						Physics.IgnoreCollision(root.GetComponent<Collider>(), collider);
					}
				}
			}
		}
		if (interactionSystem.OnInteractionPickUp != null)
		{
			interactionSystem.OnInteractionPickUp(effectorType, interactionObject);
		}
	}

	public bool Stop()
	{
		if (!inInteraction)
		{
			return false;
		}
		bool pickUp = false;
		bool pause = false;
		TriggerUntriggeredEvents(checkTime: false, out pickUp, out pause);
		if (interactionSystem.OnInteractionStop != null)
		{
			interactionSystem.OnInteractionStop(effectorType, interactionObject);
		}
		if (interactionTarget != null)
		{
			interactionTarget.ResetRotation();
		}
		interactionObject = null;
		weight = 0f;
		timer = 0f;
		isPaused = false;
		target = null;
		defaults = false;
		resetTimer = 1f;
		if (poser != null && !pickedUp)
		{
			poser.weight = 0f;
		}
		pickedUp = false;
		return true;
	}

	public void OnPostFBBIK(IKSolverFullBodyBiped fullBody)
	{
		if (inInteraction)
		{
			float num = interactionObject.GetValue(InteractionObject.WeightCurve.Type.RotateBoneWeight, interactionTarget, timer) * weight;
			if (num > 0f)
			{
				Quaternion b = ((!pickedUp) ? effector.rotation : pickUpRotation);
				Quaternion quaternion = Quaternion.Slerp(effector.bone.rotation, b, num * num);
				effector.bone.localRotation = Quaternion.Inverse(effector.bone.parent.rotation) * quaternion;
			}
			if (pickUpOnPostFBBIK)
			{
				interactionObject.targetsRoot.parent = effector.bone;
				interactionObject.targetsRoot.localPosition = Quaternion.Inverse(pickUpRotation) * (interactionObject.targetsRoot.position - pickUpPosition);
				pickUpOnPostFBBIK = false;
			}
		}
	}
}
