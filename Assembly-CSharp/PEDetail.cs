using UnityEngine;

public class PEDetail : MonoBehaviour
{
	public Transform master;

	private void FixedUpdate()
	{
		if (GetComponent<Rigidbody>() != null)
		{
			GetComponent<Rigidbody>().MovePosition(master.position);
			GetComponent<Rigidbody>().MoveRotation(master.rotation);
		}
	}
}
