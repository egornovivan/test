using System;
using Pathea;
using PETools;
using UnityEngine;

namespace AnimFollow;

public class RagdollControl_AF : MonoBehaviour
{
	public readonly int version = 4;

	[SerializeField]
	protected AnimFollow_AF animFollow;

	[SerializeField]
	protected Animator anim;

	[SerializeField]
	public Transform ragdollRootBone;

	[SerializeField]
	protected GameObject master;

	[SerializeField]
	protected Rigidbody[] slaveRigidBodies;

	[SerializeField]
	protected Transform masterRootBone;

	public Transform[] IceOnGetup;

	public string[] ignoreCollidersWithTag = new string[1] { "IgnoreMe" };

	[Range(10f, 170f)]
	public float getupAngularDrag = 50f;

	[Range(5f, 85f)]
	public float getupDrag = 25f;

	[Range(10f, 170f)]
	public float localAngularDrag = 50f;

	[Range(5f, 85f)]
	public float localDrag = 25f;

	[Range(0.5f, 4.5f)]
	public float fallLerp = 1.5f;

	[Range(0f, 0.2f)]
	public float residualTorque;

	[Range(0f, 0.2f)]
	public float residualForce = 0.1f;

	[Range(0f, 120f)]
	public float residualJointTorque = 120f;

	[Range(0f, 1f)]
	public float residualIdleFactor;

	[Range(2f, 26f)]
	public float graceSpeed = 8f;

	[Range(0.1f, 1.7f)]
	public float noGhostLimit = 0.5f;

	[Range(5f, 45f)]
	public float noGhostLimit2 = 15f;

	[Range(0f, 1.2f)]
	public float glideFree = 0.3f;

	public bool falling;

	public bool gettingUp;

	public bool jointLimits;

	private Vector3 limbError;

	public bool fellOnSpeed;

	public float limbErrorMagnitude;

	[Range(0f, 0.4f)]
	public float settledSpeed = 0.2f;

	[Range(0f, 0.6f)]
	public float masterFallAnimatorSpeedFactor = 0.4f;

	[Range(0f, 0.4f)]
	public float getup1AnimatorSpeedFactor = 0.25f;

	[Range(0f, 1f)]
	public float getup2AnimatorSpeedFactor = 0.65f;

	[Range(0f, 10f)]
	public float contactTorque = 1f;

	[Range(0f, 10f)]
	public float contactForce = 1f;

	[Range(0f, 30f)]
	public float contactJointTorque = 3f;

	[Range(0.04f, 0.48f)]
	public float getupLerp1 = 0.15f;

	[Range(0.5f, 6.5f)]
	public float getupLerp2 = 2f;

	[Range(0.05f, 0.65f)]
	public float wakeUpStrength = 0.2f;

	[Range(0f, 700f)]
	public float toContactLerp = 70f;

	[Range(0f, 10f)]
	public float fromContactLerp = 1f;

	[Range(0f, 100f)]
	public float maxTorque = 100f;

	[Range(0f, 100f)]
	public float maxForce = 100f;

	[Range(0f, 10000f)]
	public float maxJointTorque = 10000f;

	[Range(0f, 1f)]
	public float maxErrorWhenMatching = 0.1f;

	[HideInInspector]
	public float orientateY;

	[HideInInspector]
	public float collisionSpeed;

	[HideInInspector]
	public int numberOfCollisions;

	private float animatorSpeed = 1f;

	private int secondaryUpdateSet;

	private float[] noIceDynFriction;

	private float[] noIceStatFriction;

	private float drag;

	private float angularDrag;

	private float contactTime;

	private float noContactTime = 10f;

	private Quaternion rootboneToForward;

	[HideInInspector]
	public bool shotByBullet;

	private bool userNeedsToAssignStuff;

	private bool delayedGetupDone;

	private bool localTorqUserSetting;

	private bool orientate;

	private bool orientated = true;

	private bool getupState;

	private bool isInTransitionToGetup;

	private bool wasInTransitionToGetup;

	private ulong i;

	private PeEntity masterEntity;

	public bool active => animFollow.active;

