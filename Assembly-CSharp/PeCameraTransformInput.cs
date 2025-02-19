using UnityEngine;

public class PeCameraTransformInput : MonoBehaviour
{
	public string VarName;

	private void Start()
	{
		PeCamera.SetTransform(VarName, base.transform);
	}

	private void Update()
	{
		PeCamera.SetTransform(VarName, base.transform);
	}
}
