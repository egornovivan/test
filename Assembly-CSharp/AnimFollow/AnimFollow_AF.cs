using System;
using PETools;
using UnityEngine;
using WhiteCat;

namespace AnimFollow;

public class AnimFollow_AF : MonoBehaviour
{
	public readonly int version = 4;

	private RagdollControl_AF ragdollControl;

	public GameObject master;

	[SerializeField]
	private Transform[] masterTransforms;

	private Transform[] masterRigidTransforms = new Transform[1];

	[SerializeField]
	private Transform[] slaveTransforms;

	[SerializeField]
	private Rigidbody[] slaveRigidbodies;

	[SerializeField]
	private MeshRenderer[] _meshRenderers;

	[SerializeField]
	private SkinnedMeshRenderer[] _skinnedRenderers;

	public Transform[] slaveRigidTransforms = new Transform[1];

	public Transform[] slaveExcludeTransforms;

	private Quaternion[] localRotations1 = new Quaternion[1];

	private Quaternion[] localRotations2 = new Quaternion[1];

	private float reciFixedDeltaTime;

	[Range(0f, 100f)]
	public float maxTorque = 100f;

	[Range(0f, 100f)]
	public float maxForce = 100f;

	[Range(0f, 10000f)]
	public float maxJointTorque = 10000f;

	[Range(0f, 10f)]
	public float jointDamping = 0.6f;

	public float[] maxTorqueProfile = new float[12]
	{
		100f, 100f, 100f, 100f, 100f, 100f, 100f, 100f, 100f, 100f,
		100f, 100f
	};

	public float[] maxForceProfile = new float[12]
	{
		1f, 0.2f, 0.2f, 0.2f, 0.2f, 1f, 1f, 0.2f, 0.2f, 0.2f,
		0.2f, 0.2f
	};

	public float[] maxJointTorqueProfile = new float[12]
	{
		1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f,
		1f, 1f
	};

	public float[] jointDampingProfile = new float[12]
	{
		1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f,
		1f, 1f
	};

	[Range(0f, 0.64f)]
	public float PTorque = 0.16f;

	[Range(0f, 160f)]
	public float PForce = 40f;

	[Range(0f, 0.008f)]
	public float DTorque = 0.002f;

	[Range(0f, 0.064f)]
	public float DForce = 0.016f;

	public float[] PTorqueProfile = new float[12]
	{
		20f, 30f, 10f, 30f, 10f, 30f, 30f, 30f, 30f, 10f,
		30f, 10f
	};

	public float[] PForceProfile = new float[12]
	{
		1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f,
		1f, 1f
	};

	[Range(0f, 48f)]
	public float angularDrag = 1f;

	[Range(0f, 2f)]
	public float drag = 0.5f;

	private float maxAngularVelocity = 1000f;

	private bool torque = true;

	private bool force = true;

	[HideInInspector]
	public bool mimicNonRigids = true;

	[Range(2f, 100f)]
	[HideInInspector]
	public int secondaryUpdate = 2;

	private int frameCounter;

	public bool hideMaster = true;

	public bool useGravity = true;

	private bool userNeedsToAssignStuff;

	private float torqueAngle;

	private Vector3 torqueAxis;

	private Vector3 torqueError;

	private Vector3 torqueSignal;

	private Vector3[] torqueLastError = new Vector3[1];

	private Vector3 torqueVelError;

	[HideInInspector]
	public Vector3 totalTorqueError;

	private Vector3 forceAxis;

	private Vector3 forceSignal;

	private Vector3 forceError;

	private Vector3[] forceLastError = new Vector3[1];

	private Vector3 forceVelError;

	[HideInInspector]
	public Vector3 totalForceError;

	public float[] forceErrorWeightProfile = new float[12]
	{
		1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f,
		1f, 1f
	};

	private float masterAngVel;

	private Vector3 masterAngVelAxis;

	private float slaveAngVel;

	private Vector3 slaveAngVelAxis;

	private Quaternion masterDeltaRotation;

	private Quaternion slaveDeltaRotation;

	private Quaternion[] lastMasterRotation = new Quaternion[1];

	private Quaternion[] lastSlaveRotation = new Quaternion[1];

	private Quaternion[] lastSlavelocalRotation = new Quaternion[1];

	private Vector3[] lastMasterPosition = new Vector3[1];

	private Vector3[] lastSlavePosition = new Vector3[1];

	private Quaternion[] startLocalRotation = new Quaternion[1];

