using System;
using UnityEngine;

[Serializable]
public class CameraModeData
{
	public int camModeIndex1st;

	public int camModeIndex3rd;

	public Vector3 offsetUp;

	public Vector3 offset;

	public Vector3 offsetDown;

	public static CameraModeData DefaultCameraData = new CameraModeData(0, 0, new Vector3(0f, 0f, 0f), new Vector3(0f, -0.1f, 0f), new Vector3(0f, 0f, 0f));

	public CameraModeData(int index1st, int index3rd, Vector3 up, Vector3 mid, Vector3 down)
	{
		camModeIndex1st = index1st;
		camModeIndex3rd = index3rd;
		offsetUp = up;
		offset = mid;
		offsetDown = down;
	}
}
