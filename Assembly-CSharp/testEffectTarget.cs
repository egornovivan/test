using UnityEngine;

public class testEffectTarget : MonoBehaviour
{
	public float lifeTime = 2f;

	public float radius = 100f;

	public float speed = 5f;

	public GameObject attacker;

	private float timeNow;

	public void InitPos()
	{
		Vector3 vector = Random.rotation * (Vector3.up * radius);
		base.transform.position += vector;
		if (base.transform.position.y < 0f)
		{
			base.transform.position = new Vector3(base.transform.position.x, 0f - base.transform.position.y, base.transform.position.z);
		}
		if (speed > 0f)
		{
			base.transform.GetComponent<Rigidbody>().velocity = new Vector3(Random.value, Random.value, Random.value).normalized * speed;
		}
	}

	private void Update()
	{
		timeNow += Time.deltaTime;
		if (attacker == null)
		{
			Object.Destroy(base.gameObject);
		}
		if (timeNow >= lifeTime)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
