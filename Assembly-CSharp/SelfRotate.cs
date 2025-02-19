using UnityEngine;

public class SelfRotate : MonoBehaviour
{
	public float Speed = 5f;

	private void Update()
	{
		base.transform.localEulerAngles = new Vector3(0f, Time.time * Speed, 0f);
	}
}
