using UnityEngine;

public class particleTrajectory : MonoBehaviour
{
	public Vector3 speed;

	public float lifeTime;

	private void Start()
	{
		Invoke("dest", lifeTime);
	}

	private void FixedUpdate()
	{
		base.transform.position += speed * Time.deltaTime;
	}

	private void dest()
	{
		Object.Destroy(base.gameObject);
	}
}
