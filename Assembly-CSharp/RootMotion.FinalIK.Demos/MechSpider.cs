using System;
using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public class MechSpider : MonoBehaviour
{
	public LayerMask raycastLayers;

	public Transform body;

	public MechSpiderLeg[] legs;

	public float legRotationWeight = 1f;

	public float rootPositionSpeed = 5f;

	public float rootRotationSpeed = 30f;

	public float breatheSpeed = 2f;

	public float breatheMagnitude = 0.2f;

	public float height = 3.5f;

	public float minHeight = 2f;

	public float raycastHeight = 10f;

	public float raycastDistance = 5f;

	private Vector3 lastPosition;

	private Vector3 defaultBodyLocalPosition;

	private float sine;

	private RaycastHit rootHit;

	private void Update()
	{
		Vector3 legsPlaneNormal = GetLegsPlaneNormal();
		Quaternion quaternion = Quaternion.FromToRotation(base.transform.up, legsPlaneNormal);
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, quaternion * base.transform.rotation, Time.deltaTime * rootRotationSpeed);
		Vector3 legCentroid = GetLegCentroid();
		Vector3 vector = Vector3.Project(legCentroid + base.transform.up * height - base.transform.position, base.transform.up);
		base.transform.position += vector * Time.deltaTime * rootPositionSpeed;
		if (Physics.Raycast(base.transform.position + base.transform.up * raycastHeight, -base.transform.up, out rootHit, raycastHeight + raycastDistance, raycastLayers))
		{
			rootHit.distance -= raycastHeight + 2f;
			if (rootHit.distance < 0f)
			{
				Vector3 b = base.transform.position - base.transform.up * rootHit.distance;
				base.transform.position = Vector3.Lerp(base.transform.position, b, Time.deltaTime * rootPositionSpeed);
			}
		}
		sine += Time.deltaTime * breatheSpeed;
		if (sine >= (float)Math.PI * 2f)
		{
			sine -= (float)Math.PI * 2f;
		}
		float num = Mathf.Sin(sine) * breatheMagnitude;
		Vector3 vector2 = base.transform.up * num;
		body.transform.position = base.transform.position + vector2;
	}

	private Vector3 GetLegCentroid()
	{
		Vector3 zero = Vector3.zero;
		float num = 1f / (float)legs.Length;
		for (int i = 0; i < legs.Length; i++)
		{
			zero += legs[i].position * num;
		}
		return zero;
	}

	private Vector3 GetLegsPlaneNormal()
	{
		Vector3 vector = base.transform.up;
		if (legRotationWeight <= 0f)
		{
			return vector;
		}
		float t = 1f / Mathf.Lerp(legs.Length, 1f, legRotationWeight);
		for (int i = 0; i < legs.Length; i++)
		{
			Vector3 vector2 = legs[i].position - (base.transform.position - base.transform.up * height);
			Vector3 normal = base.transform.up;
			Vector3 tangent = vector2;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			Quaternion b = Quaternion.FromToRotation(tangent, vector2);
			b = Quaternion.Lerp(Quaternion.identity, b, t);
			vector = b * vector;
		}
		return vector;
	}
}
