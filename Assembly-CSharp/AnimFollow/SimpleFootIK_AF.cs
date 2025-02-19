using UnityEngine;

namespace AnimFollow;

public class SimpleFootIK_AF : MonoBehaviour
{
	private AnimFollow_AF animFollow;

	public Transform ragdoll;

	private Animator animator;

	public LayerMask layerMask;

	public string[] ignoreLayers = new string[1] { "Water" };

	private float deltaTime;

	private RaycastHit raycastHitLeftFoot;

	private RaycastHit raycastHitRightFoot;

	private RaycastHit raycastHitToe;

	[Range(4f, 20f)]
	public float raycastLength = 5f;

	[Range(0.2f, 0.9f)]
	public float maxStepHeight = 0.5f;

	[Range(0f, 1f)]
	public float footIKWeight = 1f;

	[Range(1f, 100f)]
	public float footNormalLerp = 40f;

	[Range(1f, 100f)]
	public float footTargetLerp = 40f;

	[Range(1f, 100f)]
	public float transformYLerp = 20f;

	[HideInInspector]
	public float extraYLerp = 1f;

	[Range(0f, 1f)]
	public float maxIncline = 0.8f;

	public bool followTerrain = true;

	private bool userNeedsToFixStuff;

	private float footHeight;

	private Transform leftToe;

	private Transform leftFoot;

	private Transform leftCalf;

	private Transform leftThigh;

	private Transform rightToe;

	private Transform rightFoot;

	private Transform rightCalf;

	private Transform rightThigh;

	private Quaternion leftFootRotation;

	private Quaternion rightFootRotation;

	private Vector3 leftFootTargetPos;

	private Vector3 leftFootTargetNormal;

	private Vector3 lastLeftFootTargetPos;

	private Vector3 lastLeftFootTargetNormal;

	private Vector3 rightFootTargetPos;

	private Vector3 rightFootTargetNormal;

	private Vector3 lastRightFootTargetPos;

	private Vector3 lastRightFootTargetNormal;

	private Vector3 footForward;

	private float leftLegTargetLength;

	private float rightLegTargetLength;

	private float thighLength;

	private float thighLengthSquared;

	private float calfLength;

	private float calfLengthSquared;

	private float reciDenominator;

	private float leftKneeAngle;

	private float leftThighAngle;

	private float rightKneeAngle;

	private float rightThighAngle;

	private void PositionFeet()
	{
		leftFootRotation = leftFoot.rotation;
		rightFootRotation = rightFoot.rotation;
		leftFootTargetNormal = Vector3.Lerp(Vector3.up, raycastHitLeftFoot.normal, footIKWeight);
		leftFootTargetNormal = Vector3.Lerp(lastLeftFootTargetNormal, leftFootTargetNormal, footNormalLerp * deltaTime);
		lastLeftFootTargetNormal = leftFootTargetNormal;
		rightFootTargetNormal = Vector3.Lerp(Vector3.up, raycastHitRightFoot.normal, footIKWeight);
		rightFootTargetNormal = Vector3.Lerp(lastRightFootTargetNormal, rightFootTargetNormal, footNormalLerp * deltaTime);
		lastRightFootTargetNormal = rightFootTargetNormal;
		leftFootTargetPos = raycastHitLeftFoot.point + leftFootTargetNormal * footHeight + (leftFoot.position.y - footHeight - base.transform.position.y) * Vector3.up;
		leftFootTargetPos = Vector3.Lerp(leftFoot.position, leftFootTargetPos, footIKWeight);
		leftFootTargetPos = Vector3.Lerp(lastLeftFootTargetPos, leftFootTargetPos, footTargetLerp * deltaTime);
		lastLeftFootTargetPos = leftFootTargetPos;
		rightFootTargetPos = raycastHitRightFoot.point + rightFootTargetNormal * footHeight + (rightFoot.position.y - footHeight - base.transform.position.y) * Vector3.up;
		rightFootTargetPos = Vector3.Lerp(rightFoot.position, rightFootTargetPos, footIKWeight);
		rightFootTargetPos = Vector3.Lerp(lastRightFootTargetPos, rightFootTargetPos, footTargetLerp * deltaTime);
		lastRightFootTargetPos = rightFootTargetPos;
		leftLegTargetLength = Mathf.Min((leftFootTargetPos - leftThigh.position).magnitude, calfLength + thighLength - 0.01f);
		leftLegTargetLength = Mathf.Max(leftLegTargetLength, 0.2f);
		leftKneeAngle = Mathf.Acos((Mathf.Pow(leftLegTargetLength, 2f) - calfLengthSquared - thighLengthSquared) * reciDenominator);
		leftKneeAngle *= 57.29578f;
		leftCalf.Rotate(0f, 0f, 180f - leftKneeAngle - leftCalf.localEulerAngles.z);
		leftThigh.rotation = Quaternion.FromToRotation(leftFoot.position - leftThigh.position, leftFootTargetPos - leftThigh.position) * leftThigh.rotation;
		rightLegTargetLength = Mathf.Min((rightFootTargetPos - rightThigh.position).magnitude, calfLength + thighLength - 0.01f);
		rightLegTargetLength = Mathf.Max(rightLegTargetLength, 0.2f);
		rightKneeAngle = Mathf.Acos((Mathf.Pow(rightLegTargetLength, 2f) - calfLengthSquared - thighLengthSquared) * reciDenominator);
		rightKneeAngle *= 57.29578f;
		rightCalf.Rotate(0f, 0f, 180f - rightKneeAngle - rightCalf.localEulerAngles.z);
		rightThigh.rotation = Quaternion.FromToRotation(rightFoot.position - rightThigh.position, rightFootTargetPos - rightThigh.position) * rightThigh.rotation;
		leftFoot.rotation = Quaternion.FromToRotation(Vector3.up, leftFootTargetNormal) * leftFootRotation;
		rightFoot.rotation = Quaternion.FromToRotation(Vector3.up, rightFootTargetNormal) * rightFootRotation;
	}

