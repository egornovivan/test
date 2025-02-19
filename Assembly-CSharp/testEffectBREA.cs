using System;
using System.Collections;
using UnityEngine;

internal class testEffectBREA : MonoBehaviour
{
	public float anglePerCycle;

	public float timePerCycle;

	public float offsetRadius;

	public float delayTime = 0.1f;

	public Transform effect;

	public testBreathType breathType;

	public int listNum;

	private float currentAngle;

	private float currentTime;

	private Queue locus = new Queue();

	private CapsuleCollider coli;

	private Vector3 tempPosition;

	private Vector3 tempForward;

	public void Start()
	{
		base.transform.position = Vector3.forward * offsetRadius;
		base.transform.forward = Vector3.forward;
		if (GetComponent<Collider>() != null)
		{
			coli = GetComponent<Collider>() as CapsuleCollider;
			StartCoroutine(LengthUp());
		}
	}

	public void Update()
	{
		currentTime += Time.deltaTime;
		if (breathType == testBreathType.FRO)
		{
			currentAngle = (currentTime - (float)(int)(currentTime / timePerCycle) * timePerCycle) / timePerCycle * anglePerCycle;
			if ((int)(currentTime / timePerCycle) % 2 == 1)
			{
				currentAngle = anglePerCycle - currentAngle;
			}
		}
		else if (breathType == testBreathType.CIRCLE)
		{
			currentAngle = currentTime / timePerCycle * anglePerCycle;
		}
		tempForward = new Vector3(Mathf.Sin(currentAngle * (float)Math.PI / 180f), 0f, Mathf.Cos(currentAngle * (float)Math.PI / 180f));
		tempPosition = tempForward * offsetRadius;
		if (delayTime != 0f)
		{
			locus.Enqueue(tempPosition);
			locus.Enqueue(tempForward);
			listNum = locus.Count;
			if (currentTime > delayTime)
			{
				base.transform.position = (Vector3)locus.Dequeue();
				base.transform.forward = (Vector3)locus.Dequeue();
			}
		}
		else
		{
			base.transform.position = tempPosition;
			base.transform.forward = tempForward;
		}
		if (effect != null)
		{
			effect.position = tempPosition;
			effect.forward = tempForward;
		}
	}

	private IEnumerator LengthUp()
	{
		float maxLength = coli.height;
		float unit = (coli.height - coli.radius) / (delayTime / 0.1f);
		while (true)
		{
			if (delayTime == 0f || coli == null)
			{
				yield return new WaitForSeconds(100f);
				continue;
			}
			coli.height = coli.radius - unit;
			while (true)
			{
				coli.height += unit;
				coli.center = new Vector3(0f, 0f, coli.height / 2f);
				if (coli.height > maxLength)
				{
					break;
				}
				yield return new WaitForSeconds(0.1f);
			}
			coli.height = maxLength;
			coli.center = new Vector3(0f, 0f, maxLength / 2f);
		}
	}
}
