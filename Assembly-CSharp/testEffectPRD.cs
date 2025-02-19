using System;
using UnityEngine;

internal class testEffectPRD : MonoBehaviour
{
	public float speed;

	public float existTime;

	public GameObject target;

	private float timeNow;

	private void Start()
	{
		Vector3 from = base.transform.position - target.transform.position;
		float num = Mathf.Sqrt(speed * speed - target.GetComponent<Rigidbody>().velocity.sqrMagnitude);
		float num2 = Mathf.Cos(Vector3.Angle(from, target.GetComponent<Rigidbody>().velocity) / 180f * (float)Math.PI);
		float num3 = from.sqrMagnitude * target.GetComponent<Rigidbody>().velocity.sqrMagnitude * num2 * num2;
		float num4 = ((!(Vector3.Angle(from, target.GetComponent<Rigidbody>().velocity) <= 90f)) ? ((Mathf.Sqrt(from.sqrMagnitude + num3 / num / num) + Mathf.Sqrt(num3 / num / num)) / num) : ((Mathf.Sqrt(from.sqrMagnitude + num3 / num / num) - Mathf.Sqrt(num3 / num / num)) / num));
		Vector3 vector = target.transform.position + target.GetComponent<Rigidbody>().velocity * num4;
		base.transform.GetComponent<Rigidbody>().velocity = (vector - base.transform.position).normalized * speed;
		base.transform.rotation = Quaternion.FromToRotation(Vector3.forward, vector - base.transform.position);
	}

	private void FixedUpdate()
	{
		timeNow += Time.deltaTime;
		if ((target.transform.position - base.transform.position).sqrMagnitude <= 0.25f)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		if (timeNow >= existTime)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
