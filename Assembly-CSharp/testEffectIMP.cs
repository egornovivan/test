using UnityEngine;

internal class testEffectIMP : MonoBehaviour
{
	public Vector3 speed;

	public float gravity;

	public float existTime;

	public impType impt;

	public destroyType dest;

	public bool followRotate;

	public Vector3 selfRotate;

	private Vector3 speedVector;

	private Vector3 speedY;

	private float rotateAngle;

	private float rotateSpeed;

	private float pastTime;

	private void Start()
	{
		rotateSpeed = selfRotate.magnitude;
	}

	public void Update()
	{
		if (pastTime == 0f)
		{
			base.transform.position = Vector3.zero;
			if (impt == impType.FORWARD)
			{
				speedVector = speed;
			}
			else if (impt == impType.FOUNTAIN)
			{
				speedVector = new Vector3(Random.value - 0.5f, Random.value * 0.5f + 0.5f, Random.value - 0.5f).normalized * speed.magnitude;
			}
			speedY = Vector3.zero;
			GetComponent<Rigidbody>().velocity = speedVector;
		}
		else
		{
			speedY += Vector3.down * gravity * Time.deltaTime;
			GetComponent<Rigidbody>().velocity = speedVector + speedY;
		}
		if (followRotate)
		{
			base.transform.rotation = Quaternion.FromToRotation(Vector3.forward, GetComponent<Rigidbody>().velocity);
		}
		if (selfRotate != Vector3.zero)
		{
			rotateAngle += rotateSpeed * Time.deltaTime;
			base.transform.rotation *= Quaternion.AngleAxis(rotateAngle, selfRotate);
		}
		pastTime += Time.deltaTime;
		if (pastTime > existTime)
		{
			if (dest == destroyType.DESTROY)
			{
				Object.Destroy(base.gameObject);
			}
			else if (dest == destroyType.REFRESH)
			{
				pastTime = 0f;
			}
		}
	}
}
