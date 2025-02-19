using InControl;
using UnityEngine;

public static class CamInput
{
	public static bool GetKey(ECamKey key)
	{
		return key switch
		{
			ECamKey.CK_Mouse0 => Input.GetMouseButton(0), 
			ECamKey.CK_Mouse1 => Input.GetMouseButton(1), 
			ECamKey.CK_MoveLeft => PeInput.Get(PeInput.LogicFunction.MoveLeft) || Input.GetKey(KeyCode.LeftArrow) || (SystemSettingData.Instance.UseController && Input.GetAxis("LeftStickHorizontal") < -0.1f), 
			ECamKey.CK_MoveRight => PeInput.Get(PeInput.LogicFunction.MoveRight) || Input.GetKey(KeyCode.RightArrow) || (SystemSettingData.Instance.UseController && Input.GetAxis("LeftStickHorizontal") > 0.1f), 
			ECamKey.CK_MoveUp => Input.GetKey(KeyCode.Space), 
			ECamKey.CK_MoveDown => Input.GetKey(KeyCode.LeftAlt), 
			ECamKey.CK_MoveForward => PeInput.Get(PeInput.LogicFunction.MoveForward) || Input.GetKey(KeyCode.UpArrow) || (SystemSettingData.Instance.UseController && Input.GetAxis("LeftStickVertical") > 0.1f), 
			ECamKey.CK_MoveBack => PeInput.Get(PeInput.LogicFunction.MoveBackward) || Input.GetKey(KeyCode.DownArrow) || (SystemSettingData.Instance.UseController && Input.GetAxis("LeftStickVertical") < -0.1f), 
			_ => false, 
		};
	}

	public static float GetAxis(ECamKey key)
	{
		return key switch
		{
			ECamKey.CK_MouseWheel => 0f - Input.GetAxis("Mouse ScrollWheel"), 
			ECamKey.CK_MouseX => Input.GetAxis("Mouse X") * ((!SystemSettingData.Instance.CameraHorizontalInverse) ? 1f : (-1f)) * SystemSettingData.Instance.CameraSensitivity, 
			ECamKey.CK_MouseY => Input.GetAxis("Mouse Y") * ((!SystemSettingData.Instance.CameraVerticalInverse) ? (-1f) : 1f) * SystemSettingData.Instance.CameraSensitivity, 
			ECamKey.CK_JoyStickX => (!SystemSettingData.Instance.UseController) ? 0f : ((float)InputManager.ActiveDevice.RightStickX * 180f * Time.deltaTime * ((!SystemSettingData.Instance.CameraHorizontalInverse) ? 1f : (-1f)) * SystemSettingData.Instance.CameraSensitivity), 
			ECamKey.CK_JoyStickY => (!SystemSettingData.Instance.UseController) ? 0f : ((float)InputManager.ActiveDevice.RightStickY * 180f * Time.deltaTime * ((!SystemSettingData.Instance.CameraVerticalInverse) ? 1f : (-1f)) * SystemSettingData.Instance.CameraSensitivity), 
			_ => (!GetKey(key)) ? 0f : 1f, 
		};
	}
}
