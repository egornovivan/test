using UnityEngine;

public class seeFoward : MonoBehaviour
{
	public Vector3 position;

	public Vector3 foward;

	public Vector3 eulerAngle;

	public Vector4 rotation;

	private void Start()
	{
	}

	private void Update()
	{
		position = base.transform.position;
		foward = base.transform.forward;
		eulerAngle = base.transform.rotation.eulerAngles;
		rotation = new Vector4(base.transform.rotation.x, base.transform.rotation.y, base.transform.rotation.z, base.transform.rotation.w);
		Debug.DrawLine(base.transform.position, base.transform.position + 30f * base.transform.forward, Color.blue);
		Debug.DrawLine(base.transform.position, base.transform.position + 30f * base.transform.up, Color.green);
		Debug.DrawLine(base.transform.position, base.transform.position + 30f * base.transform.right, Color.red);
	}
}
