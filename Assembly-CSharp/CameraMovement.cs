using DunGen;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
	public float MovementSpeed = 100f;

	private void Start()
	{
		RuntimeDungeon runtimeDungeon = Object.FindObjectOfType<RuntimeDungeon>();
		if (runtimeDungeon != null)
		{
			base.transform.forward = -runtimeDungeon.Generator.UpVector;
		}
	}

	private void Update()
	{
		Vector3 zero = Vector3.zero;
		zero += base.transform.up * Input.GetAxisRaw("Vertical");
		zero += base.transform.right * Input.GetAxisRaw("Horizontal");
		zero.Normalize();
		Vector3 vector = zero * MovementSpeed * Time.deltaTime;
		if (Input.GetKey(KeyCode.LeftShift))
		{
			vector *= 2f;
		}
		float axisRaw = Input.GetAxisRaw("Mouse ScrollWheel");
		vector += base.transform.forward * axisRaw * Time.deltaTime * MovementSpeed * 100f;
		base.transform.position += vector;
	}
}
