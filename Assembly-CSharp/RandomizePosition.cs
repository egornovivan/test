using UnityEngine;

public class RandomizePosition : MonoBehaviour
{
	public float Radius = 3f;

	public bool IsPlanar;

	private void Start()
	{
		Vector3 vector = Random.insideUnitSphere * Radius;
		Vector3 insideUnitSphere = Random.insideUnitSphere;
		if (IsPlanar)
		{
			vector.y = 0f;
			insideUnitSphere.x = 0f;
			insideUnitSphere.z = 0f;
		}
		base.transform.position += vector * Radius;
		base.transform.rotation = Quaternion.Euler(insideUnitSphere * 360f);
		Object.Destroy(this);
	}
}
