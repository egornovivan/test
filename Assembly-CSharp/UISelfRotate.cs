using UnityEngine;

public class UISelfRotate : MonoBehaviour
{
	public float Speed = 20f;

	private void Update()
	{
		base.transform.eulerAngles = base.transform.eulerAngles + Vector3.forward * Speed * Time.deltaTime;
	}
}
