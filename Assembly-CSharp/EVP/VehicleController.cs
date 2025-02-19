using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace EVP;

[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour
{
	public enum BrakeMode
	{
		Slip,
		Ratio
	}

	public enum UpdateRate
	{
		OnUpdate,
		OnFixedUpdate
	}

	public enum PositionMode
	{
		Accurate,
		Fast
	}

	public delegate void OnImpact();

	public Wheel[] wheels = new Wheel[0];

	[Header("Handling & behavior")]
	public Transform centerOfMass;

	[Range(0f, 3f)]
	public float tireFriction = 1f;

	[Range(0f, 1f)]
	public float antiRoll = 0.2f;

	[Range(0f, 89f)]
	public float maxSteerAngle = 35f;

	public float aeroDrag;

	public float aeroDownforce = 1f;

	public float aeroAppPointOffset = -0.5f;

	[Header("Motor")]
	[FormerlySerializedAs("maxSpeed")]
	public float maxSpeedForward = 27.78f;

	public float maxSpeedReverse = 12f;

	public float maxDriveForce = 2000f;

	[Range(0.0001f, 0.9999f)]
	public float forceCurveShape = 0.5f;

	public float maxDriveSlip = 4f;

	public float driveForceToMaxSlip = 1000f;

	[Header("Brakes")]
	public float maxBrakeForce = 3000f;

	public float dynamicBrakeForce = 800f;

	public float brakeForceToMaxSlip = 1000f;

	public BrakeMode brakeMode;

	public float maxBrakeSlip = 2f;

	[Range(0f, 1f)]
	public float maxBrakeRatio = 0.5f;

	public BrakeMode handbrakeMode;

	public float maxHandbrakeSlip = 10f;

	[Range(0f, 1f)]
	public float maxHandbrakeRatio = 1f;

	[Header("Driving aids")]
	public bool tcEnabled;

	[Range(0f, 1f)]
	public float tcRatio = 1f;

	public bool absEnabled;

	[Range(0f, 1f)]
	public float absRatio = 1f;

	public bool espEnabled;

	[Range(0f, 1f)]
	public float espRatio = 0.5f;

	[Header("Vehicle controls")]
	[Range(-1f, 1f)]
	public float steerInput;

	[Range(-1f, 1f)]
	public float throttleInput;

	[Range(0f, 1f)]
	public float brakeInput;

	[Range(0f, 1f)]
	public float handbrakeInput;

	[Header("Visual wheels")]
	public UpdateRate spinUpdateRate;

	public PositionMode wheelPositionMode;

	[Header("Optimization & Debug")]
	public bool disallowRuntimeChanges;

	public bool showContactGizmos;

	[NonSerialized]
	public bool processContacts;

	[NonSerialized]
	public float impactThreeshold = 0.6f;

	[NonSerialized]
	public float impactInterval = 0.2f;

	[NonSerialized]
	public float impactIntervalRandom = 0.4f;

	[NonSerialized]
	public float impactMinSpeed = 2f;

	public OnImpact onImpact;

	public static VehicleController current;

	public static float RpmToW = (float)Math.PI / 30f;

	public static float WToRpm = 30f / (float)Math.PI;

	private Transform m_transform;

	private Rigidbody m_rigidbody;

	[NonSerialized]
	public string debugText = string.Empty;

	private WheelData[] m_wheelData = new WheelData[0];

	private float m_speed;

	private float m_speedAngle;

	private CommonTools.BiasLerpContext m_forceBiasCtx = new CommonTools.BiasLerpContext();

	private float sleep_anglevel = 0.5f;

	private Collider[] m_colliders = new Collider[0];

	private int[] m_colLayers = new int[0];

	private WheelFrictionCurve m_colliderFriction;

	private int m_sumImpactCount;

	private Vector3 m_sumImpactPosition = Vector3.zero;

	private Vector3 m_sumImpactVelocity = Vector3.zero;

	private float m_sumImpactFriction;

	private float m_lastImpactTime;

	private Vector3 m_localDragPosition = Vector3.zero;

	private Vector3 m_localDragVelocity = Vector3.zero;

	private float m_localDragFriction;

	public Vector3 localImpactPosition => m_sumImpactPosition;

	public Vector3 localImpactVelocity => m_sumImpactVelocity;

	public float localImpactFriction => m_sumImpactFriction;

	public Vector3 localDragPosition => m_localDragPosition;

	public Vector3 localDragVelocity => m_localDragVelocity;

	public float localDragFriction => m_localDragFriction;

	public WheelData[] wheelData => m_wheelData;

	public float speed => m_speed;

	public float speedAngle => m_speedAngle;

	public Transform cachedTransform => m_transform;

	public Rigidbody cachedRigidbody => m_rigidbody;

	private void OnValidate()
	{
		maxDriveSlip = Mathf.Max(maxDriveSlip, 0f);
		maxBrakeSlip = Mathf.Max(maxBrakeSlip, 0f);
		maxHandbrakeSlip = Mathf.Max(maxHandbrakeSlip, 0f);
		maxDriveForce = Mathf.Max(maxDriveForce, 0f);
		maxBrakeForce = Mathf.Max(maxBrakeForce, 0f);
		driveForceToMaxSlip = Mathf.Max(driveForceToMaxSlip, 1f);
		brakeForceToMaxSlip = Mathf.Max(brakeForceToMaxSlip, 1f);
		maxSpeedForward = Mathf.Max(maxSpeedForward, 0f);
		maxSpeedReverse = Mathf.Max(maxSpeedReverse, 0f);
	}

	private void OnEnable()
	{
		m_transform = GetComponent<Transform>();
		m_rigidbody = GetComponent<Rigidbody>();
		FindColliders();
		m_rigidbody.maxAngularVelocity = 14f;
		if ((bool)centerOfMass)
		{
			m_rigidbody.centerOfMass = m_transform.InverseTransformPoint(centerOfMass.position);
		}
		m_wheelData = new WheelData[wheels.Length];
		for (int i = 0; i < m_wheelData.Length; i++)
		{
			m_wheelData[i] = new WheelData();
			m_wheelData[i].wheel = wheels[i];
			if (wheels[i].wheelCollider == null)
			{
				Debug.LogError("A WheelCollider is missing in the list of wheels for this vehicle: " + base.gameObject.name);
			}
			m_wheelData[i].collider = wheels[i].wheelCollider;
			m_wheelData[i].transform = wheels[i].wheelCollider.transform;
			UpdateWheelCollider(m_wheelData[i].collider);
			m_wheelData[i].forceDistance = GetWheelForceDistance(m_wheelData[i].collider);
		}
		WheelCollider componentInChildren = GetComponentInChildren<WheelCollider>();
		componentInChildren.ConfigureVehicleSubsteps(1000f, 1, 1);
		Wheel[] array = wheels;
		foreach (Wheel wheel in array)
		{
			SetupWheelCollider(wheel.wheelCollider);
			UpdateWheelCollider(wheel.wheelCollider);
		}
	}

	private void Update()
	{
		if (spinUpdateRate == UpdateRate.OnUpdate || wheelPositionMode == PositionMode.Accurate)
		{
			bool flag = m_rigidbody.interpolation != 0 && wheelPositionMode == PositionMode.Accurate;
			if (flag)
			{
				DisableCollidersRaycast();
			}
			WheelData[] array = m_wheelData;
			foreach (WheelData wd in array)
			{
				UpdateSteering(wd);
				UpdateTransform(wd);
			}
			if (flag)
			{
				EnableCollidersRaycast();
			}
		}
		if (processContacts)
		{
			UpdateDragState(Vector3.zero, Vector3.zero, localDragFriction);
		}
	}

	private void FixedUpdate()
	{
		throttleInput = Mathf.Clamp(throttleInput, -1f, 1f);
		brakeInput = Mathf.Clamp01(brakeInput);
		handbrakeInput = Mathf.Clamp01(handbrakeInput);
		m_speed = Vector3.Dot(m_rigidbody.velocity, m_transform.forward);
		m_speedAngle = Vector3.Angle(m_rigidbody.velocity, m_transform.forward) * Mathf.Sign(m_speed);
		bool flag = spinUpdateRate == UpdateRate.OnFixedUpdate && wheelPositionMode == PositionMode.Fast;
		int num = 0;
		WheelData[] array = m_wheelData;
		foreach (WheelData wheelData in array)
		{
			if (!disallowRuntimeChanges)
			{
				UpdateWheelCollider(wheelData.collider);
			}
			if (flag)
			{
				UpdateSteering(wheelData);
			}
			UpdateSuspension(wheelData);
			UpdateLocalFrame(wheelData);
			ComputeTireForces(wheelData);
			ApplyTireForces(wheelData);
			if (flag)
			{
				UpdateTransform(wheelData);
			}
			if (wheelData.grounded)
			{
				num++;
			}
		}
		float sqrMagnitude = m_rigidbody.velocity.sqrMagnitude;
		Vector3 force = (0f - aeroDrag) * sqrMagnitude * m_rigidbody.velocity.normalized;
		Vector3 force2 = (0f - aeroDownforce) * sqrMagnitude * m_transform.up;
		Vector3 position = m_transform.position + m_transform.forward * aeroAppPointOffset;
		m_rigidbody.AddForceAtPosition(force, position);
		if (num > 0)
		{
			m_rigidbody.AddForceAtPosition(force2, position);
		}
		if (processContacts)
		{
			HandleImpacts();
		}
	}

	private void UpdateSteering(WheelData wd)
	{
		if (wd.wheel.steer)
		{
			if (espEnabled && m_speed > 0f)
			{
				float num = m_speed * espRatio;
				float a = Mathf.Asin(Mathf.Clamp01(3f / num)) * 57.29578f;
				a = Mathf.Max(a, m_speedAngle);
				wd.steerAngle = Mathf.Clamp(maxSteerAngle * steerInput, 0f - a, a);
			}
			else
			{
				wd.steerAngle = maxSteerAngle * steerInput;
			}
		}
		else
		{
			wd.steerAngle = 0f;
		}
		wd.collider.steerAngle = FixSteerAngle(wd, wd.steerAngle);
	}

	private float FixSteerAngle(WheelData wd, float inputSteerAngle)
	{
		Quaternion quaternion = Quaternion.AngleAxis(inputSteerAngle, wd.transform.up);
		Vector3 vector = quaternion * wd.transform.forward;
		Vector3 vector2 = vector - Vector3.Project(vector, m_transform.up);
		return Vector3.Angle(m_transform.forward, vector2) * Mathf.Sign(Vector3.Dot(m_transform.right, vector2));
	}

	private void UpdateSuspension(WheelData wd)
	{
		wd.grounded = wd.collider.GetGroundHit(out wd.hit);
		if (wd.grounded && Physics.Raycast(wd.transform.position, -wd.transform.up, out var hitInfo, wd.collider.suspensionDistance + wd.collider.radius))
		{
			wd.hit.point = hitInfo.point;
			wd.hit.normal = hitInfo.normal;
		}
		if (wd.grounded)
		{
			wd.suspensionCompression = 1f - (0f - wd.transform.InverseTransformPoint(wd.hit.point).y - wd.collider.radius) / wd.collider.suspensionDistance;
			wd.downforce = Mathf.Clamp(wd.hit.force, 0f, m_rigidbody.mass * (0f - Physics.gravity.y));
		}
		else
		{
			wd.suspensionCompression = 0f;
			wd.downforce = 0f;
		}
	}

	private void UpdateLocalFrame(WheelData wd)
	{
		if (!wd.grounded)
		{
			wd.hit.point = wd.transform.position - wd.transform.up * (wd.collider.suspensionDistance + wd.collider.radius);
			wd.hit.normal = wd.transform.up;
			wd.hit.collider = null;
		}
		Vector3 pointVelocity = m_rigidbody.GetPointVelocity(wd.hit.point);
		if (wd.hit.collider != null)
		{
			Rigidbody attachedRigidbody = wd.hit.collider.attachedRigidbody;
			if (attachedRigidbody != null)
			{
				pointVelocity -= attachedRigidbody.GetPointVelocity(wd.hit.point);
			}
		}
		wd.velocity = pointVelocity - Vector3.Project(pointVelocity, wd.hit.normal);
		wd.localVelocity.y = Vector3.Dot(wd.hit.forwardDir, wd.velocity);
		wd.localVelocity.x = Vector3.Dot(wd.hit.sidewaysDir, wd.velocity);
		if (!wd.grounded)
		{
			wd.localRigForce = Vector2.zero;
			return;
		}
		float num = Mathf.InverseLerp(1f, 0.25f, wd.velocity.sqrMagnitude);
		Vector2 zero = default(Vector2);
		if (num > 0f)
		{
			float num2 = Vector3.Dot(Vector3.up, wd.hit.normal);
			Vector3 rhs;
			if (num2 > 1E-06f)
			{
				Vector3 vector = Vector3.up * wd.hit.force / num2;
				rhs = vector - Vector3.Project(vector, wd.hit.normal);
			}
			else
			{
				rhs = Vector3.up * 100000f;
			}
			zero.y = Vector3.Dot(wd.hit.forwardDir, rhs);
			zero.x = Vector3.Dot(wd.hit.sidewaysDir, rhs);
			zero *= num;
		}
		else
		{
			zero = Vector2.zero;
		}
		float num3 = Mathf.Clamp(wd.hit.force / (0f - Physics.gravity.y), 0f, wd.collider.sprungMass) * 0.5f;
		Vector2 vector2 = (0f - num3) * wd.localVelocity / Time.deltaTime;
		wd.localRigForce = vector2 + zero;
	}

	private void ComputeTireForces(WheelData wd)
	{
		float f = ((!wd.wheel.drive) ? 0f : throttleInput);
		float num = maxDriveSlip;
		if (Mathf.Sign(f) != Mathf.Sign(wd.localVelocity.y))
		{
			num -= wd.localVelocity.y * Mathf.Sign(f);
		}
		float num2 = 0f;
		float brakeRatio = 0f;
		float brakeSlip = 0f;
		if (wd.wheel.brake && wd.wheel.handbrake)
		{
			num2 = Mathf.Max(brakeInput, handbrakeInput);
			if (handbrakeInput >= brakeInput)
			{
				ComputeBrakeValues(wd, handbrakeMode, maxHandbrakeSlip, maxHandbrakeRatio, out brakeSlip, out brakeRatio);
			}
			else
			{
				ComputeBrakeValues(wd, brakeMode, maxBrakeSlip, maxBrakeRatio, out brakeSlip, out brakeRatio);
			}
		}
		else if (wd.wheel.brake)
		{
			num2 = brakeInput;
			ComputeBrakeValues(wd, brakeMode, maxBrakeSlip, maxBrakeRatio, out brakeSlip, out brakeRatio);
		}
		else if (wd.wheel.handbrake)
		{
			num2 = handbrakeInput;
			ComputeBrakeValues(wd, handbrakeMode, maxHandbrakeSlip, maxHandbrakeRatio, out brakeSlip, out brakeRatio);
		}
		float num3 = Mathf.Abs(f);
		float num4;
		bool flag;
		if (num3 >= num2)
		{
			num4 = (num3 - num2) * Mathf.Sign(f);
			flag = false;
		}
		else
		{
			num4 = num2 - num3;
			flag = true;
		}
		if (Mathf.Abs(wd.angularVelocity) < sleep_anglevel)
		{
			if (flag && Mathf.Abs(wd.angularVelocity) > 0.001f)
			{
				wd.sleep_time += Time.fixedDeltaTime * 5f;
			}
			if (Mathf.Abs(throttleInput) > 0.01f || Mathf.Abs(steerInput) > 0.01f || Mathf.Abs(brakeInput) > 0.01f)
			{
				wd.sleep_time = 0f;
			}
			else
			{
				wd.sleep_time += Time.fixedDeltaTime;
			}
			if (wd.sleep_time == 0f)
			{
				m_rigidbody.WakeUp();
			}
		}
		else
		{
			wd.sleep_time = 0f;
		}
		if (wd.sleep_time > 1f)
		{
			wd.angularVelocity *= 0.95f;
			if (Mathf.Abs(wd.angularVelocity) < 0.005f)
			{
				wd.angularVelocity = 0f;
				wd.localVelocity = Vector2.zero;
				m_rigidbody.angularVelocity = Vector3.zero;
				m_rigidbody.velocity = Vector3.zero;
				m_rigidbody.Sleep();
			}
			return;
		}
		float num5 = ((!flag) ? ComputeDriveForce(num4, num4 * maxDriveForce, maxDriveForce, wd.angularVelocity) : (Mathf.Clamp(Mathf.Abs(wd.angularVelocity), 0f, 1f) * maxBrakeForce));
		if (wd.grounded)
		{
			if (tcEnabled)
			{
				num = Mathf.Lerp(num, 0.1f, tcRatio);
			}
			if (absEnabled && brakeInput > handbrakeInput)
			{
				brakeSlip = Mathf.Lerp(brakeSlip, 0.1f, absRatio);
				brakeRatio = Mathf.Lerp(brakeRatio, brakeRatio * 0.1f, absRatio);
			}
		}
		if (wd.grounded)
		{
			wd.tireSlip.x = wd.localVelocity.x;
			wd.tireSlip.y = wd.localVelocity.y - wd.angularVelocity * wd.collider.radius;
			float num6;
			if (flag)
			{
				float max = Mathf.Max(Mathf.Abs(wd.localVelocity.y * brakeRatio), brakeSlip);
				num6 = Mathf.Clamp(Mathf.Abs(num5 * wd.tireSlip.x) / (tireFriction * wd.downforce), 0f, max);
			}
			else
			{
				num6 = Mathf.Min(Mathf.Abs(num5 * wd.tireSlip.x) / (tireFriction * wd.downforce), num);
				if (num5 != 0f && num6 < 0.1f)
				{
					num6 = 0.1f;
				}
			}
			if (Mathf.Abs(wd.tireSlip.y) < num6)
			{
				wd.tireSlip.y = num6 * Mathf.Sign(wd.tireSlip.y);
			}
			Vector2 vector = (0f - tireFriction) * wd.downforce * wd.tireSlip.normalized;
			vector.x = Mathf.Abs(vector.x);
			vector.y = Mathf.Abs(vector.y);
			wd.tireForce.x = Mathf.Clamp(wd.localRigForce.x, 0f - vector.x, vector.x);
			if (flag)
			{
				float num7 = Mathf.Min(vector.y, num5);
				wd.tireForce.y = Mathf.Clamp(wd.localRigForce.y, 0f - num7, num7);
			}
			else
			{
				wd.tireForce.y = Mathf.Clamp(num5, 0f - vector.y, vector.y);
			}
		}
		else
		{
			wd.tireSlip = Vector2.zero;
			wd.tireForce = Vector2.zero;
		}
		float num8 = ((!flag) ? driveForceToMaxSlip : brakeForceToMaxSlip);
		float num9 = Mathf.Clamp01((Mathf.Abs(num5) - Mathf.Abs(wd.tireForce.y)) / num8);
		float num10 = ((!flag) ? (num9 * num * Mathf.Sign(num5)) : Mathf.Clamp((0f - num9) * wd.localVelocity.y * brakeRatio, 0f - brakeSlip, brakeSlip));
		wd.angularVelocity = (wd.localVelocity.y + num10) / wd.collider.radius;
	}

	private void ApplyTireForces(WheelData wd)
	{
		if (wd.sleep_time < 1f && wd.grounded)
		{
			if (!disallowRuntimeChanges)
			{
				wd.forceDistance = GetWheelForceDistance(wd.collider);
			}
			Vector3 position = wd.hit.point + wd.transform.up * antiRoll * wd.forceDistance;
			Vector3 vector = wd.hit.forwardDir * wd.tireForce.y;
			Vector3 vector2 = wd.hit.sidewaysDir * wd.tireForce.x;
			m_rigidbody.AddForceAtPosition(vector, wd.hit.point);
			m_rigidbody.AddForceAtPosition(vector2, position);
			Rigidbody attachedRigidbody = wd.hit.collider.attachedRigidbody;
			if (attachedRigidbody != null && !attachedRigidbody.isKinematic)
			{
				attachedRigidbody.AddForceAtPosition(-vector, wd.hit.point);
				attachedRigidbody.AddForceAtPosition(-vector2, position);
			}
		}
	}

	private float ComputeDriveForce(float input, float demandedForce, float maxForce, float vel)
	{
		float num = Mathf.Abs(m_speed);
		float num2 = ((!(demandedForce >= 0f)) ? maxSpeedReverse : maxSpeedForward);
		float num3 = dynamicBrakeForce;
		if (Mathf.Abs(input) < 0.01f)
		{
			num3 *= 3f;
		}
		if (num < num2)
		{
			maxForce *= CommonTools.BiasedLerp(1f - num / num2, forceCurveShape, m_forceBiasCtx);
			return Mathf.Clamp(demandedForce, 0f - maxForce, maxForce) - Mathf.Clamp(vel, -1f, 1f) * num3;
		}
		return maxForce * Mathf.Max(1f - num / num2, -1f) * Mathf.Sign(m_speed) - Mathf.Clamp(vel, -1f, 1f) * num3;
	}

	private void ComputeBrakeValues(WheelData wd, BrakeMode mode, float maxSlip, float maxRatio, out float brakeSlip, out float brakeRatio)
	{
		if (mode == BrakeMode.Slip)
		{
			brakeSlip = maxSlip;
			brakeRatio = 1f;
		}
		else
		{
			brakeSlip = Mathf.Abs(wd.localVelocity.y);
			brakeRatio = maxRatio;
		}
	}

	private void UpdateTransform(WheelData wd)
	{
		if (wd.wheel.wheelTransform != null)
		{
			wd.angularPosition = (wd.angularPosition + wd.angularVelocity * Time.deltaTime) % ((float)Math.PI * 2f);
			RaycastHit hitInfo;
			if (wheelPositionMode == PositionMode.Fast)
			{
				wd.elongationTarget = wd.collider.suspensionDistance * (1f - wd.suspensionCompression) + wd.collider.radius * 0.05f;
			}
			else if (Physics.Raycast(wd.transform.position, -wd.transform.up, out hitInfo, wd.collider.suspensionDistance + wd.collider.radius))
			{
				wd.elongationTarget = hitInfo.distance - wd.collider.radius * 0.95f;
			}
			else
			{
				wd.elongationTarget = wd.collider.suspensionDistance + wd.collider.radius * 0.05f;
			}
			if (wd.elongation > wd.elongationTarget)
			{
				wd.elongation = wd.elongationTarget;
			}
			else
			{
				wd.elongation = Mathf.Lerp(wd.elongation, wd.elongationTarget, 0.2f);
			}
			Vector3 position = wd.transform.position - wd.transform.up * wd.elongation;
			wd.wheel.wheelTransform.position = position;
			wd.wheel.wheelTransform.rotation = wd.transform.rotation * Quaternion.Euler(wd.angularPosition * 57.29578f, wd.steerAngle, 0f);
		}
	}

	public void ResetVehicle()
	{
		Vector3 localEulerAngles = base.transform.localEulerAngles;
		m_rigidbody.MoveRotation(Quaternion.Euler(0f, localEulerAngles.y, 0f));
		m_rigidbody.MovePosition(m_rigidbody.position + Vector3.up * 1.6f);
		m_rigidbody.velocity = Vector3.zero;
		m_rigidbody.angularVelocity = Vector3.zero;
	}

	private void FindColliders()
	{
		m_colliders = GetComponentsInChildren<Collider>();
		m_colLayers = new int[m_colliders.Length];
		int i = 0;
		for (int num = m_colliders.Length; i < num; i++)
		{
			m_colLayers[i] = m_colliders[i].gameObject.layer;
		}
	}

	private void DisableCollidersRaycast()
	{
		Collider[] colliders = m_colliders;
		foreach (Collider collider in colliders)
		{
			collider.gameObject.layer = 2;
		}
	}

	private void EnableCollidersRaycast()
	{
		int i = 0;
		for (int num = m_colliders.Length; i < num; i++)
		{
			m_colliders[i].gameObject.layer = m_colLayers[i];
		}
	}

	private float GetWheelForceDistance(WheelCollider col)
	{
		return m_rigidbody.centerOfMass.y - m_transform.InverseTransformPoint(col.transform.position).y + col.radius + (1f - col.suspensionSpring.targetPosition) * col.suspensionDistance;
	}

	private void UpdateWheelCollider(WheelCollider col)
	{
		if (col.enabled)
		{
			float num = m_rigidbody.mass / (float)wheels.Length;
			float num2 = 0.3f;
			JointSpring suspensionSpring = col.suspensionSpring;
			suspensionSpring.targetPosition = num2;
			float num3 = num2 * col.suspensionDistance;
			suspensionSpring.spring = (0f - num) * Physics.gravity.y / num3;
			if (suspensionSpring.spring < 100000f)
			{
				suspensionSpring.damper = suspensionSpring.spring * 0.1f;
			}
			else if (suspensionSpring.spring < 1000000f)
			{
				suspensionSpring.damper = suspensionSpring.spring * 0.06f + 4000f;
			}
			else
			{
				suspensionSpring.damper = suspensionSpring.spring * 0.03f + 34000f;
			}
			col.suspensionSpring = suspensionSpring;
		}
	}

	private void SetupWheelCollider(WheelCollider col)
	{
		m_colliderFriction.stiffness = 0f;
		col.sidewaysFriction = m_colliderFriction;
		col.forwardFriction = m_colliderFriction;
		col.motorTorque = 1E-07f;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (processContacts)
		{
			ProcessContacts(collision, forceImpact: true);
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (processContacts)
		{
			ProcessContacts(collision, forceImpact: false);
		}
	}

	private void ProcessContacts(Collision col, bool forceImpact)
	{
		int num = 0;
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		float num2 = 0f;
		int num3 = 0;
		Vector3 zero3 = Vector3.zero;
		Vector3 zero4 = Vector3.zero;
		float num4 = 0f;
		float num5 = impactMinSpeed * impactMinSpeed;
		ContactPoint[] contacts = col.contacts;
		for (int i = 0; i < contacts.Length; i++)
		{
			ContactPoint contactPoint = contacts[i];
			Collider otherCollider = contactPoint.otherCollider;
			float num6 = ((!(otherCollider.sharedMaterial != null)) ? 0.6f : otherCollider.sharedMaterial.staticFriction);
			Vector3 pointVelocity = m_rigidbody.GetPointVelocity(contactPoint.point);
			if (otherCollider.attachedRigidbody != null)
			{
				pointVelocity -= otherCollider.attachedRigidbody.GetPointVelocity(contactPoint.point);
			}
			float num7 = Vector3.Dot(pointVelocity, contactPoint.normal);
			if (num7 < 0f - impactThreeshold || (forceImpact && col.relativeVelocity.sqrMagnitude > num5))
			{
				num++;
				zero += contactPoint.point;
				zero2 += col.relativeVelocity;
				num2 += num6;
				if (showContactGizmos)
				{
					Debug.DrawLine(contactPoint.point, contactPoint.point + CommonTools.Lin2Log(pointVelocity), Color.red);
				}
			}
			else if (num7 < impactThreeshold)
			{
				num3++;
				zero3 += contactPoint.point;
				zero4 += pointVelocity;
				num4 += num6;
				if (showContactGizmos)
				{
					Debug.DrawLine(contactPoint.point, contactPoint.point + CommonTools.Lin2Log(pointVelocity), Color.cyan);
				}
			}
			if (showContactGizmos)
			{
				Debug.DrawLine(contactPoint.point, contactPoint.point + contactPoint.normal * 0.25f, Color.yellow);
			}
		}
		if (num > 0)
		{
			float num8 = 1f / (float)num;
			zero *= num8;
			zero2 *= num8;
			num2 *= num8;
			m_sumImpactCount++;
			m_sumImpactPosition += m_transform.InverseTransformPoint(zero);
			m_sumImpactVelocity += m_transform.InverseTransformDirection(zero2);
			m_sumImpactFriction += num2;
		}
		if (num3 > 0)
		{
			float num9 = 1f / (float)num3;
			zero3 *= num9;
			zero4 *= num9;
			num4 *= num9;
			UpdateDragState(m_transform.InverseTransformPoint(zero3), m_transform.InverseTransformDirection(zero4), num4);
		}
	}

	private void HandleImpacts()
	{
		if (Time.time - m_lastImpactTime >= impactInterval && m_sumImpactCount > 0)
		{
			float num = 1f / (float)m_sumImpactCount;
			m_sumImpactPosition *= num;
			m_sumImpactVelocity *= num;
			m_sumImpactFriction *= num;
			if (onImpact != null)
			{
				current = this;
				onImpact();
				current = null;
			}
			if (showContactGizmos && localImpactVelocity.sqrMagnitude > 0.001f)
			{
				Debug.DrawLine(base.transform.TransformPoint(localImpactPosition), base.transform.TransformPoint(localImpactPosition) + CommonTools.Lin2Log(base.transform.TransformDirection(localImpactVelocity)), Color.red, 0.2f, depthTest: false);
			}
			m_sumImpactCount = 0;
			m_sumImpactPosition = Vector3.zero;
			m_sumImpactVelocity = Vector3.zero;
			m_sumImpactFriction = 0f;
			m_lastImpactTime = Time.time + impactInterval * UnityEngine.Random.Range(0f - impactIntervalRandom, impactIntervalRandom);
		}
	}

	private void UpdateDragState(Vector3 dragPosition, Vector3 dragVelocity, float dragFriction)
	{
		if (dragVelocity.sqrMagnitude > 0.001f)
		{
			m_localDragPosition = Vector3.Lerp(m_localDragPosition, dragPosition, 10f * Time.deltaTime);
			m_localDragVelocity = Vector3.Lerp(m_localDragVelocity, dragVelocity, 20f * Time.deltaTime);
			m_localDragFriction = dragFriction;
		}
		else
		{
			m_localDragVelocity = Vector3.Lerp(m_localDragVelocity, Vector3.zero, 10f * Time.deltaTime);
		}
		if (showContactGizmos && localDragVelocity.sqrMagnitude > 0.001f)
		{
			Debug.DrawLine(base.transform.TransformPoint(localDragPosition), base.transform.TransformPoint(localDragPosition) + CommonTools.Lin2Log(base.transform.TransformDirection(localDragVelocity)), Color.cyan, 0.05f, depthTest: false);
		}
	}

	[ContextMenu("Adjust WheelColliders to their meshes")]
	private void AdjustWheelColliders()
	{
		Wheel[] array = wheels;
		foreach (Wheel wheel in array)
		{
			if (wheel.wheelCollider != null)
			{
				AdjustColliderToWheelMesh(wheel.wheelCollider, wheel.wheelTransform);
			}
		}
	}

	private static void AdjustColliderToWheelMesh(WheelCollider wheelCollider, Transform wheelTransform)
	{
		if (wheelTransform == null)
		{
			Debug.LogError(wheelCollider.gameObject.name + ": A Wheel transform is required");
			return;
		}
		wheelCollider.transform.position = wheelTransform.position + wheelTransform.up * wheelCollider.suspensionDistance * 0.5f;
		wheelCollider.transform.rotation = wheelTransform.rotation;
		MeshFilter[] componentsInChildren = wheelTransform.GetComponentsInChildren<MeshFilter>();
		if (componentsInChildren == null || componentsInChildren.Length == 0)
		{
			Debug.LogWarning(wheelTransform.gameObject.name + ": Couldn't calculate radius. There are no meshes in the Wheel transform or its children");
			return;
		}
		Bounds scaledBounds = GetScaledBounds(componentsInChildren[0]);
		int i = 1;
		for (int num = componentsInChildren.Length; i < num; i++)
		{
			Bounds scaledBounds2 = GetScaledBounds(componentsInChildren[i]);
			scaledBounds.Encapsulate(scaledBounds2.min);
			scaledBounds.Encapsulate(scaledBounds2.max);
		}
		if (Mathf.Abs(scaledBounds.extents.y - scaledBounds.extents.z) > 0.01f)
		{
			Debug.LogWarning(wheelTransform.gameObject.name + ": The Wheel mesh might not be a correct wheel. The calculated radius is different along forward and vertical axis.");
		}
		wheelCollider.radius = scaledBounds.extents.y;
	}

	private static Bounds GetScaledBounds(MeshFilter meshFilter)
	{
		Bounds bounds = meshFilter.sharedMesh.bounds;
		Vector3 lossyScale = meshFilter.transform.lossyScale;
		bounds.max = Vector3.Scale(bounds.max, lossyScale);
		bounds.min = Vector3.Scale(bounds.min, lossyScale);
		return bounds;
	}
}
