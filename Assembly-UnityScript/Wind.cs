using System;
using UnityEngine;

[Serializable]
public class Wind : MonoBehaviour
{
	public Vector3 speed;

	public Vector3 speedRedirect;

	public virtual void Start()
	{
		speed += new Vector3(UnityEngine.Random.Range(0f - speedRedirect.x, speedRedirect.x), UnityEngine.Random.Range(0f - speedRedirect.y, speedRedirect.y), UnityEngine.Random.Range(0f - speedRedirect.z, speedRedirect.z));
	}

	public virtual void Update()
	{
		GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity + speed * Time.deltaTime;
	}

	public virtual void Main()
	{
	}
}
