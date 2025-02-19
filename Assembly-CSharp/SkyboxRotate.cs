using UnityEngine;

public class SkyboxRotate : MonoBehaviour
{
	public Transform Planet;

	private void Update()
	{
		base.transform.rotation = Planet.rotation;
	}
}
