using System;
using UnityEngine;

[Serializable]
public class randomRotate : MonoBehaviour
{
	private Quaternion rotTarget;

	public float rotateEverySecond;

	public randomRotate()
	{
		rotateEverySecond = 1f;
	}

	public virtual void Start()
	{
		randomRot();
		InvokeRepeating("randomRot", 0f, rotateEverySecond);
	}

	public virtual void Update()
	{
		transform.rotation = Quaternion.Lerp(transform.rotation, rotTarget, Time.time * Time.deltaTime);
	}

	public virtual void randomRot()
	{
		rotTarget = UnityEngine.Random.rotation;
	}

	public virtual void Main()
	{
	}
}
