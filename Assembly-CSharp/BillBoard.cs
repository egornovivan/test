using PETools;
using UnityEngine;

public class BillBoard : MonoBehaviour
{
	public Camera cameraToLookAt;

	private void Start()
	{
		cameraToLookAt = Camera.main;
	}

	private void Update()
	{
		base.transform.rotation = PEUtil.MainCamTransform.rotation;
	}
}