	public void Awake()
	{
		if (!WeHaveAllTheStuff())
		{
			userNeedsToAssignStuff = true;
			return;
		}
		if (anim != null && anim.runtimeAnimatorController != null)
		{
			anim.speed = animatorSpeed;
		}
		secondaryUpdateSet = animFollow.secondaryUpdate;
		animFollow.maxTorque = maxTorque;
		animFollow.maxForce = maxForce;
		animFollow.maxJointTorque = maxJointTorque;
		if (slaveRigidBodies == null || slaveRigidBodies.Length == 0)
		{
			slaveRigidBodies = GetComponentsInChildren<Rigidbody>();
		}
		Array.Resize(ref noIceDynFriction, IceOnGetup.Length);
		Array.Resize(ref noIceStatFriction, IceOnGetup.Length);
		for (int i = 0; i < IceOnGetup.Length; i++)
		{
			noIceDynFriction[i] = IceOnGetup[i].GetComponent<Collider>().material.dynamicFriction;
			noIceStatFriction[i] = IceOnGetup[i].GetComponent<Collider>().material.staticFriction;
		}
		drag = animFollow.drag;
		angularDrag = animFollow.angularDrag;
		if (animFollow.version != version)
		{
			Debug.LogWarning("RagdollControll script is version " + version + " but animFollow script is version " + animFollow.version + "\n");
		}
	}

	private void Start()
	{
		masterEntity = GetComponentInParent<PeEntity>();
	}

	private bool IsIdle(Animator anim)
	{
		int fullPathHash = anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
		return fullPathHash == HashIDs.Idle || fullPathHash == HashIDs.IdleWater || fullPathHash == HashIDs.MonsterIdle;
	}

	private bool GetGetUpState(Animator anim)
	{
		int fullPathHash = anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
		return fullPathHash == HashIDs.GetupFront || fullPathHash == HashIDs.GetupBack || fullPathHash == HashIDs.GetupFrontMirror || fullPathHash == HashIDs.GetupBackMirror || fullPathHash == HashIDs.GetupFrontWater || fullPathHash == HashIDs.GetupBackWater || fullPathHash == HashIDs.GetupFrontMirrorWater || fullPathHash == HashIDs.GetupBackMirrorWater;
	}

	private bool GetTransition(Animator anim)
	{
		int nameHash = anim.GetAnimatorTransitionInfo(0).nameHash;
		return nameHash == HashIDs.AnyStateToGetupFront || nameHash == HashIDs.AnyStateToGetupBack || nameHash == HashIDs.AnyStateToGetupFrontMirror || nameHash == HashIDs.AnyStateToGetupBackMirror || nameHash == HashIDs.AnyStateToGetupFrontWater || nameHash == HashIDs.AnyStateToGetupBackWater || nameHash == HashIDs.AnyStateToGetupFrontMirrorWater || nameHash == HashIDs.AnyStateToGetupBackMirrorWater;
	}

