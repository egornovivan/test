using System;
using UnityEngine;

internal class testEffectVLS : MonoBehaviour
{
	public float maxTraction = 25f;

	public float upDistance = 10f;

	public float dragCoefficient = 0.15f;

	public float gravity = 9.8f;

	public float lifeTime = 2f;

	public float maxHalfAngle = 90f;

	public float smoothAngleStart = 30f;

	public float smoothPercent = 0.6f;

	public GameObject target;

	private float timeNow;

	private bool track;

	private Vector3 startPos;

	private Vector3 velocity;

	private Vector3 drag;

	private Vector3 traction;

	private Vector3 acceleration;

	private float angle;

	private Vector3 subX;

	private Vector3 subY;

	private float percent;

	private float maxv;

	public void Start()
	{
		startPos = base.transform.position;
		base.transform.rotation = Quaternion.FromToRotation(Vector3.forward, Vector3.up);
		traction = Vector3.up * maxTraction;
		acceleration = traction + Vector3.down * gravity;
		base.transform.GetComponent<Rigidbody>().AddForce(acceleration, ForceMode.Acceleration);
	}

	public void FixedUpdate()
	{
		timeNow += Time.deltaTime;
		velocity = base.transform.GetComponent<Rigidbody>().velocity;
		if (velocity.magnitude > maxv)
		{
			maxv = velocity.magnitude;
		}
		if (timeNow >= lifeTime)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		if (Vector3.Distance(base.transform.position, target.transform.position) <= 1f)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		drag = velocity.normalized * 0.054707f * dragCoefficient * velocity.sqrMagnitude + Vector3.up * gravity;
		if (!track)
		{
			if ((base.transform.position - startPos).sqrMagnitude < upDistance * upDistance)
			{
				base.transform.GetComponent<Rigidbody>().AddForce(Vector3.up * maxTraction + drag, ForceMode.Acceleration);
				return;
			}
			track = true;
		}
		subX = Vector3.Cross(velocity, target.transform.position - base.transform.position);
		subY = Vector3.Cross(subX, velocity).normalized;
		if (Vector3.Angle(velocity, target.transform.position - base.transform.position) <= smoothAngleStart)
		{
			if (drag.sqrMagnitude >= maxTraction * maxTraction)
			{
				traction = drag.normalized * maxTraction;
				base.transform.GetComponent<Rigidbody>().AddForce(traction - drag, ForceMode.Acceleration);
			}
			else
			{
				percent = Vector3.Angle(velocity, target.transform.position - base.transform.position) / smoothAngleStart * smoothPercent + 1f - smoothPercent;
				if (Vector3.Angle(velocity, target.transform.position - base.transform.position) < 1f)
				{
					percent = 0f;
				}
				traction = Vector3.Lerp(velocity.normalized, subY, percent);
				angle = Vector3.Angle(traction, drag);
				traction *= Mathf.Sqrt(maxTraction * maxTraction - drag.sqrMagnitude * Mathf.Sin(angle / 180f * (float)Math.PI) * Mathf.Sin(angle / 180f * (float)Math.PI)) - drag.magnitude * Mathf.Cos(angle / 180f * (float)Math.PI);
				base.transform.GetComponent<Rigidbody>().AddForce(traction, ForceMode.Acceleration);
			}
		}
		else
		{
			percent = maxHalfAngle / 90f;
			traction = Vector3.Lerp(velocity.normalized, subY, percent) * maxTraction;
			base.transform.GetComponent<Rigidbody>().AddForce(traction - drag, ForceMode.Acceleration);
		}
		base.transform.rotation = Quaternion.FromToRotation(Vector3.forward, velocity);
	}
}
