using System;
using UnityEngine;

internal class testEffectPRB : MonoBehaviour
{
	public float speed = 4f;

	public float minMagnitude = 1f;

	public float maxMagnitude = 1.5f;

	public float angleScope = 180f;

	public float distance = 10f;

	private Vector3 speedVector;

	private Vector3 speedY;

	private float progress;

	private float mag;

	private float angle;

	private float transMag;

	private float dis;

	public void Start()
	{
		mag = UnityEngine.Random.Range(minMagnitude, maxMagnitude);
		angle = UnityEngine.Random.Range(180f - angleScope, angleScope);
		base.transform.position = Vector3.zero;
	}

	public void FixedUpdate()
	{
		dis += speed * Time.deltaTime;
		progress = dis / distance;
		if (progress > 1f)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		transMag = progress * 2f - 1f;
		transMag = (1f - transMag * transMag) * mag;
		base.transform.position = dis * Vector3.forward + new Vector3(Mathf.Cos(angle / 180f * (float)Math.PI), Mathf.Sin(angle / 180f * (float)Math.PI), 0f) * transMag;
	}
}
