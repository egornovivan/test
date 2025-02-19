using CameraForge;
using UnityEngine;

public class Test : MonoBehaviour
{
	public CameraController camCtrl;

	private void Start()
	{
		CameraController.SetTransform("Character", GameObject.Find("Character").transform);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			camCtrl.CrossFade("Test Blender", 0, 0.2f);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			camCtrl.CrossFade("Test Blender", 1, 0.2f);
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			camCtrl.CrossFade("Test Blender", 2, 0.2f);
		}
	}
}
