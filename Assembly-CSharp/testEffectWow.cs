using System;
using UnityEngine;

internal class testEffectWow : MonoBehaviour
{
	public float speed = 20f;

	public float missileIndex = 1f;

	public float missileCount = 1f;

	public float targetPosZ = 100f;

	public float maxProgress = 1f;

	public bool destroy;

	private float startPosZ;

	private float totalDistance;

	private float distance;

	private float progress;

	private float timeNow;

	private float speedScalar = 1f;

	private float modelPitch;

	private float modelYaw;

	private float modelRoll;

	private Vector3 offset = Vector3.zero;

	private float transMag;

	private float transUp;

	private float transFront;

	private float transRight;

	private float transAngle;

	private float magnitude;

	public void Awake()
	{
		base.transform.position = Vector3.zero;
		base.transform.forward = Vector3.forward;
		totalDistance = targetPosZ - startPosZ;
	}

	public void Start()
	{
		magnitude = totalDistance * 2f;
	}

	public void Update()
	{
		timeNow += Time.deltaTime;
		if (timeNow <= 0f)
		{
			return;
		}
		distance += speed * speedScalar * Time.deltaTime;
		progress = distance / totalDistance;
		if (progress > maxProgress)
		{
			if (destroy)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			base.transform.position = Vector3.zero;
			timeNow = -2f;
			distance = 0f;
			progress = 0f;
		}
		else
		{
			WowCode();
		}
	}

	public void LateUpdate()
	{
		offset = new Vector3(Mathf.Sin(transAngle * (float)Math.PI / 180f), Mathf.Cos(transAngle * (float)Math.PI / 180f), 0f);
		base.transform.position = distance * Vector3.forward + transMag * offset;
		base.transform.position += transFront * Vector3.forward + transUp * Vector3.up + transRight * Vector3.right;
		base.transform.rotation = Quaternion.AngleAxis(modelPitch, Vector3.right) * Quaternion.AngleAxis(modelYaw, Vector3.up) * Quaternion.AngleAxis(modelRoll, Vector3.forward);
	}

	public void WowCode()
	{
		transUp = magnitude * (1f - progress);
		transFront = DistanceToImpactPos();
	}

	public float DistanceToImpactPos()
	{
		return targetPosZ - distance;
	}
}