	private void Awake2()
	{
		string[] array = ignoreLayers;
		foreach (string layerName in array)
		{
			layerMask = (int)layerMask | (1 << LayerMask.NameToLayer(layerName));
		}
		layerMask = ~(int)layerMask;
		if (!ragdoll)
		{
			Debug.LogWarning("ragdoll not assigned in SimpleFootIK script on " + base.name + "\nThis Foot IK is for use with an AnimFollow system\n");
			userNeedsToFixStuff = true;
		}
		else
		{
			if (!(animFollow = ragdoll.GetComponent<AnimFollow_AF>()))
			{
				Debug.LogWarning("Missing script: AnimFollow on " + ragdoll.name + "\nThis Foot IK is for use with an AnimFollow system\n");
				userNeedsToFixStuff = true;
			}
			bool flag = false;
			string[] array2 = ignoreLayers;
			foreach (string layerName2 in array2)
			{
				if (ragdoll.gameObject.layer.Equals(LayerMask.NameToLayer(layerName2)))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Debug.LogWarning("Layer for " + ragdoll.name + " and its children must be set to an ignored layer\n");
				userNeedsToFixStuff = true;
			}
		}
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
		for (int k = 0; k < componentsInChildren.Length; k++)
		{
			if (componentsInChildren[k].name.ToLower().Contains("foot") && componentsInChildren[k].name.ToLower().Contains("left"))
			{
				leftToe = componentsInChildren[k + 1];
				leftFoot = componentsInChildren[k];
				leftCalf = componentsInChildren[k - 1];
				leftThigh = componentsInChildren[k - 2];
				if ((bool)rightFoot)
				{
					break;
				}
			}
			if (componentsInChildren[k].name.ToLower().Contains("foot") && componentsInChildren[k].name.ToLower().Contains("right"))
			{
				rightToe = componentsInChildren[k + 1];
				rightFoot = componentsInChildren[k];
				rightCalf = componentsInChildren[k - 1];
				rightThigh = componentsInChildren[k - 2];
				if ((bool)leftFoot)
				{
					break;
				}
			}
		}
		if (!leftToe || !rightToe)
		{
			Debug.LogWarning("Auto assigning of legs failed. Look at lines 32-57 in script IK_Setup\n");
			userNeedsToFixStuff = true;
			return;
		}
		thighLength = (rightThigh.position - rightCalf.position).magnitude;
		thighLengthSquared = (rightThigh.position - rightCalf.position).sqrMagnitude;
		calfLength = (rightCalf.position - rightFoot.position).magnitude;
		calfLengthSquared = (rightCalf.position - rightFoot.position).sqrMagnitude;
		reciDenominator = -0.5f / calfLength / thighLength;
		if (footHeight == 0f)
		{
			footHeight = 0.132f;
		}
	}

