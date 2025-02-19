using System;
using UnityEngine;

internal class TRSiloMissile : Trajectory
{
	public int index;

	public float random1;

	public float delay = 0.1f;

	public float interval = 0.1f;

	public float maxSpeed = 35f;

	public float acceleration = 50f;

	public float lerpSpeed = 90f;

	public float parameterA = 3f;

	public float parameterB = 1.5f;

	public Vector3 fwd = Vector3.zero;

	public float maxUpSpeed = 20f;

	public float AngleUnit = 150f;

	public float angleSpeed = 3f;

	public float angleSpeed2 = 2f;

	public float lerpMin = 0.25f;

	public float progress1 = 0.05f;

	public float progress2 = 0.15f;

	public float progress3 = 0.5f;

	public float progress4 = 0.8f;

	public float angle1 = 0.3333f;

	public float angle2 = 0.6666f;

	public float angle3 = 1.3333f;

	public float angle4 = 2f;

	private Transform myTarget;

	private bool valid;

	private bool trackChance = true;

	private bool loseTarget;

	private float angle;

	private Vector3 direction;

	private Vector3 mainPos;

	private Vector3 refPos;

	private Vector3 finalPos;

	private float timeNow;

	private Vector3 startPos;

	private float progress;

	private Vector3 subZ;

	private float speed;

	private Vector3 refY;

	private Vector3 refX = Vector3.one;

	private Vector3 refZ = Vector3.one;

	private float angleStart;

	private Vector3 targetCenter;

	private float upSpeed;

	private float coordinateX;

	private float coordinateZ;

	private float lerpX;

	private float lerpY;

	private void Start()
	{
		index = m_Index;
		random1 = 0.5f;
		if (null != m_Emitter)
		{
			if ((bool)m_Target)
			{
				Emit(m_Target, m_Emitter.forward);
			}
			else
			{
				Emit(m_TargetPosition, m_Emitter.forward);
			}
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void Emit(Transform target, Vector3 emitfwd)
	{
		myTarget = target;
		angleStart = AngleUnit * (float)index + random1 * 360f;
		subZ = GetTargetCenter(target) - startPos;
		if (fwd == Vector3.zero)
		{
			direction = emitfwd;
		}
		else
		{
			direction = fwd;
		}
		refY = direction;
		Vector3.OrthoNormalize(ref refY, ref refX, ref refZ);
		base.transform.transform.GetChild(0).gameObject.SetActive(value: false);
		base.transform.transform.GetChild(1).gameObject.SetActive(value: false);
	}

	public void Emit(Vector3 targetPos, Vector3 emitfwd)
	{
		angleStart = AngleUnit * (float)index + random1 * 360f;
		subZ = targetPos - startPos;
		if (fwd == Vector3.zero)
		{
			direction = emitfwd;
		}
		else
		{
			direction = fwd;
		}
		refY = direction;
		Vector3.OrthoNormalize(ref refY, ref refX, ref refZ);
		base.transform.transform.GetChild(0).gameObject.SetActive(value: false);
		base.transform.transform.GetChild(1).gameObject.SetActive(value: false);
	}

	public override Vector3 Track(float deltaTime)
	{
		if (!valid)
		{
			MissileSwitch(deltaTime);
		}
		if (valid)
		{
			if ((bool)myTarget)
			{
				targetCenter = GetTargetCenter(myTarget);
			}
			else
			{
				targetCenter = m_TargetPosition;
			}
			timeNow += deltaTime;
			subZ = (targetCenter - base.transform.position).normalized;
			speed = Mathf.Min(acceleration * timeNow, maxSpeed);
			mainPos += direction * speed * deltaTime;
			angle = Vector3.Angle(direction, subZ);
			if (trackChance && angle < 20f)
			{
				trackChance = false;
			}
			if (!trackChance && !loseTarget && angle > 80f)
			{
				loseTarget = true;
			}
			if (!loseTarget)
			{
				direction = Vector3.Slerp(direction, subZ, lerpSpeed * deltaTime / angle);
			}
			progress = timeNow * speed / Mathf.Min(100f, (targetCenter - startPos).magnitude);
			coordinateX = 3f * parameterB * Mathf.Cos(timeNow * angleSpeed + angleStart) + parameterA * Mathf.Cos(angleSpeed2 * timeNow * angleSpeed - angleStart);
			coordinateZ = parameterB * Mathf.Sin(timeNow * angleSpeed + angleStart) + parameterA * Mathf.Sin(angleSpeed2 * timeNow * angleSpeed - angleStart);
			coordinateX = ((index % 2 != 1) ? (0f - coordinateX) : coordinateX);
			coordinateZ = ((index % 2 != 1) ? (0f - coordinateZ) : coordinateZ);
			upSpeed = maxUpSpeed / maxSpeed * speed;
			refPos = startPos + refY * upSpeed * timeNow + refX * coordinateX + refZ * coordinateZ;
			if (progress <= progress1)
			{
				lerpX = progress / progress1 * angle1 * (float)Math.PI;
			}
			else if (progress <= progress2)
			{
				lerpX = ((progress - progress1) / (progress2 - progress1) * (angle2 - angle1) + angle1) * (float)Math.PI;
			}
			else if (progress <= progress3)
			{
				lerpX = ((progress - progress2) / (progress3 - progress2) * (angle3 - angle2) + angle2) * (float)Math.PI;
			}
			else if (progress <= progress4)
			{
				lerpX = ((progress - progress3) / (progress4 - progress3) * (angle4 - angle3) + angle3) * (float)Math.PI;
			}
			else
			{
				lerpX = 0f;
			}
			lerpY = (Mathf.Cos(lerpX) + (1f + lerpMin) / (1f - lerpMin)) / (2f / (1f - lerpMin));
			finalPos = Vector3.Lerp(refPos, mainPos, lerpY);
			return finalPos - base.transform.position;
		}
		return direction * 0.0001f;
	}

	public override Quaternion Rotate(float deltaTime)
	{
		if (valid)
		{
			return Quaternion.FromToRotation(Vector3.forward, base.moveVector);
		}
		return Quaternion.FromToRotation(Vector3.forward, direction);
	}

	private void MissileSwitch(float deltaTime)
	{
		timeNow += deltaTime;
		if (timeNow > delay + interval * (float)index - interval)
		{
			valid = true;
			timeNow = 0f;
			startPos = base.transform.position;
			mainPos = base.transform.position;
			base.transform.transform.GetChild(0).gameObject.SetActive(value: true);
			base.transform.transform.GetChild(1).gameObject.SetActive(value: true);
		}
	}
}
