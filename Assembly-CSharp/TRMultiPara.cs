using System;
using UnityEngine;

public class TRMultiPara : Trajectory
{
	public float speed = 50f;

	public float MagScaleMin = 0.1f;

	public float MagScaleMax = 0.2f;

	public float angleScope = 160f;

	private Transform myTarget;

	private Vector3 startPos;

	private float totalLength;

	private float progressOffset;

	private float progress;

	private float transAngle;

	private float maxMagnitude;

	private float transMag;

	private Vector3 subX;

	private Vector3 subY;

	private Vector3 subZ;

	private Vector3 offset = Vector3.zero;

	private Vector3 mainPath = Vector3.zero;

	public void Ini(Transform tra)
	{
		startPos = base.transform.position;
		subZ = Tes(tra, startPos, speed) - startPos;
		totalLength = subZ.magnitude;
		subZ = subZ.normalized;
		base.transform.forward = subZ;
	}

	private Vector3 Tes(Transform target, Vector3 startPos, float speed)
	{
		Vector3 from = startPos - target.position;
		float num = Mathf.Sqrt(speed * speed - target.GetComponent<Rigidbody>().velocity.sqrMagnitude);
		float num2 = Mathf.Cos(Vector3.Angle(from, target.GetComponent<Rigidbody>().velocity) / 180f * (float)Math.PI);
		float num3 = from.sqrMagnitude * target.GetComponent<Rigidbody>().velocity.sqrMagnitude * num2 * num2;
		float num4 = ((!(Vector3.Angle(from, target.GetComponent<Rigidbody>().velocity) <= 90f)) ? ((Mathf.Sqrt(from.sqrMagnitude + num3 / num / num) + Mathf.Sqrt(num3 / num / num)) / num) : ((Mathf.Sqrt(from.sqrMagnitude + num3 / num / num) - Mathf.Sqrt(num3 / num / num)) / num));
		return target.position + target.GetComponent<Rigidbody>().velocity * num4;
	}

	public void Emit(Transform target)
	{
		myTarget = target;
		startPos = base.transform.position;
		subZ = GetPredictPosition(myTarget, startPos, speed) - startPos;
		totalLength = subZ.magnitude;
		subZ = subZ.normalized;
		base.transform.forward = subZ;
	}

	private void Start()
	{
		if (GetComponent<Rigidbody>() != null)
		{
			GetComponent<Rigidbody>().useGravity = false;
		}
		maxMagnitude = UnityEngine.Random.Range(MagScaleMin, MagScaleMax);
		transAngle = UnityEngine.Random.Range((0f - angleScope) / 2f, angleScope / 2f);
		Emit(m_Target);
	}

	public override Vector3 Track(float deltaTime)
	{
		progressOffset += speed * Time.deltaTime;
		progress = progressOffset / totalLength;
		transMag = 4f * maxMagnitude * totalLength * (progress - progress * progress);
		subX = Vector3.Cross(Vector3.up, new Vector3(subZ.x, 0f, subZ.z)).normalized;
		subY = Vector3.Cross(subZ, subX);
		if (transAngle < 0f)
		{
			transAngle += (float)((int)(transAngle / 360f) + 1) * 360f;
		}
		offset.x = (int)(transAngle / 90f);
		if (offset.x == 0f)
		{
			offset = Vector3.Slerp(subY, subX, transAngle / 90f);
		}
		else if (offset.x == 1f)
		{
			offset = Vector3.Slerp(subX, -subY, (transAngle - 90f) / 90f);
		}
		else if (offset.x == 2f)
		{
			offset = Vector3.Slerp(-subY, -subX, (transAngle - 180f) / 90f);
		}
		else
		{
			offset = Vector3.Slerp(-subX, subY, (transAngle - 270f) / 90f);
		}
		offset *= transMag;
		mainPath = subZ * totalLength * progress;
		return startPos + mainPath + offset - base.transform.position;
	}

	public override Quaternion Rotate(float deltaTime)
	{
		return Quaternion.FromToRotation(Vector3.forward, base.moveVector);
	}
}