	private void ShootIKRays()
	{
		if (!Physics.Raycast(rightFoot.position + Vector3.up * maxStepHeight, Vector3.down, out raycastHitRightFoot, raycastLength, layerMask) && !Physics.Raycast(rightFoot.position + Vector3.up * 2f, Vector3.down, out raycastHitRightFoot, raycastLength * 4f, layerMask))
		{
			raycastHitRightFoot.normal = Vector3.up;
			raycastHitRightFoot.point = new Vector3(rightFoot.position.x, base.transform.position.y - 0.01f, rightFoot.position.z);
		}
		footForward = rightToe.position - rightFoot.position;
		footForward = new Vector3(footForward.x, 0f, footForward.z);
		footForward = Quaternion.FromToRotation(Vector3.up, raycastHitRightFoot.normal) * footForward;
		if (!Physics.Raycast(rightFoot.position + footForward + Vector3.up * maxStepHeight, Vector3.down, out raycastHitToe, maxStepHeight * 2f, layerMask))
		{
			raycastHitToe.normal = raycastHitRightFoot.normal;
			raycastHitToe.point = raycastHitRightFoot.point + footForward;
		}
		else
		{
			if (raycastHitRightFoot.point.y < raycastHitToe.point.y - footForward.y)
			{
				raycastHitRightFoot.point = new Vector3(raycastHitRightFoot.point.x, raycastHitToe.point.y - footForward.y, raycastHitRightFoot.point.z);
			}
			raycastHitRightFoot.normal = (raycastHitRightFoot.normal + raycastHitToe.normal).normalized;
		}
		if (!Physics.Raycast(leftFoot.position + Vector3.up * maxStepHeight, Vector3.down, out raycastHitLeftFoot, raycastLength, layerMask) && !Physics.Raycast(leftFoot.position + Vector3.up * 2f, Vector3.down, out raycastHitLeftFoot, raycastLength * 4f, layerMask))
		{
			raycastHitLeftFoot.normal = Vector3.up;
			raycastHitLeftFoot.point = new Vector3(leftFoot.position.x, base.transform.position.y - 0.01f, leftFoot.position.z);
		}
		footForward = leftToe.position - leftFoot.position;
		footForward = new Vector3(footForward.x, 0f, footForward.z);
		footForward = Quaternion.FromToRotation(Vector3.up, raycastHitLeftFoot.normal) * footForward;
		if (!Physics.Raycast(leftFoot.position + footForward + Vector3.up * maxStepHeight, Vector3.down, out raycastHitToe, maxStepHeight * 2f, layerMask))
		{
			raycastHitToe.normal = raycastHitLeftFoot.normal;
			raycastHitToe.point = raycastHitLeftFoot.point + footForward;
		}
		else
		{
			if (raycastHitLeftFoot.point.y < raycastHitToe.point.y - footForward.y)
			{
				raycastHitLeftFoot.point = new Vector3(raycastHitLeftFoot.point.x, raycastHitToe.point.y - footForward.y, raycastHitLeftFoot.point.z);
			}
			raycastHitLeftFoot.normal = (raycastHitLeftFoot.normal + raycastHitToe.normal).normalized;
		}
		if (Vector3.Dot(raycastHitLeftFoot.normal, Vector3.up) < 1f - maxIncline)
		{
			raycastHitLeftFoot.normal = Vector3.RotateTowards(Vector3.up, raycastHitLeftFoot.normal, Mathf.Asin(maxIncline), 0f);
		}
		if (Vector3.Dot(raycastHitRightFoot.normal, Vector3.up) < 1f - maxIncline)
		{
			raycastHitRightFoot.normal = Vector3.RotateTowards(Vector3.up, raycastHitRightFoot.normal, Mathf.Asin(maxIncline), 0f);
		}
		if (followTerrain)
		{
			base.transform.position = new Vector3(base.transform.position.x, Mathf.Lerp(base.transform.position.y, Mathf.Min(raycastHitLeftFoot.point.y, raycastHitRightFoot.point.y), transformYLerp * extraYLerp * deltaTime), base.transform.position.z);
		}
	}

	private void Awake()
	{
		Awake2();
	}

	private void FixedUpdate()
	{
		deltaTime = Time.fixedDeltaTime;
		DoSimpleFootIK();
	}

	private void DoSimpleFootIK()
	{
		if (!userNeedsToFixStuff)
		{
			ShootIKRays();
			PositionFeet();
			animFollow.DoAnimFollow();
		}
	}
}
