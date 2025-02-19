using System;
using UnityEngine;

[Serializable]
public class Slash : MonoBehaviour
{
	public Vector3 speed;

	public Slash()
	{
		speed = Vector3.one;
	}

	public virtual void Update()
	{
		float x = transform.localScale.x + speed.x * Time.deltaTime;
		Vector3 localScale = transform.localScale;
		float num = (localScale.x = x);
		Vector3 vector2 = (transform.localScale = localScale);
		float y = transform.localScale.y + speed.y * Time.deltaTime;
		Vector3 localScale2 = transform.localScale;
		float num2 = (localScale2.y = y);
		Vector3 vector4 = (transform.localScale = localScale2);
		if (!(transform.localScale.y >= 0f))
		{
			int num3 = 0;
			Vector3 localScale3 = transform.localScale;
			float num4 = (localScale3.y = num3);
			Vector3 vector6 = (transform.localScale = localScale3);
		}
	}

	public virtual void Main()
	{
	}
}
