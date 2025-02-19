using UnityEngine;

public class LinearBullet : MonoBehaviour
{
	public float speed;

	private void FixedUpdate()
	{
		base.transform.position += base.transform.forward * speed * Time.deltaTime;
	}
}