	private ConfigurableJoint[] configurableJoints = new ConfigurableJoint[1];

	private Quaternion[] localToJointSpace = new Quaternion[1];

	private JointDrive jointDrive = default(JointDrive);

	private bool mActive;

	public bool active
	{
		get
		{
			return mActive;
		}
		set
		{
			if (mActive == value)
			{
				return;
			}
			mActive = value;
			BoneCollector componentInParent = GetComponentInParent<BoneCollector>();
			if ((bool)componentInParent)
			{
				componentInParent.isRagdoll = value;
			}
			SyncTransforms();
			PERagdollController pERagdollController = ragdollControl as PERagdollController;
			if (pERagdollController != null && pERagdollController.Handler != null)
			{
				pERagdollController.Handler.ActivateRagdollRenderer(mActive);
			}
			Rigidbody[] array = slaveRigidbodies;
			foreach (Rigidbody rigidbody in array)
			{
				if (rigidbody.tag != "RagdollLock")
				{
					rigidbody.isKinematic = !mActive;
				}
				else
				{
					rigidbody.isKinematic = true;
				}
				rigidbody.GetComponent<Collider>().isTrigger = !mActive;
				rigidbody.GetComponent<Collider>().enabled = mActive;
			}
		}
	}

	public void ResetModelInfo()
	{
		slaveTransforms = PEUtil.GetCmpts<Transform>(base.transform);
		masterTransforms = PEUtil.GetCmpts<Transform>(master.transform);
		slaveRigidbodies = PEUtil.GetCmpts<Rigidbody>(base.transform);
		_meshRenderers = PEUtil.GetCmpts<MeshRenderer>(base.transform);
		_skinnedRenderers = PEUtil.GetCmpts<SkinnedMeshRenderer>(base.transform);
	}

