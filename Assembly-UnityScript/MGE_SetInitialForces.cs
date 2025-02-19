using System;
using UnityEngine;

[Serializable]
public class MGE_SetInitialForces : MonoBehaviour
{
	public bool relativeForce;

	public float x;

	public float xDeviation;

	public float y;

	public float yDeviation;

	public float z;

	public float zDeviation;

	public bool relativeTorque;

	public float torqueScale;

	public float xRot;

	public float xRotDeviation;

	public float yRot;

	public float yRotDeviation;

	public float zRot;

	public float zRotDeviation;

	public MGE_SetInitialForces()
	{
		relativeForce = true;
		relativeTorque = true;
		torqueScale = 100f;
	}

	public virtual void Start()
	{
		if (relativeForce)
		{
			GetComponent<Rigidbody>().AddRelativeForce(UnityEngine.Random.Range(x - xDeviation, x + xDeviation), UnityEngine.Random.Range(y - yDeviation, y + yDeviation), UnityEngine.Random.Range(z - zDeviation, z + zDeviation));
		}
		if (!relativeForce)
		{
			GetComponent<Rigidbody>().AddForce(UnityEngine.Random.Range(x - xDeviation, x + xDeviation), UnityEngine.Random.Range(y - yDeviation, y + yDeviation), UnityEngine.Random.Range(z - zDeviation, z + zDeviation));
		}
		if (relativeTorque)
		{
			GetComponent<Rigidbody>().AddRelativeTorque(UnityEngine.Random.Range(xRot - xRotDeviation, xRot + xRotDeviation) * torqueScale, UnityEngine.Random.Range(yRot - yRotDeviation, yRot + yRotDeviation) * torqueScale, UnityEngine.Random.Range(zRot - zRotDeviation, zRot + zRotDeviation) * torqueScale);
		}
		if (!relativeTorque)
		{
			GetComponent<Rigidbody>().AddTorque(UnityEngine.Random.Range(xRot - xRotDeviation, xRot + xRotDeviation) * torqueScale, UnityEngine.Random.Range(yRot - yRotDeviation, yRot + yRotDeviation) * torqueScale, UnityEngine.Random.Range(zRot - zRotDeviation, zRot + zRotDeviation) * torqueScale);
		}
	}

	public virtual void FixedUpdate()
	{
	}

	public virtual void Main()
	{
	}
}
