using UnityEngine;

public class LableBoard : MonoBehaviour
{
	public Camera cameraToLookAt;

	public float far;

	public Vector3 Cameraposition;

	public Vector3 Nameposition;

	private void Start()
	{
		cameraToLookAt = Camera.main;
	}

	private void RotationEchange(Vector3 Camerapos, Vector3 Namepos)
	{
		far = (Camerapos - Namepos).magnitude;
	}

	private void changeScale(float Nowfar)
	{
	}

	private void Update()
	{
		Nameposition = base.transform.position;
		Cameraposition = cameraToLookAt.transform.position;
		RotationEchange(Cameraposition, Nameposition);
	}
}