	public void DoRagdollControl()
	{
		if (userNeedsToAssignStuff)
		{
			return;
		}
		if (this.i == 2)
		{
			rootboneToForward = Quaternion.Inverse(masterRootBone.rotation) * master.transform.rotation;
		}
		this.i++;
		if (anim != null && anim.runtimeAnimatorController != null)
		{
			getupState = GetGetUpState(anim);
		}
		wasInTransitionToGetup = isInTransitionToGetup;
		if (anim != null && anim.runtimeAnimatorController != null)
		{
			isInTransitionToGetup = GetTransition(anim);
		}
		limbError = animFollow.totalForceError;
		limbErrorMagnitude = limbError.magnitude;
		if (shotByBullet || (limbErrorMagnitude > noGhostLimit2 && orientated))
		{
			if (!falling)
			{
				if (anim != null && !anim.GetCurrentAnimatorStateInfo(0).nameHash.Equals(HashIDs.Idle) && !getupState)
				{
					animFollow.maxTorque = residualTorque;
					animFollow.maxForce = residualForce;
					animFollow.maxJointTorque = residualJointTorque;
					animFollow.SetJointTorque(residualJointTorque);
				}
				else
				{
					animFollow.maxTorque = residualTorque * residualIdleFactor;
					animFollow.maxForce = residualForce * residualIdleFactor;
					animFollow.maxJointTorque = residualJointTorque * residualIdleFactor;
					animFollow.SetJointTorque(animFollow.maxJointTorque);
				}
				animFollow.EnableJointLimits(jointLimits: true);
				jointLimits = true;
				animFollow.secondaryUpdate = 100;
				for (int i = 0; i < IceOnGetup.Length; i++)
				{
					IceOnGetup[i].GetComponent<Collider>().material.dynamicFriction = noIceDynFriction[i];
					IceOnGetup[i].GetComponent<Collider>().material.staticFriction = noIceStatFriction[i];
				}
				animFollow.angularDrag = angularDrag;
				animFollow.drag = drag;
				OnFallBegin();
			}
			shotByBullet = false;
			falling = true;
			gettingUp = false;
			orientated = false;
			if (anim != null && anim.runtimeAnimatorController != null)
			{
				anim.speed = animatorSpeed;
			}
			delayedGetupDone = false;
			fellOnSpeed = numberOfCollisions > 0 && collisionSpeed > graceSpeed;
		}
		else if (falling || gettingUp)
		{
			if (gettingUp)
			{
				if (orientate && !isInTransitionToGetup && wasInTransitionToGetup)
				{
					falling = false;
					master.transform.rotation = ragdollRootBone.rotation * Quaternion.Inverse(masterRootBone.rotation) * master.transform.rotation;
					master.transform.rotation = Quaternion.LookRotation(new Vector3(master.transform.forward.x, 0f, master.transform.forward.z), Vector3.up);
					orientate = false;
					orientated = true;
					for (int j = 0; j < IceOnGetup.Length; j++)
					{
						IceOnGetup[j].GetComponent<Collider>().material.dynamicFriction = 0f;
						IceOnGetup[j].GetComponent<Collider>().material.staticFriction = 0f;
					}
					animFollow.angularDrag = getupAngularDrag;
					animFollow.drag = getupDrag;
					OnGetupBegin();
				}
				if (orientated)
				{
					if (animFollow.maxTorque < wakeUpStrength)
					{
						if (anim != null && anim.runtimeAnimatorController != null)
						{
							anim.speed = getup1AnimatorSpeedFactor * animatorSpeed;
						}
						animFollow.maxTorque = Mathf.Lerp(animFollow.maxTorque, contactTorque, getupLerp1 * Time.fixedDeltaTime);
						animFollow.maxForce = Mathf.Lerp(animFollow.maxForce, contactForce, getupLerp1 * Time.fixedDeltaTime);
						animFollow.maxJointTorque = Mathf.Lerp(animFollow.maxJointTorque, contactJointTorque, getupLerp1 * Time.fixedDeltaTime);
						animFollow.secondaryUpdate = 20;
					}
					else if (!isInTransitionToGetup && !getupState)
					{
						animFollow.angularDrag = angularDrag;
						animFollow.drag = drag;
						if (anim != null && anim.runtimeAnimatorController != null)
						{
							anim.speed = animatorSpeed;
						}
						animFollow.secondaryUpdate = secondaryUpdateSet;
						for (int k = 0; k < IceOnGetup.Length; k++)
						{
							IceOnGetup[k].GetComponent<Collider>().material.dynamicFriction = noIceDynFriction[k];
							IceOnGetup[k].GetComponent<Collider>().material.staticFriction = noIceStatFriction[k];
						}
						if (limbErrorMagnitude < maxErrorWhenMatching || IsIdle(anim))
						{
							gettingUp = false;
							delayedGetupDone = false;
							animFollow.active = false;
							OnGetupFinished();
						}
						else
						{
							delayedGetupDone = true;
						}
					}
					else
					{
						if (anim != null)
						{
							anim.speed = getup2AnimatorSpeedFactor * animatorSpeed;
						}
						animFollow.maxTorque = Mathf.Lerp(animFollow.maxTorque, contactTorque, getupLerp2 * Time.fixedDeltaTime);
						animFollow.maxForce = Mathf.Lerp(animFollow.maxForce, contactForce, getupLerp2 * Time.fixedDeltaTime);
						animFollow.maxJointTorque = Mathf.Lerp(animFollow.maxJointTorque, contactJointTorque, getupLerp2 * Time.fixedDeltaTime);
						animFollow.secondaryUpdate = secondaryUpdateSet * 2;
						if (jointLimits)
						{
							animFollow.EnableJointLimits(jointLimits: false);
							jointLimits = false;
						}
					}
				}
			}
			else
			{
				animFollow.maxTorque = Mathf.Lerp(animFollow.maxTorque, 0f, fallLerp * Time.fixedDeltaTime);
				animFollow.maxForce = Mathf.Lerp(animFollow.maxForce, 0f, fallLerp * Time.fixedDeltaTime);
				animFollow.maxJointTorque = Mathf.Lerp(animFollow.maxJointTorque, 0f, fallLerp * Time.fixedDeltaTime);
				animFollow.SetJointTorque(animFollow.maxJointTorque);
				bool flag = false;
				if (masterEntity != null && masterEntity.animCmpt != null)
				{
					int layerMask = 6144;
					flag = PEUtil.CheckPositionUnderWater(ragdollRootBone.position - Vector3.up * 0.5f) && !Physics.Raycast(ragdollRootBone.position, Vector3.down, masterEntity.maxHeight + 0.5f, layerMask);
					masterEntity.animCmpt.SetBool("ragdollInWater", flag);
				}
				if (!flag)
				{
					master.transform.position = ragdollRootBone.position;
				}
				else
				{
					master.transform.position = ragdollRootBone.position - Vector3.up * masterEntity.maxHeight;
				}
				if (ragdollRootBone.GetComponent<Rigidbody>().velocity.magnitude < settledSpeed)
				{
					if (IsGetupReady())
					{
						gettingUp = true;
						orientate = true;
						if (anim != null && anim.runtimeAnimatorController != null)
						{
							anim.speed = masterFallAnimatorSpeedFactor * animatorSpeed;
						}
						animFollow.maxTorque = 0f;
						animFollow.maxForce = 0f;
						animFollow.maxJointTorque = 0f;
						Vector3 lhs = ragdollRootBone.rotation * rootboneToForward * Vector3.forward;
						if (anim != null && anim.runtimeAnimatorController != null)
						{
							if (Vector3.Dot(lhs, Vector3.down) >= 0f)
							{
								if (!anim.GetCurrentAnimatorStateInfo(0).fullPathHash.Equals(HashIDs.GetupFront))
								{
									anim.SetBool(HashIDs.FrontTrigger, value: true);
								}
								else
								{
									anim.SetBool(HashIDs.FrontMirrorTrigger, value: true);
								}
							}
							else if (!anim.GetCurrentAnimatorStateInfo(0).fullPathHash.Equals(HashIDs.GetupBack))
							{
								anim.SetBool(HashIDs.BackTrigger, value: true);
							}
							else
							{
								anim.SetBool(HashIDs.BackMirrorTrigger, value: true);
							}
						}
					}
					OnFallFinished();
				}
			}
		}
		collisionSpeed = 0f;
		if (numberOfCollisions == 0)
		{
			noContactTime += Time.fixedDeltaTime;
			contactTime = 0f;
			if ((!gettingUp && !falling) || delayedGetupDone)
			{
				animFollow.maxTorque = Mathf.Lerp(animFollow.maxTorque, maxTorque, fromContactLerp * Time.fixedDeltaTime);
				animFollow.maxForce = Mathf.Lerp(animFollow.maxForce, maxForce, fromContactLerp * Time.fixedDeltaTime);
				animFollow.maxJointTorque = Mathf.Lerp(animFollow.maxJointTorque, maxJointTorque, fromContactLerp * Time.fixedDeltaTime);
			}
		}
		else
		{
			contactTime += Time.fixedDeltaTime;
			noContactTime = 0f;
			if ((!gettingUp && !falling) || delayedGetupDone)
			{
				animFollow.maxTorque = Mathf.Lerp(animFollow.maxTorque, contactTorque, toContactLerp * Time.fixedDeltaTime);
				animFollow.maxForce = Mathf.Lerp(animFollow.maxForce, contactForce, toContactLerp * Time.fixedDeltaTime);
				animFollow.maxJointTorque = Mathf.Lerp(animFollow.maxJointTorque, contactJointTorque, toContactLerp * Time.fixedDeltaTime);
			}
		}
	}