	private void Awake()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		reciFixedDeltaTime = 1f / Time.fixedDeltaTime;
		if (!master)
		{
			userNeedsToAssignStuff = true;
			return;
		}
		if ((bool)master.GetComponent<SimpleFootIK_AF>())
		{
			userNeedsToAssignStuff = true;
		}
		if (_meshRenderers.Length == 0)
		{
			_meshRenderers = GetComponentsInChildren<MeshRenderer>();
		}
		MeshRenderer[] meshRenderers = _meshRenderers;
		foreach (MeshRenderer meshRenderer in meshRenderers)
		{
			meshRenderer.enabled = false;
		}
		_meshRenderers = null;
		if (_skinnedRenderers.Length == 0)
		{
			_skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
		}
		SkinnedMeshRenderer[] skinnedRenderers = _skinnedRenderers;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedRenderers)
		{
			skinnedMeshRenderer.enabled = false;
		}
		_skinnedRenderers = null;
		if (!(ragdollControl = GetComponent<RagdollControl_AF>()))
		{
			userNeedsToAssignStuff = true;
		}
		if (slaveTransforms.Length == 0)
		{
			slaveTransforms = GetComponentsInChildren<Transform>(includeInactive: true);
		}
		if (masterTransforms.Length == 0)
		{
			masterTransforms = master.GetComponentsInChildren<Transform>(includeInactive: true);
		}
		Array.Resize(ref localRotations1, slaveTransforms.Length);
		Array.Resize(ref localRotations2, slaveTransforms.Length);
		if (masterTransforms.Length != slaveTransforms.Length)
		{
			userNeedsToAssignStuff = true;
			return;
		}
		if (slaveRigidbodies.Length == 0)
		{
			slaveRigidbodies = GetComponentsInChildren<Rigidbody>();
		}
		num2 = slaveRigidbodies.Length;
		Array.Resize(ref masterRigidTransforms, num2);
		Array.Resize(ref slaveRigidTransforms, num2);
		Array.Resize(ref maxTorqueProfile, num2);
		Array.Resize(ref maxForceProfile, num2);
		Array.Resize(ref maxJointTorqueProfile, num2);
		Array.Resize(ref jointDampingProfile, num2);
		Array.Resize(ref PTorqueProfile, num2);
		Array.Resize(ref PForceProfile, num2);
		Array.Resize(ref forceErrorWeightProfile, num2);
		Array.Resize(ref torqueLastError, num2);
		Array.Resize(ref forceLastError, num2);
		Array.Resize(ref lastMasterRotation, num2);
		Array.Resize(ref lastSlaveRotation, num2);
		Array.Resize(ref lastSlavelocalRotation, num2);
		Array.Resize(ref lastMasterPosition, num2);
		Array.Resize(ref lastSlavePosition, num2);
		Array.Resize(ref startLocalRotation, num2);
		Array.Resize(ref configurableJoints, num2);
		Array.Resize(ref localToJointSpace, num2);
		num2 = 0;
		Transform[] array = slaveTransforms;
		foreach (Transform transform in array)
		{
			bool flag = false;
			if (transform.gameObject.activeSelf)
			{
				for (int l = 0; l < slaveRigidbodies.Length; l++)
				{
					if (slaveRigidbodies[l].transform == transform)
					{
						flag = true;
						break;
					}
				}
			}
			else
			{
				flag = null != transform.GetComponent<Rigidbody>();
			}
			if (flag)
			{
				slaveRigidTransforms[num2] = transform;
				masterRigidTransforms[num2] = masterTransforms[num];
				ConfigurableJoint component = transform.GetComponent<ConfigurableJoint>();
				if (component != null)
				{
					configurableJoints[num2] = component;
					Vector3 forward = Vector3.Cross(configurableJoints[num2].axis, configurableJoints[num2].secondaryAxis);
					Vector3 secondaryAxis = configurableJoints[num2].secondaryAxis;
					ref Quaternion reference = ref localToJointSpace[num2];
					reference = Quaternion.LookRotation(forward, secondaryAxis);
					ref Quaternion reference2 = ref startLocalRotation[num2];
					reference2 = transform.localRotation * localToJointSpace[num2];
					jointDrive = configurableJoints[num2].slerpDrive;
					jointDrive.mode = JointDriveMode.Position;
					configurableJoints[num2].slerpDrive = jointDrive;
					num4++;
				}
				else if (num2 > 0)
				{
					userNeedsToAssignStuff = true;
					return;
				}
				num2++;
			}
			else
			{
				bool flag2 = false;
				Transform[] array2 = slaveExcludeTransforms;
				foreach (Transform transform2 in array2)
				{
					if (transform == transform2)
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					slaveTransforms[num3] = transform;
					masterTransforms[num3] = masterTransforms[num];
					ref Quaternion reference3 = ref localRotations1[num3];
					reference3 = transform.localRotation;
					num3++;
				}
			}
			num++;
		}
		localRotations2 = localRotations1;
		Array.Resize(ref masterTransforms, num3);
		Array.Resize(ref slaveTransforms, num3);
		Array.Resize(ref localRotations1, num3);
		Array.Resize(ref localRotations2, num3);
		if (num4 == 0)
		{
			userNeedsToAssignStuff = true;
			return;
		}
		SetJointTorque(maxJointTorque);
		EnableJointLimits(jointLimits: false);
	}

	private void Start()
	{
		if (userNeedsToAssignStuff)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < slaveRigidTransforms.Length; i++)
		{
			Rigidbody component = slaveRigidTransforms[i].GetComponent<Rigidbody>();
			component.useGravity = useGravity;
			component.angularDrag = angularDrag;
			component.drag = drag;
			component.maxAngularVelocity = maxAngularVelocity;
			if (!active)
			{
				component.isKinematic = true;
				component.GetComponent<Collider>().isTrigger = true;
				component.GetComponent<Collider>().enabled = false;
			}
			num++;
		}
	}

	public void DoAnimFollow()
	{
		if (userNeedsToAssignStuff)
		{
			return;
		}
		ragdollControl.DoRagdollControl();
		if (!mActive)
		{
			return;
		}
		totalTorqueError = Vector3.zero;
		totalForceError = Vector3.zero;
		if (frameCounter % secondaryUpdate == 0)
		{
			if (mimicNonRigids)
			{
				for (int i = 2; i < slaveTransforms.Length - 1; i++)
				{
					ref Quaternion reference = ref localRotations2[i];
					reference = masterTransforms[i].localRotation;
				}
			}
			SetJointTorque(maxJointTorque, jointDamping);
		}
		if (frameCounter % 2 == 0)
		{
			for (int j = 2; j < slaveTransforms.Length - 1; j++)
			{
				if (secondaryUpdate > 2)
				{
					ref Quaternion reference2 = ref localRotations1[j];
					reference2 = Quaternion.Lerp(localRotations1[j], localRotations2[j], 2f / (float)secondaryUpdate);
					slaveTransforms[j].localRotation = localRotations1[j];
				}
				else
				{
					slaveTransforms[j].localRotation = localRotations2[j];
				}
			}
		}
		for (int k = 0; k < slaveRigidTransforms.Length; k++)
		{
			slaveRigidbodies[k].angularDrag = angularDrag;
			slaveRigidbodies[k].drag = drag;
			if (torque)
			{
				(masterRigidTransforms[k].rotation * Quaternion.Inverse(slaveRigidTransforms[k].rotation)).ToAngleAxis(out torqueAngle, out torqueAxis);
				torqueError = FixEuler(torqueAngle) * torqueAxis;
				if (torqueAngle != 360f)
				{
					totalTorqueError += torqueError;
					PDControl(PTorque * PTorqueProfile[k], DTorque, out torqueSignal, torqueError, ref torqueLastError[k], reciFixedDeltaTime);
				}
				else
				{
					torqueSignal = new Vector3(0f, 0f, 0f);
				}
				torqueSignal = Vector3.ClampMagnitude(torqueSignal, maxTorque * maxTorqueProfile[k]);
				slaveRigidbodies[k].AddTorque(torqueSignal, ForceMode.VelocityChange);
			}
			forceError = masterRigidTransforms[k].position - slaveRigidTransforms[k].position;
			totalForceError += forceError * forceErrorWeightProfile[k];
			if (force)
			{
				PDControl(PForce * PForceProfile[k], DForce, out forceSignal, forceError, ref forceLastError[k], reciFixedDeltaTime);
				forceSignal = Vector3.ClampMagnitude(forceSignal, maxForce * maxForceProfile[k]);
				slaveRigidbodies[k].AddForceAtPosition(forceSignal, slaveRigidTransforms[k].position, ForceMode.VelocityChange);
			}
			if (k > 0 && configurableJoints[k] != null)
			{
				configurableJoints[k].targetRotation = Quaternion.Inverse(localToJointSpace[k]) * Quaternion.Inverse(masterRigidTransforms[k].localRotation) * startLocalRotation[k];
			}
		}
		frameCounter++;
	}

	public void SetJointTorque(float positionSpring, float positionDamper)
	{
		for (int i = 1; i < configurableJoints.Length; i++)
		{
			if (!(configurableJoints[i] == null))
			{
				jointDrive.positionSpring = positionSpring * maxJointTorqueProfile[i];
				jointDrive.positionDamper = positionDamper * jointDampingProfile[i];
				configurableJoints[i].slerpDrive = jointDrive;
			}
		}
		maxJointTorque = positionSpring;
		jointDamping = positionDamper;
	}

	public void SetJointTorque(float positionSpring)
	{
		for (int i = 1; i < configurableJoints.Length; i++)
		{
			if (!(configurableJoints[i] == null))
			{
				jointDrive.positionSpring = positionSpring * maxJointTorqueProfile[i];
				configurableJoints[i].slerpDrive = jointDrive;
			}
		}
		maxJointTorque = positionSpring;
	}

	public void EnableJointLimits(bool jointLimits)
	{
		for (int i = 1; i < configurableJoints.Length; i++)
		{
			if (!(configurableJoints[i] == null))
			{
				if (jointLimits)
				{
					configurableJoints[i].angularXMotion = ConfigurableJointMotion.Limited;
					configurableJoints[i].angularYMotion = ConfigurableJointMotion.Limited;
					configurableJoints[i].angularZMotion = ConfigurableJointMotion.Limited;
				}
				else
				{
					configurableJoints[i].angularXMotion = ConfigurableJointMotion.Free;
					configurableJoints[i].angularYMotion = ConfigurableJointMotion.Free;
					configurableJoints[i].angularZMotion = ConfigurableJointMotion.Free;
				}
			}
		}
	}

	private float FixEuler(float angle)
	{
		if (angle > 180f)
		{
			return angle - 360f;
		}
		return angle;
	}

	public static void PDControl(float P, float D, out Vector3 signal, Vector3 error, ref Vector3 lastError, float reciDeltaTime)
	{
		signal = P * (error + D * (error - lastError) * reciDeltaTime);
		lastError = error;
	}

	public void SyncTransforms()
	{
		try
		{
			for (int i = 0; i < slaveRigidbodies.Length; i++)
			{
				if (slaveRigidbodies[i] != null && masterRigidTransforms[i] != null)
				{
					slaveRigidbodies[i].transform.position = masterRigidTransforms[i].position;
					slaveRigidbodies[i].transform.rotation = masterRigidTransforms[i].rotation;
				}
			}
		}
		catch
		{
			throw new NullReferenceException(base.name);
		}
	}

	private void FixedUpdate()
	{
		DoAnimFollow();
	}
}
