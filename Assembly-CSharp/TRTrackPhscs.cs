using System;
using UnityEngine;

public class TRTrackPhscs : Trajectory
{
	public float maxTraction = 40f;

	public float dragCoefficient = 0.15f;

	public float gravity = 9.8f;

	public float maxHalfAngle = 90f;

	public float smoothAngleStart = 30f;

	public float smoothPercent = 0.7f;

	private new Transform target;

	private float timeNow;

	private Vector3 startPos;

	private Vector3 startFwd;

	private Vector3 velocity = Vector3.zero;

	private Vector3 velocityNext = Vector3.zero;

	private Vector3 drag;

	private Vector3 traction;

	private Vector3 acceleration;

	private float angle;

	private Vector3 subX;

	private Vector3 subY;

	private Vector3 targetPos;

	private float percent;

	public void Emit(Transform target, Vector3 emitFwd)
	{
		this.target = target;
		startFwd = emitFwd;
	}

	private void Start()
	{
		if (GetComponent<Rigidbody>() != null)
		{
			GetComponent<Rigidbody>().useGravity = false;
			startPos = base.transform.position;
			base.transform.rotation = Quaternion.FromToRotation(Vector3.forward, startFwd);
			traction = startFwd * maxTraction;
			acceleration = traction + Vector3.down * gravity;
			velocityNext = 0.5f * acceleration * 0.01f;
		}
		if (null != m_Emitter)
		{
			Emit(m_Target, m_Emitter.forward);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public override Vector3 Track(float deltaTime)
	{
		if (target == null)
		{
			return Vector3.zero;
		}
		timeNow += Time.deltaTime;
		velocity = velocityNext;
		targetPos = GetTargetCenter(target);
		drag = velocity.normalized * 0.054707f * dragCoefficient * velocity.sqrMagnitude + Vector3.up * gravity;
		subX = Vector3.Cross(velocity, targetPos - base.transform.position);
		subY = Vector3.Cross(subX, velocity).normalized;
		if (Vector3.Angle(velocity, targetPos - base.transform.position) <= smoothAngleStart)
		{
			if (drag.sqrMagnitude >= maxTraction * maxTraction)
			{
				traction = drag.normalized * maxTraction;
				acceleration = traction - drag;
			}
			else
			{
				percent = Vector3.Angle(velocity, targetPos - base.transform.position) / smoothAngleStart * smoothPercent + 1f - smoothPercent;
				if (Vector3.Angle(velocity, targetPos - base.transform.position) < 1f)
				{
					percent = 0f;
				}
				traction = Vector3.Lerp(velocity.normalized, subY, percent);
				angle = Vector3.Angle(traction, drag);
				traction *= Mathf.Sqrt(maxTraction * maxTraction - drag.sqrMagnitude * Mathf.Sin(angle / 180f * (float)Math.PI) * Mathf.Sin(angle / 180f * (float)Math.PI)) - drag.magnitude * Mathf.Cos(angle / 180f * (float)Math.PI);
				acceleration = traction;
			}
		}
		else
		{
			percent = maxHalfAngle / 90f;
			traction = Vector3.Lerp(velocity.normalized, subY.normalized, percent) * maxTraction;
			acceleration = traction - drag;
		}
		velocityNext = velocity + acceleration * deltaTime;
		return velocity * deltaTime + 0.5f * acceleration * deltaTime * deltaTime;
	}

	public override Quaternion Rotate(float deltaTime)
	{
		return Quaternion.FromToRotation(Vector3.forward, velocity);
	}
}
