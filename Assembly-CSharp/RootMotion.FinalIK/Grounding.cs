using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class Grounding
{
	[Serializable]
	public enum Quality
	{
		Fastest,
		Simple,
		Best
	}

	public class Leg
	{
		private Grounding grounding;

		private float lastTime;

		private float deltaTime;

		private Vector3 lastPosition;

		private Quaternion toHitNormal;

		private Quaternion r;

		private RaycastHit heelHit;

		private Vector3 up = Vector3.up;

		public bool isGrounded { get; private set; }

		public Vector3 IKPosition { get; private set; }

		public Quaternion rotationOffset { get; private set; }

		public bool initiated { get; private set; }

		public float heightFromGround { get; private set; }

		public Vector3 velocity { get; private set; }

		public Transform transform { get; private set; }

		public float IKOffset { get; private set; }

		public float stepHeightFromGround => Mathf.Clamp(heightFromGround, 0f - grounding.maxStep, grounding.maxStep);

		private float rootYOffset => grounding.GetVerticalOffset(transform.position, grounding.root.position - up * grounding.heightOffset);

		public void Initiate(Grounding grounding, Transform transform)
		{
			initiated = false;
			this.grounding = grounding;
			this.transform = transform;
			up = Vector3.up;
			initiated = true;
			OnEnable();
		}

		public void OnEnable()
		{
			if (initiated)
			{
				lastPosition = transform.position;
				lastTime = Time.deltaTime;
			}
		}

		public void Process()
		{
			if (!initiated || grounding.maxStep <= 0f)
			{
				return;
			}
			deltaTime = Time.time - lastTime;
			lastTime = Time.time;
			if (deltaTime == 0f)
			{
				return;
			}
			up = grounding.up;
			heightFromGround = float.PositiveInfinity;
			velocity = (transform.position - lastPosition) / deltaTime;
			velocity = grounding.Flatten(velocity);
			lastPosition = transform.position;
			Vector3 vector = velocity * grounding.prediction;
			if (grounding.footRadius <= 0f)
			{
				grounding.quality = Quality.Fastest;
			}
			switch (grounding.quality)
			{
			case Quality.Fastest:
			{
				RaycastHit raycastHit3 = GetRaycastHit(vector);
				SetFootToPoint(raycastHit3.normal, raycastHit3.point);
				break;
			}
			case Quality.Simple:
			{
				heelHit = GetRaycastHit(Vector3.zero);
				RaycastHit raycastHit = GetRaycastHit(grounding.root.forward * grounding.footRadius + vector);
				RaycastHit raycastHit2 = GetRaycastHit(grounding.root.right * grounding.footRadius * 0.5f);
				Vector3 vector2 = Vector3.Cross(raycastHit.point - heelHit.point, raycastHit2.point - heelHit.point).normalized;
				if (Vector3.Dot(vector2, up) < 0f)
				{
					vector2 = -vector2;
				}
				SetFootToPlane(vector2, heelHit.point, heelHit.point);
				break;
			}
			case Quality.Best:
			{
				heelHit = GetRaycastHit(Vector3.zero);
				RaycastHit capsuleHit = GetCapsuleHit(vector);
				SetFootToPlane(capsuleHit.normal, capsuleHit.point, heelHit.point);
				break;
			}
			}
			isGrounded = heightFromGround < grounding.maxStep;
			float num = stepHeightFromGround;
			if (!grounding.rootGrounded)
			{
				num = 0f;
			}
			IKOffset = Interp.LerpValue(IKOffset, num, grounding.footSpeed, grounding.footSpeed);
			IKOffset = Mathf.Lerp(IKOffset, num, deltaTime * grounding.footSpeed);
			float verticalOffset = grounding.GetVerticalOffset(transform.position, grounding.root.position);
			float num2 = Mathf.Clamp(grounding.maxStep - verticalOffset, 0f, grounding.maxStep);
			IKOffset = Mathf.Clamp(IKOffset, 0f - num2, IKOffset);
			RotateFoot();
			IKPosition = transform.position - up * IKOffset;
			float footRotationWeight = grounding.footRotationWeight;
			rotationOffset = ((!(footRotationWeight >= 1f)) ? Quaternion.Slerp(Quaternion.identity, r, footRotationWeight) : r);
		}

		private RaycastHit GetCapsuleHit(Vector3 offsetFromHeel)
		{
			RaycastHit hitInfo = default(RaycastHit);
			Vector3 vector = transform.position + grounding.root.forward * grounding.footRadius;
			hitInfo.point = vector - up * grounding.maxStep * 2f;
			hitInfo.normal = up;
			Vector3 vector2 = vector + grounding.maxStep * up;
			Vector3 point = vector2 + offsetFromHeel;
			Physics.CapsuleCast(vector2, point, grounding.footRadius, -up, out hitInfo, grounding.maxStep * 3f, grounding.layers, QueryTriggerInteraction.Ignore);
			return hitInfo;
		}

		private RaycastHit GetRaycastHit(Vector3 offsetFromHeel)
		{
			RaycastHit hitInfo = default(RaycastHit);
			Vector3 vector = transform.position + offsetFromHeel;
			hitInfo.point = vector - up * grounding.maxStep * 2f;
			hitInfo.normal = up;
			if (grounding.maxStep <= 0f)
			{
				return hitInfo;
			}
			Physics.Raycast(vector + grounding.maxStep * up, -up, out hitInfo, grounding.maxStep * 3f, grounding.layers, QueryTriggerInteraction.Ignore);
			return hitInfo;
		}

		private Vector3 RotateNormal(Vector3 normal)
		{
			if (grounding.quality == Quality.Best)
			{
				return normal;
			}
			return Vector3.RotateTowards(up, normal, grounding.maxFootRotationAngle * ((float)Math.PI / 180f), deltaTime);
		}

		private void SetFootToPoint(Vector3 normal, Vector3 point)
		{
			toHitNormal = Quaternion.FromToRotation(up, RotateNormal(normal));
			heightFromGround = GetHeightFromGround(point);
		}

		private void SetFootToPlane(Vector3 planeNormal, Vector3 planePoint, Vector3 heelHitPoint)
		{
			planeNormal = RotateNormal(planeNormal);
			toHitNormal = Quaternion.FromToRotation(up, planeNormal);
			Vector3 hitPoint = V3Tools.LineToPlane(transform.position + up * grounding.maxStep, -up, planeNormal, planePoint);
			heightFromGround = GetHeightFromGround(hitPoint);
			float max = GetHeightFromGround(heelHitPoint);
			heightFromGround = Mathf.Clamp(heightFromGround, float.NegativeInfinity, max);
		}

		private float GetHeightFromGround(Vector3 hitPoint)
		{
			return grounding.GetVerticalOffset(transform.position, hitPoint) - rootYOffset;
		}

		private void RotateFoot()
		{
			Quaternion rotationOffsetTarget = GetRotationOffsetTarget();
			r = Quaternion.Slerp(r, rotationOffsetTarget, deltaTime * grounding.footRotationSpeed);
		}

		private Quaternion GetRotationOffsetTarget()
		{
			if (grounding.maxFootRotationAngle <= 0f)
			{
				return Quaternion.identity;
			}
			if (grounding.maxFootRotationAngle >= 180f)
			{
				return toHitNormal;
			}
			return Quaternion.RotateTowards(Quaternion.identity, toHitNormal, grounding.maxFootRotationAngle);
		}
	}

	public class Pelvis
	{
		private Grounding grounding;

		private Vector3 lastRootPosition;

		private float damperF;

		private bool initiated;

		private float lastTime;

		public Vector3 IKOffset { get; private set; }

		public float heightOffset { get; private set; }

		public void Initiate(Grounding grounding)
		{
			this.grounding = grounding;
			initiated = true;
			OnEnable();
		}

		public void OnEnable()
		{
			if (initiated)
			{
				lastRootPosition = grounding.root.transform.position;
				lastTime = Time.deltaTime;
			}
		}

		public void Process(float lowestOffset, float highestOffset, bool isGrounded)
		{
			if (!initiated)
			{
				return;
			}
			float num = Time.time - lastTime;
			lastTime = Time.time;
			if (!(num <= 0f))
			{
				float b = lowestOffset + highestOffset;
				if (!grounding.rootGrounded)
				{
					b = 0f;
				}
				heightOffset = Mathf.Lerp(heightOffset, b, num * grounding.pelvisSpeed);
				Vector3 p = grounding.root.position - lastRootPosition;
				lastRootPosition = grounding.root.position;
				damperF = Interp.LerpValue(damperF, (!isGrounded) ? 0f : 1f, 1f, 10f);
				heightOffset -= grounding.GetVerticalOffset(p, Vector3.zero) * grounding.pelvisDamper * damperF;
				IKOffset = grounding.up * heightOffset;
			}
		}
	}

	public LayerMask layers;

	public float maxStep = 0.5f;

	public float heightOffset;

	public float footSpeed = 2.5f;

	public float footRadius = 0.15f;

	public float prediction = 0.05f;

	[Range(0f, 1f)]
	public float footRotationWeight = 1f;

	public float footRotationSpeed = 7f;

	[Range(0f, 90f)]
	public float maxFootRotationAngle = 45f;

	public bool rotateSolver;

	public float pelvisSpeed = 5f;

	[Range(0f, 1f)]
	public float pelvisDamper;

	public float lowerPelvisWeight = 1f;

	public float liftPelvisWeight;

	public float rootSphereCastRadius = 0.1f;

	public Quality quality = Quality.Best;

	private bool initiated;

	public Leg[] legs { get; private set; }

	public Pelvis pelvis { get; private set; }

	public bool isGrounded { get; private set; }

	public Transform root { get; private set; }

	public RaycastHit rootHit { get; private set; }

	public bool rootGrounded => rootHit.distance < maxStep * 2f;

	public Vector3 up => (!useRootRotation) ? Vector3.up : root.up;

	private bool useRootRotation
	{
		get
		{
			if (!rotateSolver)
			{
				return false;
			}
			if (root.up == Vector3.up)
			{
				return false;
			}
			return true;
		}
	}

	public RaycastHit GetRootHit(float maxDistanceMlp = 10f)
	{
		RaycastHit hitInfo = default(RaycastHit);
		Vector3 vector = up;
		hitInfo.point = root.position - vector * maxStep * 10f;
		float num = maxDistanceMlp + 1f;
		hitInfo.distance = maxStep * num;
		if (maxStep <= 0f)
		{
			return hitInfo;
		}
		if (quality != Quality.Best)
		{
			Physics.Raycast(root.position + vector * maxStep, -vector, out hitInfo, maxStep * num, layers, QueryTriggerInteraction.Ignore);
		}
		else
		{
			Physics.SphereCast(root.position + vector * maxStep, rootSphereCastRadius, -up, out hitInfo, maxStep * num, layers, QueryTriggerInteraction.Ignore);
		}
		return hitInfo;
	}

	public bool IsValid(bool log)
	{
		if (root == null)
		{
			if (log)
			{
				LogWarning("Root transform is null. Can't initiate Grounding.");
			}
			return false;
		}
		if (legs == null)
		{
			if (log)
			{
				LogWarning("Grounding legs is null. Can't initiate Grounding.");
			}
			return false;
		}
		if (pelvis == null)
		{
			if (log)
			{
				LogWarning("Grounding pelvis is null. Can't initiate Grounding.");
			}
			return false;
		}
		if (legs.Length == 0)
		{
			if (log)
			{
				LogWarning("Grounding has 0 legs. Can't initiate Grounding.");
			}
			return false;
		}
		return true;
	}

	public void Initiate(Transform root, Transform[] feet)
	{
		this.root = root;
		initiated = false;
		rootHit = default(RaycastHit);
		if (legs == null)
		{
			legs = new Leg[feet.Length];
		}
		if (legs.Length != feet.Length)
		{
			legs = new Leg[feet.Length];
		}
		for (int i = 0; i < feet.Length; i++)
		{
			if (legs[i] == null)
			{
				legs[i] = new Leg();
			}
		}
		if (pelvis == null)
		{
			pelvis = new Pelvis();
		}
		if (IsValid(log: true) && Application.isPlaying)
		{
			for (int j = 0; j < feet.Length; j++)
			{
				legs[j].Initiate(this, feet[j]);
			}
			pelvis.Initiate(this);
			initiated = true;
		}
	}

	public void Update()
	{
		if (!initiated)
		{
			return;
		}
		if ((int)layers == 0)
		{
			LogWarning("Grounding layers are set to nothing. Please add a ground layer.");
		}
		maxStep = Mathf.Clamp(maxStep, 0f, maxStep);
		footRadius = Mathf.Clamp(footRadius, 0.0001f, maxStep);
		pelvisDamper = Mathf.Clamp(pelvisDamper, 0f, 1f);
		rootSphereCastRadius = Mathf.Clamp(rootSphereCastRadius, 0.0001f, rootSphereCastRadius);
		maxFootRotationAngle = Mathf.Clamp(maxFootRotationAngle, 0f, 90f);
		prediction = Mathf.Clamp(prediction, 0f, prediction);
		footSpeed = Mathf.Clamp(footSpeed, 0f, footSpeed);
		rootHit = GetRootHit();
		float num = float.NegativeInfinity;
		float num2 = float.PositiveInfinity;
		isGrounded = false;
		Leg[] array = legs;
		foreach (Leg leg in array)
		{
			leg.Process();
			if (leg.IKOffset > num)
			{
				num = leg.IKOffset;
			}
			if (leg.IKOffset < num2)
			{
				num2 = leg.IKOffset;
			}
			if (leg.isGrounded)
			{
				isGrounded = true;
			}
		}
		pelvis.Process((0f - num) * lowerPelvisWeight, (0f - num2) * liftPelvisWeight, isGrounded);
	}

	public Vector3 GetLegsPlaneNormal()
	{
		if (!initiated)
		{
			return Vector3.up;
		}
		Vector3 vector = up;
		Vector3 vector2 = vector;
		for (int i = 0; i < legs.Length; i++)
		{
			Vector3 vector3 = legs[i].IKPosition - root.position;
			Vector3 normal = vector;
			Vector3 tangent = vector3;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			Quaternion quaternion = Quaternion.FromToRotation(tangent, vector3);
			vector2 = quaternion * vector2;
		}
		return vector2;
	}

	public void LogWarning(string message)
	{
		Warning.Log(message, root);
	}

	public float GetVerticalOffset(Vector3 p1, Vector3 p2)
	{
		if (useRootRotation)
		{
			return (Quaternion.Inverse(root.rotation) * (p1 - p2)).y;
		}
		return p1.y - p2.y;
	}

	public Vector3 Flatten(Vector3 v)
	{
		if (useRootRotation)
		{
			Vector3 tangent = v;
			Vector3 normal = root.up;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			return Vector3.Project(v, tangent);
		}
		v.y = 0f;
		return v;
	}
}