	private bool WeHaveAllTheStuff()
	{
		if (null == animFollow && null == (animFollow = GetComponent<AnimFollow_AF>()))
		{
			return false;
		}
		if (null == master && null == (master = animFollow.master))
		{
			return false;
		}
		if ((bool)master.GetComponent<SimpleFootIK_AF>())
		{
			return false;
		}
		if (!master.activeInHierarchy)
		{
			return false;
		}
		if (null == ragdollRootBone || null == ragdollRootBone.GetComponent<Rigidbody>() || ragdollRootBone.root != base.transform.root)
		{
			ragdollRootBone = GetComponentInChildren<Rigidbody>().transform;
		}
		if (null == masterRootBone)
		{
			int num = 0;
			Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
			Transform[] array = componentsInChildren;
			foreach (Transform transform in array)
			{
				if (transform == ragdollRootBone)
				{
					masterRootBone = master.GetComponentsInChildren<Transform>()[num];
					break;
				}
				num++;
			}
		}
		if (null == anim)
		{
			anim = master.GetComponentInChildren<Animator>();
		}
		if (IceOnGetup.Length != 0 && IceOnGetup[IceOnGetup.Length - 1] == null)
		{
			return false;
		}
		if (!base.transform.root.GetComponent<ragdollHitByBullet_AF>() && fellOnSpeed)
		{
			MonoBehaviour.print("This will never show and is here just to avoid a compiler warning");
		}
		return true;
	}

	protected virtual bool IsGetupReady()
	{
		return false;
	}

	protected virtual void OnFallBegin()
	{
	}

	protected virtual void OnFallFinished()
	{
	}

	protected virtual void OnGetupBegin()
	{
	}

	protected virtual void OnGetupFinished()
	{
	}
}
